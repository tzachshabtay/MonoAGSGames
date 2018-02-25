using System.Collections.Generic;
using AGS.API;

namespace LastAndFurious
{
    /// <summary>
    /// Describes a region on the racing track.
    /// </summary>
    public class TrackRegion
    {
        private float _terraGrip;

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
        public float TerraGrip { get => _terraGrip; set => _terraGrip = MathHelper.Clamp(value, 0.0F, 1.0F); }
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
    /// Describes AI data available for the racing track.
    /// </summary>
    public class TrackAIData
    {
        public IBitmap AIRegionMask { get; set; }
        public Dictionary<Color, float> AIRegionAngles { get; set; }
    }

    /// <summary>
    /// Describes the racing track and its properties.
    /// </summary>
    public class Track
    {
        IImage _background;
        int[,] _regionMap;
        int[] _regionMapSize;
        IBitmap _regionMask; // TODO: replace with regionMap
        Dictionary<Color, int> _regionColorMap;
        TrackRegion _nullRegion; // return for safety from GetRegionAt
        TrackRegion[] _regions;

        // Supported AI data
        TrackAIData _aiData;


        public IImage Background { get => _background; }
        public TrackRegion[] Regions { get => _regions; }
        public TrackAIData AiData { get => _aiData; }

        /// Get/set track's gravity (default is 9.807)
        public float Gravity { get; set; }
        /// Get/set track's air resistance factor. Air resistance force is applied to any object moving on the
        /// track, and is proportional to its squared velocity.
        public float AirResistance { get; set; }
        

        public Track(IImage background, int regionCount, int[,] regionMap, TrackAIData aiData)
        {
            init(background, regionCount, aiData);
            
            _regionMap = regionMap;
            _regionMapSize = new int[2] { regionMap.GetLength(0), _regionMap.GetLength(1) };
        }

        // TODO: temporary solution, remove later
        public Track(IImage background, int regionCount, IBitmap regionMask, Dictionary<Color, int> regionColorMap, TrackAIData aiData)
        {
            init(background, regionCount, aiData);

            _background = background;
            _regionMask = regionMask;
            _regionColorMap = regionColorMap;
            _regionMapSize = new int[2] { _regionMask.Width, _regionMask.Height };
        }

        private void init(IImage background, int regionCount, TrackAIData aiData)
        {
            _background = background;

            _regions = new TrackRegion[regionCount];
            for (int i = 0; i < regionCount; ++i)
                _regions[i] = new TrackRegion(i);
            _nullRegion = new TrackRegion(-1);

            Gravity = 9.807F;
            AirResistance = 0.01F;

            _aiData = aiData;
        }

        /// <summary>
        /// Returns a region at the given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public TrackRegion GetRegionAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _regionMapSize[0] || y >= _regionMapSize[1])
                return _nullRegion;
            // TODO: safety checks and throws
            // TODO: remove use of regionMask and use solely regionMap
            if (_regionMap != null)
            {
                return _regions[_regionMap[x, y]];
            }
            else
            {
                // NOTE: since MonoAGS has Y axis pointing up, we need to invert the lookup array's Y index
                y = _regionMapSize[1] - y;
                Color col = _regionMask.GetPixel(x, y);
                int index = 0;
                _regionColorMap.TryGetValue(col, out index);
                return _regions[index];
            }
        }
    }
}
