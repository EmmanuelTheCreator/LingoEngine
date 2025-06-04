using LingoEngine.FrameworkCommunication;
using LingoEngine.Sounds;

namespace LingoEngine.Movies
{
    public interface ILingoMovieEnvironment
    {
        ILingoPlayer Player { get; }
        ILingoKey Key { get; }
        ILingoSound Sound { get; }
        ILingoMouse Mouse { get; }
        ILingoSystem System { get; }
        ILingoMovie Movie { get; }
        ILingoCast CastLib { get; }
        ILingoClock Clock { get; }
        ILingoFrameworkFactory Factory { get; }
    }
    public class LingoMovieEnvironment : ILingoMovieEnvironment
    {
        private readonly LingoPlayer _player;
        private readonly LingoKey _LingoKey;
        private readonly LingoSound _Sound;
        private readonly LingoMouse _Mouse;
        private readonly LingoCast _InternalCast;
        private readonly LingoSystem _System;
        private readonly ILingoClock _clock;
        private readonly ILingoMovie _Movie;
        private readonly ILingoFrameworkFactory _Factory;

        public ILingoPlayer Player => _player;

        public ILingoKey Key => _LingoKey;

        public ILingoSound Sound => _Sound;

        public ILingoMouse Mouse => _Mouse;

        public ILingoSystem System => _System;

        public ILingoMovie Movie => _Movie;

        public ILingoCast CastLib => _InternalCast;
        public ILingoClock Clock => _clock;

        public ILingoFrameworkFactory Factory => _Factory;

        internal LingoMovieEnvironment(string name, int number, LingoPlayer player, LingoKey lingoKey, LingoSound sound, LingoMouse mouse, LingoMovieStage stage, LingoCast internalCast, LingoSystem system, ILingoClock clock, ILingoFrameworkFactory factory)
        {
            _player = player;
            _LingoKey = lingoKey;
            _Sound = sound;
            _Mouse = mouse;
            _InternalCast = internalCast;
            _System = system;
            _clock = clock;
            _Factory = factory;
            var score = new LingoScore(this, stage, name, number);
            _Movie = new LingoMovie(this, score, stage, internalCast);
        }
    }
}
