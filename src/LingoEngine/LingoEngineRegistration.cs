using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Xtras.BuddyApi;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine
{
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory;
        LingoPlayer Build();
        LingoPlayer Build(IServiceProvider serviceProvider);
    }
    public interface IMovieRegistration
    {
        IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior;
        IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript;
    }
    public static class LingoEngineRegistrationExtensions
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container, Action<ILingoEngineRegistration> config)
        {
            var engineRegistration = new LingoEngineRegistration(container);
            engineRegistration.RegisterCommonServices();
            container.AddSingleton<ILingoEngineRegistration>(engineRegistration);
            return container;
        }
    }
    public class LingoEngineRegistration : ILingoEngineRegistration
    {
        private readonly IServiceCollection _container;
        private readonly Dictionary<string, MovieRegistration> _Movies = new();
        private Action<ILingoFrameworkFactory>? _FrameworkFactorySetup;
        private IServiceProvider? _serviceProvider;

        public LingoEngineRegistration(IServiceCollection container)
        {
            _container = container;
        }
        public void RegisterCommonServices()
        {
            _container
               .AddTransient<LingoSprite>()
               .AddTransient<ILingoMemberFactory, LingoMemberFactory>()
               .AddScoped<ILingoMovieEnvironment, LingoMovieEnvironment>()
               // Xtras
               .AddScoped<IBuddyAPI, BuddyAPI>()
               ;
        }

        public ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class,ILingoFrameworkFactory
        {
            _container.AddSingleton<ILingoFrameworkFactory, T>();
            if (setup != null)
                _FrameworkFactorySetup = f => setup((T)f);
            return this;
        }
     
        

        public LingoPlayer Build()
            => Build(_container.BuildServiceProvider());
        public LingoPlayer Build(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var player = new LingoPlayer(serviceProvider, ActionOnNewMovie);
            if (_FrameworkFactorySetup != null)
                _FrameworkFactorySetup(serviceProvider.GetRequiredService<ILingoFrameworkFactory>());
            return player;
        }

        public ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action)
        {
            var registration = new MovieRegistration(_container, name);
            _Movies.Add(name, registration);
            return this;
        }
        private void ActionOnNewMovie(LingoMovie movie)
        {
            var registration = _Movies[movie.Name];
            var ctor = registration.GetAllMovieScriptsCtors();
            foreach (var item in ctor)
                movie.AddMovieScript(item(_serviceProvider!));
        }

        private class MovieRegistration : IMovieRegistration
        {
            private readonly IServiceCollection _container;
            private readonly string _movieName;
            private readonly List<Func<IServiceProvider, LingoMovieScript>> _MovieScripts = new();

            public MovieRegistration(IServiceCollection container, string movieName)
            {
                _container = container;
                _movieName = movieName;
            }
            public Func<IServiceProvider, LingoMovieScript>[] GetAllMovieScriptsCtors() => _MovieScripts.ToArray();

            public IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior
            {
                _container.AddScoped<T>();
                return this;
            }

            public IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript
            {
                _container.AddScoped<T>();
                _MovieScripts.Add(p => p.GetRequiredService<T>());
                return this;
            }

           
        }
    }
}

