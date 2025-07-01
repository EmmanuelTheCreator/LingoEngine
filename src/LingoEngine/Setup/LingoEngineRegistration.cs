using LingoEngine.Commands;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Sprites;
using LingoEngine.Styles;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine.Setup
{
    public interface ILingoEngineRegistration
    {
        ILingoEngineRegistration Services(Action<IServiceCollection> services);
        ILingoEngineRegistration AddFont(string name, string pathAndName);
        ILingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action);
        ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory;
        ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup);
        LingoPlayer Build();
        LingoPlayer Build(IServiceProvider serviceProvider);
        ILingoEngineRegistration AddBuildAction(Action<IServiceProvider> buildAction);
    }
    public interface IMovieRegistration
    {
        IMovieRegistration AddBehavior<T>() where T : LingoSpriteBehavior;
        IMovieRegistration AddParentScript<T>() where T : LingoParentScript;
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
        private readonly List<Action<IServiceProvider>> _BuildActions = new();
        private Action<ILingoFrameworkFactory>? _FrameworkFactorySetup;
        private IServiceProvider? _serviceProvider;
        private Action<LingoProjectSettings> _projectSettingsSetup = p => { };

        public LingoEngineRegistration(IServiceCollection container)
        {
            _container = container;
        }
        public void RegisterCommonServices()
        {
            _container.WithGodotEngine();
        }

        public ILingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, ILingoFrameworkFactory
        {
            _container.AddSingleton<ILingoFrameworkFactory, T>();
            if (setup != null)
                _FrameworkFactorySetup = f => setup((T)f);
            return this;
        }

        public ILingoEngineRegistration WithProjectSettings(Action<LingoProjectSettings> setup)
        {
            _projectSettingsSetup = setup;
            return this;
        }



        public LingoPlayer Build()
            => Build(_container.BuildServiceProvider());
        public LingoPlayer Build(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _projectSettingsSetup(serviceProvider.GetRequiredService<LingoProjectSettings>());
            LoadFonts(serviceProvider);
            _BuildActions.ForEach(b => b(serviceProvider));
            var player = serviceProvider.GetRequiredService<LingoPlayer>();
            player.SetActionOnNewMovie(ActionOnNewMovie);
            if (_FrameworkFactorySetup != null)
                _FrameworkFactorySetup(serviceProvider.GetRequiredService<ILingoFrameworkFactory>());
            serviceProvider.GetRequiredService<ILingoCommandManager>()
                .DiscoverAndSubscribe(serviceProvider);
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

        public ILingoEngineRegistration AddBuildAction(Action<IServiceProvider> buildAction)
        {
            _BuildActions.Add(buildAction);
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
                _MovieScripts.Add(movie =>
                {
                    movie.AddMovieScript<T>();
                });
                return this;
            }
            public IMovieRegistration AddParentScript<T>() where T : LingoParentScript
            {
                _container.AddTransient<T>();
                return this;
            }


        }
    }
}

