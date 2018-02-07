using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public const string ObjectAssetFolder = BaseAssetFolder + "Objects/";
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

        public static class Rooms
        {
            public static List<RoomScript> AllRooms = new List<RoomScript>();
            public static TitleScreen TitleScreen;
            public static RaceRoom RaceRoom;

            public static void PrecreateAll(IGame game)
            {
                TitleScreen = new TitleScreen(game);
                RaceRoom = new RaceRoom(game);

                AllRooms.Add(TitleScreen);
                AllRooms.Add(RaceRoom);
            }
        }

        public static class RaceAssets
        {
            public static string[] Names = { "blue", "yellow", "green", "red", "violet", "gray"};
            public static Dictionary<string, IImage> CarModels = new Dictionary<string, IImage>();
            public static Dictionary<string, DriverCharacter> Drivers = new Dictionary<string, DriverCharacter>();

            public static async Task LoadAll(IGame game)
            {
                IGraphicsFactory f = game.Factory.Graphics;
                for (int i = 0; i < Names.Length; ++i)
                {
                    string name = Names[i];
                    IImage carmodel = await f.LoadImageAsync(String.Format("{0}carmodel{1}.png", ObjectAssetFolder, i + 1), LF.MagicColor.TopLeftPixel);
                    CarModels.Add(name, carmodel);
                    Drivers.Add(name, new DriverCharacter(name, null, carmodel, 90.0F));
                }
            }
        }
    }
}
