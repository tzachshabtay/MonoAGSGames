using System;
using System.Collections.Generic;
using AGS.API;
using AGS.Engine;

namespace LastAndFurious
{
    // TODO: redesign into non-static class
    public static class RaceUI
    {
        private const int FramePosX = 2;
        private const int FramePosY = 0;
        private const int FrameSpacing = 53;
        private const int FrameFaceXOff = 2;
        private const int FrameFaceYOff = 2;
        private const int FrameTimeXOff = 40;
        private const int FrameTimeYOff = 22;

        private static IGame _game;
        private static Race _race;
        private static IObject _pane; // main object
        // TODO: an object with multiple (at least two) images combined on it?
        private static IObject[] _frames; // frames with position number
        private static IObject[] _faces; // portraits
        private static IObject[] _driverTimes; // labels to indicate finishing time
        private static IObject _lapLabel;
        private static IObject _timeLabel;

        /// <summary>
        /// Get parent UI object.
        /// </summary>
        public static IObject Pane { get => _pane; }
        public static bool IsShown { get => _pane != null && _pane.Visible; }

        private static IObject makeUIObject(string id, IImage image = null, IImageRenderer customRenderer = null)
        {
            IObject o = _game.Factory.Object.GetObject(id);
            o.IgnoreViewport = true;
            o.Pivot = new PointF(0, 1); // top-left corner
            o.Image = image;
            o.CustomRenderer = customRenderer;
            o.RenderLayer = AGSLayers.UI;
            _game.State.UI.Add(o);
            return o;
        }

        private static IObject addChildObject(string id, IObject parent, IImage image = null, IImageRenderer customRenderer = null)
        {
            IObject o = makeUIObject(id, image, customRenderer);
            parent.TreeNode.AddChild(o);
            o.Z = _pane.Z - 1;
            return o;
        }

        public static void Init(IGame game)
        {
            _game = game;
        }

        private static void createUI()
        {
            // Need to revert Y coordinates for GUI
            int ry = _game.Settings.VirtualResolution.Height;
            _pane = makeUIObject("RaceUI");
            _pane.Pivot = new PointF(0, 0); // bottom-left corner
            _pane.X = 0;
            _pane.Y = 0;

            _frames = new IObject[Race.MAX_RACING_CARS];
            _faces = new IObject[Race.MAX_RACING_CARS];
            _driverTimes = new IObject[Race.MAX_RACING_CARS];
            for (int i = 0; i < Race.MAX_RACING_CARS; i++)
            {
                int x = FramePosX;
                int y = FramePosY + FrameSpacing * i;
                IObject frame = addChildObject($"RacerFrame{i}", _pane, LF.RaceAssets.RacerFrames[i]);
                frame.X = x;
                frame.Y = ry - y;
                _frames[i] = frame;
                IObject o = addChildObject($"RacerFace{i}", frame);
                o.X = FrameFaceXOff;
                o.Y = -FrameFaceYOff + frame.Height;
                _faces[i] = o;
                o = addChildObject($"RacerTime{i}", frame, null, new SpriteFontRenderer(LF.Fonts.AzureItalicFont, null, AGSGame.GLUtils));
                o.X = FrameTimeXOff;
                o.Y = -FrameTimeYOff + frame.Height;
                _driverTimes[i] = o;
            }

            _lapLabel = addChildObject("RaceUI.LapLabel", _pane, null, new SpriteFontRenderer(LF.Fonts.AzureItalicFont, null, AGSGame.GLUtils));
            _lapLabel.X = 3;
            _lapLabel.Y = 22;
            _timeLabel = addChildObject("RaceUI.TimeLabel", _pane, null, new SpriteFontRenderer(LF.Fonts.AzureItalicFont, null, AGSGame.GLUtils));
            _timeLabel.X = 3;
            _timeLabel.Y = 10;
        }

        public static void Show(Race race)
        {
            if (_pane == null)
                createUI();
            _race = race;
            Update();
            _pane.Visible = true;
        }

        public static void Hide()
        {
            if (_pane == null)
                return;
            _pane.Visible = false;
            _race = null;
        }

        private static string makeRacerTime(Racer racer)
        {
            float total_time = racer.Time;
            int minutes = (int)(total_time / 60.0f);
            int seconds = (int)(total_time - minutes * 60.0f);
            int subsecs = (int)((total_time - minutes * 60.0 - seconds) * 60.0);
            return string.Format("{0,0:D2}:{1,0:D2}:{2,0:D2}", minutes, seconds, subsecs);
        }
        
        public static void Update()
        {
            IList<Racer> racers = _race.RacerPositions;

            // Draw portraits
            int pos = 0;
            for (; pos < Race.MAX_RACING_CARS && pos < racers.Count; ++pos)
            {
                var racer = racers[pos];
                if (racer == null)
                {
                    _frames[pos].Visible = false;
                    continue;
                }
                _frames[pos].Visible = true;
                _faces[pos].Image = racer.Driver.Portrait;
                if (racer.Finished > 0)
                    (_driverTimes[pos].CustomRenderer as SpriteFontRenderer).Text = makeRacerTime(racer);
                else
                    (_driverTimes[pos].CustomRenderer as SpriteFontRenderer).Text = "";
            }
            // Hide remaining
            for (; pos < Race.MAX_RACING_CARS; ++pos)
                _frames[pos].Visible = false;

            Racer player = _race.Player;
            (_lapLabel.CustomRenderer as SpriteFontRenderer).Text =
                string.Format("{0,0:D2}/{1,0:D2}", player.Lap, _race.Laps);
            (_timeLabel.CustomRenderer as SpriteFontRenderer).Text =
                makeRacerTime(player);
        }

        public static void Clear()
        {
            if (_pane == null)
                return;

            Hide();
            var ui = _game.State.UI;

            foreach (var o in _frames)
                ui.Remove(o);
            _frames = null;
            foreach (var o in _faces)
                ui.Remove(o);
            _faces = null;
            foreach (var o in _driverTimes)
                ui.Remove(o);
            _driverTimes = null;
            ui.Remove(_lapLabel);
            ui.Remove(_timeLabel);
            _lapLabel = null;
            _timeLabel = null;

            ui.Remove(_pane);
            _pane = null;
        }
    }
}
