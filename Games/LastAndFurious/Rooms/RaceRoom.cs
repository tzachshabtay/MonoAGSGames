using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public struct RaceEventConfig
    {
        public int PlayerDriver; // TODO: use string ID instead?
        public int Opponents;
        public int Laps;
        public RacePhysicsMode Physics;
        public bool CarCollisions;

        public RaceEventConfig(int player, int opponents, int laps, RacePhysicsMode mode, bool carCollide)
        {
            PlayerDriver = player;
            Opponents = opponents;
            Laps = laps;
            Physics = mode;
            CarCollisions = carCollide;
        }
    }

    public class RaceRoom : RoomScript, IThisGameState
    {
        private const string ROOM_ID = "RaceRoom";
        private const float CHANGE_AI_CAMERA_TIME = 8000f;

        private IAudioClip _music;

        private Vector2[] _startingGrid; // TODO: move to Track config
        private Vecregion _lapTestRegion; // TODO: move to Track config
        private Race _race;
        private Track _track;
        // Current AI controller
        private AIController _ai;
        // Supported ai controller types
        private AIRegionBased _aiRegionBased;
        private AIPathBase _aiPathBased;
        // TODO: move to camera manager
        private Camera _camera;
        private IObject _cameraTarget;
        private Timer _tChangeAICamera;

        private bool _isAIRace;

        private int _raceStartSequence;
        private int _raceEndSequence;
        private IObject _banner;
        private Tween _bannerTween;
        private Timer _bannerTimer;


        public RaceRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        public void StartSinglePlayer(RaceEventConfig eventCfg)
        {
            LF.Menu.HideMenu();
            // FadeOut(50); TODO
            setupSinglePlayerRace(eventCfg);
            // FadeIn(50); TODO
            runStartSequence();
        }

        public void StartAIDemo(RaceEventConfig eventCfg)
        {
            LF.Menu.HideMenu();
            // FadeOut(50); TODO
            setupAIRace(eventCfg);
            // FadeIn(50); TODO
        }

        protected void clearRoom()
        {
            clearRace();

            RaceUI.Clear();

            _camera = null;
            _aiRegionBased = null;
            _aiPathBased = null;
            
            _race = null;
            _track = null;
            _startingGrid = null;
            _lapTestRegion = new Vecregion();
        }

        protected override async Task<IRoom> loadAsync()
        {
            clearRoom();

            IGameFactory factory = _game.Factory;
            _room = factory.Room.GetRoom(ROOM_ID);
            _room.RoomLimitsProvider = AGSRoomLimits.FromBackground;

            _music = await factory.Sound.LoadAudioClipAsync(LF.MusicAssetFolder + "Welcome_to_the_Show.ogg");

            _room.Events.OnBeforeFadeIn.Subscribe(onLoad);
            _room.Events.OnAfterFadeIn.Subscribe(onAfterFadeIn);
            _room.Events.OnAfterFadeOut.Subscribe(onLeave);

            _track = await TrackLoader.LoadAsync(_roomAssetFolder + "track01.ini", _roomAssetFolder, _game.Factory.Graphics);
            _race = new Race(_game, _room, _track);

            _room.Background = addObject("RaceRoom.BG", _track.Background);

            // TODO: move to track config
            _startingGrid = new Vector2[Race.MAX_RACING_CARS];
            _startingGrid[0] = compatVector(1140, 326 + 12);
            _startingGrid[1] = compatVector(1172, 273 + 12);
            _startingGrid[2] = compatVector(1204, 326 + 12);
            _startingGrid[3] = compatVector(1236, 273 + 12);
            _startingGrid[4] = compatVector(1268, 326 + 12);
            _startingGrid[5] = compatVector(1300, 273 + 12);
            Vector2 p1 = compatVector(1104, 399);
            Vector2 p2 = compatVector(1119, 190);
            _lapTestRegion = new Vecregion(p1, p2);

            // Create Ai controllers supported by this track
            if (_track.AiData.AIRegionMask != null && _track.AiData.AIRegionAngles != null)
                _aiRegionBased = new AIRegionBased(_game, _track.AiData.AIRegionMask, _track.AiData.AIRegionAngles);
            if (_track.AiData.AIPathNodes != null)
                _aiPathBased = new AIPathBase(_game, _track.AiData.AIPathNodes);

            _camera = new Camera();
            _game.State.Viewport.Camera = _camera;

            return _room;
        }

        private void onLoad()
        {
            /* TODO:
            FadeOut(0);
            StopAllAudio();
            */

            RaceUI.Init(_game);

            RaceEventConfig cfg = LF.Menu.RaceConfig;
            cfg.PlayerDriver = -1; // no player
            cfg.Opponents = LF.RaceAssets.Drivers.Count; // max drivers
            cfg.Laps = 0; // drive forever
            setupAIRace(cfg);
            _music.Play(true);

            GameStateManager.PushState(this);
        }

        private void onAfterFadeIn()
        {
            /* TODO:
            FadeIn(50);
            */
        }

        private void onLeave()
        {
            GameStateManager.PopState(this);

            clearRace();
            /* TODO:
            StopAllAudio();
            */
        }

        public bool IsBlocking { get => false; }

        public void RepExec(float deltaTime)
        {
            // TODO: temp, remove/change
            IInput input = _game.Input;
            if (input.IsKeyDown(Key.PageDown))
                _game.State.Viewport.ScaleX = _game.State.Viewport.ScaleY = _game.State.Viewport.ScaleX - 0.02F;
            if (input.IsKeyDown(Key.PageUp))
                _game.State.Viewport.ScaleX = _game.State.Viewport.ScaleY = _game.State.Viewport.ScaleX + 0.02F;
            if (input.IsKeyDown(Key.Insert))
                _game.State.Viewport.Angle += 1F;
            if (input.IsKeyDown(Key.Delete))
                _game.State.Viewport.Angle -= 1F;

            if (_raceStartSequence > 0)
                return; // wait until sequence ends

            _race.Run(deltaTime);

            // TODO: move to Race class at some point
            testLapComplete();

            if (RaceUI.IsShown)
                RaceUI.Update();
        }

        public bool OnKeyDown(KeyboardEventArgs args)
        {
            if (_isAIRace || args.Key == Key.Escape)
            {
                // TODO: need to have resume callback from menu
                //if (_raceStartSequence > 0 && _bannerTween != null)
                  //  _bannerTween.Pause();
                if (_isAIRace)
                    LF.Menu.ShowMenu(MenuClass.eMenuMain, false);
                else
                    LF.Menu.ShowMenu(MenuClass.eMenuMainInGame, true);
                return true;
            }
            return false;
        }

        private void _tChangeAICamera_Elapsed(object sender, ElapsedEventArgs e)
        {
            cameraTargetRandomCar(false);
        }

        private void clearRace()
        {
            RaceUI.Hide();

            if (_tChangeAICamera != null)
                _tChangeAICamera.Dispose();
            _tChangeAICamera = null;
            _cameraTarget = null;

            if (_race != null)
                _race.Clear();

            _ai = null;

            _raceStartSequence = 0;
            _raceEndSequence = 0;

            if (_bannerTimer != null)
                _bannerTimer.Dispose();
            _bannerTimer = null;
            if (_bannerTween != null)
                _bannerTween.Stop(TweenCompletion.Stay);
            _bannerTween = null;
            if (_banner != null)
                _game.State.UI.Remove(_banner);
            _banner = null;
        }

        private void positionCarOnGrid(VehicleObject car, int gridpos)
        {
            // TODO: checkme, I do not remember why it needed all this adjustment
            var ph = car.Veh.Physics;
            Vector2 pos = _startingGrid[gridpos];
            pos.X += 4 + ph.BodyLength / 2;
            pos.Y += 1;
            ph.Reset(_track, pos, new Vector2(-1, 0));
        }

        private void cameraTargetPlayerCar(bool snap)
        {
            if (_tChangeAICamera != null)
            {
                _tChangeAICamera.Dispose();
                _tChangeAICamera = null;
            }

            _cameraTarget = _race.Player.Car.O;
            _camera.TargettingAcceleration = 0f;
            if (snap)
                _camera.Snap();
        }

        private void cameraTargetRandomCar(bool snap)
        {
            if (_tChangeAICamera == null)
            {
                _tChangeAICamera = new Timer(CHANGE_AI_CAMERA_TIME);
                _tChangeAICamera.AutoReset = true;
                _tChangeAICamera.Elapsed += _tChangeAICamera_Elapsed;
                _tChangeAICamera.Start();
            }

            _cameraTarget = _race.Cars[MathUtils.Random().Next(0, _race.Cars.Count - 1)].O;
            _camera.TargettingAcceleration = 0.5f;
            if (snap)
                _camera.Snap();
        }

        // TODO: work around this
        private IObject getCameraTarget()
        {
            return _cameraTarget;
        }

        private void setupRace(RaceEventConfig eventCfg)
        {
            clearRace();

            // Prepare list of drivers
            DriverCharacter playerDriver;
            List<DriverCharacter> drivers;
            prepareListOfDrivers(eventCfg, out drivers, out playerDriver);

            // Switch to the desired AI type
            if (eventCfg.Physics == RacePhysicsMode.Safe && _aiRegionBased != null)
                _ai = _aiRegionBased;
            else
                _ai = _aiPathBased;

            // Create cars, assign AI, put on the starting grid...
            createRacingCars(drivers, playerDriver);

            // Read customizable settings for the track and vehicle behavior
            readAIAndPhysicsConfig(eventCfg.Physics == RacePhysicsMode.Safe ? "race_safe.ini" : "race_wild.ini");

            _race.Start(eventCfg.Laps, eventCfg.CarCollisions);
        }

        private void prepareListOfDrivers(RaceEventConfig eventCfg,
            out List<DriverCharacter> drivers, out DriverCharacter playerDriver)
        {
            playerDriver = null;
            drivers = new List<DriverCharacter>();

            // Add player, if its required
            if (eventCfg.PlayerDriver >= 0)
            {
                playerDriver = LF.RaceAssets.Drivers[LF.RaceAssets.Names[eventCfg.PlayerDriver]];
                drivers.Add(playerDriver);
            }
            // Choose random set of opponents
            if (eventCfg.Opponents > 0)
            {
                DriverCharacter[] allDrivers = new DriverCharacter[LF.RaceAssets.Drivers.Count];
                LF.RaceAssets.Drivers.Values.CopyTo(allDrivers, 0);
                Utils.Shuffle(allDrivers, new System.Random());
                for (int i = 0, cnt = 0; cnt < eventCfg.Opponents; ++i)
                {
                    if (allDrivers[i] != playerDriver)
                    {
                        drivers.Add(allDrivers[i]);
                        cnt++;
                    }
                }
            }
        }

        private void createRacingCars(List<DriverCharacter> drivers, DriverCharacter playerDriver)
        {
            for (int i = 0; i < drivers.Count; ++i)
            {
                Racer racer;
                if (drivers[i] == playerDriver)
                    racer = _race.AddPlayer(drivers[i], new VehiclePlayerUI(_game));
                else
                    racer = _race.AddRacer(drivers[i], _ai.GetVehicleAI());
                positionCarOnGrid(racer.Car, i);
            }
        }

        // TODO: share duplicate code with setupSinglePlayerRace
        private void setupAIRace(RaceEventConfig eventCfg)
        {
            _game.State.Paused = true;

            setupRace(new RaceEventConfig(-1, eventCfg.Opponents, eventCfg.Laps, eventCfg.Physics, eventCfg.CarCollisions));

            _game.State.Viewport.Camera.Target = getCameraTarget;
            cameraTargetRandomCar(true);

            _isAIRace = true;
            _game.State.Paused = false;
        }

        private void setupSinglePlayerRace(RaceEventConfig eventCfg)
        {
            _game.State.Paused = true;

            setupRace(eventCfg);

            _game.State.Viewport.Camera.Target = getCameraTarget;
            cameraTargetPlayerCar(true);

            _isAIRace = false;

            RaceUI.Show(_race);
            _game.State.Paused = false;
        }

        private void readAIAndPhysicsConfig(string cfgAsset)
        {
            // TODO: switch between config types
            TrackConfig tcfg = TrackConfigurator.LoadConfig(_track, _roomAssetFolder + cfgAsset);
            if (tcfg != null)
            {
                TrackConfigurator.ApplyConfig(_track, tcfg);
            }

            VehicleConfig vcfg = VehicleConfigurator.LoadConfig(_track, _roomAssetFolder + cfgAsset);
            if (vcfg != null)
            {
                foreach (var car in _race.Cars)
                    VehicleConfigurator.ApplyConfig(car.Veh, vcfg);
            }
        }

        private void createBanner(IImage image)
        {
            int rx = _game.Settings.VirtualResolution.Width;
            int ry = _game.Settings.VirtualResolution.Height;

            _banner = addObject("RaceRoom.Banner", image, rx / 2, ry);
            _banner.IgnoreViewport = true;
            _banner.RenderLayer = AGSLayers.UI;
            _banner.Z = -3;
            _game.State.UI.Add(_banner);
        }
        
        // TODO: cs locks
        private async void runStartSequence()
        {
            _raceStartSequence = 1;
            createBanner(LF.RaceAssets.BannerReady);
            RaceUI.Pane.X = -100; // TODO: make RaceUI calculate its size
            _bannerTween = _banner.TweenY(160, 0.8f);
            await _bannerTween.Task;
            _raceStartSequence++;
            // TODO: I do not know how to make pausable timer/delay task,
            // so using this dummy tween for now
            _bannerTween = _banner.TweenZ(1, 0.8f);
            await _bannerTween.Task;
            _raceStartSequence++;
            _banner.Image = LF.RaceAssets.BannerSet;
            _bannerTween = _banner.TweenZ(1, 0.8f);
            await _bannerTween.Task;
            _raceStartSequence++;
            _banner.Image = LF.RaceAssets.BannerGo;
            _raceStartSequence = 0;
            RaceUI.Pane.TweenX(0, 0.8f);
            await _banner.TweenY(-_banner.Image.Height, 0.8f).Task;
            _game.State.UI.Remove(_banner);
            _banner.Dispose();
            _banner = null;
        }

        void runEndSequence()
        {
            _raceEndSequence = 1;
            RaceUI.Pane.Opacity = 255 * 80 / 100;
            IImage image;
            if (_race.Player.Finished == 1)
                image = LF.RaceAssets.BannerWin;
            else
                image = LF.RaceAssets.BannerLoose;
            createBanner(image);
            _banner.TweenY(160, 0.8f); // don't wait
        }

        void onPlayerFinishedRace()
        {
            runEndSequence();
        }

        // TODO: probably should be in Race class
        private void testLapComplete()
        {
            foreach (var racer in _race.Racers)
            {
                if (racer.Car.Veh.Control == null)
                    continue; // skip those that are not controlled

                if (!(racer.CurrentCheckpoint.order == 0 && racer.CheckpointsPassed > 0))
                    continue;

                bool hit_lap_test = racer.Car.Veh.Physics.TestCollide(_lapTestRegion);

                if (hit_lap_test)
                {
                    racer.SwitchToNextNode();
                    _race.OnLapComplete(racer);

                    if (racer.Finished > 0)
                    {
                        if (racer == _race.Player)
                            onPlayerFinishedRace();
                        else
                            racer.Car.Veh.DisableControl();
                    }
                }
            }
        }
    }
}
