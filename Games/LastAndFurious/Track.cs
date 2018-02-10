
namespace LastAndFurious
{
    /// <summary>
    /// Describes a region on the racing track.
    /// </summary>
    public class TrackRegion
    {
        /// <summary>
        /// Region's unique identifier
        /// </summary>
        public int ID { get; private set; }

        /// Get/set if this area is an obstacle and cannot be moved onto.
        public bool IsObstacle { get; set; }
        /// Get/set terrain's slide friction factor for the particular walkable area.
        /// Slide friction is a force that is applied to an object sliding (but not rolling) upon the surface.
        public float TerraSlideFriction { get; set; }
        /// Get/set terrain's rolling friction factor for the particular walkable area.
        /// Rolling friction is a force that is applied to an object rolling upon the surface.
        /// Usually, the harder and flatter the surface is, the LOWER the rolling friction is. Softer
        /// terrain (sand, muddy soil) produce higher rolling friction.
        public float TerraRollFriction { get; set; }
        /// Get/set terrain grip factor for the walkable area.
        /// Grip is a value between 0.0 and 1.0 which determines how well the engine's power translates through
        /// the wheels into surface, and pushing the car. 
        /// TODO: in theory the grip should somehow be connected with friction, but I'd leave it separated
        /// for now, for the sake of simplicity (we lack proper formulas to make it work well).
        public float TerraGrip { get; set; }
        /// Get/set additional enviroment resistance factor.
        /// This is an abstract force applied to an object moving in the unusual area: water streams, sand dunes,
        /// shrubbery, and so forth.
        public float EnvResistance { get; set; }

        public TrackRegion(int id)
        {
            ID = id;
        }
    }

    /// <summary>
    /// Describes the racing track and its properties.
    /// </summary>
    public class Track
    {
        TrackRegion _dummyRegion;

        /// Get/set track's gravity (default is 9.807)
        public float Gravity { get; set; }
        /// Get/set track's air resistance factor. Air resistance force is applied to any object moving on the
        /// track, and is proportional to its squared velocity.
        public float AirResistance { get; set; }

        public Track()
        {
            Gravity = 9.807F;
            AirResistance = 0.01F;
            _dummyRegion = new TrackRegion(0);
            _dummyRegion.IsObstacle = false;
            _dummyRegion.TerraSlideFriction = 24.0F;
            _dummyRegion.TerraRollFriction = 0.7F;
            _dummyRegion.TerraGrip = 0.6F;
            _dummyRegion.EnvResistance = 0.45F;
        }

        /// <summary>
        /// Returns a region at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TrackRegion GetRegion(int x, int y)
        {
            // TODO: implement a regions mask
            return _dummyRegion;
        }
    }
}
