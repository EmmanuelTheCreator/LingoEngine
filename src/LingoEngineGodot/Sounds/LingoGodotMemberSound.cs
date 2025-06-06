using Godot;
using LingoEngine.Sounds;

namespace LingoEngineGodot.Sounds
{
    public class LingoGodotMemberSound : ILingoFrameworkMemberSound, IDisposable
    {
        private LingoMemberSound _lingoMemberSound;
        public bool IsLoaded { get; private set; }
        internal AudioStream? AudioStream { get; private set; }
        /// <summary>
        ///  length of this audio stream, in seconds.
        /// </summary>
        public double Length { get; private set; }
        public string StreamName { get; private set; } = "";

        public bool Stereo { get; private set; }

#pragma warning disable CS8618 
        public LingoGodotMemberSound()
#pragma warning restore CS8618 
        {
            
        }

        internal void Init(LingoMemberSound memberSound)
        {
            _lingoMemberSound = memberSound;
            
            //_lingoMemberSound.CreationDate = new FileInfo(Format).LastWriteTime;
        }

        public void Dispose()
        {
            Unload();
        }

        public void CopyToClipBoard()
        {
        }

        public void Erase()
        {
        }

        public void ImportFileInto()
        {
        }

        public void PasteClipBoardInto()
        {
        }

        public void Preload()
        {
            if (IsLoaded) return;
            IsLoaded = true;
            AudioStream = GD.Load<AudioStream>($"res://{_lingoMemberSound.FileName}");
            Length = AudioStream.GetLength();
            StreamName = AudioStream._GetStreamName();
            Stereo = !AudioStream.IsMonophonic();
            _lingoMemberSound.Size = Convert.ToInt32(AudioStream._GetLength() * 44100);
        }

        public void Unload()
        {
            if (!IsLoaded) return;
            _lingoMemberSound.Size = 0;
            StreamName = "";
            Length = 0;
            AudioStream?.Dispose();
            IsLoaded = false;
        }
    }
}
