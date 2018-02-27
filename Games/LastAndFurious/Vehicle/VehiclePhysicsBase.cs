using System;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// Base class for vehicle physics
    /// </summary>
    public abstract class VehiclePhysicsBase : AGSComponent
    {
        public const int NUM_COLLISION_POINTS = 4;

        protected IGame _game;
        protected IObject _object;
        protected Track _track;

        protected float bodyLength;
        protected float bodyWidth;
        protected float carModelAngle;

        protected Vector2 position;
        protected Vector2 direction;
        protected Vector2 velocity;
        protected float angularVelocity;

        /// <summary>
        /// Length of the vehicle.
        /// </summary>
        public float BodyLength { get => bodyLength; }
        /// <summary>
        /// Width of the vehicle.
        /// </summary>
        public float BodyWidth { get => bodyWidth; }
        /// <summary>
        /// Angle at which the vehicle is depicted on the provided sprite.
        /// Required to properly apply current rotation to the visible object.
        /// </summary>
        /// TODO: instead, work with Sprite and have initial rotation set in sprite!
        public float CarModelAngle { get => carModelAngle; }
        /// <summary>
        /// Vehicle position.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; syncObject(); }
        }
        /// <summary>
        /// Vehicle face direction.
        /// </summary>
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; syncObject(); }
        }
        /// <summary>
        /// Final linear velocity, summing up all the forces
        /// </summary>
        public Vector2 Velocity { get => velocity; }
        /// <summary>
        /// Final angular velocity (positive value rotates vehicle clockwise)
        /// </summary>
        public float AngularVelocity { get => angularVelocity; }
        /// <summary>
        /// Relative points on the car's body at which to check the collision and interaction with enviroment
        /// </summary>
        protected Vector2[] collPointOff = new Vector2[NUM_COLLISION_POINTS];
        /// <summary>
        /// Points at which to check the collision and interaction with enviroment (absolute coordinates)
        /// </summary>
        protected Vector2[] collPoint = new Vector2[NUM_COLLISION_POINTS];

        /// <summary>
        /// Gets the racing Track this vehicle is currently on.
        /// </summary>
        public Track RaceTrack { get => _track; }


        public VehiclePhysicsBase(IGame game)
        {
            _game = game;
        }

        public override void Init(IEntity entity)
        {
            base.Init(entity);
            // TODO: perhaps that'd be more correct to retrieve actual components we need (ITranslate, IRotate),
            // but I were lazy
            if (entity is IObject o)
            {
                _object = o;
            }
            _game.Events.OnRepeatedlyExecute.Subscribe(repExec);
        }

        /// <summary>
        /// Provide information about graphic model, necessary to synchronize physics model
        /// with the graphical representation.
        /// </summary>
        /// TODO: this should not be like this in the component-based engine.
        /// Rather make car model another component, that works through ImageComponent or even SpriteRenderComponent.
        /// Or even require setting up boundary box explicitly.
        public virtual void AttachCarModel(IImage image, float originalModelAngle)
        {
            DetachCarModel();

            // car's length and width
            float carl, carw;
            // TODO: Pi/2 and similar constants
            if (MathUtils.FloatEquals(originalModelAngle, 90.0F) || MathUtils.FloatEquals(originalModelAngle, 270.0F))
            {
                carl = image.Height;
                carw = image.Width;
            }
            else if (MathUtils.FloatEquals(originalModelAngle, 0.0F) || MathUtils.FloatEquals(originalModelAngle, 180.0F))
            {
                carl = image.Width;
                carw = image.Height;
            }
            else
            {
                throw new ArgumentException("Car model angle cannot be diagonal, or out of [0, 359] range, please provide sprite having one of the following angles: 0, 90, 180 or 270 degrees.");
            }
            
            carModelAngle = originalModelAngle;

            bodyLength = carl;
            bodyWidth = carw;

            collPointOff[0] = new Vector2(carl / 2, -carw / 2);
            collPointOff[1] = new Vector2(carl / 2, carw / 2);
            collPointOff[2] = new Vector2(-carl / 2, carw / 2);
            collPointOff[3] = new Vector2(-carl / 2, -carw / 2);

            syncObject();
        }

        public void DetachCarModel()
        {
            bodyLength = 0F;
            bodyWidth = 0F;
            carModelAngle = 0F;

            for (int i = 0; i < NUM_COLLISION_POINTS; ++i)
                collPointOff[i] = new Vector2();
        }

        /// <summary>
        /// Resets vehicle with the new position and direction.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public virtual void Reset(Track track, Vector2 pos, Vector2 dir)
        {
            resetBase(track, pos, dir);
            updateBody();
            syncObject();
        }

        /// Run vehicle physics
        public virtual void Run(float deltaTime)
        {
            runPhysicsBase(deltaTime);
            updateBody();
            syncObject();
        }

        protected void repExec()
        {
            if (LF.GameState.Paused)
                return;
            // TODO: get delta time from one API, using more precise calculation
            float delta_time = (float)(1.0 / AGSGame.UPDATE_RATE);
            Run(delta_time);
        }

        protected void resetBase(Track track, Vector2 pos, Vector2 dir)
        {
            _track = track;
            if (pos == null)
                position = new Vector2(_object.X, _object.Y);
            else
                position = pos;
            if (dir == null)
                direction = new Vector2(0, 1);
            else
            {
                direction = Vectors.SafeNormalize(dir);
            }
            
            for (int i = 0; i < NUM_COLLISION_POINTS; ++i)
                collPoint[i] = new Vector2();

            velocity = Vector2.Zero;
            angularVelocity = 0;
        }

        protected void runPhysicsBase(float deltaTime)
        {
            // update position using last velocity scaled by time
            position = Vectors.AddScaled(position, velocity, deltaTime);
            float rot_angle = angularVelocity * deltaTime;
            if (rot_angle != 0.0)
                direction = Vectors.Rotate(direction, rot_angle);
        }

        protected void syncObject()
        {
            if (_object == null || Position == null)
                return;

            _object.X = position.X;//(float)Math.Round(position.X);
            _object.Y = position.Y;//(float)Math.Round(position.Y);

            float angle = MathHelper.RadiansToDegrees(direction.Angle());
            //angle = (float)Math.Round(angle);
            angle = angle - CarModelAngle;
            angle = MathEx.Angle360(angle);
            _object.Angle = angle;
        }

        protected void updateBody()
        {
            // update collision points with the new position and direction
            for (int i = 0; i < NUM_COLLISION_POINTS; ++i)
            {
                Vector2 cp = collPointOff[i];
                cp = Vectors.Rotate(cp, direction.Angle());
                cp = Vector2.Add(cp, position);
                collPoint[i] = cp;
            }
        }
    }
}
