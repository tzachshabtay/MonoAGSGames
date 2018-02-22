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
                await LF.Rooms.TitleScreen.GotoAsync();
            });

            game.Start(new AGSGameSettings("Last & Furious", new AGS.API.Size(640, 400), windowState: WindowState.Normal));
        }

        private static void setupGame(IGame game)
        {
            game.Factory.Resources.ResourcePacks.Add(new ResourcePack(new FileSystemResourcePack(AGSGame.Device.FileSystem), 0));
            game.State.RoomTransitions.Transition = AGSRoomTransitions.Instant();

            loadFonts(game);
            loadUI(game);
            LF.Rooms.PrecreateAll(game);
        }

        private static void loadFonts(IGame game)
        {
            IGraphicsFactory f = game.Factory.Graphics;

            // Silver 'Racer' font
            int last = 126;
            int total = last + 1;
            int[] offs = new int[total];
            int[] widths = new int[total];
            for (int i = 0; i < total; i++)
                offs[i] = 0;
            for (int i = 0; i < total; i++)
                widths[i] = 28;
            // !, '(aposthrophe), .(dot), ,(comma), I, i, j, l, :, ; and |
            //offs['1'] = 4;
            offs['t'] = 3;
            offs['!'] = 9;
            offs['`'] = 9;
            offs['.'] = 9;
            offs[','] = 9;
            offs['I'] = 9;
            offs['i'] = 9;
            offs['j'] = 0;
            offs['l'] = 9;
            offs[':'] = 9;
            offs[';'] = 9;
            offs['|'] = 9;
            //widths['1'] -= (4 + 9) - 4;
            widths['t'] -= (3 + 7) - 4;
            widths['!'] -= 18 - 4;
            widths['`'] -= 18 - 4;
            widths['.'] -= 18 - 4;
            widths[','] -= 18 - 4;
            widths['I'] -= 18 - 4;
            widths['i'] -= 18 - 4;
            widths['j'] -= 9 - 4;
            widths['l'] -= 18 - 4;
            widths[':'] -= 18 - 4;
            widths[';'] -= 18 - 4;
            widths['|'] -= 18 - 4;

            var fontImage = f.LoadImage(LF.FontAssetFolder + "font-racer-silver.png", LF.MagicColor.TopLeftPixel);
            LF.Fonts.SilverFont = SpriteFont.CreateFromBitmap(fontImage.OriginalBitmap, f, 32, 34, 24, 0, last, offs, widths);
        }

        private static void loadUI(IGame game)
        {
            IGraphicsFactory f = game.Factory.Graphics;
            LF.StartMenu.Selector = f.LoadImage(LF.UIAssetFolder + "diamond.png", LF.MagicColor.TopLeftPixel);
        }
    }
}
