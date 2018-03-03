using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// VehicleBehavior is a controller component that sets up a vehicle object.
    /// </summary>
    /// TODO: simplify access to most useful underlying properties, such as position
    /// IDEA: what if override ITranslate and reroute onto physics component?
    public class VehicleBehavior : AGSComponent
    {
        public VehicleRacer Racer { get; private set; }
        public VehiclePhysics Physics { get; private set; }
        public VehicleControl Control { get; private set; }

        public VehicleBehavior()
        {
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            Racer = entity.AddComponent<VehicleRacer>();
            Physics = entity.AddComponent<VehiclePhysics>();
            entity.Bind<VehicleControl>(c => Control = c, _ => Control = null);
        }
    }
}
