using System.Collections.Generic;
using AGS.API;

namespace AudioMixer
{
    /// <summary>
    /// Audio channel, which only allows to read played clip instance, but not set a new one.
    /// </summary>
    public interface IReadOnlyAudioChannel
    {
        ISound Playback { get; }
    }

    /// <summary>
    /// Interface for a container which only allows content with matching tags.
    /// </summary>
    public interface ITaggableContainer
    {
        ISet<string> Tags { get; }
    }

    /// <summary>
    /// Locked audio channel combines API for read-only audio channel and
    /// a taggable container, which set of tags still may be modified.
    /// </summary>
    public interface ILockedAudioChannel : IReadOnlyAudioChannel, ITaggableContainer
    {
    }

    /// <summary>
    /// Container for sound playbacks. Defines a set of tags, which determine
    /// the kind of clips allowed to be put in it.
    /// </summary>
    public class AudioChannel : ILockedAudioChannel
    {
        readonly int _id;
        readonly HashSet<string> _tags = new HashSet<string>();
        ISound _playback;
        MediaInfo _playinfo;

        /// <summary>
        /// Channel's numeric ID.
        /// </summary>
        public int ID { get => _id; }
        /// <summary>
        /// Channel's tags.
        /// </summary>
        public ISet<string> Tags { get => _tags; }

        /// <summary>
        /// The sound instance which is currently playing on this channel.
        /// </summary>
        public ISound Playback { get => _playback; }
        /// <summary>
        /// The media info of the currently playing sound.
        /// </summary>
        public MediaInfo PlayInfo { get => _playinfo; }

        public AudioChannel(int id)
        {
            _id = id;
        }

        public void AssignPlayback(ISound playback, MediaInfo info)
        {
            _playback = playback;
            _playinfo = info;
        }
    }
}
