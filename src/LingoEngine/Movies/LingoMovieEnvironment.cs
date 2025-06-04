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
        ILingoClock Clock { get; }
        ILingoFrameworkFactory Factory { get; }

        ILingoCast? GetCastLib(int number);
        ILingoCast? GetCastLib(string name);
        internal ILingoCastLibsContainer CastLibsContainer { get; }
        T? GetMember<T>(int number) where T : class, ILingoMember;
        T? GetMember<T>(string name) where T : class, ILingoMember;
    }
    public class LingoMovieEnvironment : ILingoMovieEnvironment , IDisposable
    {
        private readonly LingoPlayer _player;
        private readonly LingoKey _key;
        private readonly LingoSound _sound;
        private readonly LingoMouse _mouse;
        private readonly LingoSystem _system;
        private readonly ILingoClock _clock;
        private readonly ILingoMovie _movie;
        private readonly ILingoFrameworkFactory _factory;
        private readonly IServiceScope _scopedServiceProvider;
        private readonly LingoCastLibsContainer _castLibsContainer;
        

        public ILingoPlayer Player => _player;

        public ILingoKey Key => _key;

        public ILingoSound Sound => _sound;

        public ILingoMouse Mouse => _mouse;

        public ILingoSystem System => _system;

        public ILingoMovie Movie => _movie;

        public ILingoClock Clock => _clock;

        public ILingoFrameworkFactory Factory => _factory;

        internal LingoMovieEnvironment(string name, int number, LingoPlayer player, LingoKey lingoKey, LingoSound sound, LingoMouse mouse, LingoStage stage, LingoSystem system, ILingoClock clock, ILingoFrameworkFactory factory, LingoCastLibsContainer lingoCastLibsContainer, IServiceProvider rootServiceProvider)
        {
            _player = player;
            _key = lingoKey;
            _sound = sound;
            _mouse = mouse;
            _system = system;
            _clock = clock;
            _factory = factory;
            _castLibsContainer = lingoCastLibsContainer;
            _scopedServiceProvider = rootServiceProvider.CreateScope();
            _movie = new LingoMovie(this, stage, name,number);
        }
        internal IServiceProvider GetServiceProvider() => _scopedServiceProvider.ServiceProvider;
        public void Dispose()
        {
            _scopedServiceProvider.Dispose();
        }

        public ILingoCastLibsContainer CastLibsContainer => _castLibsContainer;
        public ILingoCast? GetCastLib(int number) => _castLibsContainer[number];
        public ILingoCast? GetCastLib(string name) => _castLibsContainer[name];

        T? ILingoMovieEnvironment.GetMember<T>(int number) where T : class => _castLibsContainer.GetMember<T>(number);  

        T? ILingoMovieEnvironment.GetMember<T>(string name) where T : class => _castLibsContainer.GetMember<T>(name);
    }
}
