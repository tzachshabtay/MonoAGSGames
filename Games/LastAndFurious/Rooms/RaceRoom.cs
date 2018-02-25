using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class RaceRoom : RoomScript
    {
        private const string ROOM_ID = "RaceRoom";
        private IAudioClip _music;

        private Vector2[] _startingGrid; // TODO: move to Track config
        private Race _race;
        private Track _track;
        // Current AI controller
        private AIController _ai;
        // Supported ai controller types
        private AIRegionBased _aiRegionBased;
        // TODO: move to camera manager
        private LastAndFurious.Camera _camera;
        private IObject _cameraTarget;


        public RaceRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        protected void clearRoom()
        {
            _race = null;
            _track = null;
            _startingGrid = null;
            _aiRegionBased = null;
            _ai = null;
            _camera = null;
            _cameraTarget = null;
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
            _game.Events.OnRepeatedlyExecute.Subscribe(repExec);
            _game.Input.KeyDown.Subscribe(onKeyDown);

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

            // TODO:
            //setupAIRace();
            setupSinglePlayerRace();

            //_music.Play(true);
        }

        private void onAfterFadeIn()
        {
            /* TODO:
            FadeIn(50);
            if (IsAIRace)
                tChangeAICamera = Timer.StartRT(CHANGE_AI_CAMERA_TIME, eRepeat);
                */
        }

        private void onLeave()
        {
            /*
            ClearRace();
            Timer.StopIt(tChangeAICamera);
            StopAllAudio();
            */
        }

        private void repExec()
        {
            if (_game.State.Paused)
                return;
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

            if (IsAIRace && Timer.HasExpired(tChangeAICamera))
            {
                CameraTargetRandomAICar(false);
            }

            if (gRaceOverlay.Visible)
            {
                if (gRaceOverlay.X < 0)
                {
                    gRaceOverlay.X = gRaceOverlay.X + 8;
                }
                else if (gRaceOverlay.X > 0)
                {
                    gRaceOverlay.X = 0;
                }
                DrawRaceOverlay(false);
            }

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
        }

        private void onKeyDown(KeyboardEventArgs args)
        {
            /*
            if (IsGamePaused())
                return;

            if (!gGameMenu.Visible && (IsAIRace || key == eKeyEscape))
            {
                if (IsAIRace)
                    DisplayGameMenu(eMenuMain, false);
                else
                    DisplayGameMenu(eMenuMainInGame, true);
                ClaimEvent();
            }
            */
        }

        private void clearRace()
        {
            /* TODO:
            gRaceOverlay.Visible = false;
            gRaceOverlay.BackgroundGraphic = 0;
            gRaceOverlay.Transparency = 0;
            if (RaceOverlay != null)
                RaceOverlay.Delete();

            ResetAI();
            */


            _race.Clear();

            /* TODO:
            player.ChangeView(CARVIEWDUMMY);
            for (i = 0; i < MAX_RACING_CARS; i++)
            {
                character[cAICar1.ID + i].ChangeView(CARVIEWDUMMY);
                character[cAICar1.ID + i].ChangeRoom(-1);
            }
            
            */
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
            _cameraTarget = _race.PlayerCar.O;
            _camera.TargettingAcceleration = 0f;
            if (snap)
                _camera.Snap();
        }

        private void cameraTargetRandomAICar(bool snap)
        {
            _cameraTarget = _race.PlayerCar.O;
            /* TODO:
            Camera.TargettingAcceleration = 0.5;
            Camera.TargetCharacter = character[cAICar1.ID + Random(5)];
            if (snap)
                Camera.Snap();
                */
        }

        // TODO: work around this
        private IObject getCameraTarget()
        {
            return _cameraTarget;
        }

        private void setupAIRace()
        {
            _game.State.Paused = true;
            _race.Clear();

            DriverCharacter[] drivers = new DriverCharacter[LF.RaceAssets.Drivers.Count];
            LF.RaceAssets.Drivers.Values.CopyTo(drivers, 0);
            Utils.Shuffle(drivers, new System.Random());

            // Switch to the desired AI type
            if (_aiRegionBased != null)
                _ai = _aiRegionBased;
            for (int i = 0; i < drivers.Length; ++i)
            {
                var car = _race.AddRacingCar(drivers[i], _ai.GetVehicleAI());
                positionCarOnGrid(car, i);
            }

            readRaceConfig();

            /*
            LoadRaceCheckpoints();

            PlayersCarIndex = -1;
            */

            _game.State.Viewport.Camera.Target = getCameraTarget;
            cameraTargetRandomAICar(true);

            /*
            IsAIRace = true;
            RaceStartSequence = 0;
            RaceEndSequence = 0;
            Timer.StopIt(tSequence);
            tSequence = null;
            */
            _game.State.Paused = false;
        }

        private void setupSinglePlayerRace()
        {
            _game.State.Paused = true;
            _race.Clear();
            // TODO: take from config
            DriverCharacter driver;
            LF.RaceAssets.Drivers.TryGetValue("blue", out driver);
            _race.PlayerDriver = driver;

            // TODO: ThisRace.Opponents
            
            DriverCharacter[] drivers = new DriverCharacter[LF.RaceAssets.Drivers.Count];
            LF.RaceAssets.Drivers.Values.CopyTo(drivers, 0);
            Utils.Shuffle(drivers, new System.Random());

            // Switch to the desired AI type
            if (_aiRegionBased != null)
                _ai = _aiRegionBased;
            for (int i = 0; i < drivers.Length; ++i)
            {
                _ai.GetVehicleAI();
                VehicleObject car;
                if (drivers[i] == _race.PlayerDriver)
                {
                    car = _race.AddRacingCar(drivers[i], new VehiclePlayerUI(_game));
                    car.Veh.Physics.StrictCollisions = true;
                    _race.PlayerCar = car;
                }
                else
                {
                    car = _race.AddRacingCar(drivers[i], _ai.GetVehicleAI());
                }
                positionCarOnGrid(car, i);
            }
            
            
            readRaceConfig();

            /*
            LoadRaceCheckpoints();

            PlayersCarIndex = 0;
            */

            _game.State.Viewport.Camera.Target = getCameraTarget;
            cameraTargetPlayerCar(true);

            /*
            IsAIRace = false;
            RaceStartSequence = 0;
            RaceEndSequence = 0;
            Timer.StopIt(tSequence);
            tSequence = null;
            HoldRace = true;
            HoldAI = true;

            RaceOverlay = DynamicSprite.CreateFromExistingSprite(5);
            DrawRaceOverlay(true);
            gRaceOverlay.BackgroundGraphic = RaceOverlay.Graphic;
            gRaceOverlay.Visible = false;
            */
            _game.State.Paused = false;
        }

        private void readRaceConfig()
        {
            // TODO: switch between config types
            TrackConfig tcfg = TrackConfigurator.LoadConfig(_track, _roomAssetFolder + "race_safe.ini");
            if (tcfg != null)
            {
                TrackConfigurator.ApplyConfig(_track, tcfg);
            }

            VehicleConfig vcfg = VehicleConfigurator.LoadConfig(_track, _roomAssetFolder + "race_safe.ini");
            if (vcfg != null)
            {
                foreach (var car in _race.Cars)
                    VehicleConfigurator.ApplyConfig(car.Veh, vcfg);
            }
        }
    }
}
