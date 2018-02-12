using AGS.API;

namespace LastAndFurious
{
    public class VehicleAI : VehicleControl
    {
        public VehicleAI(IGame game) : base(game)
        {
        }

        protected override void repExec()
        {
            if (_game.State.Paused)
                return;
        }
    }
}
