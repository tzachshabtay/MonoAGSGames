using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// VehicleBehavior is a controller component that sets up a vehicle object.
    /// </summary>
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
            entity.Bind<VehiclePlayerUI>(c => Control = c, _ => Control = null);
            entity.Bind<VehicleAI>(c => Control = c, _ => Control = null);
        }
    }
}
