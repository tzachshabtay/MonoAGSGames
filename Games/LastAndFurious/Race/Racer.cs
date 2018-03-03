using System.Collections.Generic;

namespace LastAndFurious
{
    /// <summary>
    /// Race event participant
    /// </summary>
    public class Racer
    {
        DriverCharacter _driver;
        VehicleBehavior _car;
        float _time; // total racing time

        /// <summary>
        /// Driving character.
        /// </summary>
        public DriverCharacter Driver { get => _driver; }
        /// <summary>
        /// Riding car.
        /// </summary>
        public VehicleBehavior Car { get => _car; }
        /// <summary>
        /// Current lap this racer is passing.
        /// </summary>
        public int Lap { get; set; }
        /// <summary>
        /// Current position in race.
        /// </summary>
        public int Place { get; set; }
        /// <summary>
        /// Position at which finished. -1 if did not finish yet.
        /// </summary>
        public int Finished { get; set; }
        /// <summary>
        /// How many checkpoints passed
        /// </summary>
        public int CheckpointsPassed { get => _checkptsPassed; }
        /// <summary>
        /// Current target checkpoint (the one this racers is heading to).
        /// </summary>
        public RaceNode CurrentCheckpoint { get => _curRaceNode; }

        RaceNode _curRaceNode;
        int _checkptsPassed; // number of passed checkpoints
        IList<RaceNode> _checkpoints;
        
        public Racer(DriverCharacter driver, VehicleBehavior car, IList<RaceNode> checkpoints)
        {
            _driver = driver;
            _car = car;
            _checkpoints = checkpoints;
        }

        public void Reset(RaceNode node)
        {
            _curRaceNode = node;
            Lap = 1;
            Place = 0;
            _time = 0f;
            Finished = 0;
        }

        public void Run(float deltaTime)
        {
            if (Finished > 0)
                return;
            // Update time
            _time += deltaTime;
            // Update aimed checkpoint
            if (_checkpoints.Count > 0)
            {
                if (_curRaceNode == null)
                {
                    _curRaceNode = _checkpoints[0];
                    _checkptsPassed = 0;
                }
                else
                {
                    RaceNode curNode = _curRaceNode;
                    RaceNode nextNode = _curRaceNode.next;
                    if (nextNode != null &&
                        Vectors.Distance(Car.Physics.Position, nextNode.pt) < Vectors.Distance(curNode.pt, nextNode.pt))
                    {
                        switchToNextNode();
                        if (curNode.order == 0 && _checkptsPassed > 1)
                        {
                            //OnLapComplete(index);
                        }
                    }
                }
            }
        }

        public void switchToNextNode()
        {
            _checkptsPassed++;
            _curRaceNode = _curRaceNode.next;
        }
    }
}
