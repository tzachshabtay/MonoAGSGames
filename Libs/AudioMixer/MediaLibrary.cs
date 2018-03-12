using System.Collections.Generic;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Default implementation of the IMediaInfoProvider.
    /// </summary>
    public class MediaLibrary
    {
        private readonly Dictionary<string, ITaggedAudioClip> _clips = new Dictionary<string, ITaggedAudioClip>();

        public IReadOnlyDictionary<string, ITaggedAudioClip> Clips { get => _clips; }

        public void AddClip(IAudioClip clip, params string[] tags)
        {
            TaggedAudioClip taggedClip = new TaggedAudioClip();
            taggedClip.Clip = clip;
            MediaInfo info = new MediaInfo();
            info.Tags = new HashSet<string>();
            foreach (var t in tags)
                info.Tags.Add(t);
            taggedClip.Info = info;
            _clips.Add(clip.ID, taggedClip);
        }
    }
}
