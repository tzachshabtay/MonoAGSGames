using System;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    public class VehicleAI : AGSComponent
    {
        private IGame _game;
        private VehiclePhysics _veh;

        public VehicleAI(IGame game)
        {
            _game = game;
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            entity.Bind<VehiclePhysics>(c => { _veh = c; _game.Events.OnRepeatedlyExecute.Subscribe(repExec); },
                                        _ => { _veh = null; _game.Events.OnRepeatedlyExecute.Unsubscribe(repExec); });
        }

        private void repExec()
        {
            if (_game.State.Paused)
                return;
        }
    }
}
