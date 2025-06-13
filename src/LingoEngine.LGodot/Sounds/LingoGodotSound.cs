using Godot;
using LingoEngine.Sounds;

namespace LingoEngine.LGodot.Sounds
{
    public class LingoGodotSound : ILingoFrameworkSound
    {
        private LingoSound _lingoSound;
        private List<LingoSoundDevice> _soundDeviceList = new List<LingoSoundDevice>();

        // Property to get or set the sound devices list
        public List<LingoSoundDevice> SoundDeviceList => _soundDeviceList;

        // Property to get or set the sound level (volume control)
        public int SoundLevel
        {
            get => (int)(AudioServer.GetBusVolumeDb(0) * 100);  // Assume the main bus (0) is used for sound
            set => AudioServer.SetBusVolumeDb(0, value / 100f); // Set the volume in dB
        }

        // Property to enable or disable sound
        public bool SoundEnable
        {
            get => AudioServer.IsBusMute(0); // Check if the main bus is muted
            set => AudioServer.SetBusMute(0, !value); // Set the mute state based on the value
        }

        // Initialize with the LingoSound object (for managing channels)
        internal void Init(LingoSound sound)
        {
            _lingoSound = sound;

        }

        // Beep functionality - just calls the system beep
        public void Beep()
        {
            Console.Beep();
        }

        // Get a specific channel by its number
        public LingoSoundChannel Channel(int channelNumber)
        {
            return _lingoSound.Channel(channelNumber);
        }

        // Update device list - Can be expanded for custom logic
        public void UpdateDeviceList()
        {
            // Implementation based on your specific requirements, perhaps refreshing the list of sound devices
        }

    }
}
