using Godot;
using LingoEngine.Sounds;
using System.IO;

namespace LingoEngineGodot.Sounds
{
    public class LingoGodotSoundChannel : ILingoFrameworkSoundChannel, IDisposable
    {

        private AudioStreamPlayer _audioPlayer;
        private AudioEffectPanner _audioEffectPanner;
        private LingoSoundChannel _lingoSoundChannel;

        public int ChannelNumber { get; private set; }

        public int SampleRate => 44100; // it seems godot can't retriev the sample rate

        public int Volume
        {
            get => (int)(_audioPlayer.VolumeDb * 100); // Volume in dB scale in Godot
            set => _audioPlayer.VolumeDb = value / 100f; // Set the volume in dB
        }

        public int Pan
        {
            get => (int)_audioEffectPanner.Pan * 100; // Pan for stereo left (-1) to right (+1)
            set => _audioEffectPanner.Pan = value / 100f; // Pan from -1 to 1 (range of Godot pan)
        }

        public float CurrentTime
        {
            get => _audioPlayer.GetPlaybackPosition(); // in seconds
            set => _audioPlayer.Seek(value); // Seek in seconds
        }
        /// <inheritdoc/>
        public bool IsPlaying => _audioPlayer.Playing;

        public int ChannelCount => 2;
        public int SampleCount => 0;
        // gives the time, in milliseconds, that the current sound member in a sound channel has been playing.
        public float ElapsedTime => _audioPlayer.GetPlaybackPosition();
        // indicates the start time of the currently playing or paused sound as set when the sound was queued.
        public float StartTime { get; set; } = 0;
        // specifies the end time of the currently playing, paused, or queued sound.
        public float EndTime { get; set; }

        // Constructor: Takes a channel number and initializes the audio player
        public LingoGodotSoundChannel(int channelNumber)
        {
            var busName = "Channel" + channelNumber;
            ChannelNumber = channelNumber;
            _audioPlayer = new AudioStreamPlayer();
            _audioEffectPanner = new AudioEffectPanner();
            _audioPlayer.Finished += _audioPlayer_Finished;
            AudioServer.AddBus(channelNumber);
            AudioServer.SetBusName(channelNumber, busName);
            AudioServer.AddBusEffect(channelNumber, _audioEffectPanner, channelNumber);
            AudioServer.SetBusSend(channelNumber, "Master");
            _audioPlayer.Bus = busName;
        }
        internal void Init(LingoSoundChannel soundChannel)
        {
            _lingoSoundChannel = soundChannel;
        }

        private void _audioPlayer_Finished()
        {
            try
            {
                _lingoSoundChannel.SoundFinished();
            }
            catch
            {
            }
        }

        /// <inheritdoc/>
        public void PlayFile(string stringFilePath)
        {
            var audioStream = GD.Load<AudioStream>(stringFilePath);
            Play(audioStream);
        }

        /// <inheritdoc/>
        public void PlayNow(LingoMemberSound member)
        {
            var stream = member.Framework<LingoGodotMemberSound>().AudioStream;
            if (stream == null) return;
            Play(stream);
        }
        private void Play(AudioStream stream)
        {
            _audioPlayer.Stream = stream;
            EndTime = (float)stream.GetLength();
            _audioPlayer.Seek(0);
            _audioPlayer.Play(StartTime);
        }

        /// <inheritdoc/>
        public void Stop()
        {
            _audioPlayer.Stop();
        }

        /// <inheritdoc/>
        public void Rewind()
        {
            _audioPlayer.Seek(0f);
        }
        /// <inheritdoc/>
        public void Pause()
        {
            _audioPlayer.StreamPaused = !_audioPlayer.StreamPaused;
        }

        /// <inheritdoc/>
        public void Repeat()
        {
            Rewind();
            _audioPlayer.Play(StartTime);
        }

        public void Dispose()
        {
            _audioPlayer.Dispose();
        }
    }

}
