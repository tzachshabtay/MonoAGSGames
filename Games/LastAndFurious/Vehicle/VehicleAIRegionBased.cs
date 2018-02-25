using System;
using System.Collections.Generic;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class VehicleAIRegionBased : VehicleControl
    {
        IBitmap _regionMask;
        Dictionary<Color, float> _regionAngles;
        float _targetAngle;

        public VehicleAIRegionBased(IGame game, IBitmap regionMask, Dictionary<Color, float> regionAngles)
            : base(game)
        {
            _regionMask = regionMask;
            _regionAngles = regionAngles;
        }

        protected override void repExec()
        {
            if (_game.State.Paused)
                return;

            // TODO: get delta time from one API, using more precise calculation
            float deltaTime = (float)(1.0 / AGSGame.UPDATE_RATE);

            float angle = 0.0f;
            // NOTE: since MonoAGS has Y axis pointing up, we need to invert the lookup array's Y index
            float x = _veh.Position.X;
            float y = _veh.Position.Y;
            y = _regionMask.Height - y;
            Color color = _regionMask.GetPixel((int)Math.Round(x), (int)Math.Round(y));
            _regionAngles.TryGetValue(color, out angle);

            // Steering
            _targetAngle = angle;
            float dirAngle = _veh.Direction.Angle();
            float angleBetween = MathEx.AnglePiFast(_targetAngle - MathEx.Angle2Pi(dirAngle));

            // HACK: reduce "jittering" when AI is trying to follow strict direction:
            // if the necessary angle is very close to the angle car will turn in one tick, then just adjust yourself
            float steeringDT = SteeringAngle * deltaTime * 1.1f;
            if (Math.Abs(angleBetween) <= Math.Abs(steeringDT))
            {
                _veh.SteeringWheelAngle = 0.0f;
                _veh.Direction = Vectors.Rotate(new Vector2(1.0f, 0.0f), _targetAngle);
            }
            // ...otherwise turn properly, without cheating
            else if (angleBetween > 0.0f)
                _veh.SteeringWheelAngle = SteeringAngle;
            else if (angleBetween < 0.0f)
                _veh.SteeringWheelAngle = -SteeringAngle;
            else
                _veh.SteeringWheelAngle = 0.0f;

            // Always accelerating
            _veh.Accelerator = 1.0f;
            _veh.Brakes = 0.0f;
        }
    }
}
