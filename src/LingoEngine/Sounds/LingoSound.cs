using LingoEngine.Movies;

namespace LingoEngine.Sounds
{
    public class LingoSoundDevice
    {

    }
    /// <summary>
    /// Represents the sound system in a Lingo movie, including device settings and channel access.
    /// Lingo equivalent: the sound
    /// </summary>
    public interface ILingoSound
    {
        /// <summary>
        /// Controls the master volume level for all sound output (0–100).
        /// Lingo: the soundLevel
        /// </summary>
        int SoundLevel { get; set; }

        /// <summary>
        /// Determines whether Flash cast members mix their audio with sounds in the Score sound channels.
        /// Lingo: the soundMixMedia
        /// </summary>
        int SoundMixMedia { get; set; }

        /// <summary>
        /// Enables or disables sound output globally.
        /// Lingo: the soundEnable
        /// </summary>
        bool SoundEnable { get; set; }

        /// <summary>
        /// For Windows only. Controls whether the sound driver is kept loaded between sounds.
        /// Default is true. Set to false if you want to release the sound device for other applications.
        /// Lingo: the soundKeepDevice
        /// </summary>
        bool SoundKeepDevice { get; set; }

        /// <summary>
        /// Specifies the active sound output device.
        /// Possible values are items from SoundDeviceList.
        /// Lingo: the soundDevice
        /// </summary>
        LingoSoundDevice SoundDevice { get; set; }

        /// <summary>
        /// A list of available sound output devices on the system.
        /// Lingo: the soundDeviceList
        /// </summary>
        List<LingoSoundDevice> SoundDeviceList { get; }

        /// <summary>
        /// Plays a system beep.
        /// Lingo: beep
        /// </summary>
        void Beep();

        /// <summary>
        /// Returns a reference to the specified sound channel.
        /// Lingo: sound channel x
        /// </summary>
        /// <param name="channelNumber">The number of the channel to access (1–8).</param>
        /// <returns>The requested sound channel, or null if unavailable.</returns>
        LingoSoundChannel? Channel(int channelNumber);
        bool SoundBusy(int channelNumber);
        void PuppetSound(int channelNumber, string memberName, float startTime = -1, float endTime = -1, int loopCount = -1, float loopStartTime = -1, float loopEndTime = -1, float preloadTime = -1);
        void StopAll();
    }


    /// <inheritdoc/>
    public class LingoSound : ILingoSound
    {
        private Dictionary<int, LingoSoundChannel> _Channels = new();
        private int numberOfSoundChannels = 8;
        private readonly ILingoMovieEnvironment _lingoEnvironment;
        private readonly ILingoFrameworkSound _frameworkSound;
        public T FrameworkObj<T>() where T : ILingoFrameworkSound => (T)_frameworkSound;

        /// <inheritdoc/>
        public LingoSoundDevice SoundDevice { get; set; } = new LingoSoundDevice();

        /// <inheritdoc/>
        public List<LingoSoundDevice> SoundDeviceList { get => _frameworkSound.SoundDeviceList; }

        /// <inheritdoc/>
        public int SoundLevel { get => _frameworkSound.SoundLevel; set => _frameworkSound.SoundLevel = value; }

        /// <inheritdoc/>
        public int SoundMixMedia { get; set; } // ignore in .NET, there is no flash

        /// <inheritdoc/>
        public bool SoundEnable { get => _frameworkSound.SoundEnable; set => _frameworkSound.SoundEnable = value; }

        /// <inheritdoc/>
        public bool SoundKeepDevice { get; set; } // ingored in .NET

        /// <inheritdoc/>
        public LingoSound(ILingoFrameworkSound frameworkSound, ILingoMovieEnvironment lingoEnvironment)
        {
            _lingoEnvironment = lingoEnvironment;
            _frameworkSound = frameworkSound;
            for (int i = 0; i < numberOfSoundChannels; i++)
            {
                var channel = lingoEnvironment.Factory.CreateSoundChannel(i+1);
                _Channels.Add(i + 1, channel);
            }
        }

        /// <inheritdoc/>
        public void Beep() => _frameworkSound.Beep();

        /// <inheritdoc/>
        public LingoSoundChannel Channel(int channelNumber)
        {
            if (channelNumber < 1 || channelNumber > numberOfSoundChannels)
                throw new ArgumentOutOfRangeException(nameof(channelNumber), "Channel number must be between 1 and " + numberOfSoundChannels);
            return _Channels[channelNumber];
        }


        public void StopAll()
        {
            for (int i = 0; i < numberOfSoundChannels; i++)
            {
                _Channels[i].Stop();
            }
        }

        public bool SoundBusy(int number) => _Channels[number - 1].IsBusy();
        public void PuppetSound(int channelNumber, string memberName, float startTime = -1, float endTime = -1, int loopCount = -1, float loopStartTime = -1, float loopEndTime = -1, float preloadTime = -1)
        {
            var member = _lingoEnvironment.CastLib.GetMember(memberName) as LingoMemberSound;
            if (member == null) return;
            _Channels[channelNumber].Play(member, startTime,endTime, loopCount, loopStartTime, loopEndTime,preloadTime);
        }
    }


    /// <summary>
    /// Sound Channel property; indicates the status of a sound channel. Read-only.
    /// </summary>
    public enum LingoSoundChannelStatus
    {
        /// <summary>
        /// No sounds are queued or playing.
        /// </summary>
        Idle = 0,
        /// <summary>
        /// A queued sound is being preloaded but is not yet playing.
        /// </summary>
        Loading = 1,
        /// <summary>
        /// The sound channel has finished preloading a queued sound but is not yet playing the sound.
        /// </summary>
        Queued = 2,
        /// <summary>
        /// A sound is playing.
        /// </summary>
        Playing = 3,
        /// <summary>
        /// Paused A sound is paused.
        /// </summary>
        Paused = 4,
    }


}
