using AGS.API;

namespace LastAndFurious
{
    public class VehiclePlayerUI : VehicleControl
    {
        public VehiclePlayerUI(IGame game) : base(game)
        {
        }

        public override void Run(float deltaTime)
        {
            // TODO: configure keys
            if (_input.IsKeyDown(Key.Up))
                _veh.Accelerator = 1.0F;
            else
                _veh.Accelerator = 0.0F;

            if (_input.IsKeyDown(Key.Down))
                _veh.Brakes = 1.0F;
            else
                _veh.Brakes = 0.0F;

            if (_input.IsKeyDown(Key.Left))
                _veh.SteeringWheelAngle = SteeringAngle;
            else if (_input.IsKeyDown(Key.Right))
                _veh.SteeringWheelAngle = -SteeringAngle;
            else
                _veh.SteeringWheelAngle = 0.0F;
        }
    }
}
