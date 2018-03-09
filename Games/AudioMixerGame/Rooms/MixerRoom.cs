﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AGS.API;
using AGS.Engine;
using AudioMixerLib;

// Music by Eric Matyas
//
// www.soundimage.org

namespace AudioMixerGame
{
    public class MixerRoom : RoomScript
    {
        private const string ROOM_ID = "TitleScreen";
        private readonly MediaLibrary _mlib;
        private readonly AudioMixer _mixer;
        private string[] _clipNames;
        private List<ILabel> _channelInfos;

        public MixerRoom(IGame game) : base(game, ROOM_ID)
        {
            _mlib = new MediaLibrary();
            _mixer = new AudioMixer(_mlib, 4);
            _channelInfos = new List<ILabel>();
        }

        protected void loadClip(string filename)
        {
            IAudioClip clip = _game.Factory.Sound.LoadAudioClip(AMG.MusicAssetFolder + filename, filename);
            _mlib.Clips[clip.ID] = clip;
        }

        protected override async Task<IRoom> loadAsync()
        {
            IGameFactory f = _game.Factory;
            _room = f.Room.GetRoom(ROOM_ID);

            _clipNames = new string[]{
                "City-of-Tomorrow_v001_Looping.ogg",
                "Corporate-Ladder_Looping.ogg",
                "Future-Business_v001.ogg",
                "Great-Minds_v001.ogg",
                "Little-Space-Probe_Very-Big-Journey_Looping.ogg",
                "Network_v001_Looping.ogg",
                "Pride_v002.ogg",
                "Progress.ogg",
                "Sculpture-Garden_Looping.ogg",
                "Young-Visionaries.ogg"
            };

            foreach (var s in _clipNames)
                loadClip(s);

            float xr = _game.Settings.VirtualResolution.Width;
            float yr = _game.Settings.VirtualResolution.Height;
            float x = 20f;
            float y = 20f;
            float w = xr - x * 2;
            float h = 40f;
            float step = 5f;
            foreach (var chan in _mixer.Channels)
            {
                ILabel label = _game.Factory.UI.GetLabel($"ChannelInfo{chan.ID}", "",
                     w, h, x, yr - y - h);
                label.TextConfig.Font = AGSGameSettings.DefaultTextFont;
                label.TextConfig.Alignment = Alignment.MiddleLeft;
                _channelInfos.Add(label);
                y += step + h;
            }

            ILabel copyright = _game.Factory.UI.GetLabel("Copyright", "Credits: Music by Eric Matyas (www.soundimage.org)", 0, h, x, 20f);
            copyright.TextConfig.Font = AGSGameSettings.DefaultTextFont;
            copyright.TextConfig.Alignment = Alignment.MiddleLeft;
            copyright.TextConfig.AutoFit = AutoFit.NoFitting;

            ILabel hint = _game.Factory.UI.GetLabel("Hint", "Help:\n1-9: play clip on the first available channel.", 0, h, x, 240f);
            copyright.TextConfig.Font = AGSGameSettings.DefaultTextFont;
            copyright.TextConfig.Alignment = Alignment.TopLeft;
            copyright.TextConfig.AutoFit = AutoFit.LabelShouldFitText;

            _room.Events.OnBeforeFadeIn.Subscribe(onLoad);
            _room.Events.OnAfterFadeIn.Subscribe(onAfterFadeIn);
            _room.Events.OnAfterFadeOut.Subscribe(onLeave);
            _game.Events.OnRepeatedlyExecute.Subscribe(onRepExec);
            _game.Input.KeyUp.Subscribe(onKeyUp);

            return _room;
        }

        private void onLoad()
        {
        }

        private void onAfterFadeIn()
        {
        }

        private void onLeave()
        {
        }

        private void onRepExec()
        {
            for (int i = 0; i < _mixer.Channels.Count; ++i)
            {
                var chan = _mixer.Channels[i];
                var label = _channelInfos[i];

                StringBuilder sb = new StringBuilder();
                foreach (var s in chan.Tags)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(s);
                }
                string chanText = string.Format("Chan {0}: Tags: {1}", chan.ID, sb.ToString());
                string clipText;
                if (chan?.Playback != null)
                {
                    var play = chan.Playback;
                    var clip = chan.Clip;
                    // TODO: clip position (time??)
                    clipText = string.Format("Playback: {0}; {1}; vol: {2}; pos: {3} / {4}",
                        clip.ID, !play.HasCompleted, play.Volume, makeTimeString(play.Seek), "--");
                }
                else
                {
                    clipText = "Playback: [none]";
                }

                label.Text = string.Format(chanText +" \n" + clipText);
            }
        }

        private void onKeyUp(KeyboardEventArgs args)
        {
            if (args.Key >= Key.Number1 && args.Key <= Key.Number9)
            {
                int num = args.Key - Key.Number1;
                string id = _clipNames[num];
                var clip = _mlib.Clips[id];
                _mixer.PlayClip(clip, true);
            }
        }

        // TODO: to the library helper functions
        private static string makeTimeString(float time)
        {
            float total_time = time;
            int minutes = (int)(total_time / 60.0f);
            int seconds = (int)(total_time - minutes * 60.0f);
            int subsecs = (int)((total_time - minutes * 60.0 - seconds) * 60.0);
            return string.Format("{0,0:D2}:{1,0:D2}:{2,0:D2}", minutes, seconds, subsecs);
        }
    }
}
