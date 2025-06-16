using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Sounds;
using LingoEngine.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Core
{


    public class LingoPlayer : ILingoPlayer
    {
        private Lazy<CsvImporter> _csvImporter = new Lazy<CsvImporter>(() => new CsvImporter());
        private readonly LingoCastLibsContainer _castLibsContainer;
        private readonly LingoSound _sound;
        private readonly ILingoWindow _window;
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<LingoMovie> _actionOnNewMovie;
        private Dictionary<string, LingoMovieEnvironment> _moviesByName = new();
        private List<LingoMovieEnvironment> _movies = new();


        private readonly LingoKey _LingoKey;
        private readonly LingoMouse _Mouse;
        private readonly LingoStage _Stage;
        private readonly LingoSystem _System;
        private readonly LingoClock _clock;
        public ILingoFrameworkFactory Factory { get; private set; }

        public ILingoClock Clock => _clock;
        public LingoKey Key => _LingoKey;
        public LingoStage Stage => _Stage;
        /// <inheritdoc/>
        public ILingoCast ActiveCastLib => _castLibsContainer.ActiveCast;
        /// <inheritdoc/>
        public ILingoSound Sound => _sound;


        /// <inheritdoc/>
        public int CurrentSpriteNum => 1;
        /// <inheritdoc/>
        public bool NetPreset => true;
        /// <inheritdoc/>
        public bool ActiveWindow => true;
        /// <inheritdoc/>
        public bool SafePlayer => throw new NotImplementedException();
        /// <inheritdoc/>
        public string OrganizationName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ApplicationName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ApplicationPath { get; set; } = string.Empty;
        /// <inheritdoc/>
        public string ProductName { get; set; } = string.Empty;
        /// <inheritdoc/>
        public int LastClick => 1;
        /// <inheritdoc/>
        public int LastEvent => 1;
        /// <inheritdoc/>
        public int LastKey => 1;
        /// <inheritdoc/>
        public Version ProductVersion { get; set; } = new Version(1, 0, 0, 0);
        /// <inheritdoc/>
        public Func<string> AlertHook { get; set; } = () => "";
        /// <inheritdoc/>
        bool ILingoPlayer.SafePlayer { get; set; }
        public ILingoMovie? ActiveMovie { get; private set; }

        internal LingoPlayer(IServiceProvider serviceProvider, Action<LingoMovie> actionOnNewMovie)
        {
            _serviceProvider = serviceProvider;
            _actionOnNewMovie = actionOnNewMovie;
            Factory = serviceProvider.GetRequiredService<ILingoFrameworkFactory>();
            _castLibsContainer = new LingoCastLibsContainer(Factory);
            _sound = Factory.CreateSound(_castLibsContainer);
            _window = new LingoWindow();
            _clock = new LingoClock();
            _LingoKey = Factory.CreateKey();
            _Stage = Factory.CreateStage(this);
            _Mouse = Factory.CreateMouse(_Stage);
            _System = new LingoSystem();
        }


        /// <inheritdoc/>
        public void Alert(string message)
        {
            Console.WriteLine(message);
        }
        /// <inheritdoc/>
        public void AppMinimize()
        {

        }
        /// <inheritdoc/>
        public void Cursor(int cursorNum)
        {
            
        }
        /// <inheritdoc/>
        public void Halt()
        {

        }
        /// <inheritdoc/>
        public void Open(string applicationName)
        {

        }
        /// <inheritdoc/>
        public void Quit()
        {
        }
        /// <inheritdoc/>
        public bool WindowPresent()
        {
            return true;
        }

        public ILingoMovie NewMovie(string name, bool andActivate = true)
        {
            // Create the default cast
            if (_castLibsContainer.Count == 0) 
                _castLibsContainer.AddCast("Internal");

            // Create a new movies scope, needed for behaviours.
            var scope = _serviceProvider.CreateScope();

            // Create the movie.
            var movieEnv = (LingoMovieEnvironment)scope.ServiceProvider.GetRequiredService<ILingoMovieEnvironment>();
            movieEnv.Init(name, _movies.Count + 1, this, _LingoKey, _sound, _Mouse, _Stage, _System, Clock, _castLibsContainer, scope,m =>
            {
                // On remove movie
                var movieEnvironment = m.GetEnvironment();
                _movies.Remove(movieEnvironment);
                _moviesByName.Remove(m.Name);
            });
            var movieTyped = (LingoMovie)movieEnv.Movie;

            // Add him
            _movies.Add(movieEnv);
            _moviesByName.Add(name, movieEnv);

            Factory.AddMovie(_Stage, movieTyped);

            // Add all movieScripts
            _actionOnNewMovie(movieTyped);

            // Activate him;
            if (andActivate)
            {
                ActiveMovie = movieEnv.Movie;
                _Stage.SetActiveMovie(movieTyped);
            }
            return movieEnv.Movie;
        }
        public void CloseMovie(ILingoMovie movie)
        {
            var typed = (LingoMovie)movie;
            typed.RemoveMe();
        }

        /// <summary>
        /// Format: comma split
        ///     Number,Type,Name,Registration Point,Filename
        ///     1,bitmap,BallB,"(5, 5)",
        /// </summary>
        public ILingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv)
        {
            var castLib = _castLibsContainer.AddCast(castlibName);
            _csvImporter.Value.ImportInCastFromCsvFile(castLib, pathAndFilenameToCsv);
            return this;
        }

        public ILingoPlayer AddCastLib(string name, Action<ILingoCast>? configure = null)
        {
            var castLib = _castLibsContainer.AddCast(name);
            if (configure != null)
                configure(castLib);
            return this;
        }

        internal void LoadMovieScripts(IEnumerable<LingoMovieScript> enumerable)
        {
            throw new NotImplementedException();
        }
    }
}



