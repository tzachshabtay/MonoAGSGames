using System.ComponentModel;
using AGS.API;

namespace AudioMixerLib
{
    /// <summary>
    /// Rules associated to the media tag. Define both the conditions
    /// under which the clip may be played by the mixer, and the
    /// sound properties.
    /// </summary>
    /// TODO: is it too much too have full ISoundProperties?
    public interface IAudioRules
    {
        /// <summary>
        /// Priority determines which clip to play when there is not
        /// enough spare channels left. Higher priority means that
        /// the clip is more likely to keep playing or replace
        /// one of the currently playing ones in its channel.
        /// </summary>
        int Priority { get; set; }
        /// <summary>
        /// Volume property acts as an additional adjustment to the
        /// playback's own volume. It is applied multiplicatively.
        /// </summary>
        float Volume { get; set; }
    }

    /// <summary>
    /// Simple container that stores audio rules.
    /// </summary>
    public class AudioRules : IAudioRules
    {
        public int Priority { get; set; }
        public float Volume { get; set; }

        public AudioRules()
        {
            Priority = 0;
            Volume = 1f;
        }
    }
    
    /// <summary>
    /// A controller class that implements audio rules and notifies about their changes.
    /// </summary>
    public class AudioControl : IAudioRules, INotifyPropertyChanged
    {
        private int _priority;
        private float _volume;

        public event PropertyChangedEventHandler PropertyChanged;

        public AudioControl()
        {
            _priority = 0;
            _volume = 1f;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Priority { get => _priority; set { _priority = value; OnPropertyChanged(nameof(Priority)); } }
        public float Volume { get => _volume; set { _volume = MathUtils.Clamp(value, 0, 1); OnPropertyChanged(nameof(Volume)); } }
    }
}
