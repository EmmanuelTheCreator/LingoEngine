using System;
using System.Threading.Tasks;
using BlingoEngine.Casts;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Sounds;
using BlingoEngine.Stages;

namespace BlingoEngine.Core
{
    /// <summary>
    /// Represents the player-level interface for accessing global runtime environment information and control,
    /// such as system settings, UI state, and playback environment. Mirrors Lingo’s player properties.
    /// Represents the core playback engine used to manage and execute the authoring environment, movies in a window(MIAWs), projectors, and Shockwave Playe
    /// </summary>
    public interface IBlingoPlayer
    {
        /// <summary>
        /// Indicates which cast library was most recently activated.
        /// Lingo: the activeCastLib
        /// </summary>
        IBlingoCast ActiveCastLib { get; }
        IBlingoMovie? ActiveMovie { get; }
        /// <summary>
        /// Raised when <see cref="ActiveMovie"/> changes.
        /// </summary>
        event Action<IBlingoMovie?> ActiveMovieChanged;
        /// <summary>
        /// Sets the active movie.
        /// </summary>
        /// <param name="movie">Movie to make active.</param>
        void SetActiveMovie(IBlingoMovie? movie);
        /// <summary>
        /// Provides access to the sound system (including channels and control).
        /// Lingo: the sound
        /// </summary>
        IBlingoSound Sound { get; }

        /// <summary>
        /// Indicates whether media assets must be preloaded asynchronously before use.
        /// </summary>
        bool MediaRequiresAsyncPreload { get; set; }

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
        IBlingoCastLibsContainer CastLibs { get; }
        IBlingoMember? GetMember(BlingoMemberRef memberRef);
        T? GetMember<T>(BlingoMemberRef memberRef) where T : class, IBlingoMember;
        IBlingoCast? GetCast(BlingoCastRef castRef);
        /// <summary>
        /// Specifies a handler hook (function) to override the default alert display behavior.
        /// Lingo: the alertHook
        /// </summary>
        Func<string> AlertHook { get; set; }
        IBlingoStage Stage { get; }
        

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

        IBlingoCast CastLib(int number);
        IBlingoCast CastLib(string name);
        IBlingoPlayer LoadCastLibFromCsv(string castlibName, string pathAndFilenameToCsv, bool isInternal = false);
        Task<IBlingoPlayer> LoadAsync<TBlingoCastLibBuilder>() where TBlingoCastLibBuilder : class, IBlingoCastLibBuilder, new();
        Task<IBlingoPlayer> LoadCastLibFromCsvAsync(string castlibName, string pathAndFilenameToCsv, bool isInternal = false);
        IBlingoPlayer AddCastLib(string name, bool isInternal = false, Action<IBlingoCast>? configure = null);
        IBlingoMovie NewMovie(string movieName, bool andActivate = true);
        IBlingoMovie? GetMovie(BlingoMovieRef movieRef);
        Task<IBlingoMovie> LoadMovieAsync(IBlingoMovieBuilder builder);

        void RunDelayed(Action action, int milliseconds, CancellationTokenSource? cts = null);

        Task RunOnUIThreadAsync(Action action, CancellationToken ct = default);
        Task<T> RunOnUIThreadAsync<T>(Func<T> func, CancellationToken ct = default);
        Task<T> RunOnUIThreadAsync<T>(Func<Task<T>> func, CancellationToken ct = default);
        void RunOnUIThread(Action action);

    }
}




