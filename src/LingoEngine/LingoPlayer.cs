using LingoEngine.Sounds;

namespace LingoEngine
{
    /// <summary>
    /// Represents the player-level interface for accessing global runtime environment information and control,
    /// such as system settings, UI state, and playback environment. Mirrors Lingo’s player properties.
    /// </summary>
    public interface ILingoPlayer
    {
        /// <summary>
        /// Indicates which cast library was most recently activated.
        /// Lingo: the activeCastLib
        /// </summary>
        ILingoCast ActiveCastLib { get; }

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
    }


    public class LingoPlayer : ILingoPlayer
    {
        public ILingoCast ActiveCastLib => throw new NotImplementedException();

        public ILingoSound Sound => throw new NotImplementedException();

        public int CurrentSpriteNum => throw new NotImplementedException();

        public bool NetPreset => throw new NotImplementedException();

        public bool ActiveWindow => throw new NotImplementedException();

        public bool SafePlayer => throw new NotImplementedException();

        public string OrganizationName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ApplicationPath { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;

        public int LastClick => throw new NotImplementedException();

        public int LastEvent => throw new NotImplementedException();

        public int LastKey => throw new NotImplementedException();

        public Version ProductVersion { get; set; } = new Version(1, 0, 0, 0);
        public Func<string> AlertHook { get; set; } = () => "";
        bool ILingoPlayer.SafePlayer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Alert(string message)
        {
            Console.WriteLine(message);
        }

        public void AppMinimize()
        {

        }

        public void Cursor(int cursorNum)
        {
            throw new NotImplementedException();
        }

        public void Halt()
        {

        }

        public void Open(string applicationName)
        {

        }

        public void Quit()
        {

        }

        public bool WindowPresent()
        {
            return true;
        }
    }
    /// <summary>
    /// Simulates Lingo's colorList: a predefined set of named system colors.
    /// Includes static properties for convenient access and a dictionary for lookup.
    /// </summary>
    public static class LingoColorList
    {
        // Static predefined colors
        public static LingoColor Black => new(0, 0, 0, 0, "black");
        public static LingoColor White => new(255, 255, 255, 255, "white");
        public static LingoColor Red => new(1, 255, 0, 0, "red");
        public static LingoColor Green => new(2, 0, 255, 0, "green");
        public static LingoColor Blue => new(3, 0, 0, 255, "blue");
        public static LingoColor Yellow => new(4, 255, 255, 0, "yellow");
        public static LingoColor Cyan => new(5, 0, 255, 255, "cyan");
        public static LingoColor Magenta => new(6, 255, 0, 255, "magenta");
        public static LingoColor Gray => new(7, 128, 128, 128, "gray");
        public static LingoColor LightGray => new(8, 192, 192, 192, "lightgray");
        public static LingoColor DarkGray => new(9, 64, 64, 64, "darkgray");

        /// <summary>
        /// Dictionary of all predefined Lingo colors by lowercase name.
        /// </summary>
        public static readonly Dictionary<string, LingoColor> Colors = new()
        {
            { "black", Black },
            { "white", White },
            { "red", Red },
            { "green", Green },
            { "blue", Blue },
            { "yellow", Yellow },
            { "cyan", Cyan },
            { "magenta", Magenta },
            { "gray", Gray },
            { "lightgray", LightGray },
            { "darkgray", DarkGray },
        };

        /// <summary>
        /// Attempts to retrieve a color from the color list by name.
        /// </summary>
        public static bool TryGetColor(string name, out LingoColor color) =>
            Colors.TryGetValue(name.ToLowerInvariant(), out color);
    }
}



