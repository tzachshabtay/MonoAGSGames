using System;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public abstract class VehicleControl : AGSComponent
    {
        protected IGame _game;
        protected IInput _input;
        protected VehiclePhysics _veh;

        /// <summary>
        /// The maximal angle player can turn car wheels with controls, in radians.
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

        protected abstract void repExec();
    }
}
