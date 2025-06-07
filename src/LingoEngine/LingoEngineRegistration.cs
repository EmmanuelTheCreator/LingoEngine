using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Movies;
using LingoEngine.Xtras.BuddyApi;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine
{
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration Services(Action<IServiceCollection> services);
        ILingoEngineRegistration AddFont(string name,string pathAndName);
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
            config(engineRegistration);
            return container;
        }
    }
    public class LingoEngineRegistration : ILingoEngineRegistration
    {
        private readonly IServiceCollection _container;
        private readonly Dictionary<string, MovieRegistration> _Movies = new();
        private readonly List<(string Name, string FileName)> _Fonts = new();
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
               .AddTransient(p => new Lazy<ILingoMemberFactory>(() => p.GetRequiredService<ILingoMemberFactory>()))
               .AddScoped<ILingoMovieEnvironment, LingoMovieEnvironment>()
               // Xtras
               .AddScoped<IBuddyAPI, BuddyAPI>()
               .AddScoped<ILingoEventMediator, LingoEventMediator>()
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
            LoadFonts(serviceProvider);
            var player = new LingoPlayer(serviceProvider, ActionOnNewMovie);
            if (_FrameworkFactorySetup != null)
                _FrameworkFactorySetup(serviceProvider.GetRequiredService<ILingoFrameworkFactory>());
            return player;
        }

        private void LoadFonts(IServiceProvider serviceProvider)
        {
            var fontsManager = serviceProvider.GetRequiredService<ILingoFontManager>();
            foreach (var font in _Fonts)
                fontsManager.AddFont(font.Name, font.FileName);
            fontsManager.LoadAll();
        }

        public ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action)
        {
            var registration = new MovieRegistration(_container, name);
            action(registration);
            _Movies.Add(name, registration);
            return this;
        }
        private void ActionOnNewMovie(LingoMovie movie)
        {
            var registration = _Movies[movie.Name];
            var ctor = registration.GetAllMovieScriptsCtors();
            foreach (var item in ctor)
                item(movie);
        }

        public ILingoEngineRegistration Services(Action<IServiceCollection> services)
        {
            services(_container);
            return this;
        }

        public ILingoEngineRegistration AddFont(string name, string pathAndName)
        {
            _Fonts.Add((name, pathAndName));
            return this;
        }

        private class MovieRegistration : IMovieRegistration
        {
            private readonly IServiceCollection _container;
            private readonly string _movieName;
            private readonly List<Action<LingoMovie>> _MovieScripts = new();

            public MovieRegistration(IServiceCollection container, string movieName)
            {
                _container = container;
                _movieName = movieName;
            }
            public Action<LingoMovie>[] GetAllMovieScriptsCtors() => _MovieScripts.ToArray();

            public IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior
            {
                _container.AddTransient<T>();
                return this;
            }

            public IMovieRegistration AddMovieScript<T>() where T : LingoMovieScript
            {
                _container.AddScoped<T>();
                _MovieScripts.Add(movie => {
                    movie.AddMovieScript<T>();
                    });
                return this;
            }

           
        }
    }
}

