using System.Collections.Generic;

namespace AudioMixerLib
{
    public interface IMediaInfo
    {
        /// <summary>
        /// Priority specifies how important it is to play the clip.
        /// When there is a lack of media channels, they may be freed
        /// if the new requested clip has higher priority than the one
        /// that is currently playing.
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// A set of custom tags associated with the media clip.
        /// Could be used to setup particular rules on how the clip
        /// is used (for example, whether it is allowed to be played
        /// on particular channel).
        /// </summary>
        ISet<string> Tags { get; }
    }

    /// <summary>
    /// Custom definition of a media clip.
    /// </summary>
    /// TODO: create Tags set in constructor?
    /// TODO: Is whole set for each single media clip too much?
    public class MediaInfo : IMediaInfo
    {
        public int Priority { get; set; }
        public ISet<string> Tags { get; set; }

        // TODO: constructors
    }

    /// <summary>
    /// Provides custom definition for the media clips.
    /// </summary>
    public interface IMediaInfoProvider
    {
        /// <summary>
        /// Returns media info associates with the given audio clip.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// TODO: return readonly media info.
        IMediaInfo GetInfo(string id);
    }
}
