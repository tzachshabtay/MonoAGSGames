using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class GameStarter
    {
        public static void Run()
        {
            IGame game = AGSGame.CreateEmpty();

            game.Events.OnLoad.Subscribe(async () =>
            {
                setupGame(game);
                await loadIntroRoom(game);
            });

            game.Start(new AGSGameSettings("Last & Furious", new AGS.API.Size(640, 400), windowState: WindowState.Normal));
        }

        private static void setupGame(IGame game)
        {
            game.State.RoomTransitions.Transition = AGSRoomTransitions.Instant();
        }

        private static async Task loadIntroRoom(IGame game)
        {
            TitleScreen ts = new TitleScreen(game);
            await waitForRoom(game, ts.LoadAsync());
            await game.State.ChangeRoomAsync(ts.Room);
        }

        private static async Task waitForRoom(IGame game, Task<IRoom> task)
        {
            var room = await task;
            game.State.Rooms.Add(room);
        }
    }
}
