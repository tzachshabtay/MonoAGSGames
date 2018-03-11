using System.Collections.Generic;
using System.ComponentModel;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Playback rules associated to the media clip tag.
    /// Applied when the audio clip is played in the audio mixer.
    /// </summary>
    /// TODO: is it too much too implement full ISoundProperties?
    public class AudioTagRules : INotifyPropertyChanged
    {
        private float _volume;

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioTagRules()
        {
            _volume = 1f;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public float Volume { get => _volume; set { _volume = MathUtils.Clamp(value, 0, 1); OnPropertyChanged(nameof(Volume)); } }
    }

    public class AudioMixer
    {
        private List<AudioChannel> _channels = new List<AudioChannel>();
        private List<AudioChannel> _utilityChannels = new List<AudioChannel>();
        private IMediaInfoProvider _miProvider = null;
        private Dictionary<string, AudioTagRules> _tagRules = new Dictionary<string, AudioTagRules>();
        private AudioTagRules _commonRules = new AudioTagRules();

        public IReadOnlyList<ILockedAudioChannel> Channels { get => _channels; }
        // TODO: replace by dictionary with changes notifications
        public IReadOnlyDictionary<string, AudioTagRules> TagRules { get => _tagRules; }
        public AudioTagRules CommonRules { get => _commonRules; }
        public IMediaInfoProvider MediaInfoProvider { get => _miProvider; set => _miProvider = value; }

        public AudioMixer(IMediaInfoProvider miProvider, int channelCount)
        {
            _miProvider = miProvider;
            for (int i = 0; i < channelCount; ++i)
                _channels.Add(new AudioChannel(i));
            CommonRules.PropertyChanged += onTagRulesChanged;
        }

        // TODO: remove if dictionary with notifications is used
        public AudioTagRules RegisterTagRules(string tag)
        {
            AudioTagRules set;
            if (!_tagRules.TryGetValue(tag, out set))
            {
                set = new AudioTagRules();
                set.PropertyChanged += onTagRulesChanged;
            }
            return set;
        }

        public bool IsAllowedToPlay(IAudioClip clip)
        {
            AudioChannel chan;
            MediaInfo info;
            return getChannelToPlay(clip, out chan, out info);
        }

        /// <summary>
        /// Plays clip, returns channel if successful.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="shouldLoop"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        /// TODO: return ISound instead?
        public AudioPlayback PlayClip(IAudioClip clip, bool shouldLoop = false, ISoundProperties properties = null)
        {
            AudioChannel chan;
            MediaInfo info;
            if (getChannelToPlay(clip, out chan, out info))
            {
                // If the channel is occupied, stop and dispose current sound
                if (chan.Playback != null)
                    chan.Playback.Stop();
                // TODO: create sound paused, and play only when it's adjusted and assigned?
                AudioPlayback playback = new AudioPlayback(clip.Play(shouldLoop, properties));
                applyTagSettings(playback, info);
                chan.AssignPlayback(playback, clip, info);
            }
            return null;
        }

        /// <summary>
        /// Finds a channel to play a new clip on.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="chan"></param>
        /// <returns></returns>
        private bool getChannelToPlay(IAudioClip clip, out AudioChannel chan, out MediaInfo info)
        {
            if (_miProvider != null)
                info = _miProvider.GetInfo(clip);
            else
                info = null;
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
                    if (ch.PlayInfo != null && ch.PlayInfo.Priority >= info.Priority)
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

        private void applyTagSettings(IPlaybackProperties props, MediaInfo info)
        {
            AudioTagRules sum = new AudioTagRules();
            mergePlaybackProps(sum, _commonRules);
            if (info != null)
            {
                foreach (var t in info.Tags)
                {
                    AudioTagRules rules;
                    if (_tagRules.TryGetValue(t, out rules))
                        mergePlaybackProps(sum, rules);
                }
            }
            applyPlaybackProps(props, sum);
        }

        private void mergePlaybackProps(AudioTagRules sum, AudioTagRules arg)
        {
            sum.Volume *= arg.Volume;
        }

        private void applyPlaybackProps(IPlaybackProperties props, AudioTagRules arg)
        {
            props.VolumeMod = arg.Volume;
        }

        private void onTagRulesChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var chan in _channels)
            {
                if (chan.Playback == null)
                    continue;
                applyTagSettings(chan.PlaybackProperties, chan.PlayInfo);
            }
        }
    }
}
