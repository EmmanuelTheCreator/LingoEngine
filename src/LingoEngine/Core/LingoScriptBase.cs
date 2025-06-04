using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using System;
using System.Numerics;

namespace LingoEngine.Core
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
        protected readonly ILingoMovieEnvironment _env;
        private static readonly Random _random = new Random();
        protected LingoScriptBase(ILingoMovieEnvironment env)
        {
            _env = env;
        }

        // Global objects ("the mouse", etc.)
        protected ILingoPlayer _Player => _env.Player;
        protected ILingoMouse _Mouse => _env.Mouse;
        protected ILingoKey _Key => _env.Key;
        protected ILingoSound _Sound => _env.Sound;
        protected ILingoMovie _Movie => _env.Movie;
        protected ILingoSystem _System => _env.System;

        // Language built-in functions
        /// <summary>
        /// Why 1 : because Lingo's random(n) returns a number between 1 and n, inclusive — unlike .NET's Next(0, n) which is exclusive on the upper bound.
        /// </summary>
        public int Random(int max) => _random.Next(1, max + 1); // Lingo: random(max)
        /// <summary>
        /// eturns a unit vector describing a randomly chosen point on the surface of a unit sphere.
        /// This function differs from vector(random(10)/10.0, random(10)/10.0, random(10)/10.0,) 
        /// in that the resulting vector using randomVector() is guaranteed to be a unit vector.
        /// A unit vector always has a length of one.
        /// </summary>
        /// <returns></returns>
        public Vector2 RandomVector() => throw new NotImplementedException();

        public LingoSymbol Symbol(string name) => LingoSymbol.Intern(name);
        public LingoPoint Point(float x, float y) => new(x, y);
        public LingoRect Rect(float left, float top, float right, float bottom) => new(left, top, right, bottom);

        public void Put(object obj) => Console.WriteLine(obj);

        public DateTime Date() => DateTime.Now;


        public LingoMember? Member(string name) => _env.CastLib.Member(name);
        public T? Member<T>(string name) where T : LingoMember => _env.CastLib.Member<T>(name);
        public void Member<T>(string name, Action<T> action) where T : LingoMember => action(_env.CastLib.Member<T>(name)!);
        public TResult Member<T, TResult>(string name, Func<T, TResult> action) where T : LingoMember => action(_env.CastLib.Member<T>(name)!);
        // We dont need scripts in c#
        //public object? Script(string name) => null; // To resolve named script instances

        /// <summary>
        /// returns a reference to a specified sound channel.
        /// </summary>
        public ILingoSoundChannel? Sound(int channelNumber) => _Sound.Channel(channelNumber);
        /// <summary>
        /// Returns a sprite channel.
        /// </summary>
        public ILingoSpriteChannel? Channel(int channelNumber) => _Movie.Channel(channelNumber);
        public ILingoSprite? Sprite(int number) => _Movie.GetSprite(number);
        public ILingoCast? CastLib(int number) => _env.GetCastLib(number);
        /// <summary>
        /// creates and returns a new image with specified dimensions.
        /// bitDepth : allowed values : 1, 2, 4, 8, 16, or 32.
        /// </summary>
        public ILingoImage? Image(int width, int height, int bitDepth) => throw new NotImplementedException(); // page 364 in manual
        public ILingoTimeoutObject Timeout(string timeoutObjName, int periodInMilliseconds,Action onTick) => _Movie.TimeOutList.New(timeoutObjName, periodInMilliseconds, onTick);
    }

}



