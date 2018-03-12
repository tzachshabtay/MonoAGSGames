using System.Collections.Generic;
using System.ComponentModel;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Playback rules associated to the media tag.
    /// Applied when the audio clip is played in the audio mixer.
    /// </summary>
    /// TODO: is it too much too implement full ISoundProperties?
    public class AudioRules : INotifyPropertyChanged
    {
        private float _volume;

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioRules()
        {
            _volume = 1f;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public float Volume { get => _volume; set { _volume = MathUtils.Clamp(value, 0, 1); OnPropertyChanged(nameof(Volume)); } }
    }

    /// <summary>
    /// A mixer that plays clips on its channels, obiding media tags and other properties,
    /// associated with tags and particular clips.
    /// </summary>
    public class AudioMixer
    {
        private List<AudioChannel> _channels = new List<AudioChannel>();
        private List<AudioChannel> _utilityChannels = new List<AudioChannel>();
        private Dictionary<string, AudioRules> _tagRules = new Dictionary<string, AudioRules>();
        private AudioRules _masterRules = new AudioRules();

        public IReadOnlyList<ILockedAudioChannel> Channels { get => _channels; }
        // TODO: replace by dictionary with changes notifications
        public IReadOnlyDictionary<string, AudioRules> TagRules { get => _tagRules; }
        public AudioRules MasterRules { get => _masterRules; }

        public AudioMixer(int channelCount)
        {
            for (int i = 0; i < channelCount; ++i)
                _channels.Add(new AudioChannel(i));
            _masterRules.PropertyChanged += onTagRulesChanged;
        }

        // TODO: remove if dictionary with notifications is used
        public AudioRules RegisterRules(string tag)
        {
            AudioRules set;
            if (!_tagRules.TryGetValue(tag, out set))
            {
                set = new AudioRules();
                _tagRules.Add(tag, set);
                set.PropertyChanged += onTagRulesChanged;
            }
            return set;
        }

        public bool IsAllowedToPlay(ITaggedAudioClip clip)
        {
            AudioChannel chan;
            return getChannelToPlay(clip.Info, out chan);
        }

        /// <summary>
        /// Plays clip, returns channel if successful.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="shouldLoop"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        /// TODO: return ISound instead?
        public AudioPlayback PlayClip(ITaggedAudioClip clip, bool shouldLoop = false, ISoundProperties properties = null)
        {
            AudioChannel chan;
            if (getChannelToPlay(clip.Info, out chan))
            {
                // If the channel is occupied, stop and dispose current sound
                if (chan.Playback != null)
                    chan.Playback.Stop();
                // TODO: create sound paused, and play only when it's adjusted and assigned?
                IAudioClip audioClip = clip.Clip;
                IMediaInfo mediaInfo = clip.Info;
                ISound sound = audioClip.Play(shouldLoop, properties);
                if (sound == null)
                    return null;
                AudioPlayback playback = new AudioPlayback(sound, mediaInfo, chan);
                applyTagSettings(playback, mediaInfo);
                chan.AssignPlayback(playback, audioClip);
            }
            return null;
        }

        /// <summary>
        /// Finds a channel to play a new clip on.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="chan"></param>
        /// <returns></returns>
        private bool getChannelToPlay(IMediaInfo info, out AudioChannel chan)
        {
            ISet<string> tags = info?.Tags;
            AudioChannel reserved = null;
            foreach (var ch in _channels)
            {
                bool mayReserve = false;
                if (ch.Playback != null)
                {
                    // If already has one reserved channel and next channel is occupied,
                    // then skip it.
                    if (reserved != null)
                        continue;
                    // If currently playing clip has same or higher priority, then skip.
                    int priorityOld = ch.Playback.MediaInfo != null ? ch.Playback.MediaInfo.Priority : 0;
                    int priorityNew = info != null ? info.Priority : 0;
                    if (priorityOld > priorityNew)
                        continue;
                    // If channel is busy, but played clip has lower priority,
                    // then we may reserve it for now, but continue searching.
                    mayReserve = true;
                }

                // If channel has no tags, this means there are no restrictions
                // to the clips it is allowed to play.
                // If channel has tags, then the clip must come with at least
                // one matching tag.
                if (ch.Tags.Count != 0 && (tags == null || !ch.Tags.Overlaps(tags)))
                    continue;
                
                if (mayReserve)
                {
                    reserved = ch;
                    continue;
                }
                // Found free and matching channel.
                chan = ch;
                return true;
            }

            // No free channel available, but maybe there is one we may free up?
            if (reserved != null)
            {
                chan = reserved;
                return true;
            }
            // No other options, return failure.
            chan = null;
            return false;
        }

        private void applyTagSettings(IPlaybackProperties props, IMediaInfo info)
        {
            AudioRules sum = new AudioRules();
            mergePlaybackProps(sum, _masterRules);
            if (info != null)
            {
                foreach (var t in info.Tags)
                {
                    AudioRules rules;
                    if (_tagRules.TryGetValue(t, out rules))
                        mergePlaybackProps(sum, rules);
                }
            }
            applyPlaybackProps(props, sum);
        }

        private void mergePlaybackProps(AudioRules sum, AudioRules arg)
        {
            sum.Volume *= arg.Volume;
        }

        private void applyPlaybackProps(IPlaybackProperties props, AudioRules arg)
        {
            props.VolumeMod = arg.Volume;
        }

        private void onTagRulesChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var chan in _channels)
            {
                if (chan.Playback == null)
                    continue;
                applyTagSettings(chan.Playback, chan.Playback.MediaInfo);
            }
        }
    }
}
