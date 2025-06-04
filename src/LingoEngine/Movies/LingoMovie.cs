using LingoEngine.Core;
using LingoEngine.Events;

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
        
        void UpdateStage();
        ILingoSpriteChannel? Channel(int channelNumber);
        ActorList ActorList { get;  }
        LingoTimeOutList TimeOutList { get; }
        /// <summary>
        /// The number/index of this Score within the movie.
        /// Lingo: the number of score
        /// </summary>
        int Number { get; }

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
        ILingoSprite GetSprite(int number);

        /// <summary>
        /// Retrieves the name of the sprite at a specific channel number.
        /// </summary>
        string GetSpriteName(int number);

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
        /// Gets the name of the member used by a sprite.
        /// </summary>
        string GetSpriteMemberName(int number);

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
        #endregion
    }


    public class LingoMovie : ILingoMovie, ILingoClockListener, IDisposable
    {
         
        private readonly LingoMovieEnvironment _environment;
        private readonly LingoStage _stage;
        private Dictionary<string, LingoSprite> _spritesByName = new();
        private List<LingoSprite> _sprites = new();
        private int _maxSpriteNum = 0;
        private readonly LingoStage _movieStage;
        private readonly LingoMouse _lingoMouse;
        private readonly LingoClock _lingoClock;
        private int _currentFrame = 1;
        private int _lastFrame = 1;
        private bool _isPlaying = false;
        private int _tempo = 30;  // Default frame rate (FPS)

        private LingoCastLibsContainer _castLibContainer = new();
        

        private readonly List<LingoSprite> _activeSprites = new();
        private readonly List<LingoSprite> _enteredSprites = new();
        private readonly List<LingoSprite> _exitedSprites = new();
        private HashSet<ILingoSpriteEventHandler> _spriteEventHandlers = new();

        // Movie Script subscriptions
        private HashSet<ILingoMovieScriptListener> _movieScriptListeners = new();
        private ActorList _actorList;

        public string Name { get; set; }
        public int Number { get; private set; }

        public int Frame => _currentFrame;
        public int FrameCount => 60; // Arbitrary default, to be replaced with actual timeline data
        public int SpriteCount => _sprites.Count;
        // Tempo (Frame Rate)
        public int Tempo
        {
            get => _tempo;
            set
            {
                if (value > 0)
                    _tempo = value;
            }
        }
        public bool IsPlaying => _isPlaying;

        public ActorList ActorList { get; private set; } = new ActorList();
        public LingoTimeOutList TimeOutList { get; private set; } = new LingoTimeOutList();
        


        public LingoMovie(LingoMovieEnvironment environment, LingoStage movieStage, string name, int number)
        {
            _environment = environment;
            _stage = movieStage;
            _environment = environment;
            _movieStage = movieStage;
            Name = name;
            Number = number;
            _lingoMouse = (LingoMouse)environment.Mouse;
            _lingoClock = (LingoClock)environment.Clock;
            _lingoClock.Subscribe(this);
        }
        public void Dispose()
        {
            _lingoClock.Unsubscribe(this);
        }



       
        


        #region Sprites
        public ILingoSpriteChannel? Channel(int channelNumber)
        {
            throw new NotImplementedException();
        }

        public void PuppetSprite(int number, bool isPuppetSprite) => ((LingoSprite)GetSprite(number)).IsPuppetSprite = isPuppetSprite;
        internal void SubscribeMovieScript(ILingoMovieScriptListener listener)
        {
            if (!_movieScriptListeners.Contains(listener))
                _movieScriptListeners.Add(listener);
            _lingoMouse.Subscribe(listener);
        }

        internal void UnsubscribeMovieScript(ILingoMovieScriptListener listener)
        {
            _movieScriptListeners.Remove(listener);
            _lingoMouse.Unsubscribe(listener);
        }

        public string GetSpriteName(int number) => _sprites[number - 1].Name;
        public ILingoSprite GetSprite(int number) => _sprites[number - 1];
        public LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null) => AddSprite<LingoSprite>(name, configure);
        public T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite
        {
            _maxSpriteNum++;
            var num = _maxSpriteNum;
            return AddSprite<T>(num, name, configure);
        }
        public LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null) => AddSprite<LingoSprite>(num, configure);
        public T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite => AddSprite<T>(num, "Sprite_" + num, configure);

        public T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite
        {
            var sprite = _environment.Factory.CreateSprite<T>(this);
            sprite.Init(num, name);
            //var sprite = new LingoSprite(_environment, this, name, num);
            _sprites.Insert(num - 1, sprite);
            _spritesByName.Add(name, sprite);
            _spriteEventHandlers.Add(sprite);
            if (num > _maxSpriteNum)
                _maxSpriteNum = num;
            if (configure != null)
                configure(sprite);
            return sprite;
        }
        public bool RemoveSprite(string name)
        {
            if (!_spritesByName.TryGetValue(name, out var sprite))
                return false;

            var index = _sprites.IndexOf(sprite);
            _sprites.RemoveAt(index);
            _spritesByName.Remove(name);
            _spriteEventHandlers.Remove(sprite);
            return true;
        }

        public bool TryGetSprite(string name, out ILingoSprite? sprite)
        {
            sprite = null;
            if (_spritesByName.TryGetValue(name, out var sprite1))
            {
                sprite = sprite1;
                return true;
            }
            return false;
        }

        public bool TryGetSprite(int number, out ILingoSprite? sprite)
        {
            if (number <= 0 || number > _sprites.Count)
            {
                sprite = null;
                return false;
            }
            sprite = _sprites[number - 1];
            return true;
        }
        public void SetSpriteMember(int number, string memberName) => GetSprite(number).SetMember(memberName);
        public void SetSpriteMember(int number, int memberNumber) => GetSprite(number).SetMember(memberNumber);
        public string GetSpriteMemberName(int number) => GetSprite(number).Name;
        public void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
          where T : LingoSpriteBehavior
        {
            var sprite = _activeSprites.FirstOrDefault(x => x.SpriteNum == spriteNumber);
            if (sprite == null) return;
            sprite.CallBehavior(actionOnSpriteBehaviour);
        }
        public TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour)
            where T : LingoSpriteBehavior
        {
            var sprite = _activeSprites.FirstOrDefault(x => x.SpriteNum == spriteNumber);
            if (sprite == null) return default;
            return sprite.CallBehavior(actionOnSpriteBehaviour);
        }

        public void SendSprite(string name, Action<LingoSprite> actionOnSprite)
        {
            var sprite = _activeSprites.FirstOrDefault(x => x.Name == Name);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }

        public void SendSprite(int spriteNumber, Action<LingoSprite> actionOnSprite)
        {
            var sprite = _activeSprites.FirstOrDefault(x => x.SpriteNum == spriteNumber);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }
        internal void CallActiveSprites(Action<LingoSprite> actionOnAllActiveSprites)
        {
            foreach (var sprite in _activeSprites)
                actionOnAllActiveSprites(sprite);
        }
        /// <summary>
        /// The rollover() method indicates whether the pointer is over the specified sprite.
        /// </summary>
        public bool RollOver(int spriteNumber)
        {
            var sprite = _sprites[spriteNumber - 1];
            return sprite.IsMouseInsideBoundingBox(_lingoMouse);
        }

        public LingoSprite? GetSpriteUnderMouse()
        {
            // Loop through all sprites and check if the mouse is inside the bounding box of the sprite
            foreach (var sprite in _sprites)
            {
                if (sprite.IsMouseInsideBoundingBox(_lingoMouse))
                {
                    return sprite; // Return the sprite the mouse is over
                }
            }
            return null; // Return null if no sprite is under the mouse
        }
        #endregion



        #region Playhead

        public void Go(string label)
        {
            // TODO: Label lookup
            _currentFrame = 1; // Placeholder fallback
        }

        public void Go(int frame)
        {
            if (frame <= 0)
                throw new ArgumentOutOfRangeException(nameof(frame));

            _lastFrame = _currentFrame;
            _currentFrame = frame;

            AdvanceFrame();
        }

        public void OnTick()
        {
            if (_isPlaying)
                AdvanceFrame();
        }
        public void AdvanceFrame()
        {
            _lastFrame = _currentFrame;
            _currentFrame++;

            _enteredSprites.Clear();
            _exitedSprites.Clear();

            // STEP 1: Find which sprites are entering and exiting
            foreach (var sprite in _sprites)
            {
                sprite.IsActive = sprite.BeginFrame <= _currentFrame && sprite.EndFrame >= _currentFrame;

                bool wasActive = sprite.BeginFrame <= _lastFrame && sprite.EndFrame >= _lastFrame;
                bool isActive = sprite.IsActive;

                // Subscription logic: Subscribe only when sprite is entering and not already subscribed
                if (!wasActive && isActive)
                {
                    _movieStage.DrawSprite(sprite);
                    _enteredSprites.Add(sprite);
                    _activeSprites.Add(sprite);
                    // Subscribe to mouse events if sprite is not already subscribed
                    if (!_lingoMouse.IsSubscribed(sprite))
                    {
                        _lingoMouse.Subscribe(sprite);
                    }
                }
                else if (wasActive && !isActive)
                {
                    _exitedSprites.Add(sprite);
                }
            }

            // STEP 2: Fire beginSprite on new sprites
            foreach (var sprite in _enteredSprites)
                sprite.DoBeginSprite();

            // At the end of each frame, update the mouse state
            _lingoMouse.UpdateMouseState();

            // STEP 3: Fire stepFrame on all active sprites
            CallActiveSprites(s => s.DoStepFrame());

            // STEP 4: Fire prepareFrame on all active sprites
            CallActiveSprites(s => s.DoPrepareFrame());


            // STEP 5: Fire enterFrame on all active sprites
            CallActiveSprites(s => s.DoEnterFrame());
            CallOnMoveScripts(s => s.DoEnterFrame());

            // After enterFrame and before exitFrame, Director handles any time delays
            // required by the tempo setting, idle events, and keyboard and mouse events

            // STEP 6: Call UpdateStage (e.g., rendering the stage content)
            UpdateStage();

            // STEP 7: Fire exitFrame on all active sprites
            CallActiveSprites(s => s.DoExitFrame());
            CallOnMoveScripts(s => s.DoExitFrame());

            /// STEP 8: Fire endSprite on exiting sprites
            foreach (var sprite in _exitedSprites)
            {
                _movieStage.RemoveSprite(sprite);
                sprite.DoEndSprite();
                _activeSprites.Remove(sprite);
                // Unsubscribe from mouse events
                if (_lingoMouse.IsSubscribed(sprite))
                    _lingoMouse.Unsubscribe(sprite);

            }
        }

        internal void CallOnMoveScripts(Action<ILingoMovieScriptListener> actionOnAllActiveSprites)
        {
            foreach (var script in _movieScriptListeners)
                actionOnAllActiveSprites(script);
        }
        // Play the movie
        public void Play()
        {
            // prepareMovie
            // PrepareFrame
            // BeginSprite
            // StartMovie
            _isPlaying = true;
        }

        private void OnStop()
        {
            // TODO EVENTS
            // EndSprite
            // StopMovie
        }
        // Halt the movie
        public void Halt()
        {
            _isPlaying = false;
        }
        public void NextFrame()
        {
            if (_isPlaying)
            {
                if (Frame < FrameCount)
                    Go(Frame + 1);
            }
        }

        public void PrevFrame()
        {
            if (_isPlaying)
            {
                if (Frame > 1)
                    Go(Frame - 1);
            }
        }
        // Go to a specific frame and stop
        public void GoToAndStop(int frame)
        {
            if (frame >= 1 && frame <= FrameCount)
            {
                _currentFrame = frame;
                _isPlaying = false;
            }
        }


        public void UpdateStage()
        {
            _actorList.Invoke();
            _movieStage.UpdateStage();
        } 
        #endregion



        // PuppetTransition (for special effects/animations, implementation is up to you)
        public void PuppetTransition(int effectNumber)
        {
            // Implement specific logic for puppet transition effects (if any)
        }


        #region CastLibs
        public ILingoCastLibsContainer CastLib => _castLibContainer;
        public ILingoMembersContainer Member => _castLibContainer.Member;
        public T? GetMember<T>(int number) where T : class,ILingoMember => _castLibContainer.GetMember<T>(number);
        public T? GetMember<T>(string name) where T : class, ILingoMember => _castLibContainer.GetMember<T>(name);
        #endregion


        internal LingoMovieEnvironment GetEnvironment() => _environment;
        public IServiceProvider GetServiceProvider() => _environment.GetServiceProvider();

       
    }
}
