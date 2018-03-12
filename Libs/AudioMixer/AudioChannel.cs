using System.Collections.Generic;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Interface for a container which only allows content with matching tags.
    /// </summary>
    public interface ITaggableContainer
    {
        /// <summary>
        /// Container's tags.
        /// </summary>
        /// TODO: use simplier container instead, like array?
        ISet<string> Tags { get; }
    }

    /// <summary>
    /// Locked audio channel combines API for read-only audio channel and
    /// a taggable container, which set of tags still may be modified.
    /// </summary>
    public interface ILockedAudioChannel : ITaggableContainer
    {
        /// <summary>
        /// Channel's numeric ID.
        /// </summary>
        int ID { get; }
        /// <summary>
        /// Original sound clip that was used to create a sound instance.
        /// </summary>
        /// TODO: use TaggedClip?
        /// TODO: move or also add to playback?
        IAudioClip Clip { get; }
        /// <summary>
        /// The sound instance which is currently playing on this channel.
        /// </summary>
        IAudioPlayback Playback { get; }
    }

    /// <summary>
    /// Container for sound playbacks. Defines a set of tags, which determine
    /// the kind of clips allowed to be put in it.
    /// </summary>
    /// TODO: tag mode: OR / AND
    public class AudioChannel : ILockedAudioChannel
    {
        readonly int _id;
        readonly HashSet<string> _tags = new HashSet<string>();
        AudioPlayback _playback;
        IAudioClip _clip;
        
        public int ID { get => _id; }
        public ISet<string> Tags { get => _tags; }
        public IAudioPlayback Playback { get => _playback; }
        public IAudioClip Clip { get => _clip; }

        public AudioChannel(int id)
        {
            _id = id;
        }

        public void AssignPlayback(AudioPlayback playback, IAudioClip clip)
        {
            _playback = playback;
            _clip = clip;
        }
    }
}
