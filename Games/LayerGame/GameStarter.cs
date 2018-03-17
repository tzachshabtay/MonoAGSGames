using AGS.API;
using AGS.Engine;

namespace LayerGame
{
    public class GameStarter
    {
        // TODO: proper struct to pass arguments
        public static void Run(string asset_path, string user_path)
        {
            Assets.Init(asset_path, user_path);
            IGame game = AGSGame.CreateEmpty();

            game.Events.OnLoad.Subscribe(async () =>
            {
                setupGame(game);
                await Assets.Rooms.ParallaxRoom.GotoAsync();
            });

            game.Start(new AGSGameSettings("Layer Game", new AGS.API.Size(640, 400), windowState: WindowState.Normal));
        }

        private static void setupGame(IGame game)
        {
            game.Factory.Resources.ResourcePacks.Add(new ResourcePack(new FileSystemResourcePack(AGSGame.Device.FileSystem), 0));
            game.State.RoomTransitions.Transition = AGSRoomTransitions.Instant();

            Assets.Rooms.PrecreateAll(game);
        }
    }
}
