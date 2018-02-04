using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public static class LF
    {
        public const string GAME_VERSION = "v0.1.0";
        public const string BaseAssetFolder = "../../Assets/";
        public const string FontAssetFolder = BaseAssetFolder + "Fonts/";
        public const string MusicAssetFolder = BaseAssetFolder + "Music/";
        public const string RoomAssetFolder = BaseAssetFolder + "Rooms/";
        public const string UIAssetFolder = BaseAssetFolder + "UI/";

        public static class MagicColor
        {
            public static ILoadImageConfig TopLeftPixel = new AGSLoadImageConfig(new Point(0, 0));
        }

        public static class Fonts
        {
            public static SpriteFont SilverFont;
        }

        public static class StartMenu
        {
            public static IImage Selector;
        }
    }
}
