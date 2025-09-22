using AbstUI;
using AbstUI.Components;
using BlingoEngine.Casts;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Core;
using BlingoEngine.Members;
using BlingoEngine.Scripts;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Tempos;
using BlingoEngine.Transitions;

namespace BlingoEngine.Movies
{
    /// <summary>
    /// Represents the Lingo _movie object, providing control over playback, navigation, and transitions.
    /// Lingo equivalents are noted for each member.
    /// </summary>
    public interface IBlingoMovie : IBlingoSpritesPlayer, IHasPropertyChanged , IAbstLayoutNode
    {

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


        string About { get; set; }
        string Copyright { get; set; }
        string UserName { get; set; }
        string CompanyName { get; set; }

        IBlingoSpriteAudioManager Audio { get; }
        IBlingoSpriteTransitionManager Transitions { get; }
        IBlingoTempoSpriteManager Tempos { get; }
        IBlingoSpriteColorPaletteSpriteManager ColorPalettes { get; }

        /// <summary>
        /// Occurs when the play state changes. Parameter indicates whether the movie is now playing.
        /// </summary>
        event Action<bool> PlayStateChanged;

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
        /// Rewinds the movie to the first frame and stops playback.
        /// Lingo: rewind
        /// </summary>
        void Rewind();

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
        /// Inserts a duplicate of the current frame during score recording.
        /// Lingo: insertFrame
        /// </summary>
        void InsertFrame();

        /// <summary>
        /// Deletes the current frame during score recording.
        /// Lingo: deleteFrame
        /// </summary>
        void DeleteFrame();

        /// <summary>
        /// Finalizes any pending changes to the current frame during recording.
        /// Lingo: updateFrame
        /// </summary>
        void UpdateFrame();

        // --- Additional Lingo movie controls ---
        /// <summary>
        /// Pauses the playhead for the specified number of ticks (1 tick = 1/60 sec).
        /// Lingo: delay
        /// </summary>
        void Delay(int ticks);

        /// <summary>
        /// Pause playback until either a mouse button is clicked or a key is pressed.
        /// </summary>
        void WaitForInput();

        /// <summary>
        /// Resumes playback after <see cref="WaitForInput"/> was called.
        /// </summary>
        void ContinueAfterInput();

        /// <summary>
        /// Pause playback until the specified cue point on a sound or video channel is reached.
        /// </summary>
        void WaitForCuePoint(int channel, int point);

        /// <summary>
        /// Sends the playhead to the next marker in the movie.
        /// Lingo: goNext
        /// </summary>
        void GoNext();

        /// <summary>
        /// Sends the playhead to the previous marker in the movie.
        /// Lingo: goPrevious
        /// </summary>
        void GoPrevious();

        /// <summary>
        /// Sends the playhead to the marker immediately preceding the current frame.
        /// Lingo: goLoop
        /// </summary>
        void GoLoop();

        /// <summary>
        /// Goes to a specific frame and stops playback.
        /// Lingo: go to frame
        /// </summary>
        void GoToAndStop(int frame);

        /// <summary>
        /// Constrain a horizontal position within the bounds of the specified sprite.
        /// Lingo: constrainH
        /// </summary>
        int ConstrainH(int spriteNumber, int pos);

        /// <summary>
        /// Constrain a vertical position within the bounds of the specified sprite.
        /// Lingo: constrainV
        /// </summary>
        int ConstrainV(int spriteNumber, int pos);

        void UpdateStage();
        IBlingoSpriteChannel Channel(int channelNumber);
        ActorList ActorList { get; }
        BlingoTimeOutList TimeOutList { get; }
        /// <summary>
        /// The number/index of this Score within the movie.
        /// Lingo: the number of score
        /// </summary>
        int Number { get; }

        #region MovieScripts
        void CallMovieScript<T>(Action<T> action) where T : IBlingoMovieScript;
        TResult? CallMovieScript<T, TResult>(Func<T, TResult> action) where T : IBlingoMovieScript;
        IBlingoMovie AddMovieScript<T>() where T : BlingoMovieScript;
        #endregion

        #region Sprites

        /// <summary>
        /// Adds a new sprite by name to the next available channel.
        /// Lingo: new sprite
        /// </summary>
        BlingoSprite2D AddSprite(string name, Action<BlingoSprite2D>? configure = null);

        /// <summary>
        /// Adds a new sprite to a specific sprite channel number.
        /// </summary>
        BlingoSprite2D AddSprite(int num, Action<BlingoSprite2D>? configure = null);
        BlingoFrameScriptSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<BlingoFrameScriptSprite>? configure = null) where TBehaviour : BlingoSpriteBehavior;
        BlingoSprite2D AddSprite(int num, int begin, int end, float x, float y, Action<BlingoSprite2D>? configure = null);

        /// <summary>
        /// Removes a sprite from the score by name.
        /// </summary>
        bool RemoveSprite(string name);

        /// <summary>
        /// Retrieves a sprite by number (channel number).
        /// Lingo: sprite x
        /// </summary>
        IBlingoSpriteChannel GetActiveSprite(int number);

        /// <summary>
        /// Tries to get a sprite by name.
        /// </summary>
        bool TryGetAllTimeSprite(string name, out BlingoSprite2D? sprite);

        /// <summary>
        /// Tries to get a sprite by number.
        /// </summary>
        bool TryGetAllTimeSprite(int number, out BlingoSprite2D? sprite);

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
        /// Raised whenever sprites are added or removed from the movie.
        /// </summary>
        event Action<int>? Sprite2DListChanged;

        /// <summary>
        /// Gets the total number of sprite channels in the Score.
        /// Lingo: the spriteCount
        /// </summary>
        int SpriteTotalCount { get; }
        /// <summary>
        /// The rollover() method indicates whether the pointer is over the specified sprite.
        /// </summary>
        bool RollOver(int spriteNumber);
        /// <summary>
        /// Returns the sprite number currently under the pointer or 0 if none.
        /// Lingo: rollOver with no parameter
        /// </summary>
        int RollOver();

        /// <summary>
        /// Performs an action on all active sprite channels.
        /// Lingo: sendAllSprites
        /// </summary>
        void SendAllSprites(Action<IBlingoSpriteChannel> actionOnSprite);

        /// <summary>
        /// Calls the given action on the requested behaviour of all active sprites.
        /// </summary>
        void SendAllSprites<T>(Action<T> actionOnSprite) where T : BlingoSpriteBehavior;

        /// <summary>
        /// Calls the given function on the requested behaviour of all active sprites and returns the results.
        /// </summary>
        IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : BlingoSpriteBehavior;

        /// <summary>
        /// Total number of sprite channels in the movie.
        /// Lingo: lastChannel
        /// </summary>
        int LastChannel { get; }

        /// <summary>
        /// Number of the last frame in the movie.
        /// Lingo: lastFrame
        /// </summary>
        int LastFrame { get; }

        /// <summary>
        /// List of markers in the movie as frameNumber:label.
        /// Lingo: markerList
        /// </summary>
        IReadOnlyDictionary<int, string> MarkerList { get; }

        IEnumerable<BlingoSprite2D> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false);
        BlingoSprite2D? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false);


        void SendSprite(string name, Action<IBlingoSpriteChannel> actionOnSprite);
        void SendSprite(int number, Action<IBlingoSpriteChannel> actionOnSprite);
        void SendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : IBlingoSpriteBehavior;
        bool TrySendSprite<T>(int spriteNumber, Action<T> actionOnSprite) where T : IBlingoSpriteBehavior;
        TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSprite) where T : IBlingoSpriteBehavior;
        #endregion

        #region Members
        public IBlingoCastLibsContainer CastLib { get; }
        public IBlingoMembersContainer Member { get; }
        public T? GetMember<T>(int number) where T : class, IBlingoMember;
        public T? GetMember<T>(string name) where T : class, IBlingoMember;
        /// <summary>
        /// creates a new cast member and allows you to assign individual property values to child objects.
        /// After newMember() is called, the new cast member is placed in the first empty cast library slot
        /// Lingo : // newBitmap = _movie.newMember(#bitmap)
        /// </summary>
        IBlingoMemberFactory New { get; }


        #endregion
        /// <summary>
        /// Resets the timer to 0;
        /// </summary>
        void StartTimer();

        int Timer { get; }
        int MaxSpriteChannelCount { get; set; }

        void SetScoreLabel(int frameNumber, string name);
        void PuppetSprite(int myNum, bool state);


        int GetNextSpriteStart(int channel, int frame);
        int GetPrevSpriteEnd(int channel, int frame);
        IBlingoServiceProvider GetServiceProvider();
        T GetRequiredService<T>() where T : notnull;


    }
}

