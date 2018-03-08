using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AGS.API;
using AGS.Engine;

namespace AudioMixerGame
{
    public static class AMG
    {
        private static string _baseAssetFolder;
        private static string _fontAssetFolder;
        private static string _musicAssetFolder;
        private static string _objectAssetFolder;
        private static string _roomAssetFolder;
        private static string _uiAssetFolder;
        private static string _userDataFolder;

        public static string BaseAssetFolder { get => _baseAssetFolder; }
        public static string FontAssetFolder { get => _fontAssetFolder; }
        public static string MusicAssetFolder { get => _musicAssetFolder; }
        public static string ObjectAssetFolder { get => _objectAssetFolder; }
        public static string RoomAssetFolder { get => _roomAssetFolder; }
        public static string UIAssetFolder { get => _uiAssetFolder; }
        public static string UserDataFolder { get => _userDataFolder; }

        public static void Init(string baseAssetPath, string userDataFolder)
        {
            Debug.WriteLine("Asset path: " + baseAssetPath);
            Debug.WriteLine("User path: " + userDataFolder);

            _baseAssetFolder = baseAssetPath;
            _fontAssetFolder = _baseAssetFolder + "Fonts/";
            _musicAssetFolder = _baseAssetFolder + "Music/";
            _objectAssetFolder = _baseAssetFolder + "Objects/";
            _roomAssetFolder = _baseAssetFolder + "Rooms/";
            _uiAssetFolder = _baseAssetFolder + "UI/";
            _userDataFolder = userDataFolder;

            Directory.CreateDirectory(_userDataFolder);
        }

        public static class MagicColor
        {
            public static ILoadImageConfig TopLeftPixel = new AGSLoadImageConfig(new Point(0, 0));
        }

        public static class Rooms
        {
            public static List<RoomScript> AllRooms = new List<RoomScript>();
            public static MixerRoom MixerRoom;

            public static void PrecreateAll(IGame game)
            {
                MixerRoom = new MixerRoom(game);

                AllRooms.Add(MixerRoom);
            }
        }
    }
}
