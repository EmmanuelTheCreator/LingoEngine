using Godot;
using LingoEngine.Sounds;

namespace LingoEngineGodot.Sounds
{
    public class LingoGodotMemberSound : ILingoFrameworkMemberSound, IDisposable
    {
        private LingoMemberSound _lingoMemberSound;

        internal AudioStream? AudioStream { get; private set; }
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length { get; private set; }
        public string StreamName { get; private set; } = "";

        public bool Stereo { get; private set; }

        internal void Init(LingoMemberSound memberSound)
        {
            _lingoMemberSound = memberSound;
            AudioStream = GD.Load<AudioStream>($"res://{memberSound.FileName}");
            Length = AudioStream.GetLength();
            StreamName = AudioStream._GetStreamName();
            Stereo = !AudioStream.IsMonophonic();
            _lingoMemberSound.Size = Convert.ToInt32(AudioStream._GetLength() * 44100);
            //_lingoMemberSound.CreationDate = new FileInfo(Format).LastWriteTime;
        }

        public void Dispose()
        {
            AudioStream?.Dispose();
        }
    }
}
