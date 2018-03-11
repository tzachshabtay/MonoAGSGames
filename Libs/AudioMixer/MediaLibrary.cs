using System.Collections.Generic;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Default implementation of the IMediaInfoProvider.
    /// </summary>
    public class MediaLibrary : IMediaInfoProvider
    {
        private readonly Dictionary<string, MediaInfo> _infos = new Dictionary<string, MediaInfo>();
        private readonly Dictionary<string, IAudioClip> _clips = new Dictionary<string, IAudioClip>();

        public IDictionary<string, IAudioClip> Clips { get => _clips; }
        public Dictionary<string, MediaInfo> MediaInfo { get => _infos; }

        public void AddClip(IAudioClip clip, params string[] tags)
        {
            Clips.Add(clip.ID, clip);
            MediaInfo info = new MediaInfo();
            _infos.Add(clip.ID, info);
            info.Tags = new HashSet<string>();
            foreach (var t in tags)
                info.Tags.Add(t);
        }

        public MediaInfo GetInfo(IAudioClip clip)
        {
            return GetInfo(clip.ID);
        }

        public MediaInfo GetInfo(string id)
        {
            MediaInfo info = null;
            _infos.TryGetValue(id, out info);
            return info;
        }
    }
}
