using System;
using System.Collections.Generic;
using IniParser;
using IniParser.Model;

namespace LastAndFurious
{
    public class TrackRegionConfig
    {
        public bool IsObstacle;
        public float TerraSlideFriction;
        public float TerraRollFriction;
        public float TerraGrip;
        public float EnvResistance;
    }

    public class TrackConfig
    {
        public float Gravity;
        public float AirResistance;

        public List<TrackRegionConfig> Regions;
    }

    public static class TrackConfigurator
    {
        public static void ApplyConfig(Track track, TrackConfig cfg)
        {
            track.Gravity = cfg.Gravity;
            track.AirResistance = cfg.AirResistance;

            var regions = track.Regions;
            for (int i = 0; i < Math.Min(regions.Length, cfg.Regions.Count); ++i)
            {
                var r = regions[i];
                TrackRegionConfig rc = cfg.Regions[i];
                r.IsObstacle = rc.IsObstacle;
                r.TerraSlideFriction = rc.TerraSlideFriction;
                r.TerraRollFriction = rc.TerraRollFriction;
                r.TerraGrip = rc.TerraGrip;
                r.EnvResistance = rc.EnvResistance;
            }
        }

        public static TrackConfig LoadConfig(Track track, string iniFilePath)
        {
            FileIniDataParser file = new FileIniDataParser();
            file.Parser.Configuration.CommentString = "//";
            IniData inidata = file.ReadFile(iniFilePath);
            if (inidata == null)
                return null;

            IniGetter ini = new IniGetter(inidata);
            TrackConfig cfg = new TrackConfig();
            cfg.Gravity = ini.GetFloat("track", "gravity");
            cfg.AirResistance = ini.GetFloat("track", "air_resistance");
            int regions = ini.GetInt("track", "regions");
            cfg.Regions = new List<TrackRegionConfig>();
            for (int i = 0; i < regions; ++i)
            {
                string secname = String.Format("area{0}", i);
                if (!inidata.Sections.ContainsSection(secname))
                    continue;
                TrackRegionConfig r = new TrackRegionConfig();
                r.IsObstacle = ini.GetBool(secname, "is_obstacle");
                r.TerraSlideFriction = ini.GetFloat(secname, "slide_friction");
                r.TerraRollFriction = ini.GetFloat(secname, "roll_friction");
                r.TerraGrip = ini.GetFloat(secname, "grip");
                r.EnvResistance = ini.GetFloat(secname, "env_resistance");
                cfg.Regions.Add(r);
            }
            return cfg;
        }
    }
}
