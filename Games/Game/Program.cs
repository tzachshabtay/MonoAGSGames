using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace Game
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

            game.Start(new AGSGameSettings("A Game", new AGS.API.Size(1280, 720), windowState: WindowState.Normal));
		}

        private static void setupGame(IGame game)
        {
            game.Factory.Resources.ResourcePacks.Add(new ResourcePack(new FileSystemResourcePack(AGSGame.Device.FileSystem), 0));
            game.State.RoomTransitions.Transition = AGSRoomTransitions.Instant();
        }

        private static async Task loadIntroRoom(IGame game)
        {
            Roadway rw = new Roadway(game);
            await waitForRoom(game, rw.LoadAsync());
            await game.State.ChangeRoomAsync(game.State.Rooms[0]);
        }

        private static async Task waitForRoom(IGame game, Task<IRoom> task)
        {
            var room = await task;
            game.State.Rooms.Add(room);
        }
    }
}
