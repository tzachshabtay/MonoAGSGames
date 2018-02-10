using System;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class VehicleControl : AGSComponent
    {
        private IGame _game;
        private IInput _input;
        private VehiclePhysics _veh;

        /// <summary>
        /// The maximal angle player can turn car wheels with controls.
        /// </summary>
        public float SteeringAngle { get; set; }

        public VehicleControl(IGame game, float steeringAngle = (float)(Math.PI / 6.0))
        {
            _game = game;
            _input = game.Input;
            SteeringAngle = steeringAngle;
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            entity.Bind<VehiclePhysics>(c => { _veh = c; _game.Events.OnRepeatedlyExecute.Subscribe(repExec); },
                                        _ => { _veh = null; _game.Events.OnRepeatedlyExecute.Unsubscribe(repExec); });
            _game.Events.OnRepeatedlyExecute.Subscribe(repExec);
        }

        private void repExec()
        {
            if (_game.State.Paused)
                return;
            
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
