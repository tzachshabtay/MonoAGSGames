using System.Collections.Generic;
using AGS.API;

namespace LastAndFurious
{
    public abstract class AIController
    {
        public abstract VehicleControl GetVehicleAI();
    }

    public class AIRegionBased : AIController
    {
        IGame _game;
        IBitmap _regionMask;
        Dictionary<Color, float> _regionAngles;

        public AIRegionBased(IGame game, IBitmap regionMask, Dictionary<Color, float> regionAngles)
        {
            _game = game;
            _regionMask = regionMask;
            _regionAngles = regionAngles;
        }

        public override VehicleControl GetVehicleAI()
        {
            return new VehicleAIRegionBased(_game, _regionMask, _regionAngles);
        }
    }

    public class AIPathBase : AIController
    {
        IGame _game;
        List<AIPathNode> _pathNodes;

        public AIPathBase(IGame game, List<AIPathNode> pathNodes)
        {
            _game = game;
            _pathNodes = pathNodes;
        }

        public override VehicleControl GetVehicleAI()
        {
            return new VehicleAIPathBased(_game, _pathNodes);
        }
    }
}
