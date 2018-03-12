using System;
using System.Collections.Generic;
using System.ComponentModel;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// A mixer that plays clips on its channels, obiding media tags and other properties,
    /// associated with tags and particular clips.
    /// </summary>
    public class AudioMixer
    {
        private List<AudioChannel> _channels = new List<AudioChannel>();
        private List<AudioChannel> _utilityChannels = new List<AudioChannel>();
        private Dictionary<string, IAudioRules> _tagRules = new Dictionary<string, IAudioRules>();
        private AudioControl _masterRules = new AudioControl();

        public IReadOnlyList<ILockedAudioChannel> Channels { get => _channels; }
        // TODO: replace by dictionary with changes notifications
        public IReadOnlyDictionary<string, IAudioRules> TagRules { get => _tagRules; }
        public IAudioRules MasterRules { get => _masterRules; }

        public AudioMixer(int channelCount)
        {
            for (int i = 0; i < channelCount; ++i)
                _channels.Add(new AudioChannel(i));
            _masterRules.PropertyChanged += onTagRulesChanged;
        }

        // TODO: remove if dictionary with notifications is used
        public IAudioRules RegisterRules(string tag)
        {
            IAudioRules rules;
            if (!_tagRules.TryGetValue(tag, out rules))
            {
                AudioControl ctrl = new AudioControl();
                ctrl.PropertyChanged += onTagRulesChanged;
                _tagRules.Add(tag, ctrl);
                rules = ctrl;
            }
            return rules;
        }

        /// <summary>
        /// Tells if the given clip has a right to be played by the mixer.
        /// This method only tests the permissions, it does not check if
        /// the mixer will be able to play this clip in exact time, because
        /// there may not be a spare channel.
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        public bool IsAllowedToPlay(ITaggedAudioClip clip)
        {
            ISet<string> tags = clip.Info?.Tags;
            foreach (var ch in _channels)
            {
                if (ch.Tags.Count == 0 || (tags != null && ch.Tags.Overlaps(tags)))
                    return true;
            }
            return false;
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
            IAudioClip audioClip = clip.Clip;
            IMediaInfo mediaInfo = clip.Info;
            AudioChannel chan;
            AudioRules rules;
            if (getChannelToPlay(mediaInfo, out chan, out rules))
            {
                // If the channel is occupied, stop and dispose current sound
                if (chan.Playback != null)
                    chan.Playback.Stop();
                // TODO: create sound paused, and play only when it's adjusted and assigned?
                ISound sound = audioClip.Play(shouldLoop, properties);
                if (sound == null)
                    return null;
                AudioPlayback playback = new AudioPlayback(sound, mediaInfo, chan);
                applyPlaybackProps(playback, rules);
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
        private bool getChannelToPlay(IMediaInfo info, out AudioChannel chan, out AudioRules rules)
        {
            ISet<string> tags = info?.Tags;
            AudioChannel reserved = null;
            rules = sumUpRules(info);
            foreach (var ch in _channels)
            {
                // If the channel is occupied, see if we may reserve it for overriding.
                bool mayReserve = false;
                if (ch.Playback != null)
                {
                    // If already has one reserved channel then skip it.
                    if (reserved != null)
                        continue;
                    IMediaInfo oldInfo = ch.Playback.MediaInfo;
                    int priorityOld = sumUpPriority(oldInfo);
                    // First compare priorities from tag rules.
                    if (priorityOld > rules.Priority)
                        continue;
                    // If tag priorities are equal, then compare individual clip priorities.
                    if (priorityOld == rules.Priority && oldInfo != null)
                    {
                        priorityOld = oldInfo.Priority;
                        int priorityNew = info != null ? info.Priority : 0;
                        if (priorityOld > priorityNew)
                            continue;
                    }
                    
                    // If currently played clip has lower priority,
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

        private AudioRules sumUpRules(IMediaInfo info)
        {
            AudioRules sum = new AudioRules();
            mergeRules(sum, _masterRules);
            if (info != null)
            {
                IAudioRules rules;
                foreach (var t in info.Tags)
                {
                    if (_tagRules.TryGetValue(t, out rules))
                        mergeRules(sum, rules);
                }
            }
            return sum;
        }

        private int sumUpPriority(IMediaInfo info)
        {
            int priority = _masterRules.Priority;
            if (info != null)
            {
                IAudioRules rules;
                foreach (var t in info.Tags)
                {
                    if (_tagRules.TryGetValue(t, out rules))
                        priority = Math.Max(priority, rules.Priority);
                }
            }
            return priority;
        }

        private void mergeRules(IAudioRules sum, IAudioRules arg)
        {
            sum.Priority = Math.Max(sum.Priority, arg.Priority);
            sum.Volume *= arg.Volume;
        }

        private void applyPlaybackProps(IPlaybackProperties props, IAudioRules arg)
        {
            props.VolumeMod = arg.Volume;
        }

        private void onTagRulesChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var chan in _channels)
            {
                if (chan.Playback == null)
                    continue;
                AudioRules rules = sumUpRules(chan.Playback.MediaInfo);
                applyPlaybackProps(chan.Playback, rules);
            }
        }
    }
}
