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

        #region Volume
        public int Volume
        {
            get => GodotToLingoVolume(_audioPlayer.VolumeDb); // Volume in dB scale in Godot
            set => _audioPlayer.VolumeDb = LingoToGodotVolume(value); // Set the volume in dB
        }

        float LingoToGodotVolume(int lingoVolume)
        {
            if (lingoVolume <= 0) return -80f; // silent
            if (lingoVolume >= 255) return 0f; // full volume

            // Linear to decibel conversion
            float normalized = lingoVolume / 255f;
            return (float)(Math.Log10(normalized) * 20.0);
        }
        int GodotToLingoVolume(float db)
        {
            if (db <= -80f) return 0;      // silent in Godot
            if (db >= 0f) return 255;      // full volume

            double normalized = Math.Pow(10.0, db / 20.0);
            return (int)Math.Clamp(normalized * 255.0, 0.0, 255.0);
        } 
        #endregion
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
        public LingoGodotSoundChannel(int channelNumber, Node parentNode)
        {
            var busName = "ChannelBus " + channelNumber;
            ChannelNumber = channelNumber;
            _audioPlayer = new AudioStreamPlayer();
            _audioPlayer.Name = "Channel " + channelNumber;
            _audioEffectPanner = new AudioEffectPanner();
            _audioPlayer.Finished += _audioPlayer_Finished;
            parentNode.AddChild(_audioPlayer);
            
            //AudioServer.AddBus(channelNumber);
            //AudioServer.SetBusName(channelNumber, busName);
            //AudioServer.AddBusEffect(channelNumber, _audioEffectPanner, channelNumber);
            //AudioServer.SetBusSend(channelNumber, "Master");
            //_audioPlayer.Bus = busName;
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
