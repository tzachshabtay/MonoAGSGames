using System.Collections.Generic;
using AGS.API;

namespace LastAndFurious
{
    public struct VehicleObject
    {
        public VehicleBehavior Veh;
        public IObject O;

        public VehicleObject(VehicleBehavior veh, IObject o)
        {
            Veh = veh;
            O = o;
        }
    }

    public class Race
    {
        public const int MAX_RACING_CARS = 6;

        IGame _game;
        IRoom _room;
        Track _track;
        // TODO: was not sure what is the best way to keep custom component field exposed
        List<VehicleObject> _cars = new List<VehicleObject>();

        // TODO:
        // AiAndPhysicsOption AiAndPhysics;
        bool CarCollisions;

        /*
        List<int> _driverPositions;
        List<int> _racersFinished;
        */

        // TODO: separate RaceConfig
        /// <summary>
        /// The driver character player chosen.
        /// </summary>
        public DriverCharacter PlayerDriver { get; set; }
        /// <summary>
        /// Number of laps race consists of.
        /// </summary>
        public int Laps { get; set; }
        /// <summary>
        /// Number of AI opponents player races against.
        /// </summary>
        public int Opponents { get; set; }

        public VehicleObject PlayerCar { get; set; }
        public IList<VehicleObject> Cars { get => _cars; }


        public Race(IGame game, IRoom room, Track track)
        {
            _game = game;
            _room = room;
            _track = track;
        }

        public void Clear()
        {
            foreach (var c in _cars)
                _room.Objects.Remove(c.O);
            _cars.Clear();

            /*
            _driverPositions.Clear();
            _racersFinished.Clear();
            */
        }

        public VehicleObject AddRacingCar(DriverCharacter c, VehicleControl control = null)
        {
            IObject o = _game.Factory.Object.GetObject(c.Name + "_car");
            VehicleBehavior beh = new VehicleBehavior();
            if (!o.AddComponent<VehicleBehavior>(beh))
                return new VehicleObject();
            if (control != null)
                o.AddComponent<VehicleControl>(control);

            beh.Racer.Driver = c;
            beh.Physics.AttachCarModel(c.CarModel, c.CarModelAngle);
            o.Image = c.CarModel;
            o.Pivot = new PointF(0.5F, 0.5F);
            _cars.Add(new VehicleObject(beh, o));
            _room.Objects.Add(o);
            return new VehicleObject(beh, o);
        }
    }
}
