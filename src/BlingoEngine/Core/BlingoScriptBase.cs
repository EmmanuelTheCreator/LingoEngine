using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Primitives;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using Microsoft.Extensions.Logging;
using System.Numerics;
using AbstUI.Primitives;
using AbstUI.Inputs;

namespace BlingoEngine.Core
{

   
    public interface IBlingoScriptBase
    {
        void Trace(string message);
        void Log(string message);
        T Global<T>() where T : BlingoGlobalVars;
        IBlingoPlayer Player { get; }
    }

  

    // https://usermanual.wiki/adobe/drmx2004scripting.537266374.pdf
    /// <summary>
    /// Base class representing built-in language features available to all Lingo scripts.
    /// This includes global objects (e.g., the mouse, the sound) and core functions (e.g., point(), random(), symbol()).
    /// </summary>
    public abstract class BlingoScriptBase : IBlingoScriptBase
    {
        protected readonly IBlingoMovieEnvironment _env;
        private readonly ILogger _logger;
        private BlingoGlobalVars _globals;
        private static readonly Random _random = new Random();
        public IBlingoPlayer Player => _env.Player;


        protected BlingoScriptBase(IBlingoMovieEnvironment env)
        {
            _env = env;
            _logger = env.Logger;
            _globals = env.Globals;
            
        }

        // Global objects ("the mouse", etc.)

        #region Global accessors
        protected IBlingoPlayer _Player => _env.Player;
        protected IBlingoStageMouse _Mouse => _env.Mouse;
        protected IBlingoKey _Key => _env.Key;
        protected IBlingoSound _Sound => _env.Sound;
        /// <summary>
        /// Retrieves the current movie
        /// </summary>
        protected IBlingoMovie _Movie => _env.Movie;
        protected IBlingoMovie _movie => _Movie;
        protected IBlingoSystem _System => _env.System;

        #endregion
        protected BlingoMemberBitmap? CursorImage { get => _env.Mouse.Cursor.Image; set => _env.Mouse.Cursor.Image = value; }
        protected int Cursor { get => _env.Mouse.Cursor.Cursor; set => _env.Mouse.Cursor.Cursor = value; }


        // We dont need scripts in c#
        //public object? Script(string name) => null; // To resolve named script instances

        /// <summary>
        /// returns a reference to a specified sound channel.
        /// </summary>
        protected IBlingoSoundChannel? Sound(int channelNumber) => _Sound.Channel(channelNumber);
        /// <summary>
        /// Returns a sprite channel.
        /// </summary>
        protected IBlingoSpriteChannel Channel(int channelNumber) => _Movie.Channel(channelNumber);
        protected IBlingoSpriteChannel Sprite(int number) => _Movie.GetActiveSprite(number);
        protected IBlingoCast? CastLib(int number) => _env.GetCastLib(number);
        protected IBlingoCast? CastLib(string name) => _env.GetCastLib(name);
        /// <summary>
        /// creates and returns a new image with specified dimensions.
        /// bitDepth : allowed values : 1, 2, 4, 8, 16, or 32.
        /// </summary>
        protected IBlingoImage? Image(int width, int height, int bitDepth) => throw new NotImplementedException(); // page 364 in manual



        #region Primitive functions

        // Language built-in functions
        /// <summary>
        /// Why 1 : because Lingo's random(n) returns a number between 1 and n, inclusive — unlike .NET's Next(0, n) which is exclusive on the upper bound.
        /// </summary>
        protected int Random(int max) => _random.Next(1, max + 1); // Lingo: random(max)
        /// <summary>
        /// Returns a unit vector describing a randomly chosen point on the surface of a unit sphere.
        /// This function differs from vector(random(10)/10.0, random(10)/10.0, random(10)/10.0,) 
        /// in that the resulting vector using randomVector() is guaranteed to be a unit vector.
        /// A unit vector always has a length of one.
        /// </summary>
        /// <returns></returns>
        protected Vector2 RandomVector() => throw new NotImplementedException();

        protected BlingoSymbol Symbol(string name) => BlingoSymbol.New(name);
        protected APoint Point(float x, float y) => new(x, y);
        protected ARect Rect(float left, float top, float right, float bottom) => new(left, top, right, bottom);
        protected DateTime Date() => DateTime.Now;
        protected IBlingoTimeoutObject Timeout(string timeoutObjName, int periodInMilliseconds, Action onTick) => _Movie.TimeOutList.New(timeoutObjName, periodInMilliseconds, onTick);


        #endregion

      



        #region Lists

        // list
        protected BlingoList<TValue> List<TValue>() => new BlingoList<TValue>();
        protected TValue? GetAt<TValue>(BlingoPropertyList<TValue> list, int number) => list.GetAt(number);
        protected void SetAt<TValue>(BlingoPropertyList<TValue> list, int number, TValue value) => list.SetAt(number, value);
        protected void DeleteAt<TValue>(BlingoPropertyList<TValue> list, int number, TValue value) => list.DeleteAt(number);

        // Property list
        protected BlingoPropertyList<TValue> PropList<TValue>() => new BlingoPropertyList<TValue>();
        protected TValue? GetAt<TValue>(BlingoList<TValue> list, int number) => list.GetAt(number);
        protected void SetAt<TValue>(BlingoList<TValue> list, int number, TValue value) => list.SetAt(number, value);
        protected void DeleteAt<TValue>(BlingoList<TValue> list, int number, TValue value) => list.DeleteAt(number);



        #endregion

        public void Trace(string message) => _logger.LogTrace(message);
        public void Log(string message) => _logger.LogInformation(message);
        public T Global<T>() where T : BlingoGlobalVars => (T)_globals;
        protected void ClearGlobals() => _globals.ClearGlobals();
        protected void ShowGlobals() => _globals.ShowGlobals(_logger);


        #region Members
        protected IBlingoMember? Member(int number, int? castlibNumber = null)
             => !castlibNumber.HasValue
                ? _env.CastLibsContainer.GetMember<IBlingoMember>(number)
                : _env.CastLibsContainer[castlibNumber.Value].GetMember<IBlingoMember>(number);
        protected IBlingoMember? Member(int number, string castlibName)
            => _env.CastLibsContainer[castlibName].GetMember<IBlingoMember>(number);

        protected IBlingoMember? Member(string name, int? castlibNumber = null)
             => !castlibNumber.HasValue
                ? _env.CastLibsContainer.GetMember<IBlingoMember>(name)
                : _env.CastLibsContainer[castlibNumber.Value].GetMember<IBlingoMember>(name);
        protected IBlingoMember? Member(string name, string castlibName)
            => _env.CastLibsContainer[castlibName].GetMember<IBlingoMember>(name);

        protected T? Member<T>(string name, int? castlibNumber = null) where T : IBlingoMember
             => !castlibNumber.HasValue
                ? _env.CastLibsContainer.GetMember<T>(name)
                : _env.CastLibsContainer[castlibNumber.Value].GetMember<T>(name);
        protected T? Member<T>(string name, string castlibName) where T : BlingoMember
             => _env.CastLibsContainer[castlibName].GetMember<T>(name);

        protected void Member<T>(string name, Action<T> action) where T : BlingoMember
            => action(_env.CastLibsContainer.GetMember<T>(name)!);
        protected TResult Member<T, TResult>(string name, Func<T, TResult> action) where T : BlingoMember
            => action(_env.CastLibsContainer.GetMember<T>(name)!);


        protected T? TryMember<T>(int number, int? castLib = null, Action<T>? action = null) where T : class, IBlingoMember
        {
            var member = _Movie.CastLib.GetMember(number, castLib ?? 1) as T;
            if (member != null && action != null)
                action(member);

            return member as T;
        }
        protected T? TryMember<T>(string name, int? castLib = null, Action<T>? action = null) where T : class, IBlingoMember
        {
            var member = _Movie.CastLib.GetMember(name, castLib ?? 1) as T;
            if (member != null && action != null)
                action(member);

            return member as T;
        }

       
        #endregion


        #region Movie methods
        protected void SendSprite(string name, Action<IBlingoSpriteChannel> actionOnSprite) => _Movie.SendSprite(name, actionOnSprite);
        protected void SendSprite(int spriteNumber, Action<IBlingoSpriteChannel> actionOnSprite) => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : IBlingoSpriteBehavior => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected bool TrySendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : IBlingoSpriteBehavior => _Movie.TrySendSprite(spriteNumber, actionOnSprite);
        protected TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : IBlingoSpriteBehavior => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected void CallMovieScript<T>(Action<T> action) where T : IBlingoMovieScript => _Movie.CallMovieScript(action);
        protected TResult? CallMovieScript<T, TResult>(Func<T, TResult> action) where T : IBlingoMovieScript => _Movie.CallMovieScript(action);
        protected BlingoList<IBlingoSpriteChannel> SpritesUnderPoint(APoint point)
            => new BlingoList<IBlingoSpriteChannel>(_Movie.GetSpritesAtPoint(point.X, point.Y).Select(s => (IBlingoSpriteChannel)s));

        protected void UpdateStage() => _Movie.UpdateStage();
        protected void StartTimer() => _env.Movie.StartTimer();
        protected int Timer => _env.Movie.Timer;
        #endregion

        protected IAbstJoystickKeyboard CreateJoystickKeyboard(Action<AbstJoystickKeyboard>? configure = null, AbstJoystickKeyboard.KeyboardLayoutType layoutType = AbstJoystickKeyboard.KeyboardLayoutType.Azerty, bool showEscapeKey = false, APoint? position = null) 
            => ((BlingoPlayer)_Player).Factory.ComponentFactory.CreateJoystickKeyboard(configure, layoutType, showEscapeKey, position);
    }

}




