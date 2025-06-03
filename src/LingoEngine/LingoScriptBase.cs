using LingoEngine.Movies;
using LingoEngine.Sounds;

namespace LingoEngine
{

    /*
    var a = LingoSymbol.Intern("hello");
var b = LingoSymbol.Intern("hello");
var c = LingoSymbol.Intern("world");

Console.WriteLine(a == b); // true
Console.WriteLine(a == c); // false
Console.WriteLine(a);      // #hello
*/

    // https://usermanual.wiki/adobe/drmx2004scripting.537266374.pdf
    /// <summary>
    /// Base class representing built-in language features available to all Lingo scripts.
    /// This includes global objects (e.g., the mouse, the sound) and core functions (e.g., point(), random(), symbol()).
    /// </summary>
    public abstract class LingoScriptBase
    {
        protected readonly ILingoEnvironment _env;
        private static readonly Random _random = new Random();
        protected LingoScriptBase(ILingoEnvironment env)
        {
            _env = env;
        }

        // Global objects ("the mouse", etc.)
        public ILingoPlayer ThePlayer => _env.Player;
        public ILingoMouse TheMouse => _env.Mouse;
        public ILingoKey TheKey => _env.Key;
        public ILingoSound TheSound => _env.Sound;
        public ILingoMovie TheMovie => _env.Movie;
        public ILingoCast TheCastLib => _env.CastLib;
        public ILingoSystem TheSystem => _env.System;

        // Language built-in functions
        /// <summary>
        /// Why 1 : because Lingo's random(n) returns a number between 1 and n, inclusive — unlike .NET's Next(0, n) which is exclusive on the upper bound.
        /// </summary>
        public int Random(int max) => _random.Next(1, max + 1); // Lingo: random(max)

        public LingoSymbol Symbol(string name) => LingoSymbol.Intern(name);
        public LingoPoint Point(float x, float y) => new(x, y);
        public LingoRect Rect(float left, float top, float right, float bottom) => new(left, top, right, bottom);

        public void Put(object obj) => Console.WriteLine(obj);

        public DateTime Date() => DateTime.Now;

        
        public LingoMember? Member(string name) => _env.CastLib.GetMember(name); 
        public T? Member<T>(string name) where T : LingoMember => _env.CastLib.GetMember<T>(name); 
        // We dont need scripts in c#
        //public object? Script(string name) => null; // To resolve named script instances
        public object? Image(string name) => null; 
        public object? Sound(string name) => null;
    }

    
} 

    

