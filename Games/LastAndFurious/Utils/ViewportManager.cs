using System;
using System.Collections.Generic;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    /// <summary>
    /// Class manages creation and positioning of secondary viewports on screen.
    /// Viewports are placed in a grid-like fashion, in even rows and columns.
    /// </summary>
    public class ViewportManager
    {
        IGameState _gs;
        float _defCameraAccel;
        List<IObject> _cameraTargets;
        IObject _mainCamTarget;

        public ViewportManager(IGameState gs, float defCameraAccel = 0f)
        {
            _gs = gs;
            _defCameraAccel = defCameraAccel;
            _cameraTargets = new List<IObject>();

            Camera mainCam = new Camera(defCameraAccel);
            mainCam.Target = () => { return _mainCamTarget; };
            MainViewport.Camera = mainCam;
        }

        public IViewport MainViewport { get => _gs.Viewport; }
        public Camera MainCamera { get => _gs.Viewport.Camera as Camera; }
        public IObject MainCameraTarget { get => _mainCamTarget; set => _mainCamTarget = value; }

        public int ViewportCount { get => _gs.SecondaryViewports.Count; }

        public IViewport GetViewport(int index)
        {
            return index >= 0 && index < _gs.SecondaryViewports.Count ? _gs.SecondaryViewports[index] : null;
        }

        public Camera GetCamera(int index)
        {
            IViewport view = GetViewport(index);
            return view?.Camera as Camera;
        }

        public IList<IObject> CameraTargets { get => _cameraTargets; }

        /// <summary>
        /// Resets to single viewport.
        /// Intended to be called before disposing of the current ViewportManager.
        /// </summary>
        public void ResetToSingleViewport()
        {
            _gs.SecondaryViewports.Clear();
            _cameraTargets.Clear();
            MainViewport.DisplayListSettings.DisplayRoom = true;
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
            MainViewport.DisplayListSettings.DisplayRoom = false;
            return view;
        }

        private IViewport addView()
        {
            IViewport v = new AGSViewport(new AGSDisplayListSettings(), new Camera(_defCameraAccel));
            v.RoomProvider = _gs;
            v.DisplayListSettings.DisplayGUIs = false;
            int camIndex = _cameraTargets.Count;
            _cameraTargets.Add(null);
            v.Camera.Target = () => { return _cameraTargets[camIndex]; };
            return v;
        }

        private void positionViewports()
        {
            var all = _gs.SecondaryViewports;
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
