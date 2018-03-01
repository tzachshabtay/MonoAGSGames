using System;
using System.IO;
using IniParser;
using IniParser.Model;

namespace LastAndFurious
{
    public static class UserConfig
    {
        public static void Load(string filepath, ref GameConfig gameCfg, ref RaceEventConfig raceCfg)
        {
            if (!File.Exists(filepath))
                return;
            FileIniDataParser file = new FileIniDataParser();
            file.Parser.Configuration.CommentString = "//";
            IniData inidata = file.ReadFile(filepath);
            IniGetter ini = new IniGetter(inidata);

            gameCfg.MusicVolume = ini.GetInt("main", "music", gameCfg.MusicVolume);

            raceCfg.PlayerDriver = ini.GetInt("race", "driver", raceCfg.PlayerDriver);
            raceCfg.Laps = ini.GetInt("race", "laps", raceCfg.Laps);
            raceCfg.Opponents = ini.GetInt("race", "opponents", raceCfg.Opponents);
            string physics = ini.GetString("race", "physics", raceCfg.Physics.ToString());
            RacePhysicsMode mode;
            if (Enum.TryParse<RacePhysicsMode>(physics, out mode))
                raceCfg.Physics = mode;
            raceCfg.CarCollisions = ini.GetBool("race", "car_collisions", raceCfg.CarCollisions);

            /* TODO
             * 
            IsDebugMode = ini.ReadBool("main", "debug_mode", IsDebugMode);
            */
        }

        public static void Save(string filepath, GameConfig gameCfg, RaceEventConfig raceCfg)
        {
            IniData inidata = new IniData();
            IniSetter ini = new IniSetter(inidata);

            ini.SetInt("main", "music", gameCfg.MusicVolume);

            ini.SetInt("race", "driver", raceCfg.PlayerDriver);
            ini.SetInt("race", "laps", raceCfg.Laps);
            ini.SetInt("race", "opponents", raceCfg.Opponents);
            ini.SetString("race", "physics", raceCfg.Physics.ToString());
            ini.SetBool("race", "car_collisions", raceCfg.CarCollisions);

            FileIniDataParser file = new FileIniDataParser();
            file.Parser.Configuration.CommentString = "//";
            file.WriteFile(filepath, inidata);

            /* TODO
             * 
            IsDebugMode = ini.ReadBool("main", "debug_mode", IsDebugMode);
            */
        }
    }
}
