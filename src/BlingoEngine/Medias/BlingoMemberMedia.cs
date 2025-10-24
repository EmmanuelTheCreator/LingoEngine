using BlingoEngine.Casts;
using BlingoEngine.Members;
using AbstUI.Primitives;

namespace BlingoEngine.Medias
{
    /// <summary>
    /// Base class for video media cast members.
    /// </summary>
    public class BlingoMemberMedia : BlingoMember
    {
        private readonly IBlingoFrameworkMemberMedia _frameworkMedia;

        public int Duration => _frameworkMedia.Duration;
        public int CurrentTime
        {
            get => _frameworkMedia.CurrentTime;
            set
            {
                if (_frameworkMedia.CurrentTime == value)
                    return;
                _frameworkMedia.CurrentTime = value;
                OnPropertyChanged();
            }
        }
        public BlingoMediaStatus MediaStatus => _frameworkMedia.MediaStatus;

        private bool _playVideo;
        public bool PlayVideo
        {
            get => _playVideo;
            set => SetProperty(ref _playVideo, value);
        }

        private bool _playAudio;
        public bool PlayAudio
        {
            get => _playAudio;
            set => SetProperty(ref _playAudio, value);
        }

        private bool _startPause;
        public bool StartPause
        {
            get => _startPause;
            set => SetProperty(ref _startPause, value);
        }

        private bool _enableLoop;
        public bool EnableLoop
        {
            get => _enableLoop;
            set => SetProperty(ref _enableLoop, value);
        }

        private int _startValueMs;
        public int StartValueMs
        {
            get => _startValueMs;
            set => SetProperty(ref _startValueMs, value);
        }

        private int _videoFps;
        public int VideoFps
        {
            get => _videoFps;
            set => SetProperty(ref _videoFps, value);
        }

        private float _durationSeconds;
        public float DurationSeconds
        {
            get => _durationSeconds;
            set => SetProperty(ref _durationSeconds, value);
        }

        private string _linkedFileName = string.Empty;
        public string LinkedFileName
        {
            get => _linkedFileName;
            set => SetProperty(ref _linkedFileName, value);
        }

        private string _linkedFolder = string.Empty;
        public string LinkedFolder
        {
            get => _linkedFolder;
            set => SetProperty(ref _linkedFolder, value);
        }

        public BlingoMemberMedia(IBlingoFrameworkMemberMedia frameworkMember, BlingoMemberType type, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
            : base(frameworkMember, type, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkMedia = frameworkMember;
        }

        public T Framework<T>() where T : class, IBlingoFrameworkMemberMedia => (T)_frameworkMedia;

        public void Play() => _frameworkMedia.Play();
        public void Pause() => _frameworkMedia.Pause();
        public void Stop() => _frameworkMedia.Stop();
        public void Seek(int milliseconds) => _frameworkMedia.Seek(milliseconds);
    }
}

