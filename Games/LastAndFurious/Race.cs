using System.Collections.Generic;
using AGS.API;

namespace LastAndFurious
{
    public class Race
    {
        public const int MAX_RACING_CARS = 6;

        IGame _game;
        IRoom _room;
        // TODO: was not sure what is the best way to keep custom component field exposed
        List<(VehicleBehavior veh, IObject o)> _cars = new List<(VehicleBehavior, IObject)>();

        int _laps;
        int _opponents;
        int _playerDriver; // player's character
        // TODO:
        // AiAndPhysicsOption AiAndPhysics;
        bool CarCollisions;
        
        /*
        List<int> _driverPositions;
        List<int> _racersFinished;
        */

        public Race(IGame game, IRoom room)
        {
            _game = game;
            _room = room;
        }

        public void Clear()
        {
            foreach (var c in _cars)
                _room.Objects.Remove(c.o);
            _cars.Clear();

            /*
            _driverPositions.Clear();
            _racersFinished.Clear();
            */
        }

        public VehicleBehavior AddRacingCar(DriverCharacter c)
        {
            IObject o = _game.Factory.Object.GetObject(c.Name + "_car");
            // TODO:
            // Cars[i].SetCharacter(character[cAICar1.ID + i], 7 + drivers[i], eDirectionUp, CARVIEWPLAYER1 + i, 0, 0);
            // TODO: make sure the object's pivot is at the center
            VehicleBehavior beh = new VehicleBehavior();
            if (!o.AddComponent<VehicleBehavior>(beh))
                return null;
            beh.Racer.Driver = c;
            beh.Physics.AttachCarModel(c.CarModel, c.CarModelAngle);
            o.Image = c.CarModel;
            o.Pivot = new PointF(0.5F, 0.5F);
            _cars.Add((beh, o));
            _room.Objects.Add(o);
            return beh;
        }
    }
}
