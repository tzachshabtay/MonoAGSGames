using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class RaceRoom : RoomScript
    {
        private const string ROOM_ID = "RaceRoom";
        private IAudioClip _music;

        private Vector2[] _startingGrid;
        private Race _race;

        public RaceRoom(IGame game) : base(game, ROOM_ID)
        {
        }

        protected override async Task<IRoom> loadAsync()
        {
            IGameFactory factory = _game.Factory;
            _room = factory.Room.GetRoom(ROOM_ID);
            _room.Background = await addObject("RaceRoom.BG", "track01.png");
            _room.RoomLimitsProvider = AGSRoomLimits.FromBackground;

            _music = await factory.Sound.LoadAudioClipAsync(LF.MusicAssetFolder + "Welcome_to_the_Show.ogg");

            _room.Events.OnBeforeFadeIn.Subscribe(onLoad);
            _room.Events.OnAfterFadeIn.Subscribe(onAfterFadeIn);
            _room.Events.OnAfterFadeOut.Subscribe(onLeave);
            _game.Events.OnRepeatedlyExecute.Subscribe(repExec);
            _game.Input.KeyDown.Subscribe(onKeyDown);


            _startingGrid = new Vector2[Race.MAX_RACING_CARS];
            _startingGrid[0] = compatVector(1140, 326 + 12);
            _startingGrid[1] = compatVector(1172, 273 + 12);
            _startingGrid[2] = compatVector(1204, 326 + 12);
            _startingGrid[3] = compatVector(1236, 273 + 12);
            _startingGrid[4] = compatVector(1268, 326 + 12);
            _startingGrid[5] = compatVector(1300, 273 + 12);

            _race = new Race(_game, _room);

            return _room;
        }

        private void onLoad()
        {
            /* TODO:
            FadeOut(0);
            StopAllAudio();
            */
            setupAIRace();
            //_music.Play(true);

            // TODO: temp, remove
            _game.State.Viewport.X = _startingGrid[0].X;
            _game.State.Viewport.Y = _startingGrid[0].Y;
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
            // TODO: temp, remove/change
            IInput input = _game.Input;
            if (input.IsKeyDown(Key.Up))
                _game.State.Viewport.Y += 5F;
            if (input.IsKeyDown(Key.Down))
                _game.State.Viewport.Y -= 5F;
            if (input.IsKeyDown(Key.Left))
                _game.State.Viewport.X -= 5F;
            if (input.IsKeyDown(Key.Right))
                _game.State.Viewport.X += 5F;
            if (input.IsKeyDown(Key.PageDown))
                _game.State.Viewport.ScaleX = _game.State.Viewport.ScaleY = _game.State.Viewport.ScaleX - 0.1F;
            if (input.IsKeyDown(Key.PageUp))
                _game.State.Viewport.ScaleX = _game.State.Viewport.ScaleY = _game.State.Viewport.ScaleX + 0.1F;
            if (input.IsKeyDown(Key.Insert))
                _game.State.Viewport.Angle += 1F;
            if (input.IsKeyDown(Key.Delete))
                _game.State.Viewport.Angle -= 1F;
            if (input.IsKeyDown(Key.PageUp))
                _game.State.Viewport.ScaleX = _game.State.Viewport.ScaleY = _game.State.Viewport.ScaleX + 0.02F;

            /* TODO:
            if (IsGamePaused())
                return;

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

        private void PositionCarOnGrid(VehicleBehavior car, int gridpos)
        {
            // TODO: checkme, I do not remember why it needed all this adjustment
            Vector2 pos = _startingGrid[gridpos];
            pos.X += 4 + car.Physics.BodyLength / 2;
            pos.Y += 1;
            car.Physics.Reset(pos, new Vector2(-1, 0));
        }

        private void setupAIRace()
        {
            _game.State.Paused = true;
            _race.Clear();

            DriverCharacter[] drivers = new DriverCharacter[LF.RaceAssets.Drivers.Count];
            LF.RaceAssets.Drivers.Values.CopyTo(drivers, 0);
            Utils.Shuffle(drivers, new System.Random());

            for (int i = 0; i < Race.MAX_RACING_CARS; ++i)
            {
                var car = _race.AddRacingCar(drivers[i]);
                PositionCarOnGrid(car, i);
            }
            /*

            ReadRaceConfig();
            LoadAI();
            LoadRaceCheckpoints();

            PlayersCarIndex = -1;
            for (i = 0; i < MAX_RACING_CARS; i++)
            {
                AssignAIToCar(i);
                Racers[i].Activate(drivers[i]);
            }

            CameraTargetRandomAICar(true);

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
            /*

            int drivers[] = new int[MAX_RACING_CARS - 1];
            int i;
            for (i = 0; i < MAX_RACING_CARS - 1; i++)
            {
                if (i < ThisRace.PlayerDriver)
                    drivers[i] = i;
                else
                    drivers[i] = i + 1;
            }
            Shuffle(drivers, MAX_RACING_CARS - 1);

            cPlayerCar.Baseline = 1;
            Cars[0].SetCharacter(cPlayerCar, 7 + ThisRace.PlayerDriver, eDirectionUp, CARVIEWPLAYER1, 0, 0);
            PositionCarOnGrid(0, 0);

            for (i = 0; i < ThisRace.Opponents; i++)
            {
                character[cAICar1.ID + i].Baseline = 1;
                Cars[i + 1].SetCharacter(character[cAICar1.ID + i], 7 + drivers[i], eDirectionUp, CARVIEWPLAYER2 + i, 0, 0);
                PositionCarOnGrid(i + 1, i + 1);
            }

            ReadRaceConfig();
            LoadAI();
            LoadRaceCheckpoints();

            PlayersCarIndex = 0;
            Cars[0].strictCollisions = true;
            Racers[0].Activate(ThisRace.PlayerDriver);
            for (i = 0; i < ThisRace.Opponents; i++)
            {
                AssignAIToCar(i + 1);
                Racers[i + 1].Activate(drivers[i]);
            }

            CameraTargetPlayerCar(true);

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
    }
}
