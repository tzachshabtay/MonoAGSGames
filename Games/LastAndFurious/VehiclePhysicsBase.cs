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
        /* TODO:
  
      /// Initialize vehicle by binding it to the given character and graphical representation
      import void SetCharacter(Character *c, int carSprite, CharacterDirection carSpriteDir, int view = 0, int loop = 0, int frame = 0);
      /// Reset vehicle, place at given position, zero all forces, etc
      import void Reset(VectorF *pos, VectorF *dir);
      /// Run vehicle physics
      import void Run(float deltaTime);
      /// Uninitialize vehicle by removing character's reference and all allocated resources
      import void UnInit();
  
      protected import void DetachCharacter();
      protected import void ResetBase(VectorF *pos, VectorF *dir);
      protected import void RunPhysicsBase(float deltaTime);
      protected import void UnInitBase();
  
      /// Synchronizes character with the vehicle position and direction
      protected import void SyncCharacter();
      /// Update vehicle body parameters with the new position and direction
      protected import void UpdateBody();
  
  
      /*protected*//*
        Character* c; // character that represents this vehicle

        protected int carSprite; // default car sprite, at angle carAngle
        

        

        
            */


        public const int NUM_COLLISION_POINTS = 4;

        protected IObject _object;

        /// <summary>
        /// Length of the vehicle.
        /// </summary>
        public float BodyLength { get; protected set; }
        /// <summary>
        /// Width of the vehicle.
        /// </summary>
        public float BodyWidth { get; protected set; }
        /// <summary>
        /// Angle at which the vehicle is depicted on the provided sprite.
        /// Required to properly apply current rotation to the visible object.
        /// </summary>
        /// TODO: instead, work with Sprite and have initial rotation set in sprite!
        public float CarModelAngle { get; protected set; }
        /// <summary>
        /// Vehicle position.
        /// </summary>
        public Vector2 Position { get; protected set; }
        /// <summary>
        /// Vehicle face direction.
        /// </summary>
        public Vector2 Direction { get; protected set; }
        /// <summary>
        /// Final linear velocity, summing up all the forces
        /// </summary>
        public Vector2 Velocity { get; protected set; }
        /// <summary>
        /// Final angular velocity (positive value rotates vehicle clockwise)
        /// </summary>
        public float AngularVelocity { get; protected set; }
        /// <summary>
        /// Relative points on the car's body at which to check the collision and interaction with enviroment
        /// </summary>
        protected Vector2[] CollPointOff = new Vector2[NUM_COLLISION_POINTS];
        /// <summary>
        /// Points at which to check the collision and interaction with enviroment (absolute coordinates)
        /// </summary>
        protected Vector2[] CollPoint = new Vector2[NUM_COLLISION_POINTS];



        public override void Init(IEntity entity)
        {
            base.Init(entity);
            // TODO: perhaps that'd be more correct to retrieve actual components we need (ITranslate, IRotate),
            // but I were lazy
            if (entity is IObject o)
            {
                _object = o;
            }
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
            
            CarModelAngle = originalModelAngle;

            BodyLength = carl;
            BodyWidth = carw;

            CollPointOff[0] = new Vector2(carl / 2, -carw / 2);
            CollPointOff[1] = new Vector2(carl / 2, carw / 2);
            CollPointOff[2] = new Vector2(-carl / 2, carw / 2);
            CollPointOff[3] = new Vector2(-carl / 2, -carw / 2);

            SyncObject();
        }

        public void DetachCarModel()
        {
            BodyLength = 0F;
            BodyWidth = 0F;
            CarModelAngle = 0F;

            for (int i = 0; i < NUM_COLLISION_POINTS; i++)
                CollPointOff[i] = new Vector2();
        }

        /// <summary>
        /// Resets vehicle with the new position and direction.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        public virtual void Reset(Vector2 pos, Vector2 dir)
        {
            ResetBase(pos, dir);
            UpdateBody();
            SyncObject();
        }

        protected void ResetBase(Vector2 pos, Vector2 dir)
        {
            if (pos == null)
                Position = new Vector2(_object.X, _object.Y);
            else
                Position = pos;
            if (dir == null)
                Direction = new Vector2(0, 1);
            else
            {
                Direction = dir;
                Direction.Normalize();
            }
            
            for (int i = 0; i < NUM_COLLISION_POINTS; i++)
                CollPoint[i] = new Vector2();

            Velocity = Vector2.Zero;
            AngularVelocity = 0;
        }

        protected void SyncObject()
        {
            if (_object == null || Position == null)
                return;

            _object.X = Position.X;
            _object.Y = Position.Y;

            float angle = MathHelper.RadiansToDegrees(Direction.Angle());
            angle = angle - CarModelAngle;
            angle = MathEx.Angle360(angle);
            _object.Angle = angle;
        }

        protected void UpdateBody()
        {
            /*
             * // update collision points with the new position and direction
              int i;
              for (i = 0; i < NUM_COLLISION_POINTS; i++) {
                VectorF *colpt = this.collPoint[i];
                colpt.set(this.collPointOff[i]);
                colpt.rotate(this.direction.angle());
                colpt.add(this.position);
              }
             */
        }
    }
}
