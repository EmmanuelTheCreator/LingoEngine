using System;

namespace LingoEngine.Movies
{
    /// <summary>
    /// Represents the Lingo _movie object, providing control over playback, navigation, and transitions.
    /// Lingo equivalents are noted for each member.
    /// </summary>
    public interface ILingoMovie
    {
        /// <summary>
        /// Gets or sets the Score object for the current movie.
        /// Lingo: the score
        /// </summary>
        ILingoScore Score { get;}

        /// <summary>
        /// Gets the current frame number of the movie.
        /// Lingo: the frame
        /// </summary>
        int Frame { get; }

        /// <summary>
        /// Gets the total number of frames in the movie.
        /// Lingo: the frameCount
        /// </summary>
        int FrameCount { get; }

        /// <summary>
        /// Gets the name of the movie.
        /// Lingo: the name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the tempo (frames per second) of the movie.
        /// Lingo: the tempo
        /// </summary>
        int Tempo { get; set; }

        /// <summary>
        /// Indicates whether the movie is currently playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Jumps to the specified frame and continues playing.
        /// Lingo: go frameNumber
        /// </summary>
        void Go(int frame);

        /// <summary>
        /// Jumps to the specified frame label and continues playing.
        /// Lingo: go "label"
        /// </summary>
        void Go(string label);

        /// <summary>
        /// Plays a transition effect before navigating.
        /// Lingo: puppetTransition effectNumber
        /// </summary>
        void PuppetTransition(int effectNumber);

        /// <summary>
        /// Stops playback immediately.
        /// Lingo: halt or stop
        /// </summary>
        void Halt();

        /// <summary>
        /// Starts or resumes playback.
        /// Lingo: play
        /// </summary>
        void Play();

        /// <summary>
        /// Advances to the next frame.
        /// Lingo: nextFrame
        /// </summary>
        void NextFrame();

        /// <summary>
        /// Goes back to the previous frame.
        /// Lingo: prevFrame
        /// </summary>
        void PrevFrame();

        /// <summary>
        /// Goes to a specific frame and stops playback.
        /// Lingo: go to frame
        /// </summary>
        void GoToAndStop(int frame);
        /// <summary>
        /// The rollover() method indicates whether the pointer is over the specified sprite.
        /// </summary>
        bool RollOver(int spriteNumber);
        
        LingoMember? GetMember(string memberName);
        LingoMember? GetMember(int number);
        void SendSprite(string name, Action<LingoSprite> actionOnSprite);
        void SendSprite(int number, Action<LingoSprite> actionOnSprite);
        void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : LingoSpriteBehavior;
        TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior;
        void UpdateStage();
        LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null);
        LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null);
        T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite;
        T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite;
        T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite;
        bool RemoveSprite(string name);
        ILingoSprite GetSprite(int number);
        string GetSpriteName(int number);
        bool TryGetSprite(string name, out ILingoSprite? sprite);
        bool TryGetSprite(int number, out ILingoSprite? sprite);
        ILingoMovieEnvironment GetEnvironment();
    }


    public class LingoMovie : ILingoMovie
    {
        private readonly ILingoMovieEnvironment _environment;
        private readonly LingoScore _score;
        private readonly LingoMovieStage _stage;
        private readonly LingoCast _cast;

        public ILingoScore Score => _score;
        /// <inheritdoc/>
        public string Name => Score.Name;
        /// <inheritdoc/>
        public int Number => Score.Number;
        /// <inheritdoc/>
        public int Frame => Score.Frame;
        /// <inheritdoc/>
        public int FrameCount => Score.FrameCount;
        /// <inheritdoc/>
        public int SpriteCount => Score.SpriteCount;
        /// <inheritdoc/>
        public int Tempo { get => Score.Tempo; set => Score.Tempo = value; }
        /// <inheritdoc/>
        public bool IsPlaying => Score.IsPlaying;

        public LingoMovie(ILingoMovieEnvironment environment, LingoScore score, LingoMovieStage movieStage, LingoCast lingoCast)
        {
            this._environment = environment;
            _score = score;
            _stage = movieStage;
            _cast = lingoCast;
        }


        /// <inheritdoc/>
        public void Go(string label) => Score.Go(label);
        /// <inheritdoc/>
        public void Go(int frame) => Score.Go(frame);
        /// <inheritdoc/>


        /// <summary>
        /// The rollover() method indicates whether the pointer is over the specified sprite.
        /// </summary>
        public bool RollOver(int spriteNumber) => Score.RollOver(spriteNumber);

        public void PuppetTransition(int effectNumber) => Score.PuppetTransition(effectNumber);

        public void Halt() => Score.Halt();

        public void Play() => Score.Play();

        public void NextFrame() => Score.NextFrame();

        public void PrevFrame() => Score.PrevFrame();

        public void GoToAndStop(int frame) => Score.GoToAndStop(frame);

        public void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : LingoSpriteBehavior
            => Score.SendSprite(spriteNumber, actionOnSprite);
        public TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior
            => Score.SendSprite(spriteNumber, actionOnSprite);
        public void SendSprite(string name, Action<LingoSprite> actionOnSprite) => Score.SendSprite(name, actionOnSprite);

        public void SendSprite(int spriteNumber, Action<LingoSprite> actionOnSprite) => Score.SendSprite(spriteNumber, actionOnSprite);

        public LingoMember? GetMember(string memberName) => _cast.GetMember(memberName);
        public LingoMember? GetMember(int number) => _cast.GetMember(number);

        public void UpdateStage() => _stage.UpdateStage();

        /// <summary>
        /// Adds a new sprite by name to the next available channel.
        /// Lingo: new sprite
        /// </summary>
        public LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null) => Score.AddSprite(name, configure);
        public LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null) => Score.AddSprite(num, configure);
        public T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite => Score.AddSprite<T>(num, configure);
        public T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite => Score.AddSprite<T>(name, configure);
        public T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite => Score.AddSprite<T>(num, name, configure);
        public bool RemoveSprite(string name) => Score.RemoveSprite(name);
        public ILingoSprite GetSprite(int number) => Score.GetSprite(number);
        public string GetSpriteName(int number) => Score.GetSpriteName(number);
        public bool TryGetSprite(string name, out ILingoSprite? sprite) => Score.TryGetSprite(name, out sprite);
        public bool TryGetSprite(int number, out ILingoSprite? sprite) => Score.TryGetSprite(number, out sprite);

        internal LingoSprite? GetSpriteUnderMouse() => _score.GetSpriteUnderMouse();

        public ILingoMovieEnvironment GetEnvironment() => _environment;
    }
}
