using System.Collections.Generic;
using AGS.API;

namespace AudioMixer
{
    public class AudioMixer
    {
        private List<AudioChannel> _channels = new List<AudioChannel>();
        private List<AudioChannel> _utilityChannels = new List<AudioChannel>();
        private IMediaInfoProvider _miProvider = null;

        public IReadOnlyList<ILockedAudioChannel> Channels { get => _channels; }
        public IMediaInfoProvider MediaInfoProvider { get => _miProvider; set => _miProvider = value; }

        public AudioMixer(IMediaInfoProvider miProvider, int channelCount)
        {
            _miProvider = miProvider;
            for (int i = 0; i < channelCount; ++i)
                _channels.Add(new AudioChannel(i));
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
        public ILockedAudioChannel PlayClip(IAudioClip clip, bool shouldLoop = false, ISoundProperties properties = null)
        {
            AudioChannel chan;
            MediaInfo info;
            if (getChannelToPlay(clip, out chan, out info))
            {
                ISound playback = clip.Play(shouldLoop, properties);
                if (playback == null)
                    return null;
                chan.AssignPlayback(playback, info);
                return chan;
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
                // If channel has no tags, this means there are no restrictions
                // to the clips it is allowed to play.
                // If channel has tags, then the clip must come with at least
                // one matching tag.
                if (ch.Tags.Count != 0 && (tags == null || !ch.Tags.Overlaps(tags)))
                    continue;
                // If channel is busy, but played clip has lower priority, then
                // remember it for now, but continue searching.
                if (ch.Playback != null && ch.PlayInfo.Priority < info.Priority)
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
    }
}
