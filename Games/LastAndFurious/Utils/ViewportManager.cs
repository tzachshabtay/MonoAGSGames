using System;
using System.Collections.Generic;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// Class manages creation and positioning of multiple viewports on screen.
    /// Viewports are placed in a grid-like fashion, in even rows and columns.
    /// </summary>
    public class ViewportManager
    {
        IGameState _gs;
        float _defCameraAccel;
        List<IViewport> _allViews;
        List<IObject> _cameraTargets;

        public ViewportManager(IGameState gs, float defCameraAccel = 0f)
        {
            _gs = gs;
            _defCameraAccel = defCameraAccel;
            _allViews = new List<IViewport>();
            _cameraTargets = new List<IObject>();

            addView(_gs.Viewport);
        }

        public int ViewportCount { get => _allViews.Count; }

        public IViewport GetViewport(int index)
        {
            return index >= 0 && index < _allViews.Count ? _allViews[index] : null;
        }

        public Camera GetCamera(int index)
        {
            IViewport view = GetViewport(index);
            return view?.Camera as Camera;
        }

        public IList<IObject> CameraTargets { get => _cameraTargets; }

        /// <summary>
        /// Resets to single viewport and default engine's camera.
        /// Intended to be called before disposing of the current ViewportManager.
        /// </summary>
        public void ResetToDefaultViewport()
        {
            _allViews.Clear();
            _cameraTargets.Clear();
            _gs.SecondaryViewports.Clear();
            _gs.Viewport.ProjectionBox = new RectangleF(0, 0, 1, 1);
            _gs.Viewport.Camera = new AGSCamera();
        }

        /// <summary>
        /// Resets to single viewport.
        /// </summary>
        public void ResetToSingleView()
        {
            _allViews.RemoveRange(1, _allViews.Count - 1);
            _cameraTargets.RemoveRange(1, _cameraTargets.Count - 1);
            _gs.SecondaryViewports.Clear();
            positionViewports();
        }

        /// <summary>
        /// Adds another viewport and repositions existing viewports.
        /// </summary>
        /// <returns>New viewport</returns>
        public IViewport AddViewport()
        {
            IViewport view = addView();
            _gs.SecondaryViewports.Add(view);
            positionViewports();
            return view;
        }

        private IViewport addView(IViewport v = null)
        {
            if (v == null)
            {
                v = new AGSViewport(new AGSDisplayListSettings(), new Camera(_defCameraAccel));
                v.RoomProvider = _gs;
            }
            else
            {
                v.Camera = new Camera(_defCameraAccel);
            }
            _allViews.Add(v);
            int camIndex = _cameraTargets.Count;
            _cameraTargets.Add(null);
            v.Camera.Target = () => { return _cameraTargets[camIndex]; };
            return v;
        }

        private void positionViewports()
        {
            var all = _allViews;
            int count = all.Count;
            int rows = (int)Math.Round(Math.Sqrt(count));
            int columns = (int)Math.Ceiling((float)count / rows);
            float sizex = 1f / columns;
            float sizey = 1f / rows;
            for (int row = 0, v = 0; row < rows; ++row)
            {
                for (int col = 0; col < columns && v < count; ++col, ++v)
                {
                    all[v].ProjectionBox = new RectangleF(col * sizex, 1f - (row + 1) * sizey, sizex, sizey);
                }
            }
        }
    }
}
