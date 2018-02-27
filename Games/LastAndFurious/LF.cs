﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public static class LF
    {
        public const string GAME_VERSION = "v0.1.0";

        private static string _baseAssetFolder;
        private static string _fontAssetFolder;
        private static string _musicAssetFolder;
        private static string _objectAssetFolder;
        private static string _roomAssetFolder;
        private static string _uiAssetFolder;

        public static string BaseAssetFolder { get => _baseAssetFolder; }
        public static string FontAssetFolder { get => _fontAssetFolder; }
        public static string MusicAssetFolder { get => _musicAssetFolder; }
        public static string ObjectAssetFolder { get => _objectAssetFolder; }
        public static string RoomAssetFolder { get => _roomAssetFolder; }
        public static string UIAssetFolder { get => _uiAssetFolder; }

        public static void Init(string baseAssetPath)
        {
            System.Console.WriteLine("Asset path: " + baseAssetPath);

            _baseAssetFolder = baseAssetPath;
            _fontAssetFolder = _baseAssetFolder + "Fonts/";
            _musicAssetFolder = _baseAssetFolder + "Music/";
            _objectAssetFolder = _baseAssetFolder + "Objects/";
            _roomAssetFolder = _baseAssetFolder + "Rooms/";
            _uiAssetFolder = _baseAssetFolder + "UI/";
        }

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

        public static class RaceMenu
        {
            public static IImage Selector;
            public static IImage VBar;
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

        // TODO: had to do this, because engine's GameState.Paused blocks literally all update,
        // including input events.
        public static class GameState
        {
            public static bool Paused { get; set; }
        }
    }
}
