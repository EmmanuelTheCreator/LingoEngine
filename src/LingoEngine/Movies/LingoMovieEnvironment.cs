using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Sounds;
using Microsoft.Extensions.DependencyInjection;

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

        ILingoCast? GetCastLib(int number);
    }
    public class LingoMovieEnvironment : ILingoMovieEnvironment , IDisposable
    {
        private readonly LingoPlayer _player;
        private readonly LingoKey _key;
        private readonly LingoSound _sound;
        private readonly LingoMouse _mouse;
        private readonly LingoCast _internalCast;
        private readonly LingoSystem _system;
        private readonly ILingoClock _clock;
        private readonly ILingoMovie _movie;
        private readonly ILingoFrameworkFactory _factory;
        private readonly IServiceScope _scopedServiceProvider;
        public ILingoPlayer Player => _player;

        public ILingoKey Key => _key;

        public ILingoSound Sound => _sound;

        public ILingoMouse Mouse => _mouse;

        public ILingoSystem System => _system;

        public ILingoMovie Movie => _movie;

        public ILingoCast CastLib => _internalCast;
        public ILingoClock Clock => _clock;

        public ILingoFrameworkFactory Factory => _factory;

        internal LingoMovieEnvironment(string name, int number, LingoPlayer player, LingoKey lingoKey, LingoSound sound, LingoMouse mouse, LingoStage stage, LingoCast internalCast, LingoSystem system, ILingoClock clock, ILingoFrameworkFactory factory, IServiceProvider rootServiceProvider)
        {
            _player = player;
            _key = lingoKey;
            _sound = sound;
            _mouse = mouse;
            _internalCast = internalCast;
            _system = system;
            _clock = clock;
            _factory = factory;
            _scopedServiceProvider = rootServiceProvider.CreateScope();
            var score = new LingoScore(this, stage, name, number);
            _movie = new LingoMovie(this, score, stage, internalCast);
        }
        internal IServiceProvider GetServiceProvider() => _scopedServiceProvider.ServiceProvider;
        public void Dispose()
        {
            _scopedServiceProvider.Dispose();
        }
    }
}
