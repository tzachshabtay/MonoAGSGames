using AGS.API;
using IniParser;
using IniParser.Model;

namespace LastAndFurious
{
    public class VehicleConfig
    {
        public float BodyLength;
        public float BodyWidth;
        public float DistanceBetweenAxles;
        public float BodyMass;
        public float BodyAerodynamics;
        public float HardImpactLossFactor;
        public float SoftImpactLossFactor;
        public float EngineMaxPower;
        public float StillTurningVelocity;
        public float DriftVelocityFactor;

        public float UISteeringAngle;
    }

    public class VehicleConfigurator
    {
        public static void ApplyConfig(VehicleBehavior beh, VehicleConfig cfg)
        {
            /* TODO: maybe
            if (cfg.BodyLength > 0.0)
                ph.BodyLength = cfg.BodyLength;
            if (cfg.BodyWidth > 0.0)
                ph.BodyWidth = cfg.BodyWidth;
            if (cfg.DistanceBetweenAxles > 0.0)
                ph.DistanceBetweenAxles = cfg.DistanceBetweenAxles;
            */
            VehiclePhysics ph = beh.Physics;
            ph.BodyMass = cfg.BodyMass;
            ph.BodyAerodynamics = cfg.BodyAerodynamics;
            ph.HardImpactLossFactor = cfg.HardImpactLossFactor;
            ph.SoftImpactLossFactor = cfg.SoftImpactLossFactor;
            ph.EngineMaxPower = cfg.EngineMaxPower;
            ph.StillTurningVelocity = cfg.StillTurningVelocity;
            ph.DriftVelocityFactor = cfg.DriftVelocityFactor;
            VehicleControl ui = beh.Control;
            ui.SteeringAngle = MathHelper.DegreesToRadians(cfg.UISteeringAngle);
        }

        public static VehicleConfig LoadConfig(Track track, string iniFilePath)
        {
            FileIniDataParser file = new FileIniDataParser();
            file.Parser.Configuration.CommentString = "//";
            IniData inidata = file.ReadFile(iniFilePath);
            if (inidata == null || !inidata.Sections.ContainsSection("car"))
                return null;

            IniGetter ini = new IniGetter(inidata);
            VehicleConfig cfg = new VehicleConfig();
            cfg.BodyLength = ini.GetFloat("car", "bodyLength");
            cfg.BodyWidth = ini.GetFloat("car", "bodyWidth");
            cfg.DistanceBetweenAxles = ini.GetFloat("car", "distanceBetweenAxles");
            cfg.BodyMass = ini.GetFloat("car", "bodyMass");
            cfg.BodyAerodynamics = ini.GetFloat("car", "bodyAerodynamics");
            cfg.HardImpactLossFactor = ini.GetFloat("car", "hardImpactLossFactor");
            cfg.SoftImpactLossFactor = ini.GetFloat("car", "softImpactLossFactor");
            cfg.EngineMaxPower = ini.GetFloat("car", "engineMaxPower");
            cfg.StillTurningVelocity = ini.GetFloat("car", "stillTurningVelocity");
            cfg.DriftVelocityFactor = ini.GetFloat("car", "driftVelocityFactor");
            cfg.UISteeringAngle = ini.GetFloat("car_control", "steeringAngle");
            return cfg;
        }
    }
}
