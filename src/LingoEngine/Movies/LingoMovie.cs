using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;

namespace LingoEngine.Movies
{


    public class LingoMovie : ILingoMovie, ILingoClockListener, IDisposable
    {
        private readonly ILingoMemberFactory _memberFactory;
        private ILingoFrameworkMovie _FrameworkMovie;
        private readonly LingoMovieEnvironment _environment;
        private readonly LingoStage _stage;
        private int _maxSpriteNum = 0;
        private readonly LingoStage _movieStage;
        private readonly Action<LingoMovie> _onRemoveMe;
        private readonly LingoMouse _lingoMouse;
        private readonly LingoClock _lingoClock;
        private int _currentFrame = 0;
        private int _NextFrame = -1;
        private int _lastFrame = 0;
        private bool _isPlaying = false;
        private int _tempo = 30;  // Default frame rate (FPS)
        private bool _needToRaiseStartMovie = false;
        private LingoCastLibsContainer _castLibContainer;
        private int _maxSpriteChannelCount;
        private Dictionary<int, LingoSpriteChannel> _spriteChannels= new();
        private Dictionary<string, int> _scoreLabels = new();
        private Dictionary<string, LingoSprite> _spritesByName = new();
        private List<LingoSprite> _allTimeSprites = new ();
        private Dictionary<int,LingoSprite> _frameSpriteBehaviors = new ();
        private readonly Dictionary<int,LingoSprite> _activeSprites = new();
        private readonly List<LingoSprite> _enteredSprites = new();
        private readonly List<LingoSprite> _exitedSprites = new();
        private bool _IsManualUpdateStage;
        private LingoSprite? _currentFrameSprite;

        // Movie Script subscriptions
        private readonly ActorList _actorList = new ActorList();
        private readonly LingoMovieScriptContainer _MovieScripts;


        #region Properties

        public ILingoFrameworkMovie FrameworkObj => _FrameworkMovie;
        public T Framework<T>() where T : class, ILingoFrameworkMovie => (T)_FrameworkMovie;

        public string Name { get; set; }
       
        public int Number { get; private set; }

        private readonly LingoEventMediator _EventMediator;

        public int Frame => _currentFrame;
        public int CurrentFrame => _currentFrame;
        public int FrameCount => 620; 
        public int Timer { get; private set; }
        public int SpriteTotalCount => _activeSprites.Count;
        public int SpriteMaxNumber => _activeSprites.Keys.Max();
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
        public int MaxSpriteChannelCount
        {
            get => _maxSpriteChannelCount;
            set
            {
                if (value > 0)
                {
                    _maxSpriteChannelCount = value;
                    if (_spriteChannels.Count < _maxSpriteChannelCount)
                    {
                        for (int i = _spriteChannels.Count; i < _maxSpriteChannelCount; i++)
                            _spriteChannels.Add(i, new LingoSpriteChannel(i)); // we need zero based because 0 is the frame script.
                    }
                }
            }
        }
        public bool IsPlaying => _isPlaying;

        public ActorList ActorList => _actorList;
        public LingoTimeOutList TimeOutList { get; private set; } = new LingoTimeOutList();

        #endregion


#pragma warning disable CS8618
        internal LingoMovie(LingoMovieEnvironment environment, LingoStage movieStage, LingoCastLibsContainer castLibContainer, ILingoMemberFactory memberFactory, string name, int number, LingoEventMediator mediator, Action<LingoMovie> onRemoveMe)
#pragma warning restore CS8618 
        {
            _castLibContainer = castLibContainer;
            _environment = environment;
            _stage = movieStage;
            _memberFactory = memberFactory;
            _environment = environment;
            _movieStage = movieStage;
            _onRemoveMe = onRemoveMe;
            Name = name;
            Number = number;
            _EventMediator = mediator;
            _MovieScripts = new(environment, mediator);
            _lingoMouse = (LingoMouse)environment.Mouse;
            _lingoClock = (LingoClock)environment.Clock;
            MaxSpriteChannelCount = 1000;
        }
        public void Init(ILingoFrameworkMovie frameworkMovie)
        {
            _FrameworkMovie = frameworkMovie;
        }
        public void Dispose()
        {
            RemoveMe();
        }
        public void RemoveMe()
        {
            Hide();
            _onRemoveMe(this);
        }

        internal void Show()
        {
            _lingoClock.Subscribe(this);
        }
        internal void Hide()
        {
            _lingoClock.Unsubscribe(this);
        }


       
        


        #region Sprites
        public ILingoSpriteChannel Channel(int channelNumber) =>_spriteChannels[channelNumber];
        public void PuppetSprite(int number, bool isPuppetSprite) => CallActiveSprite(number, sprite => sprite.IsPuppetSprite = isPuppetSprite);
       
        public ILingoSpriteChannel GetActiveSprite(int number) => _spriteChannels[number];
        public LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null) => AddSprite<LingoSprite>(name, configure);
        public T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite
        {
            _maxSpriteNum++;
            var num = _maxSpriteNum;
            return AddSprite<T>(num, name, configure);
        }
        public LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null) => AddSprite<LingoSprite>(num, configure);
        public LingoSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoSprite>? configure = null) where TBehaviour : LingoSpriteBehavior
        {
            var sprite = _environment.Factory.CreateSprite<LingoSprite>(this, s =>
            {
                // On remove method
                var index = _frameSpriteBehaviors.Remove(frameNumber);
            });
            sprite.Init(0, "FrameSprite_"+frameNumber);
            if (_frameSpriteBehaviors.ContainsKey(frameNumber))
                _frameSpriteBehaviors[frameNumber] = sprite;
            else
                _frameSpriteBehaviors.Add(frameNumber, sprite);
            sprite.BeginFrame = frameNumber;
            sprite.EndFrame = frameNumber;
            
            var behaviour = sprite.SetBehavior<TBehaviour>();
            if (configureBehaviour != null) configureBehaviour(behaviour);
            if (configure != null) configure(sprite);
            return sprite;
        }
          
        public LingoSprite AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite>? configure = null)
            => AddSprite<LingoSprite>(num, c =>
            {
                c.BeginFrame = begin;
                c.EndFrame = end;
                c.LocH = x; c.LocV = y;
                if (configure != null)
                    configure(c);
            });
        public T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite => AddSprite<T>(num, "Sprite_" + num, configure);

        public T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite
        {
            var sprite = _environment.Factory.CreateSprite<T>(this, s =>
            {
                // On remove method
                var index = _allTimeSprites.IndexOf(s);
                _allTimeSprites.RemoveAt(index);
                _spritesByName.Remove(name);
            });
            sprite.Init(num, name);
            //var sprite = new LingoSprite(_environment, this, name, num);
            _allTimeSprites.Add(sprite);
            if (!_spritesByName.ContainsKey(name))
                _spritesByName.Add(name, sprite);
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
            sprite.RemoveMe();
            return true;
        }

        public bool TryGetAllTimeSprite(string name, out ILingoSprite? sprite)
        {
            sprite = null;
            if (_spritesByName.TryGetValue(name, out var sprite1))
            {
                sprite = sprite1;
                return true;
            }
            return false;
        }

        public bool TryGetAllTimeSprite(int number, out ILingoSprite? sprite)
        {
            if (number <= 0 || number > _allTimeSprites.Count)
            {
                sprite = null;
                return false;
            }
            sprite = _allTimeSprites[number - 1];
            return true;
        }
        public void SetSpriteMember(int number, string memberName) => CallActiveSprite(number, s => s.SetMember(memberName));
        public void SetSpriteMember(int number, int memberNumber) => CallActiveSprite(number, s => s.SetMember(memberNumber));
        public void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
          where T : LingoSpriteBehavior
                  => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));
        public TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour)
            where T : LingoSpriteBehavior
                => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));
        

        public void SendSprite(string name, Action<ILingoSpriteChannel> actionOnSprite)
        {
            // uses sprite channels, for visibility
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.Name == name);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }

        public void SendSprite(int spriteNumber, Action<ILingoSpriteChannel> actionOnSprite)
        {
            // uses sprite channels, for visibility
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.SpriteNum == spriteNumber);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }
       
        /// <summary>
        /// The rollover() method indicates whether the pointer is over the specified sprite.
        /// </summary>
        public bool RollOver(int spriteNumber)
        {
            var sprite = _activeSprites[spriteNumber];
            return sprite.IsMouseInsideBoundingBox(_lingoMouse);
        }

        public LingoSprite? GetSpriteUnderMouse()
        {
            // Loop through all sprites and check if the mouse is inside the bounding box of the sprite
            foreach (var sprite in _activeSprites.Values)
            {
                if (sprite.IsMouseInsideBoundingBox(_lingoMouse))
                {
                    return sprite; // Return the sprite the mouse is over
                }
            }
            return null; // Return null if no sprite is under the mouse
        }
        private void CallActiveSprites(Action<LingoSprite> actionOnAllActiveSprites)
        {
            foreach (var sprite in _activeSprites.Values)
                actionOnAllActiveSprites(sprite);
        }
        private void CallActiveSprite(int number, Action<LingoSprite> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return;
            spriteAction(sprite);
        }
        private TResult? CallActiveSprite<TResult>(int number, Func<LingoSprite, TResult?> spriteAction) 
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return default;
            return spriteAction(sprite);
        }
     
        #endregion



        #region Playhead

        public void GoTo(string label) => Go(label);
        public void Go(string label)
        {
            if (_scoreLabels.TryGetValue(label, out var scoreLabel))
                _NextFrame = scoreLabel; 
        }

        public void GoTo(int frame) => Go(frame);
        public void Go(int frame)
        {
            if (frame <= 0)
                throw new ArgumentOutOfRangeException(nameof(frame));
            _NextFrame = frame;
        }

        public void OnTick()
        {
            if (_isPlaying)
            {
                if(_IsManualUpdateStage)
                    OnUpdateStage();
                else
                    AdvanceFrame();
            }
        }
        private bool _isAdvancing;
        public void AdvanceFrame()
        {
            if (_isAdvancing) return;
            _isAdvancing = true;
            try
            {
                var frameChanged = _currentFrame != _lastFrame;
                _lastFrame = _currentFrame;
                if (_NextFrame<0)
                    _currentFrame++;
                else
                {
                    _currentFrame = _NextFrame;
                    _NextFrame = -1;
                }
                _enteredSprites.Clear();
                _exitedSprites.Clear();

                // STEP 1: Find which sprites are entering and exiting
                foreach (var sprite in _allTimeSprites)
                {
                    if (sprite == null) continue;
                    sprite.IsActive = sprite.BeginFrame <= _currentFrame && sprite.EndFrame >= _currentFrame; // &&  _spriteChannels[sprite.SpriteNum - 1].Visibility;

                    bool wasActive = sprite.BeginFrame <= _lastFrame && sprite.EndFrame >= _lastFrame;
                    bool isActive = sprite.IsActive;

                    // Subscription logic: Subscribe only when sprite is entering and not already subscribed
                    if (!wasActive && isActive)
                    {
                        sprite.FrameworkObj.Show();
                        _enteredSprites.Add(sprite);
                        if (_activeSprites.TryGetValue(sprite.SpriteNum, out var existingSprite))
                            throw new Exception($"Operlapping sprites:{existingSprite.Name}:{existingSprite.Member?.Name} and {sprite.Name}:{sprite.Member?.Name}");
                        _spriteChannels[sprite.SpriteNum].SetSprite(sprite);
                        _activeSprites.Add(sprite.SpriteNum, sprite);
                        // Subscribe to mouse events if sprite is not already subscribed
                        if (!_lingoMouse.IsSubscribed(sprite))
                            _lingoMouse.Subscribe(sprite);
                    }
                    else if (wasActive && !isActive)
                        _exitedSprites.Add(sprite);
                }

                if (_frameSpriteBehaviors.TryGetValue(_currentFrame, out var frameSprite))
                    _currentFrameSprite = frameSprite;
                else
                    _currentFrameSprite = null;



                // STEP 2: Fire beginSprite on new sprites
                foreach (var sprite in _enteredSprites)
                        sprite.DoBeginSprite();

                _currentFrameSprite?.DoBeginSprite(); // must always be Latest

                //_EventMediator.RaiseBeginSprite(); -> not alowed, only the _enteredSprites may do this

                if (_needToRaiseStartMovie)
                    _EventMediator.RaiseStartMovie();

                // At the end of each frame, update the mouse state
                _lingoMouse.UpdateMouseState();

                // STEP 3: Fire stepFrame on all active sprites
                //_currentFrameSprite?.DoStepFrame();// must always be first
                //CallActiveSprites(s => s.DoStepFrame());
                _EventMediator.RaiseStepFrame();

                // STEP 4: Fire prepareFrame on all active sprites
                //CallActiveSprites(s => s.DoPrepareFrame());
                _EventMediator.RaisePrepareFrame();
                //_currentFrameSprite?.DoPrepareFrame();

                // STEP 5: Fire enterFrame on all active sprites
                //_currentFrameSprite?.DoEnterFrame();// must always be first
                //CallActiveSprites(s => s.DoEnterFrame());
                _EventMediator.RaiseEnterFrame();

                // After enterFrame and before exitFrame, Director handles any time delays
                // required by the tempo setting, idle events, and keyboard and mouse events

                // STEP 6: Call UpdateStage (e.g., rendering the stage content)
                OnUpdateStage();

                // STEP 7: Fire exitFrame on all active sprites
                //_currentFrameSprite?.DoExitFrame(); // must always be first
                //CallActiveSprites(s => s.DoExitFrame());
                _EventMediator.RaiseExitFrame();
            }
            finally
            {
                /// STEP 8: Fire endSprite on exiting sprites
                DoEndSprite();
                _currentFrameSprite = null;
                _isAdvancing = false;
            }
            
        }

        private void DoEndSprite()
        {
            //_EventMediator.RaiseEndSprite();-> not alowed, only the _exitedSprites may do this
            _currentFrameSprite?.DoEndSprite();
            foreach (var sprite in _exitedSprites)
            {
                sprite.FrameworkObj.Hide();
                sprite.DoEndSprite();
                _activeSprites.Remove(sprite.SpriteNum);
                _spriteChannels[sprite.SpriteNum].RemoveSprite();
                // Unsubscribe from mouse events
                if (_lingoMouse.IsSubscribed(sprite))
                    _lingoMouse.Unsubscribe(sprite);
            }
        }

        // Play the movie
        public void Play()
        {
            _EventMediator.RaisePrepareMovie();
            _needToRaiseStartMovie = true;
            // prepareMovie
            // PrepareFrame
            // BeginSprite
            // StartMovie
            _isPlaying = true;
            OnTick();
            _needToRaiseStartMovie = false;
           
        }

        private void OnStop()
        {
            _isPlaying = false;
            DoEndSprite();
            _EventMediator.RaiseStopMovie();
            // EndSprite
            // StopMovie
        }
        // Halt the movie
        public void Halt()
        {
            OnStop();
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
            // a manual update stage needs to run on same framerate, it means that the head player will not advance
            _IsManualUpdateStage = true;
        }
        private void OnUpdateStage()
        {
            
            Timer++;
            _actorList.Invoke();
            _FrameworkMovie.UpdateStage();
            _IsManualUpdateStage = false;
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


        #region MovieScripts
        public ILingoMovie AddMovieScript<T>()
            where T: LingoMovieScript
        {
            _MovieScripts.Add<T>();
            return this;
        } 
        public void CallMovieScript<T>(Action<T> action) where T : LingoMovieScript
            => _MovieScripts.Call(action);
        public TResult? CallMovieScript<T, TResult>(Func<T, TResult> action) where T : LingoMovieScript
            => _MovieScripts.Call(action);
           
        private void CallOnAllMovieScripts(Action<LingoMovieScript> actionOnAll)
            => _MovieScripts.CallAll(actionOnAll);

        #endregion


        internal LingoMovieEnvironment GetEnvironment() => _environment;
        public IServiceProvider GetServiceProvider() => _environment.GetServiceProvider();

        public void StartTimer() => Timer = 0;

        public void SetScoreLabel(int frameNumber, string? name)
        {
            string? existingLabel = null;

            // search for any label already associated with this frame
            foreach (var item in _scoreLabels)
            {
                if (item.Value == frameNumber)
                {
                    existingLabel = item.Key;
                    break;
                }
            }

            if (existingLabel != null)
                _scoreLabels.Remove(existingLabel);

            if (!string.IsNullOrEmpty(name))
                _scoreLabels[name] = frameNumber;
        }

        internal IReadOnlyDictionary<string, int> GetScoreLabels() => _scoreLabels;
        internal IReadOnlyDictionary<int, LingoSprite> GetFrameSpriteBehaviors() => _frameSpriteBehaviors;

        internal int GetMaxLocZ() => _activeSprites.Values.Max(x => x.LocZ);

        public ILingoMemberFactory New => _memberFactory;

        public LingoMember? MouseMemberUnderMouse() // todo : implement
            => null;
    }
}
