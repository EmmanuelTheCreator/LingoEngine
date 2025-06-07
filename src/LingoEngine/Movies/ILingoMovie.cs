using LingoEngine.Core;

namespace LingoEngine.Movies
{
    /// <summary>
    /// Represents the Lingo _movie object, providing control over playback, navigation, and transitions.
    /// Lingo equivalents are noted for each member.
    /// </summary>
    public interface ILingoMovie
    {
        /// <summary>
        /// Gets the name of the movie.
        /// Lingo: the name
        /// </summary>
        string Name { get; }

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
        void GoTo(int frame);

        /// <summary>
        /// Jumps to the specified frame label and continues playing.
        /// Lingo: go "label"
        /// </summary>
        void Go(string label);
        void GoTo(string label);

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
        
        void UpdateStage();
        ILingoSpriteChannel? Channel(int channelNumber);
        ActorList ActorList { get;  }
        LingoTimeOutList TimeOutList { get; }
        /// <summary>
        /// The number/index of this Score within the movie.
        /// Lingo: the number of score
        /// </summary>
        int Number { get; }

        #region MovieScripts
        void CallMovieScript<T>(Action<T> action) where T : LingoMovieScript;
        TResult? CallMovieScript<T, TResult>(Func<T, TResult> action) where T : LingoMovieScript;
        ILingoMovie AddMovieScript<T>() where T : LingoMovieScript;
        #endregion

        #region Sprites

        /// <summary>
        /// Adds a new sprite by name to the next available channel.
        /// Lingo: new sprite
        /// </summary>
        LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null);

        /// <summary>
        /// Adds a new sprite to a specific sprite channel number.
        /// </summary>
        LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null);
        LingoSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoSprite>? configure = null) where TBehaviour : LingoSpriteBehavior;
        LingoSprite AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite>? configure = null);
        /// <summary>
        /// Adds a new sprite to a specific sprite channel number.
        /// </summary>
        T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite;
        T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite;

        public T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite;

        /// <summary>
        /// Removes a sprite from the score by name.
        /// </summary>
        bool RemoveSprite(string name);

        /// <summary>
        /// Retrieves a sprite by number (channel number).
        /// Lingo: sprite x
        /// </summary>
        ILingoSprite? GetSprite(int number);

        /// <summary>
        /// Tries to get a sprite by name.
        /// </summary>
        bool TryGetSprite(string name, out ILingoSprite? sprite);

        /// <summary>
        /// Tries to get a sprite by number.
        /// </summary>
        bool TryGetSprite(int number, out ILingoSprite? sprite);

        /// <summary>
        /// Sets the member of a sprite using the member's name.
        /// Lingo: the member of sprite x
        /// </summary>
        void SetSpriteMember(int number, string memberName);

        /// <summary>
        /// Sets the member of a sprite using the member's number.
        /// </summary>
        void SetSpriteMember(int number, int memberNumber);

        /// <summary>
        /// Gets the total number of sprite channels in the Score.
        /// Lingo: the spriteCount
        /// </summary>
        int SpriteCount { get; }
        /// <summary>
        /// The rollover() method indicates whether the pointer is over the specified sprite.
        /// </summary>
        bool RollOver(int spriteNumber);


        void SendSprite(string name, Action<LingoSprite> actionOnSprite);
        void SendSprite(int number, Action<LingoSprite> actionOnSprite);
        void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : LingoSpriteBehavior;
        TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior;
        #endregion

        #region Members
        public ILingoCastLibsContainer CastLib { get; }
        public ILingoMembersContainer Member { get; }
        public T? GetMember<T>(int number) where T : class, ILingoMember;
        public T? GetMember<T>(string name) where T : class, ILingoMember;
        /// <summary>
        /// creates a new cast member and allows you to assign individual property values to child objects.
        /// After newMember() is called, the new cast member is placed in the first empty cast library slot
        /// Lingo : // newBitmap = _movie.newMember(#bitmap)
        /// </summary>
        ILingoMemberFactory New { get; }
        int CurrentFrame { get; }
        

        #endregion
        /// <summary>
        /// Resets the timer to 0;
        /// </summary>
        void StartTimer();

        int Timer { get; }
        void SetScoreLabel(int frameNumber, string name);

    }
}
