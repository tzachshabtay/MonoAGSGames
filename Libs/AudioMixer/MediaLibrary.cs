using System.Collections.Generic;
using AGS.API;

namespace AudioMixer
{
    /// <summary>
    /// Default implementation of the IMediaInfoProvider.
    /// </summary>
    public class MediaLibrary : IMediaInfoProvider
    {
        private Dictionary<string, MediaInfo> _infos = new Dictionary<string, MediaInfo>();

        public Dictionary<string, MediaInfo> MediaInfo { get => _infos; }

        public MediaInfo GetInfo(IAudioClip clip)
        {
            MediaInfo info = null;
            _infos.TryGetValue(clip.ID, out info);
            return info;
        }
    }
}
