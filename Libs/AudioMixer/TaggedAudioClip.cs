using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// An interface that binds IAudioClip with IMediaInfo.
    /// </summary>
    /// TODO: better name.
    public interface ITaggedAudioClip
    {
        IAudioClip Clip { get; }
        IMediaInfo Info { get; }
    }
    
    /// <summary>
    /// Binds audio clip with media info for storage purposes.
    /// </summary>
    public class TaggedAudioClip : ITaggedAudioClip
    {
        public IAudioClip Clip { get; set; }
        public IMediaInfo Info { get; set; }

        // TODO: constructors
    }
}
