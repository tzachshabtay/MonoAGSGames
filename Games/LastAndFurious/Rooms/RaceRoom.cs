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

    public class RaceRoom : RoomScript
    {
        private const string ROOM_ID = "RaceRoom";
        private const float CHANGE_AI_CAMERA_TIME = 8000f;

        private IAudioClip _music;

        private Vector2[] _startingGrid; // TODO: move to Track config
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


        public RaceRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        public void StartSinglePlayer(RaceEventConfig eventCfg)
        {
            GameMenu.HideMenu();
            // FadeOut(50); TODO
            setupSinglePlayerRace(eventCfg);
            // FadeIn(50); TODO
            runStartSequence();
        }

        public void StartAIDemo(RaceEventConfig eventCfg)
        {
            GameMenu.HideMenu();
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

            RaceEventConfig cfg = GameMenu.RaceConfig;
            cfg.PlayerDriver = -1; // no player
            cfg.Opponents = LF.RaceAssets.Drivers.Count; // max drivers
            cfg.Laps = 0; // drive forever
            setupAIRace(cfg);
            _music.Play(true);

            _game.Events.OnRepeatedlyExecute.Subscribe(repExec);
            _game.Input.KeyDown.Subscribe(onKeyDown);
        }

        private void onAfterFadeIn()
        {
            /* TODO:
            FadeIn(50);
            */
        }

        private void onLeave()
        {
            _game.Events.OnRepeatedlyExecute.Unsubscribe(repExec);
            _game.Input.KeyDown.Unsubscribe(onKeyDown);

            clearRace();
            /* TODO:
            StopAllAudio();
            */
        }

        private void repExec()
        {
            if (LF.GameState.Paused)
                return;

            // TODO: get delta time from one API, using more precise calculation
            float deltaTime = (float)(1.0 / AGSGame.UPDATE_RATE);

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

            /* TODO:
            

            if (RaceStartSequence > 0)
            {
                RunStartSequence();
                return;
            }
            if (!IsAIRace)
            {
                TestLapComplete();
            }
            */
            

            if (RaceUI.IsShown)
                RaceUI.Update();

            /*
            if (RaceEndSequence > 0)
            {
                RunEndSequence();
                return;
            }

            if (gBanner.Visible)
            {
                gBanner.Y = gBanner.Y + 8;
                if (gBanner.Y > System.ViewportHeight)
                    gBanner.Visible = false;
            }
            */

            _race.Run(deltaTime);
        }

        private void onKeyDown(KeyboardEventArgs args)
        {
            if (LF.GameState.Paused)
                return;

            // TODO: a way to lock input focus?
            if (!GameMenu.IsShown && (_isAIRace || args.Key == Key.Escape))
            {
                if (_isAIRace)
                    GameMenu.ShowMenu(MenuClass.eMenuMain, false);
                else
                    GameMenu.ShowMenu(MenuClass.eMenuMainInGame, true);

                // ClaimEvent();
            }
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

            /*
            LoadRaceCheckpoints();
            */
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
            /*
            RaceStartSequence = 0;
            RaceEndSequence = 0;
            Timer.StopIt(tSequence);
            tSequence = null;
            */
            _game.State.Paused = false;
        }

        private void setupSinglePlayerRace(RaceEventConfig eventCfg)
        {
            _game.State.Paused = true;

            setupRace(eventCfg);

            _game.State.Viewport.Camera.Target = getCameraTarget;
            cameraTargetPlayerCar(true);

            _isAIRace = false;
            /*
            RaceStartSequence = 0;
            RaceEndSequence = 0;
            Timer.StopIt(tSequence);
            tSequence = null;
            HoldRace = true;
            HoldAI = true;
            */

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

        void runStartSequence()
        {
            /*
            if (RaceStartSequence == 0)
            {
                gBanner.BackgroundGraphic = 15;
                gBanner.X = (System.ViewportWidth - gBanner.Width) / 2;
                gBanner.Y = -gBanner.Height;
                gBanner.Visible = true;
                gRaceOverlay.X = -gRaceOverlay.Width;
                gRaceOverlay.Visible = false;
                RaceStartSequence = 1;
            }
            else if (RaceStartSequence == 1)
            {
                if (gBanner.Y < 160)
                {
                    gBanner.Y = gBanner.Y + 12;
                }
                if (gBanner.Y > 160)
                {
                    gBanner.Y = 160;
                    tSequence = Timer.StartRT(0.8);
                    RaceStartSequence = 2;
                }
            }
            else if (RaceStartSequence == 2)
            {
                if (Timer.HasExpired(tSequence))
                {
                    gBanner.BackgroundGraphic = 16;
                    tSequence = Timer.StartRT(0.8);
                    RaceStartSequence = 3;
                }
            }
            else if (RaceStartSequence == 3)
            {
                if (Timer.HasExpired(tSequence))
                {
                    gBanner.BackgroundGraphic = 16;
                    tSequence = Timer.StartRT(0.8);
                    gBanner.BackgroundGraphic = 17;
                    gRaceOverlay.Visible = true;
                    RaceStartSequence = 0;
                    HoldRace = false;
                    HoldAI = false;
                }
            }
            */
        }

        /* TODO:
        void RunEndSequence()
        {
            if (RaceEndSequence == 0)
            {
                gRaceOverlay.Transparency = 20;
                if (Racers[0].Finished == 1)
                {
                    gBanner.BackgroundGraphic = 21;
                }
                else
                {
                    gBanner.BackgroundGraphic = 20;
                }
                gBanner.X = (System.ViewportWidth - gBanner.Width) / 2;
                gBanner.Y = -gBanner.Height;
                gBanner.Visible = true;
                RaceEndSequence = 1;
            }
            else if (RaceEndSequence == 1)
            {
                if (gBanner.Y < 160)
                {
                    gBanner.Y = gBanner.Y + 12;
                }
                if (gBanner.Y > 160)
                {
                    gBanner.Y = 160;
                    RaceEndSequence = 2;
                }
            }
            else if (RaceStartSequence == 2)
            {
            }
        }

        void OnPlayerFinishedRace()
        {
            RunEndSequence();
        }

        void TestLapComplete()
        {
            int i;
            for (i = 0; i < MAX_RACING_CARS; i++)
            {
                if (!Racers[i].IsActive)
                    continue;
                if (i > 1 && !IsAIEnabledForCar(i))
                    continue;

                if (!(Checkpoints[Racers[i].CurRaceNode].order == 0 && Racers[i].CheckptsPassed > 0))
                    continue;

                int pt;
                int hit_lap_test;
                for (pt = 0; pt < NUM_COLLISION_POINTS; pt++)
                {
                    Region* r = Region.GetAtRoomXY(FloatToInt(Cars[i].collPoint[pt].x, eRoundNearest), FloatToInt(Cars[i].collPoint[pt].y, eRoundNearest));
                    if (r.ID == 1)
                    {
                        hit_lap_test = true;
                        break;
                    }
                }

                if (hit_lap_test)
                {
                    Racers[i].SwitchToNextNode();
                    OnLapComplete(i);

                    if (Racers[i].Finished > 0)
                    {
                        if (i == 0)
                            OnPlayerFinishedRace();
                        else
                            DisableAIForCar(i);
                    }
                }
            }
        }
        */
    }
}
