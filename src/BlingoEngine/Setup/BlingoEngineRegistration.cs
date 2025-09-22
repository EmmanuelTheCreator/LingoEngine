using AbstUI;
using AbstUI.Commands;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Windowing;
using BlingoEngine.Casts;
using BlingoEngine.Casts.Commands;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Scripts;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Commands;
using BlingoEngine.Projects;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Commands;
using BlingoEngine.Members.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Xml.Linq;

namespace BlingoEngine.Setup
{
    public class BlingoEngineRegistration : IBlingoEngineRegistration
    {
        private readonly IServiceCollection _container;
        private readonly BlingoProxyServiceCollection _proxy;
        private readonly Dictionary<string, MovieRegistration> _movies = new();
        private readonly List<(string Name, AbstFontStyle Style, string FileName)> _fonts = new();
        private readonly List<Action<IServiceProvider>> _prebuildActions = new();
        private readonly List<Action<IBlingoServiceProvider>> _buildActions = new();
        private readonly List<Action<IServiceProvider>> _postBuildActions = new();
        private Action<IBlingoFrameworkFactory>? _frameworkFactorySetup;
        private IServiceProvider? _serviceProvider;
        private readonly IBlingoServiceProvider _blingoServiceProvider;
        private Action<BlingoProjectSettings> _projectSettingsSetup = p => { };
        private bool _hasBeenBuild = false;
        private BlingoPlayer? _player;
        private Func<IBlingoProjectFactory>? _makeFactoryMethod;
        private IBlingoProjectFactory? _projectFactory;
        private IBlingoMovie? _startupMovie;
        public IBlingoServiceProvider ServiceProvider => _blingoServiceProvider;

        public BlingoEngineRegistration(IServiceCollection container, IBlingoServiceProvider blingoServiceProvider)
        {
            _container = container;
            _proxy = new BlingoProxyServiceCollection(container);
            _blingoServiceProvider = blingoServiceProvider;
        }

        public void UnloadMovie(string? preserveNamespaceFragment = null)
        {
            if (_player?.ActiveMovie is BlingoMovie active)
            {
                if (active.IsPlaying)
                    active.Halt();
                _player.CloseMovie(active);
                _player.SetActiveMovie(null);
            }

            _player?.UnloadInternalCastLibs();

            _proxy.UnregisterMovie(preserveNamespaceFragment);
            _movies.Clear();
            _fonts.Clear();
            _buildActions.Clear();

            if (_serviceProvider != null)
            {
                var cmdManager = _blingoServiceProvider.GetService<IAbstCommandManager>();
                cmdManager?.Clear(preserveNamespaceFragment);
                var eventMediator = _blingoServiceProvider.GetService<IBlingoEventMediator>();
                eventMediator?.Clear(preserveNamespaceFragment);
            }
        }

        public void RegisterCommonServices()
        {
            _container.WithBlingoEngineBase();
        }

        public IBlingoEngineRegistration WithFrameworkFactory<T>(Action<T>? setup = null) where T : class, IBlingoFrameworkFactory
        {
            _container.AddSingleton<IBlingoFrameworkFactory, T>();
            if (setup != null)
                _frameworkFactorySetup = f => setup((T)f);
            return this;
        }

        public IBlingoEngineRegistration WithProjectSettings(Action<BlingoProjectSettings> setup)
        {
            _projectSettingsSetup = setup;
            return this;
        }

        public IBlingoEngineRegistration WithGlobalVarsR<TGlobalVars>(Action<TGlobalVars>? setup = null) where TGlobalVars : BlingoGlobalVars
        {
            _container.RemoveAll<BlingoGlobalVars>();
            _container.AddSingleton(sp =>
            {
                var globals = ActivatorUtilities.CreateInstance<TGlobalVars>(sp);
                setup?.Invoke(globals);
                return globals;
            });
            _container.AddSingleton<BlingoGlobalVars>(sp => sp.GetRequiredService<TGlobalVars>());
            return this;
        }
        public IBlingoEngineRegistration WithGlobalVarsDefault() => WithGlobalVars<DefaultGlobalVars>();
        public IBlingoEngineRegistration WithGlobalVars<TGlobalVars>(Action<TGlobalVars>? setup = null) where TGlobalVars : BlingoGlobalVars, new()
        {
            _container.RemoveAll<BlingoGlobalVars>();
            _container.AddSingleton(sp =>
            {
                var globals = new TGlobalVars();
                setup?.Invoke(globals);
                return new TGlobalVars();
            });
            if (typeof(TGlobalVars) == typeof(BlingoGlobalVars))
                _container.AddSingleton<BlingoGlobalVars>();
            else
                _container.AddSingleton<BlingoGlobalVars>(sp => sp.GetRequiredService<TGlobalVars>());
            return this;
        }

        public IBlingoEngineRegistration BuildDelayed()
        {
            if (_hasBeenBuild && _player != null) return this;
            CreateProjectFactory();
            return this;
        }

        public BlingoPlayer Build() => BuildAsync().GetAwaiter().GetResult();

        public async Task<BlingoPlayer> BuildAsync()
        {
            if (_hasBeenBuild && _player != null) return _player;
            CreateProjectFactory();
            EnsureGlobalVars();
            _serviceProvider = _container.BuildServiceProvider();
            return await BuildAsync(_serviceProvider);
        }

        public BlingoPlayer Build(IServiceProvider serviceProvider, bool allowInitializeProject = true)
            => BuildAsync(serviceProvider, allowInitializeProject).GetAwaiter().GetResult();

        public async Task<BlingoPlayer> BuildAsync(IServiceProvider serviceProvider, bool allowInitializeProject = true)
        {
            _serviceProvider = serviceProvider;
            _blingoServiceProvider.SetServiceProvider(_serviceProvider);
            foreach (var preBuild in _prebuildActions)
                preBuild(serviceProvider);
            BlingoPlayer player;
            try
            {
                player = _blingoServiceProvider.GetRequiredService<BlingoPlayer>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error building BlingoPlayer: " + ex.Message+":"+ex.StackTrace);
                throw;
            }
            
            player.SetActionOnNewMovie(ActionOnNewMovie);
            if (_frameworkFactorySetup != null)
                _frameworkFactorySetup(_blingoServiceProvider.GetRequiredService<IBlingoFrameworkFactory>());
            _player = player;

            ApplyProjectSettings();
            if (allowInitializeProject)
                await InitializeProjectAsync();
            else
                FinalyzeBuild();
            foreach (var postAction in _postBuildActions)
                postAction(serviceProvider);
            _hasBeenBuild = true;
            return player;
        }

        public IBlingoProjectFactory BuildAndRunProject(Action<IServiceProvider>? afterStart = null)
        {
            Build();
            return RunProject(afterStart);
        }

        public IBlingoProjectFactory RunProject(Action<IServiceProvider>? afterStart = null)
        {
            if (_projectFactory == null) throw new InvalidOperationException("Project factory has not been set up. Use AddProjectFactory<TBlingoProjectFactory>() to set it up. and run Build first");
            if (_startupMovie != null)
                _projectFactory.Run(_startupMovie, !BlingoEngineGlobal.IsRunningDirector);
            if (afterStart != null && _serviceProvider != null) afterStart(_serviceProvider);
            return _projectFactory;
        }

        public IBlingoEngineRegistration ForMovie(string name, Action<IMovieRegistration> action)
        {
            var registration = new MovieRegistration(_proxy, name);
            action(registration);
            _movies.Add(name, registration);
            return this;
        }

        private void ActionOnNewMovie(BlingoMovie movie)
        {
            if (!_movies.TryGetValue(movie.Name, out var registration))
            {
                registration = new MovieRegistration(_proxy, movie.Name);
                _movies.Add(movie.Name, registration);
            }
            else
                registration = _movies[movie.Name];
            var ctor = registration.GetAllMovieScriptsCtors();
            foreach (var item in ctor)
                item(movie);
        }

        public IBlingoEngineRegistration ServicesMain(Action<IServiceCollection> services)
        {
            services(_container);
            return this;
        }

        public IBlingoEngineRegistration ServicesBlingo(Action<IServiceCollection> services)
        {
            services(_proxy);
            return this;
        }

        public IBlingoEngineRegistration AddFont(string name, string pathAndName, AbstFontStyle style = AbstFontStyle.Regular)
        {
            _fonts.Add((name, style, pathAndName));
            return this;
        }

        public IBlingoEngineRegistration AddBuildAction(Action<IBlingoServiceProvider> buildAction)
        {
            _buildActions.Add(buildAction);
            return this;
        }
        public IBlingoEngineRegistration AddPreBuildAction(Action<IServiceProvider> buildAction)
        {
            _prebuildActions.Add(buildAction);
            return this;
        }
        public IBlingoEngineRegistration AddPostBuildAction(Action<IServiceProvider> buildAction)
        {
            _postBuildActions.Add(buildAction);
            return this;
        }

        public IBlingoEngineRegistration RegisterWindows(Action<IAbstFameworkWindowRegistrator>? windowRegistrations)
        {
            if (windowRegistrations == null) return this;
            AbstUISetup.WithAbstUI(windowRegistrations);
            return this;
        }
        public IBlingoEngineRegistration RegisterComponents(Action<IAbstFameworkComponentRegistrator>? componentRegistrations)
        {
            if (componentRegistrations == null) return this;
            AbstUISetup.WithAbstUI(componentRegistrations);
            return this;
        }

        #region Projects

        public IBlingoEngineRegistration SetTheProjectFactory(Type factoryType)
        {
            var method = typeof(IBlingoEngineRegistration).GetMethod(nameof(SetProjectFactory), Type.EmptyTypes);
            var genericMethod = method!.MakeGenericMethod(factoryType);
            genericMethod.Invoke(this, null);
            return this;
        }

        public IBlingoEngineRegistration SetProjectFactory<TBlingoProjectFactory>() where TBlingoProjectFactory : IBlingoProjectFactory, new()
        {
            if (_makeFactoryMethod != null)
            {
                // there was already a project loaded, so unload the previous project
                UnloadMovie();
            }

            _makeFactoryMethod = () =>
            {
                var factory = new TBlingoProjectFactory();
                factory.Setup(this);
                return factory;
            };

            if (_hasBeenBuild && _serviceProvider != null)
            {
                CreateProjectFactory();
                _serviceProvider = _container.BuildServiceProvider();
                _blingoServiceProvider.SetServiceProvider(_serviceProvider);
                ApplyProjectSettings();
                InitializeProject();
            }

            return this;
        }

        private void CreateProjectFactory()
        {
            if (_makeFactoryMethod == null)
                return;
            _projectFactory = _makeFactoryMethod();
        }

        private void EnsureGlobalVars()
        {
            if (!_container.Any(s => s.ServiceType == typeof(BlingoGlobalVars)))
            {
                WithGlobalVars<BlingoGlobalVars>();
            }
        }

        private void ApplyProjectSettings()
        {
            if (_serviceProvider == null || _player == null)
                return;

            var settings = _serviceProvider.GetRequiredService<BlingoProjectSettings>();
            _projectSettingsSetup(settings);

            if (settings.StageWidth > 0 && settings.StageHeight > 0)
            {
                var stageWidth = Math.Min(settings.StageWidth, 800);
                var stageHeight = Math.Min(settings.StageHeight, 600);
                _player.Stage.Width = settings.StageWidth;
                _player.Stage.Height = settings.StageHeight;
                if (!BlingoEngineGlobal.IsRunningDirector)
                {
                    var mainWindow = _serviceProvider.GetService<AbstMainWindow>();
                    mainWindow?.SetSize(new APoint(stageWidth, stageHeight));
                }
            }
        }

        public void InitializeProject() => InitializeProjectAsync().GetAwaiter().GetResult();

        public async Task InitializeProjectAsync()
        {
            if (_projectFactory == null || _serviceProvider == null || _player == null)
                return;
            FinalyzeBuild();
            //    .DiscoverAndSubscribe(_blingoServiceProvider, typeof(BlingoEngineRegistration).Assembly);< -slows down startup a lot
            await _projectFactory.LoadCastLibsAsync(_player.CastLibs, _player);
            _startupMovie = await _projectFactory.LoadStartupMovieAsync(_blingoServiceProvider, _player);
        }

        public void FinalyzeBuild()
        {
            LoadFonts(_blingoServiceProvider);
            _buildActions.ForEach(b => b(_blingoServiceProvider));
            _blingoServiceProvider.GetRequiredService<IAbstCommandManager>()
                 .Register<BlingoPlayer, MovieRewindCommand>()
                 .Register<BlingoPlayer, PlayMovieCommand>()
                 .Register<BlingoPlayer, StepFrameCommand>()

                 .Register<BlingoFrameLabelManager, DeleteFrameLabelCommand>()
                 .Register<BlingoFrameLabelManager, SetFrameLabelCommand>()
                 .Register<BlingoFrameLabelManager, AddFrameLabelCommand>()
                 .Register<BlingoFrameLabelManager, UpdateFrameLabelCommand>()
                 .Register<BlingoSpriteCommandHandler, BlingoUpdateSpritePropertiesCommand>()
                 .Register<BlingoMemberCommandHandler, BlingoUpdateMemberPropertiesCommand>()
                 .Register<BlingoCastCommandHandler, BlingoUpdateCastPropertiesCommand>()
                 ;
        }

        private void LoadFonts(IBlingoServiceProvider serviceProvider)
        {
            var fontsManager = serviceProvider.GetRequiredService<IAbstFontManager>();
            foreach (var font in _fonts)
                fontsManager.AddFont(font.Name, font.FileName, font.Style);
            fontsManager.LoadAll();
        }


        #endregion


        private class MovieRegistration : IMovieRegistration
        {
            private readonly IServiceCollection _container;
            private readonly string _movieName;
            private readonly List<Action<BlingoMovie>> _movieScripts = new();

            public MovieRegistration(IServiceCollection container, string movieName)
            {
                _container = container;
                _movieName = movieName;
            }

            public Action<BlingoMovie>[] GetAllMovieScriptsCtors() => _movieScripts.ToArray();

            public IMovieRegistration AddBehavior<T>() where T : BlingoSpriteBehavior
            {
                _container.AddTransient<T>();
                return this;
            }

            public IMovieRegistration AddMovieScript<T>() where T : BlingoMovieScript
            {
                _container.AddScoped<T>();
                _movieScripts.Add(movie =>
                {
                    movie.AddMovieScript<T>();
                });
                return this;
            }

            public IMovieRegistration AddParentScript<T>() where T : BlingoParentScript
            {
                _container.AddTransient<T>();
                return this;
            }
        }
    }
}


