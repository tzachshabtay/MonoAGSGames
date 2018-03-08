using AGS.API;
using AGS.Engine;

namespace AudioMixerGame
{
    public class GameStarter
    {
        // TODO: proper struct to pass arguments
        public static void Run(string asset_path, string user_path)
        {
            AMG.Init(asset_path, user_path);
            IGame game = AGSGame.CreateEmpty();

            game.Events.OnLoad.Subscribe(async () =>
            {
                setupGame(game);
                await AMG.Rooms.MixerRoom.GotoAsync();
            });

            game.Start(new AGSGameSettings("Audio Mixer Test Game", new AGS.API.Size(1280, 800), windowState: WindowState.Normal));
        }

        private static void setupGame(IGame game)
        {
            game.Factory.Resources.ResourcePacks.Add(new ResourcePack(new FileSystemResourcePack(AGSGame.Device.FileSystem), 0));
            game.State.RoomTransitions.Transition = AGSRoomTransitions.Instant();

            AMG.Rooms.PrecreateAll(game);
        }
    }
}
