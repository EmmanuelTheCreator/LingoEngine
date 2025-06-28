using LingoEngine.Casts;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using System;
using System.Numerics;

namespace LingoEngine.Core
{


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

        #region Global accessors
        protected ILingoPlayer _Player => _env.Player;
        protected ILingoMouse _Mouse => _env.Mouse;
        protected ILingoKey _Key => _env.Key;
        protected ILingoSound _Sound => _env.Sound;
        /// <summary>
        /// Retrieves the current movie
        /// </summary>
        protected ILingoMovie _Movie => _env.Movie;
        protected ILingoSystem _System => _env.System;

        #endregion
        protected LingoMemberBitmap? CursorImage { get  => _env.Mouse.Cursor.Image; set => _env.Mouse.Cursor.Image = value; }
        protected int Cursor { get  => _env.Mouse.Cursor.Cursor; set => _env.Mouse.Cursor.Cursor = value; }


        // We dont need scripts in c#
        //public object? Script(string name) => null; // To resolve named script instances

        /// <summary>
        /// returns a reference to a specified sound channel.
        /// </summary>
        protected ILingoSoundChannel? Sound(int channelNumber) => _Sound.Channel(channelNumber);
        /// <summary>
        /// Returns a sprite channel.
        /// </summary>
        protected ILingoSpriteChannel Channel(int channelNumber) => _Movie.Channel(channelNumber);
        protected ILingoSpriteChannel Sprite(int number) => _Movie.GetActiveSprite(number);
        protected ILingoCast? CastLib(int number) => _env.GetCastLib(number);
        protected ILingoCast? CastLib(string name) => _env.GetCastLib(name);
        /// <summary>
        /// creates and returns a new image with specified dimensions.
        /// bitDepth : allowed values : 1, 2, 4, 8, 16, or 32.
        /// </summary>
        protected ILingoImage? Image(int width, int height, int bitDepth) => throw new NotImplementedException(); // page 364 in manual



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

        protected LingoSymbol Symbol(string name) => LingoSymbol.New(name);
        protected LingoPoint Point(float x, float y) => new(x, y);
        protected LingoRect Rect(float left, float top, float right, float bottom) => new(left, top, right, bottom);
        protected DateTime Date() => DateTime.Now;
        protected ILingoTimeoutObject Timeout(string timeoutObjName, int periodInMilliseconds, Action onTick) => _Movie.TimeOutList.New(timeoutObjName, periodInMilliseconds, onTick);


        #endregion

        protected void Put(object obj) => Console.WriteLine(obj);
        
        


        #region Lists

        // list
        protected LingoList<TValue> List<TValue>() => new LingoList<TValue>();
        protected TValue GetAt<TValue>(LingoPropertyList<TValue> list,int number) => list.GetAt(number);
        protected void SetAt<TValue>(LingoPropertyList<TValue> list,int number, TValue value) => list.SetAt(number, value);
        protected void DeleteAt<TValue>(LingoPropertyList<TValue> list,int number, TValue value) => list.DeleteAt(number);

        // Property list
        protected LingoPropertyList<TValue> PropList<TValue>() => new LingoPropertyList<TValue>();
        protected TValue GetAt<TValue>(LingoList<TValue> list,int number) => list.GetAt(number);
        protected void SetAt<TValue>(LingoList<TValue> list,int number, TValue value) => list.SetAt(number, value);
        protected void DeleteAt<TValue>(LingoList<TValue> list,int number, TValue value) => list.DeleteAt(number);
        


        #endregion


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


        protected T? TryMember<T>(int number, int? castLib = null, Action<T>? action = null) where T : class, ILingoMember
        {
            var member = _Movie.CastLib.GetMember(number, castLib ?? 1) as T;
            if (member != null && action != null)
                action(member);
            
            return member as T;
        }
        protected T? TryMember<T>(string name, int? castLib = null, Action<T>? action = null) where T : class, ILingoMember
        {
            var member = _Movie.CastLib.GetMember(name, castLib ?? 1) as T;
            if (member != null && action != null)
                action(member);
            
            return member as T;
        }

        protected void PutTextIntoField(string name, string text)
        {
            var field = TryMember<ILingoMemberField>(name);
            if (field != null)
                field.Text = text;
        }
        #endregion


        #region Movie methods
        protected void SendSprite(string name, Action<ILingoSpriteChannel> actionOnSprite) => _Movie.SendSprite(name, actionOnSprite);
        protected void SendSprite(int spriteNumber, Action<ILingoSpriteChannel> actionOnSprite) => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : LingoSpriteBehavior => _Movie.SendSprite(spriteNumber, actionOnSprite);
        protected TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior => _Movie.SendSprite(spriteNumber, actionOnSprite);

        protected LingoList<ILingoSpriteChannel> SpritesUnderPoint(LingoPoint point)
            => new LingoList<ILingoSpriteChannel>(_Movie.GetSpritesAtPoint(point.X, point.Y).Select(s => (ILingoSpriteChannel)s));

        protected void UpdateStage() => _Movie.UpdateStage();
        protected void StartTimer() => _env.Movie.StartTimer();
        protected int Timer => _env.Movie.Timer; 
        #endregion
    }

}



