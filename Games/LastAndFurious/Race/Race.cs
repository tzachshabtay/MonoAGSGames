using System.Collections.Generic;
using AGS.API;

namespace LastAndFurious
{
    // TODO: find a better (stricter) way to connect these two together
    // A dictionary mapping object to VehicleBehavior perhaps?
    // NOTE: we need IObject reference, because Room is storing list of IObjects, not entities
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
        List<Racer> _racers = new List<Racer>();

        // Rules
        bool _carCollisions;
        int _laps;
        // int _opponents;
        Racer _player;

        // racer indices sorted in the order of their positions
        // TODO: look into removing _driverPositions and using only _racerPositions instead
        List<int> _driverPositions = new List<int>();
        List<Racer> _racerPositions = new List<Racer>();
        int _racersFinished;
        
        /// <summary>
        /// The driver character player chosen.
        /// </summary>
        public Racer Player { get => _player; }
        /// <summary>
        /// Number of laps race consists of.
        /// </summary>
        public int Laps { get => _laps; }
        /* TODO ?
        /// <summary>
        /// Number of AI opponents player races against.
        /// </summary>
        public int Opponents { get => _opponents; }
        */
        
        public IList<VehicleObject> Cars { get => _cars; }
        /// <summary>
        /// Unordered list of race participants (in the order of creation)
        /// </summary>
        public IList<Racer> Racers { get => _racers; }
        /// <summary>
        /// Sorted list of race participants, in the order of currentplacement
        /// </summary>
        /// TODO: do we actually need two lists?
        public IList<Racer> RacerPositions { get => _racerPositions; }
        public bool CarCollisions { get => _carCollisions; set => _carCollisions = value; }


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

            _racers.Clear();
            _driverPositions.Clear();
            _racerPositions.Clear();

            _player = null;
            //_opponents = 0;
            _laps = 0;
            _racersFinished = 0;
        }

        public Racer AddRacer(DriverCharacter c, VehicleControl control = null)
        {
            IObject o = _game.Factory.Object.GetObject(c.Name + "_car");
            VehicleBehavior beh = new VehicleBehavior();
            if (!o.AddComponent<VehicleBehavior>(beh))
                return null;
            if (control != null)
                o.AddComponent<VehicleControl>(control);

            beh.Racer.Driver = c;
            beh.Physics.AttachCarModel(c.CarModel, c.CarModelAngle);
            o.Image = c.CarModel;
            o.Pivot = new PointF(0.5F, 0.5F);
            _cars.Add(new VehicleObject(beh, o));
            _room.Objects.Add(o);

            _driverPositions.Add(-1);
            _racerPositions.Add(null);
            VehicleObject car = new VehicleObject(beh, o);
            Racer racer = new Racer(c, car, _track.Checkpoints);
            _racers.Add(racer);
            return racer;
        }

        public Racer AddPlayer(DriverCharacter c, VehicleControl control = null)
        {
            Racer racer = AddRacer(c, control);
            racer.Car.Veh.Physics.StrictCollisions = true;
            _player = racer;
            return racer;
        }

        public void Run(float deltaTime)
        {
            // Update state of cars and participants logic
            foreach (var car in _cars)
            {
                car.Veh.Physics.Run(deltaTime);
                if (CarCollisions)
                    runVeh2VehCollision();
            }
            foreach (var racer in _racers)
                racer.Run(deltaTime);
            // First fill in placements based on order in array; if there are no checkpoints this will remain
            for (int i = 0; i < _driverPositions.Count; ++i)
            {
                _driverPositions[i] = i;
            }
            IList<RaceNode> checkpoints = _track.Checkpoints;
            if (checkpoints != null && checkpoints.Count > 0)
            {
                // Find out participants placements
                // Insertion sort algorithm
                /*
                i <- 1
                while i < length(A)
                    j <- i
                    while j > 0 and A[j-1] > A[j]
                        swap A[j] and A[j-1]
                        j <- j - 1
                    end while
                    i <- i + 1
                end while
                */
                
                int i = 1;
                while (i < _racers.Count)
                {
                    int j = i;
                    while (j > 0 && racerIsBehind(_racers[_driverPositions[j - 1]], _racers[_driverPositions[j]]))
                    {
                        int temp = _driverPositions[j];
                        _driverPositions[j] = _driverPositions[j - 1];
                        _driverPositions[j - 1] = temp;
                        j--;
                    }
                    i++;
                }
                /*
                String s = String.Format("%d - %d - %d - %d - %d - %d",
                  Racers[0].CheckptsPassed, Racers[1].CheckptsPassed, Racers[2].CheckptsPassed, Racers[3].CheckptsPassed, Racers[4].CheckptsPassed, Racers[5].CheckptsPassed);
                s = s.Append(
                String.Format("[%d - %d - %d - %d - %d - %d", ThisRace.DriverPositions[0], ThisRace.DriverPositions[1], ThisRace.DriverPositions[2], ThisRace.DriverPositions[3], ThisRace.DriverPositions[4], ThisRace.DriverPositions[5]));
                player.SayBackground(s);*/
            }
            // Now, set proper position for each racer
            for (int i = 0; i < _driverPositions.Count; ++i)
            {
                int racer = _driverPositions[i];
                _racers[racer].Place = i;
                _racerPositions[i] = _racers[racer];
            }
        }

        private void onFinishedRace(int racer)
        {
            _racersFinished++;
            Racers[racer].Finished = _racersFinished;
        }

        private void onLapComplete(int racer)
        {
            if (Racers[racer].Lap == Laps)
            {
                onFinishedRace(racer);
            }
            else
            {
                Racers[racer].Lap++;
            }
        }
        
        private static bool racerIsBehind(Racer racer1, Racer racer2)
        {
            // when the driver needs to move back in the list?
            // finished the race later
            if (racer2.Finished > 0 && racer1.Finished == 0)
                return true;
            if (racer1.Finished > 0 && racer2.Finished == 0)
                return false;
            if (racer1.Finished > 0 && racer2.Finished > 0)
                return racer1.Finished > racer2.Finished;
            // being left behind
            if (racer1.CheckpointsPassed < racer2.CheckpointsPassed)
                return true;
            if (racer1.CheckpointsPassed > racer2.CheckpointsPassed)
                return false;
            return Vectors.Distance(racer1.Car.Veh.Physics.Position, racer1.CurrentCheckpoint.pt) >
                    Vectors.Distance(racer2.Car.Veh.Physics.Position, racer2.CurrentCheckpoint.pt);
        }

        private void runVeh2VehCollision()
        {
            /* TODO:
            // Detect collisions between each pair of vehicles
            bool impactPairs[MAX_RACING_CARS_SQUARED];
            VectorF* rect[] = new VectorF[4];
            int i;
            for (i = 0; i < MAX_RACING_CARS; i++)
            {
                if (!Cars[i].IsInit)
                    continue;
                int j;
                for (j = 0; j < MAX_RACING_CARS; j++)
                {
                    if (j == i)
                        continue;
                    if (!Cars[j].IsInit)
                        continue;
                    if (i > j && impactPairs[i * MAX_RACING_CARS + j])
                        continue; // already has impact for this car pair
                    rect[0] = Cars[j].collPoint[0];
                    rect[1] = Cars[j].collPoint[1];
                    rect[2] = Cars[j].collPoint[2];
                    rect[3] = Cars[j].collPoint[3];
                    VectorF* impact = Cars[i].DetectCollision(rect, Cars[j].velocity, j);
                    if (impact != null)
                    {
                        impactPairs[i * MAX_RACING_CARS + j] = true;
                        Cars[i].velocity.add(impact);
                        impact.negate();
                        Cars[j].velocity.add(impact);
                    }
                }
            }
            */
        }
    }
}
