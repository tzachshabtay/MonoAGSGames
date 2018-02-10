using System;
using AGS.API;

namespace LastAndFurious
{
    public class VehiclePhysics : VehiclePhysicsBase
    {
        /// Get/set engine acceleration (0.0 - 1.0)
        public float Accelerator
        {
            get { return engineAccelerator; }
            set { engineAccelerator = MathUtils.Clamp(value, 0.0F, 1.0F); }
        }
        /// Get current engine's power
        public float EnginePower { get => enginePower; }
        /// Get/set brake force
        public float Brakes { get => brakePower; set => brakePower = MathUtils.Clamp(value, 0.0F, 1.0F); }

        /// The angle between steering wheel's direction and car's direction (or rather drive wheel's direction, for that's matters)
        public float SteeringWheelAngle { get => steeringWheelAngle; set => steeringWheelAngle = value; }

        public float BodyMass { get => bodyMass; set => bodyMass = value; }
        public float BodyAerodynamics { get => bodyAerodynamics; set => bodyAerodynamics = value; }
        public float HardImpactLossFactor { get => hardImpactLossFactor; set => hardImpactLossFactor = value; }
        public float SoftImpactLossFactor { get => softImpactLossFactor; set => softImpactLossFactor = value; }

        public float EngineMaxPower { get => engineMaxPower; set => engineMaxPower = value; }
        public float StillTurningVelocity { get => stillTurningVelocity; set => stillTurningVelocity = value; }
        public float DriftVelocityFactor { get => driftVelocityFactor; set => driftVelocityFactor = value; }



        // TODO: reorganize the variable declaration
        float steeringWheelAngle;

        //
        // Car body properties
        //
        // The mass of the car
        /*protected*/
        float bodyMass;
        // Abstract aerodynamic factor (less is better)
        /*protected*/
        float bodyAerodynamics;
        // How much of car's energy is lost during impact (0.0 - 1.0)
        /*protected*/
        float hardImpactLossFactor;
        /*protected*/
        float softImpactLossFactor;

        //
        // Engine
        //
        // We do not have transmission in this simulation, engine is directly connected to the
        // driving wheels, does not have "workload" concept, and cannot stall.
        //
        // * Engine has "max power" constant, and "current power" dynamic value.
        // * When acceleration is enabled (power goal set), current power becomes equal to the
        //   fraction of the max power, defined by "accelerator" value (or "power goal" value).
        // * Current power directly translates into drive wheel's torque.
        // * Power can be negative, in which case the drive wheel is supposed to rotate backwards.
        //

        // The maximal amount of power that engine can produce.
        // Can be positive or negative, depending on the engine mode.
        /*protected*/
        float engineMaxPower;
        // Percentage of power use (0.0 - 1.0)
        /*protected*/
        float engineAccelerator;
        // The engine's power goal. This is the value current power is trying to achieve.
        /*protected*/
        float enginePowerGoal;
        // Current engine's power.
        /*protected*/
        float enginePower;

        //
        // Brakes
        //
        // Brakes work simple: they instantly reduce driving wheel's torque by certain fraction.
        //

        // The power applied against the driving wheel's "torque". In fraction of torque loss (0.0 - 1.0).
        /*protected*/
        float brakePower;

        //
        // Driving wheels.
        //
        // Simulation of driving wheels goes as this: torque is calculated from engine's power as 1:1,
        // and creates the driving force that pushes the car.
        // This force may be adjusted by the "grip" factor, which determines how much of the torque
        // converts into driving force. Therefore with the best grip driving force equals torque.
        //

        // The "torque" is a measure of how powerfull the wheel rotation is.
        /*protected*/
        float driveWheelTorque;
        // The abstract "grip" of the drive wheel, works as a factor in relation between wheel's torque and drive force.
        // Proper values are 0.0 to 1.0. This is a dynamic property that changes depending on a surface the car is on.
        /*protected*/
        float driveWheelGrip;
        // Driving force is trying to move the car. Depends on the torque and wheel's grip.
        // This force is one-dimensional (therefore represented by float), when applied to 2D space its vector always
        // matches one of a car's body.
        /*protected*/
        float driveWheelForce;

        //
        // Steering wheel (we assume there's only one "global one", separate from driving wheel, for simplicity)
        // 

        // Distance between driving and steering axle
        /*protected*/
        float distanceBetweenAxles;
        /*protected*/
        float stillTurningVelocity;
        
        /*protected*/
        Vector2 turningAccel;
        /*protected*/
        float driftVelocityFactor;

        //
        // Enviroment
        //
        // Vehicle's weight force (depends on mass, gravity and elevation of surface)
        /*protected*/
        float weightForce;
        // Average surface grip
        /*protected*/
        float envGrip;
        // Average surface's slide friction
        /*protected*/
        float envSlideFriction;
        // Average surface's roll friction
        /*protected*/
        float envRollFriction;
        // Average custom enviroment resistance
        /*protected*/
        float envResistance;

        bool strictCollisions;
        // Parameters telling if particular point is colliding with an obstacle (stores area ID)
        /*protected*/
        int[] collPtHit = new int[NUM_COLLISION_POINTS];
        /*protected*/
        int[] collPtCarHit = new int[NUM_COLLISION_POINTS];
        /*protected*/
        int numHits;
        /*protected*/
        int numCarHits;
        // Previous positions of the collision points
        /*protected*/
        Vector2[] oldCollPt = new Vector2[NUM_COLLISION_POINTS];
        // Previous hit flags
        /*protected*/
        int[] oldCollPtHit = new int[NUM_COLLISION_POINTS];
        /*protected*/
        int[] oldCollPtCarHit = new int[NUM_COLLISION_POINTS];

        //
        // Information for external scripts
        //
        // Enviroment resistance forces for rolling and sliding directions.
        float infoRollAntiforce;
        float infoSlideAntiforce;
        Vector2 infoImpact;


        public VehiclePhysics(IGame game) : base(game)
        {
        }

        public override void Reset(Track track, Vector2 pos, Vector2 dir)
        {
            resetBase(track, pos, dir);

            engineMaxPower = 200.0F;
            engineAccelerator = 0.0F;
            enginePowerGoal = 0.0F;
            enginePower = 0.0F;
            brakePower = 0.0F;
            driveWheelTorque = 0.0F;
            driveWheelForce = 0.0F;

            distanceBetweenAxles = bodyLength / 2.0F;
            stillTurningVelocity = 4.0F;
            driftVelocityFactor = 240.0F;
            steeringWheelAngle = 0.0F;
            turningAccel = new Vector2();

            // TODO: move to init function, set by user
            bodyMass = 1.0F;
            bodyAerodynamics = 1.0F;
            hardImpactLossFactor = 0.5F;
            softImpactLossFactor = 0.8F;

            weightForce = 0.0F;
            envGrip = 0.0F;
            envSlideFriction = 0.0F;
            envRollFriction = 0.0F;
            envResistance = 0.0F;

            infoRollAntiforce = 0.0F;
            infoSlideAntiforce = 0.0F;
            infoImpact = new Vector2();

            int i;
            for (i = 0; i < NUM_COLLISION_POINTS; i++)
            {
                oldCollPt[i] = new Vector2();
                collPtHit[i] = -1;
                oldCollPtHit[i] = -1;
                collPtCarHit[i] = -1;
                oldCollPtCarHit[i] = -1;
            }
            numHits = 0;
            numCarHits = 0;

            updateBody();
            syncObject();
        }

        public override void Run(float deltaTime)
        {
            runPhysics(deltaTime);
            syncObject();
        }


        protected void updateEnviroment()
        {
            float[] slide_friction = new float[NUM_COLLISION_POINTS];
            float[] roll_friction = new float[NUM_COLLISION_POINTS];
            float[] env_res = new float[NUM_COLLISION_POINTS];
            float[] grip = new float[NUM_COLLISION_POINTS];
            float avg_slide_friction = 0.0F;
            float avg_roll_friction = 0.0F;
            float avg_env_res = 0.0F;
            float avg_grip = 0.0F;

            numHits = 0;

            int valid_terrain = 0;
            for (int i = 0; i < NUM_COLLISION_POINTS; ++i)
            {
                Vector2 colpt = collPoint[i];
                int room_x = (int)Math.Round(colpt.X);
                int room_y = (int)Math.Round(colpt.Y);
                TrackRegion region = _track.GetRegion(room_x, room_y);
                if (region.IsObstacle)
                    collPtHit[i] = region.ID; // this is obstacle
                else
                    collPtHit[i] = -1;

                if (collPtHit[i] >= 0)
                {
                    numHits++;
                    continue;
                }

                valid_terrain++;
                slide_friction[i] = region.TerraSlideFriction;
                avg_slide_friction += slide_friction[i];
                roll_friction[i] = region.TerraRollFriction;
                avg_roll_friction += roll_friction[i];
                env_res[i] = region.EnvResistance;
                avg_env_res += env_res[i];
                grip[i] = region.TerraGrip;
                avg_grip += grip[i];
            }

            // Apply average friction and resistance values
            if (valid_terrain > 0)
            {
                float valid_terrain_f = valid_terrain;
                avg_slide_friction /= valid_terrain_f;
                envSlideFriction = avg_slide_friction;
                avg_roll_friction /= valid_terrain_f;
                envRollFriction = avg_roll_friction;
                avg_env_res /= valid_terrain_f;
                envResistance = avg_env_res;
                avg_grip /= valid_terrain_f;
                envGrip = avg_grip;
            }
            else
            {
                envSlideFriction = 0.0F;
                envRollFriction = 0.0F;
                envResistance = 0.0F;
                envGrip = 0.0F;
            }

            weightForce = bodyMass * _track.Gravity; // TODO: need to count elevation when we support one

            driveWheelGrip = envGrip; // TODO: need to count vehicle's height above ground when we support jumping
                                                // TODO: also, what about connecting to weightForce here?  
        }

        protected void runCollision(Vector2 impactVelocity, float deltaTime)
        {
            if (numHits == 0)
                return; // no collisions

            // Calculate impact vectors
            Vector2 posImpact = Vector2.Zero;
            Vector2 negImpact = Vector2.Zero;
            int i;
            for (i = 0; i < NUM_COLLISION_POINTS; i++)
            {
                if (collPtHit[i] < 0)
                    continue; // point is not colliding
                if (oldCollPtHit[i] == collPtHit[i])
                    continue; // HACK: colliding same obstacle, ignore this one

                // Create impact vector, which is a direction from old point's position to the new one
                // (if they match, point did not move in absolute coordinates, and so there is no impact)
                Vector2 impact = Vector2.Subtract(collPoint[i], oldCollPt[i]);
                if (impact.IsZero())
                    continue;
                impact = Vectors.SafeNormalize(impact);
                // Making projection of velocity vector on this direction of impact
                float velProjection = Vectors.Projection(velocity, impact);
                // If projection is positive, then turn vector around and scale to projection value
                if (velProjection > 0.0)
                {
                    // Impact must fully negate current velocity project, plus add a negative fraction of original velocity
                    Vector2.Multiply(impact, -velProjection * (2.0F - hardImpactLossFactor));
                    // Note that we do not simply summ impact forces, we take the maximum of each vector component,
                    // because car can hit obstacles with more than one point.
                    posImpact = Vector2.Max(posImpact, impact);
                    negImpact = Vector2.Min(negImpact, impact);

                    //Display("velProjection = %f[impact=(%f,%f)[posImpact=(%f,%f)[negImpact=(%f,%f)", velProjection, 
                    //      impact.X, impact.Y, posImpact.X, posImpact.Y, negImpact.X, negImpact.Y);
                }
            }
            // Finally, sum up positive and negative direction impacts
            Vector2.Add(impactVelocity, posImpact);
            Vector2.Add(impactVelocity, negImpact);
        }

        protected void runPhysics(float deltaTime)
        {
            // If there was collision, we need to first return the car to previous position.
            // Normally, we could also check which points did the hit, and add corresponding rotation to the car.
            // But for now we do just linear transition.
            if (strictCollisions && numHits > 0)
            {
                // Restore old state and reverse-calculate car position from its points
                float x = 0;
                float y = 0;
                for (int i = 0; i < NUM_COLLISION_POINTS; i++)
                {
                    collPoint[i] = oldCollPt[i];
                    collPtHit[i] = oldCollPtHit[i];
                    x += collPoint[i].X;
                    y += collPoint[i].Y;
                }
                position.X = x / NUM_COLLISION_POINTS;
                position.Y = y / NUM_COLLISION_POINTS;
            }
            else
            {
                // Save old state
                for (int i = 0; i < NUM_COLLISION_POINTS; i++)
                {
                    oldCollPt[i] = collPoint[i];
                    //oldCollPtHit[i] = collPtHit[i];
                    //oldCollPtCarHit[i] = collPtCarHit[i];
                }
            }
            // Save old state
            for (int i = 0; i < NUM_COLLISION_POINTS; i++)
            {
                oldCollPtHit[i] = collPtHit[i];
                oldCollPtCarHit[i] = collPtCarHit[i];
            }

            runPhysicsBase(deltaTime);
            updateBody();
            updateEnviroment();

            if (engineAccelerator > 0)
            {

            }

            //
            // Engine
            //
            enginePower = engineMaxPower * engineAccelerator;

            //
            // Wheel drive
            //
            // Wheel's torque and drive force match engine's power, minus brakes
            driveWheelTorque = enginePower;
            driveWheelTorque -= driveWheelTorque * brakePower;
            // Wheel's force depends on torque and grip
            driveWheelForce = driveWheelTorque * driveWheelGrip;

            //
            // Steering
            //
            // Split current velocity on two components: rolling velocity (car's direction)
            // and sliding velocity (perpendicular to car's direction).
            //
            Vector2 rollDirection = direction;
            Vector2 slideDirection = rollDirection;
            slideDirection = Vectors.Rotate(slideDirection, (float)(Math.PI / 2.0));
            float rollingVelocity = Vectors.Projection(velocity, rollDirection);
            float slidingVelocity = Vectors.Projection(velocity, slideDirection);

            turningAccel = Vector2.Zero;
            if (velocity.IsZero())
            {
                // If the car is standing still, we allow player to turn the car without linear velocity
                angularVelocity = steeringWheelAngle * stillTurningVelocity;
            }
            else
            {
                // Rolling direction is basically our drive wheel direction.
                // We calculate the direction and rolling projection of the steering wheel now.
                if (steeringWheelAngle != 0.0)
                {
                    // Create the centripetal acceleration, which is going to be perpendicular to the steering direction.
                    // Note, that if we create ideal acceleration, it will fully translate existing rolling velocity
                    // to the new direction, which will totally negate any kind of drift. While this may be more or less
                    // okay for low car speeds, that's not acceptable for hugh-speed racing.
                    //
                    // TODO: find a simplier/faster calculation?!

                    float steerAngle = steeringWheelAngle;

                    Vector2 driveWheelPos = position;
                    Vector2 steerWheelPos = position;
                    driveWheelPos = Vectors.AddScaled(driveWheelPos, direction, -distanceBetweenAxles / 2.0F);
                    steerWheelPos = Vectors.AddScaled(steerWheelPos, direction, distanceBetweenAxles / 2.0F);
                    Vector2 driveWheelMovement = rollDirection;
                    Vector2 steerWheelMovement = rollDirection;
                    steerWheelMovement = Vectors.Rotate(steerWheelMovement, steerAngle);
                    driveWheelPos = Vectors.AddScaled(driveWheelPos, driveWheelMovement, rollingVelocity);
                    steerWheelPos = Vectors.AddScaled(steerWheelPos, steerWheelMovement, rollingVelocity);
                    Vector2 newPosDir = Vector2.Subtract(steerWheelPos, driveWheelPos);
                    newPosDir = Vectors.SafeNormalize(newPosDir);

                    angularVelocity = Vectors.AngleBetween(direction, newPosDir);
                    // simple wheel grip dependance (we do not want it turn as fast on slippery surfaces)
                    angularVelocity *= envGrip;

                    float dumb_drift_factor = (float)(Math.Atan(Math.Abs(rollingVelocity / driftVelocityFactor)) / (Math.PI / 2.0F));
                    turningAccel = Vector2.Subtract(newPosDir, rollDirection);
                    turningAccel = Vector2.Multiply(turningAccel, rollingVelocity * (1.0F - dumb_drift_factor) * envGrip);
                }
                else
                {
                    angularVelocity = 0.0F;
                }
            }

            //
            // Applying forces
            //
            Vector2 rollResDir = rollDirection;
            // TODO: find simplier way to get correct vector direction
            rollResDir = Vector2.Multiply(rollResDir, -rollingVelocity);
            rollResDir = Vectors.SafeNormalize(rollResDir);
            Vector2 slideResDir = slideDirection;
            // TODO: find simplier way to get correct vector direction
            slideResDir = Vector2.Multiply(slideResDir, -slidingVelocity);
            slideResDir = Vectors.SafeNormalize(slideResDir);

            // Set drive force
            float driveForce = (driveWheelForce * deltaTime) / bodyMass;

            // Both friction and resistance can work against both projections, but there is difference:
            // * enviromental resistance force always fully works against both projections;
            //   it is proportional to the actual velocity (higher velocity makes higher enviromental resistance).
            // * enviromental friction force always fully works against sliding projection, and
            //   is appliance against rolling projection is proportional on brakePower (how much the wheels are locked);
            //   in general it is proportional to the car's weightForce.
            //
            // TODO: apply very small but distinct friction to rolling dir
            rollingVelocity = Math.Abs(rollingVelocity);
            slidingVelocity = Math.Abs(slidingVelocity);

            float slide_friction = envSlideFriction * weightForce; // Slide_friction_force = friction_factor * weight_force
            float roll_friction = envRollFriction * weightForce; // Roll_friction_force = (friction_factor / wheelradius) * weight_force
            float airres_force = 0.5F * _track.AirResistance * bodyAerodynamics;  // Air resistance = 1/2 * enviroment_factor * aerodynamic_factor * v^2
            float env_res_force = envResistance; // We take Env resistance = factor * v (that's general simplification)

            // Final anti-forces
            float rollAF = ((slide_friction * brakePower + roll_friction * (1.0F - brakePower) +
                              airres_force * rollingVelocity * rollingVelocity + env_res_force * rollingVelocity) * deltaTime) / bodyMass;
            float slideAF = ((slide_friction + airres_force * slidingVelocity * slidingVelocity + env_res_force * slidingVelocity) * deltaTime) / bodyMass;

            rollAF = MathUtils.Clamp(rollAF, 0.0F, rollingVelocity);
            slideAF = MathUtils.Clamp(slideAF, 0.0F, slidingVelocity);

            infoRollAntiforce = rollAF;
            infoSlideAntiforce = slideAF;

            //
            // Finally, apply all forces in the correspoding directions
            //
            // First goes drive thrust
            velocity = Vectors.AddScaled(velocity, rollDirection, driveForce);
            // Then we apply friction... and making sure it does not push object to the opposite direction;
            // we have to do this, because floating point calculations are never precise!
            float x1 = velocity.X;
            float y1 = velocity.Y;
            velocity = Vectors.AddScaled(velocity, slideResDir, slideAF);
            velocity = Vectors.AddScaled(velocity, rollResDir, rollAF);
            float x2 = velocity.X;
            float y2 = velocity.Y;
            if (x1 >= 0.0 && x2 < 0.0 || x1 <= 0.0 && x2 > 0.0)
                velocity.X = 0.0F;
            if (y1 >= 0.0 && y2 < 0.0 || y1 <= 0.0 && y2 > 0.0)
                velocity.Y = 0.0F;
            // Apply turning acceleration
            velocity = Vectors.AddScaled(velocity, turningAccel, deltaTime);

            //
            // Run collisions.
            // Do this last, so that new velocities will be taken into account,
            // otherwise car may continue moving into an obstacle.
            //
            Vector2 impactVelocity = Vector2.Zero;
            runCollision(impactVelocity, deltaTime);
            // ...and apply impact forces
            velocity = Vector2.Add(velocity, impactVelocity);
            // save impact forces for external reading
            infoImpact = impactVelocity;
        }

        protected Vector2? detectCollision(Vector2[] rect, Vector2 otherVelocity, int otherIndex)
        {
            // TODO: rewrite algorithm into collision with polygon (any number of points)
            // Rectangle hit algorithm copied from the internet in haste
            /*
          p21 = (x2 - x1, y2 - y1)
        p41 = (x4 - x1, y4 - y1)

        p21magnitude_squared = p21[0]^2 + p21[1]^2
        p41magnitude_squared = p41[0]^2 + p41[1]^2

        for x, y in list_of_points_to_test:

            p = (x - x1, y - y1)

            if 0 <= p[0] * p21[0] + p[1] * p21[1] <= p21magnitude_squared:
                if 0 <= p[0] * p41[0] + p[1] * p41[1]) <= p41magnitude_squared:
                    return "Inside"
                else:
                    return "Outside"
            else:
                return "Outside"
          */

            numCarHits = 0;

            Vector2 p21 = Vector2.Subtract(rect[1], rect[0]);
            Vector2 p41 = Vector2.Subtract(rect[3], rect[0]);

            float p21magnitude_squared = p21.X * p21.X + p21.Y * p21.Y;
            float p41magnitude_squared = p41.X * p41.X + p41.Y * p41.Y;

            int i;
            for (i = 0; i < NUM_COLLISION_POINTS; i++)
            {
                collPtCarHit[i] = -1;
                Vector2 p = Vector2.Subtract(collPoint[i], rect[0]);
                float pp21 = p.X * p21.X + p.Y * p21.Y;
                if (pp21 >= 0.0 && pp21 <= p21magnitude_squared)
                {
                    float pp41 = p.X * p41.X + p.Y * p41.Y;
                    if (pp41 >= 0.0 && pp41 <= p41magnitude_squared)
                    {
                        collPtCarHit[i] = otherIndex;
                        numCarHits++;
                    }
                }
            }

            if (numCarHits == 0)
                return null;


            //
            // TERRIBLE!!!
            //
            // What follows is a copy-paste of the RunCollisions function
            // Because I do not have time to devise a generic function :(((((
            //
            Vector2 impactVelocity = new Vector2();

            // Calculate impact vectors
            Vector2 posImpact = Vector2.Zero;
            Vector2 negImpact = Vector2.Zero;
            for (i = 0; i < NUM_COLLISION_POINTS; i++)
            {
                if (collPtCarHit[i] < 0)
                    continue; // point is not colliding
                if (oldCollPtCarHit[i] == collPtCarHit[i])
                    continue; // HACK: colliding same obstacle, ignore this one

                // TODO: here we only take THIS car's movement into account, which is wrong,
                // we also need to know how other car had moved

                // Create impact vector, which is a direction from old point's position to the new one
                // (if they match, point did not move in absolute coordinates, and so there is no impact)
                Vector2 impact = Vector2.Subtract(collPoint[i], oldCollPt[i]);
                if (impact.IsZero())
                    continue;
                impact = Vectors.SafeNormalize(impact);
                // Making projection of velocity vector on this direction of impact
                float velProjection = Vectors.Projection(velocity, impact);
                // If projection is positive, then turn vector around and scale to projection value
                if (velProjection > 0.0)
                {
                    // Unlike hit with the wall, impact here is a smaller negative fraction of original velocity
                    impact = Vector2.Multiply(impact, -velProjection * (1.0F - softImpactLossFactor));
                    // Note that we do not simply summ impact forces, we take the maximum of each vector component,
                    // because car can hit obstacles with more than one point.
                    posImpact = Vectors.Max(posImpact, impact);
                    negImpact = Vectors.Min(negImpact, impact);

                    //Display("velProjection = %f[impact=(%f,%f)[posImpact=(%f,%f)[negImpact=(%f,%f)", velProjection, 
                    //      impact.X, impact.Y, posImpact.X, posImpact.Y, negImpact.X, negImpact.Y);
                    impact = Vectors.SafeNormalize(Vectors.Negate(impact));
                }
                // And project other body's velocity
                float otherProjection = Vectors.Projection(otherVelocity, impact);
                if (otherProjection < 0.0)
                {
                    Vector2.Multiply(impact, otherProjection * (1.0F - softImpactLossFactor));
                    posImpact = Vectors.Max(posImpact, impact);
                    negImpact = Vectors.Min(negImpact, impact);
                }
            }
            // Finally, sum up positive and negative direction impacts
            Vector2.Add(impactVelocity, posImpact);
            Vector2.Add(impactVelocity, negImpact);

            return impactVelocity;
        }
    }
}
