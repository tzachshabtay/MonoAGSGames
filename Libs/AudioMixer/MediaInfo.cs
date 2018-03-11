using System.Collections.Generic;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Custom definition of a media clip.
    /// </summary>
    /// TODO: a readonly interface.
    /// TODO: create Tags set in constructor?
    /// TODO: Is whole set for each single media clip too much?
    public class MediaInfo
    {
        /// <summary>
        /// Priority specifies how important it is to play the clip.
        /// When there is a lack of media channels, they may be freed
        /// if the new requested clip has higher priority than the one
        /// that is currently playing.
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// A set of custom tags associated with the media clip.
        /// Could be used to setup particular rules on how the clip
        /// is used (for example, whether it is allowed to be played
        /// on particular channel).
        /// </summary>
        public ISet<string> Tags { get; set; }
    }

    /// <summary>
    /// Provides custom definition for the media clips.
    /// </summary>
    public interface IMediaInfoProvider
    {
        /// <summary>
        /// Returns media info associates with the given audio clip.
        /// </summary>
        /// <param name="clip"></param>
        /// <returns></returns>
        /// TODO: return readonly media info.
        MediaInfo GetInfo(IAudioClip clip);
    }
}
