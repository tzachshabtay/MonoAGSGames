using System.Threading.Tasks;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// An interface which expands playback properties for our special needs.
    /// </summary>
    public interface IPlaybackProperties : ISoundProperties
    {
        /// <summary>
        /// Tells real volume, which may be a combination of parameters.
        /// </summary>
        float RealVolume { get; }
        /// <summary>
        /// A multiplier for the sound's volume. May be used when you need
        /// to keep original volume unchanged for the reference.
        /// </summary>
        float VolumeMod { get; set; }
    }

    /// <summary>
    /// A proxy wrapper around actual ISound.
    /// Required to control composite volume setting. This is necessary, because sound
    /// volume may be set as a combination of playback's own volume and other modifiers,
    /// such as category's master volume (e.g. music volume setting) and temporary
    /// sound volume reduction or mute.
    /// </summary>
    /// TODO: look for better solution?
    public class AudioPlayback : ISound, IPlaybackProperties
    {
        private readonly ISound _sound;
        private float _ownVolume;
        private float _volumeMod;

        public AudioPlayback(ISound sound)
        {
            _sound = sound;
            _ownVolume = sound.Volume;
            _volumeMod = 1f;
        }

        public float RealVolume { get => _sound.Volume; }

        public float VolumeMod
        {
            get => _volumeMod;
            set
            {
                _volumeMod = value;
                _sound.Volume = _ownVolume * _volumeMod;
            }
        }

        public float Volume
        {
            get => _ownVolume;
            set
            {
                _ownVolume = value;
                _sound.Volume = _ownVolume * _volumeMod;
            }
        }

        public float Pitch { get => _sound.Pitch; set => _sound.Pitch = value; }
        public float Panning { get => _sound.Panning; set => _sound.Panning = value; }

        public int SourceID { get => _sound.SourceID; }
        public bool IsValid { get => _sound.IsValid; }
        public bool IsPaused { get => _sound.IsPaused; }
        public bool IsLooping { get => _sound.IsLooping; }
        public bool HasCompleted { get => _sound.HasCompleted; }
        public Task Completed { get => _sound.Completed; }
        public float Seek { get => _sound.Seek; set => _sound.Seek = value; }
        public void Pause() { _sound.Pause(); }
        public void Resume() { _sound.Resume(); }
        public void Rewind() { _sound.Rewind(); }
        public void Stop() { _sound.Stop(); }
    }
}
