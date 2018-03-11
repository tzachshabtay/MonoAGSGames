using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Simple sound properties implementation.
    /// </summary>
    public class SoundProperties : ISoundProperties
    {
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public float Panning { get; set; }

        public SoundProperties()
        {
            Volume = 1f;
            Pitch = 1;
            Panning = 0f;
        }

        public SoundProperties(ISoundProperties props)
        {
            Volume = props.Volume;
            Pitch = props.Pitch;
            Panning = props.Panning;
        }
    }
}
