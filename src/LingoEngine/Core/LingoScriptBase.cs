using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
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
        private readonly ILingoMovieEnvironment _env;
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
        protected int Random(int max) => _random.Next(1, max + 1); // Lingo: random(max)
        /// <summary>
        /// eturns a unit vector describing a randomly chosen point on the surface of a unit sphere.
        /// This function differs from vector(random(10)/10.0, random(10)/10.0, random(10)/10.0,) 
        /// in that the resulting vector using randomVector() is guaranteed to be a unit vector.
        /// A unit vector always has a length of one.
        /// </summary>
        /// <returns></returns>
        protected Vector2 RandomVector() => throw new NotImplementedException();

        protected LingoSymbol Symbol(string name) => LingoSymbol.Intern(name);
        protected LingoPoint Point(float x, float y) => new(x, y);
        protected LingoRect Rect(float left, float top, float right, float bottom) => new(left, top, right, bottom);

        protected void Put(object obj) => Console.WriteLine(obj);

        protected DateTime Date() => DateTime.Now;

        #region Members
        protected ILingoMember? Member(int number, int? castlibNumber = null)
             => !castlibNumber.HasValue
                ? _env.CastLibsContainer.GetMember<ILingoMember>(number)
                : _env.CastLibsContainer[castlibNumber.Value].GetMember<ILingoMember>(number);
        protected ILingoMember? Member(int number, string castlibName)
            => _env.CastLibsContainer[castlibName].GetMember<ILingoMember>(number);

        protected ILingoMember? Member(string name, int? castlibNumber = null)
             => !castlibNumber.HasValue
                ? _env.CastLibsContainer.GetMember<ILingoMember>(name)
                : _env.CastLibsContainer[castlibNumber.Value].GetMember<ILingoMember>(name);
        protected ILingoMember? Member(string name, string castlibName)
            => _env.CastLibsContainer[castlibName].GetMember<ILingoMember>(name);

        protected T? Member<T>(string name, int? castlibNumber = null) where T : LingoMember
             => !castlibNumber.HasValue
                ? _env.CastLibsContainer.GetMember<T>(name)
                : _env.CastLibsContainer[castlibNumber.Value].GetMember<T>(name);
        protected T? Member<T>(string name, string castlibName) where T : LingoMember
             => _env.CastLibsContainer[castlibName].GetMember<T>(name);

        protected void Member<T>(string name, Action<T> action) where T : LingoMember
            => action(_env.CastLibsContainer.GetMember<T>(name)!);
        protected TResult Member<T, TResult>(string name, Func<T, TResult> action) where T : LingoMember
            => action(_env.CastLibsContainer.GetMember<T>(name)!); 
        #endregion
        // We dont need scripts in c#
        //public object? Script(string name) => null; // To resolve named script instances

        /// <summary>
        /// returns a reference to a specified sound channel.
        /// </summary>
        protected ILingoSoundChannel? Sound(int channelNumber) => _Sound.Channel(channelNumber);
        /// <summary>
        /// Returns a sprite channel.
        /// </summary>
        protected ILingoSpriteChannel? Channel(int channelNumber) => _Movie.Channel(channelNumber);
        protected ILingoSprite? Sprite(int number) => _Movie.GetActiveSprite(number);
        protected ILingoCast? CastLib(int number) => _env.GetCastLib(number);
        protected ILingoCast? CastLib(string name) => _env.GetCastLib(name);
        /// <summary>
        /// creates and returns a new image with specified dimensions.
        /// bitDepth : allowed values : 1, 2, 4, 8, 16, or 32.
        /// </summary>
        protected ILingoImage? Image(int width, int height, int bitDepth) => throw new NotImplementedException(); // page 364 in manual
        protected ILingoTimeoutObject Timeout(string timeoutObjName, int periodInMilliseconds,Action onTick) => _Movie.TimeOutList.New(timeoutObjName, periodInMilliseconds, onTick);

        protected void UpdateStage() => _Movie.UpdateStage();
        protected void SendSprite(string name, Action<LingoSprite> actionOnSprite) => _Movie.SendSprite(name, actionOnSprite);
        protected void SendSprite(int spriteNumber, Action<LingoSprite> actionOnSprite) => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : LingoSpriteBehavior => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior => _Movie.SendSprite(spriteNumber, actionOnSprite);

        protected void StartTimer() => _env.Movie.StartTimer();
        protected int Timer => _env.Movie.Timer;
    }

}



