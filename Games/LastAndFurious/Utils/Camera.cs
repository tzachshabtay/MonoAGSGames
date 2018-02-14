using System;
using AGS.API;

namespace LastAndFurious
{
    public class Camera : ICamera
    {
        private float _targettingAcceleration;
        private float _speedX, _speedY, _speedScaleX, _speedScaleY;
        private float _oldTargetX, _oldTargetY;
        private bool _doSnap;

        /// <summary>
        /// Linear acceleration of the camera movement when snapping to the target (0 for instant snap).
        /// </summary>
        public float TargettingAcceleration { get => _targettingAcceleration; set => _targettingAcceleration = value; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AGSCamera"/> class.
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="startSpeedScale">Start speed scale (in percentage, i.e 30 is 30% percent zoom toward the target).</param>
        public Camera(float acceleration = 0f, float startSpeedScale = 8f)
        {
            Enabled = true;
            _targettingAcceleration = 0F;
            StartSpeedScale = startSpeedScale;
            SpeedScaleX = StartSpeedScale;
            SpeedScaleY = StartSpeedScale;
        }

        public bool Enabled { get; set; }
        
        public float StartSpeedScale { get; set; }

        public float SpeedX { get => _speedX; set => _speedX = value; }
        public float SpeedY { get => _speedY; set => _speedY = value; }
        public float SpeedScaleX { get => _speedScaleX; set => _speedScaleX = value; }
        public float SpeedScaleY { get => _speedScaleY; set => _speedScaleY = value; }

        #region ICamera implementation

        public void Tick(IViewport viewport, RectangleF roomLimits, Size virtualResoution, bool resetPosition)
        {
            IObject target = Target == null ? null : Target();
            if (!Enabled || target == null)
                return;

            setScale(target, viewport, resetPosition);

            //todo: Allow control over which point in the target to follow
            float targetX = target.X;//target.CenterPoint == null ? target.X : target.CenterPoint.X;
            float targetY = target.Y;//target.CenterPoint == null ? target.Y : target.CenterPoint.Y;
            float maxResolutionX = virtualResoution.Width / viewport.ScaleX;
            float maxResolutionY = virtualResoution.Height / viewport.ScaleY;
            targetX = getTargetPos(targetX, roomLimits.X, roomLimits.Width, maxResolutionX);
            targetY = getTargetPos(targetY, roomLimits.Y, roomLimits.Height, maxResolutionY);
            if (resetPosition)
            {
                viewport.X = targetX;
                viewport.Y = targetY;
                return;
            }


            float newX, newY;
            if (_doSnap ||
                MathUtils.FloatEquals(viewport.X, _oldTargetX) && MathUtils.FloatEquals(viewport.Y, _oldTargetY) ||
                MathUtils.FloatEquals(_targettingAcceleration, 0.0F))
            {
                // Already snapped to target, or no acceleration, - directly snap to the target
                newX = targetX;
                newY = targetY;
            }
            else
            {
                doMove(viewport, targetX, targetY, _targettingAcceleration, out newX, out newY);
            }

            _oldTargetX = targetX;
            _oldTargetY = targetY;
            _doSnap = false;

            viewport.X = clamp(newX, roomLimits.X, roomLimits.Width, maxResolutionX);
            viewport.Y = clamp(newY, roomLimits.Y, roomLimits.Height, maxResolutionY);
        }

        public Func<IObject> Target { get; set; }

        public void Snap()
        {
            _doSnap = true;
        }

        private void setScale(IObject target, IViewport viewport, bool resetPosition)
        {
            float scale = getTargetZoom(target, viewport);
            if (resetPosition)
            {
                viewport.ScaleX = scale;
                viewport.ScaleY = scale;
                return;
            }
            float newScaleX = getPos(viewport.ScaleX, scale, StartSpeedScale, 0.001f, ref _speedScaleX);
            float newScaleY = getPos(viewport.ScaleY, scale, StartSpeedScale, 0.001f, ref _speedScaleY);
            viewport.ScaleX = newScaleX;
            viewport.ScaleY = newScaleY;
        }

        private float getTargetZoom(IObject target, IViewport viewport)
        {
            if (target.Room == null || target.Room.Areas == null)
                return viewport.ScaleX;

            foreach (var area in target.Room.GetMatchingAreas(target.Location.XY, target.ID))
            {
                var zoomArea = area.GetComponent<IZoomArea>();
                if (zoomArea == null || !zoomArea.ZoomCamera)
                    continue;
                float scale = zoomArea.GetZoom(target.Y);
                return scale;
            }

            return 1;
        }

        private float getTargetPos(float target, float minRoom, float maxRoom, float maxResolution)
        {
            float value = target - maxResolution / 2;
            return clamp(value, minRoom, maxRoom, maxResolution);
        }

        private float clamp(float target, float minRoom, float maxRoom, float maxResolution)
        {
            float max = Math.Max(minRoom, maxRoom - maxResolution);
            return MathUtils.Clamp(target, minRoom, max);
        }

        private float getPos(float source, float target, float defaultSpeed, float minDistance, ref float speed)
        {
            float distance = Math.Abs(target - source);
            if (distance <= minDistance)
            {
                speed = defaultSpeed;
                return target;
            }

            float offset = speed / 100f * distance;
            if (target > source)
                source += offset;
            else
                source -= offset;
            if (offset > 2)
                speed *= (95f / 100f);
            return source;
        }

        private void doMove(IViewport viewport, float x, float y, float accel, out float cameraX, out float cameraY)
        {
            cameraX = viewport.X;
            cameraY = viewport.Y;
            // Check if the target is in the snap range
            float dist = (float)Math.Sqrt(Math.Pow(x - cameraX, 2.0F) + Math.Pow(y - cameraY, 2.0F));
            if (MathUtils.FloatEquals(dist, 0.0F))
            {
                // So close, quick-snap
                cameraX = x;
                cameraY = y;
            }
            else
            {
                // Accelerate and move towards target
                // TODO: proper vector logic here
                float dirx = 1.0F;
                float diry = 1.0F;
                if (x < cameraX)
                    dirx = -1.0F;
                if (y < cameraY)
                    diry = -1.0F;

                // If direction changed suddenly then reset speed
                if (_speedX < 0.0 && dirx >= 0.0 || _speedX > 0.0 && dirx < 0.0)
                    _speedX = 0.0F;
                if (_speedY < 0.0 && diry >= 0.0 || _speedY > 0.0 && diry < 0.0)
                    _speedY = 0.0F;

                // Do camera movement
                if (accel > 0.0)
                {
                    _speedX += accel * dirx;
                    _speedY += accel * diry;
                }

                // Do not let the camera fly over the target
                if (dirx >= 0.0 && cameraX + _speedX > x ||
                    dirx <= 0.0 && cameraX + _speedX < x)
                    cameraX = x;
                else
                    cameraX += _speedX;
                if (diry >= 0.0 && cameraY + _speedY > y ||
                    diry <= 0.0 && cameraY + _speedY < y)
                    cameraY = y;
                else
                    cameraY += _speedY;
            }
        }

        #endregion
    }
}
