using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;
using LingoEngine.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LingoEngine.Core
{
    /// <summary>
    /// Represents the player-level interface for accessing global runtime environment information and control,
    /// such as system settings, UI state, and playback environment. Mirrors Lingo’s player properties.
    /// Represents the core playback engine used to manage and execute the authoring environment, movies in a window(MIAWs), projectors, and Shockwave Playe
    /// </summary>
    public interface ILingoPlayer
    {
        /// <summary>
        /// Indicates which cast library was most recently activated.
        /// Lingo: the activeCastLib
        /// </summary>
        ILingoCast ActiveCastLib { get; }
        ILingoMovie? ActiveMovie { get; }
        /// <summary>
        /// Provides access to the sound system (including channels and control).
        /// Lingo: the sound
        /// </summary>
        ILingoSound Sound { get; }

        /// <summary>
        /// Indicates the sprite channel number of the sprite whose script is currently executing.
        /// Lingo: the currentSpriteNum
        /// </summary>
        int CurrentSpriteNum { get; }

        /// <summary>
        /// Indicates whether network presetting is active.
        /// Lingo: the netPreset
        /// </summary>
        bool NetPreset { get; }

        /// <summary>
        /// Indicates whether the movie window is currently active (focused).
        /// Lingo: the activeWindow
        /// </summary>
        bool ActiveWindow { get; }

        /// <summary>
        /// Controls whether Director's safety features are turned on.
        /// Always true in Shockwave Player.
        /// Lingo: the safePlayer
        /// </summary>
        bool SafePlayer { get; set; }

        /// <summary>
        /// Organization or company name associated with the application.
        /// Lingo: the organizationName
        /// </summary>
        string OrganizationName { get; set; }

        /// <summary>
        /// The name of the executable or primary application.
        /// Lingo: the applicationName
        /// </summary>
        string ApplicationName { get; set; }

        /// <summary>
        /// The absolute file path to the application.
        /// Lingo: the applicationPath
        /// </summary>
        string ApplicationPath { get; set; }

        /// <summary>
        /// The product name of the application or runtime environment.
        /// Lingo: the productName
        /// </summary>
        string ProductName { get; set; }

        /// <summary>
        /// Returns the number of ticks (1 tick = 1/60 sec) since the last mouse click.
        /// Lingo: the lastClick
        /// </summary>
        int LastClick { get; }

        /// <summary>
        /// Returns the number of ticks since the last input event (click, keypress, rollover).
        /// Lingo: the lastEvent
        /// </summary>
        int LastEvent { get; }

        /// <summary>
        /// Returns the number of ticks since the last keypress.
        /// Lingo: the lastKey
        /// </summary>
        int LastKey { get; }

        /// <summary>
        /// Returns or sets the current version of the product runtime.
        /// Lingo: the productVersion
        /// </summary>
        Version ProductVersion { get; set; }

        /// <summary>
        /// Specifies a handler hook (function) to override the default alert display behavior.
        /// Lingo: the alertHook
        /// </summary>
        Func<string> AlertHook { get; set; }

        /// <summary>
        /// Displays a system alert dialog.
        /// Lingo: alert "message"
        /// </summary>
        /// <param name="message">The message to display.</param>
        void Alert(string message);

        /// <summary>
        /// Minimizes the player application window.
        /// Lingo: appMinimize
        /// </summary>
        void AppMinimize();

        /// <summary>
        /// Halts playback immediately.
        /// Lingo: halt
        /// </summary>
        void Halt();

        /// <summary>
        /// Changes the system cursor by number.
        /// Lingo: cursor n
        /// </summary>
        /// <param name="cursorNum">The cursor ID number.</param>
        void Cursor(int cursorNum);

        /// <summary>
        /// Opens the specified application by name or path.
        /// Lingo: open "AppName"
        /// </summary>
        /// <param name="applicationName">The name or path of the application.</param>
        void Open(string applicationName);

        /// <summary>
        /// Exits the runtime application.
        /// Lingo: quit
        /// </summary>
        void Quit();

        /// <summary>
        /// Determines if the movie window is present and active.
        /// Lingo: the windowPresent
        /// </summary>
        /// <returns>True if the window is present; false otherwise.</returns>
        bool WindowPresent();
        ILingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv);
    }


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
            _LingoKey = new LingoKey();
            _Stage = Factory.CreateStage(_clock);
            _Mouse = new LingoMouse(_Stage);
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

        internal void LoadMovieScripts(IEnumerable<LingoMovieScript> enumerable)
        {
            throw new NotImplementedException();
        }
    }
}



