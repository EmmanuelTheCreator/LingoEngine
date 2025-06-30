using System;
using System.Collections.Generic;
using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Sounds;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Projects;

namespace LingoEngine.Movies
{


    public class LingoMovie : ILingoMovie, ILingoClockListener, IDisposable
    {
        private readonly ILingoMemberFactory _memberFactory;
        private ILingoFrameworkMovie _FrameworkMovie;
        private readonly LingoMovieEnvironment _environment;
        private readonly LingoStage _stage;
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
        private readonly LingoFrameManager _frameManager;
        private readonly LingoSpriteManager _spriteManager;
        private bool _IsManualUpdateStage;
        public event Action? SpriteListChanged;
        public event Action? AudioClipListChanged;

        private readonly List<LingoMovieAudioClip> _audioClips = new();

        private void RaiseSpriteListChanged()
            => SpriteListChanged?.Invoke();
        private void RaiseAudioClipListChanged()
            => AudioClipListChanged?.Invoke();

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
        public int SpriteTotalCount => _spriteManager.SpriteTotalCount;
        public int SpriteMaxNumber => _spriteManager.SpriteMaxNumber;
        public int LastChannel => _spriteManager.MaxSpriteChannelCount;
        public int LastFrame => FrameCount;
        public IReadOnlyDictionary<int, string> MarkerList =>
            _frameManager.MarkerList;
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
            get => _spriteManager.MaxSpriteChannelCount;
            set => _spriteManager.MaxSpriteChannelCount = value;
        }
        public bool IsPlaying => _isPlaying;

        public event Action<bool>? PlayStateChanged;

        public ActorList ActorList => _actorList;
        public LingoTimeOutList TimeOutList { get; private set; } = new LingoTimeOutList();

        #endregion


#pragma warning disable CS8618
        protected internal LingoMovie(LingoMovieEnvironment environment, LingoStage movieStage, LingoCastLibsContainer castLibContainer, ILingoMemberFactory memberFactory, string name, int number, LingoEventMediator mediator, Action<LingoMovie> onRemoveMe, LingoProjectSettings projectSettings)
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
            _spriteManager = new LingoSpriteManager(this, environment, RaiseSpriteListChanged);
            MaxSpriteChannelCount = projectSettings.MaxSpriteChannelCount;
            _frameManager = new LingoFrameManager(this, environment, _spriteManager.AllTimeSprites, RaiseSpriteListChanged);
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
        public ILingoSpriteChannel Channel(int channelNumber) => _spriteManager.Channel(channelNumber);
        public void PuppetSprite(int number, bool isPuppetSprite) => _spriteManager.PuppetSprite(number, isPuppetSprite);
        public ILingoSpriteChannel GetActiveSprite(int number) => _spriteManager.GetActiveSprite(number);
        public LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null) => _spriteManager.AddSprite(name, configure);
        public T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite => _spriteManager.AddSprite<T>(name, configure);
        public LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null) => _spriteManager.AddSprite(num, configure);
        public LingoSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoSprite>? configure = null) where TBehaviour : LingoSpriteBehavior
            => _spriteManager.AddFrameBehavior(frameNumber, configureBehaviour, configure);
        public LingoSprite AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite>? configure = null)
            => _spriteManager.AddSprite(num, begin, end, x, y, configure);
        public T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite => _spriteManager.AddSprite<T>(num, configure);
        public T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite => _spriteManager.AddSprite<T>(num, name, configure);
        public bool RemoveSprite(string name) => _spriteManager.RemoveSprite(name);
        public bool TryGetAllTimeSprite(string name, out ILingoSprite? sprite) => _spriteManager.TryGetAllTimeSprite(name, out sprite);
        public bool TryGetAllTimeSprite(int number, out ILingoSprite? sprite) => _spriteManager.TryGetAllTimeSprite(number, out sprite);
        public void SetSpriteMember(int number, string memberName) => _spriteManager.SetSpriteMember(number, memberName);
        public void SetSpriteMember(int number, int memberNumber) => _spriteManager.SetSpriteMember(number, memberNumber);
        public void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour) where T : LingoSpriteBehavior => _spriteManager.SendSprite(spriteNumber, actionOnSpriteBehaviour);
        public TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour) where T : LingoSpriteBehavior => _spriteManager.SendSprite<T, TResult>(spriteNumber, actionOnSpriteBehaviour);
        public void SendSprite(string name, Action<ILingoSpriteChannel> actionOnSprite) => _spriteManager.SendSprite(name, actionOnSprite);
        public void SendSprite(int spriteNumber, Action<ILingoSpriteChannel> actionOnSprite) => _spriteManager.SendSprite(spriteNumber, actionOnSprite);
        public void SendAllSprites(Action<ILingoSpriteChannel> actionOnSprite) => _spriteManager.SendAllSprites(actionOnSprite);
        public void SendAllSprites<T>(Action<T> actionOnSprite) where T : LingoSpriteBehavior => _spriteManager.SendAllSprites(actionOnSprite);
        public IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior => _spriteManager.SendAllSprites<T, TResult>(actionOnSprite);
        public bool RollOver(int spriteNumber) => _spriteManager.RollOver(spriteNumber);
        public int RollOver() => _spriteManager.RollOver();
        public int ConstrainH(int spriteNumber, int pos) => _spriteManager.ConstrainH(spriteNumber, pos);
        public int ConstrainV(int spriteNumber, int pos) => _spriteManager.ConstrainV(spriteNumber, pos);
        public LingoSprite? GetSpriteUnderMouse(bool skipLockedSprites = false) => _spriteManager.GetSpriteUnderMouse(skipLockedSprites);
        public IEnumerable<LingoSprite> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false) => _spriteManager.GetSpritesAtPoint(x, y, skipLockedSprites);
        public LingoSprite? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false) => _spriteManager.GetSpriteAtPoint(x, y, skipLockedSprites);
        public void ChangeSpriteChannel(ILingoSprite sprite, int newChannel) => _spriteManager.ChangeSpriteChannel(sprite, newChannel);

        #endregion



        #region Playhead

        public void GoTo(string label) => Go(label);
        public void Go(string label)
        {
            if (_frameManager.ScoreLabels.TryGetValue(label, out var scoreLabel))
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
                if (_delayTicks > 0)
                {
                    _delayTicks--;
                    return;
                }
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
                if (_NextFrame < 0)
                    _currentFrame++;
                else
                {
                    _currentFrame = _NextFrame;
                    _NextFrame = -1;
                }

                _spriteManager.UpdateActiveSprites(_currentFrame, _lastFrame);
                _spriteManager.BeginSprites();

                if (_needToRaiseStartMovie)
                    _EventMediator.RaiseStartMovie();

                _lingoMouse.UpdateMouseState();

                _EventMediator.RaiseStepFrame();
                _EventMediator.RaisePrepareFrame();
                _EventMediator.RaiseEnterFrame();

                OnUpdateStage();

                _EventMediator.RaiseExitFrame();
            }
            finally
            {
                _spriteManager.EndSprites();
                _isAdvancing = false;
            }

        }

        private void DoEndSprite()
        {
            _spriteManager.EndSprites();
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
            PlayStateChanged?.Invoke(true);
            OnTick();
            _needToRaiseStartMovie = false;
           
        }

        private void OnStop()
        {
            _isPlaying = false;
            PlayStateChanged?.Invoke(false);
            _spriteManager.EndSprites();
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

        private int _delayTicks;
        public void Delay(int ticks)
        {
            if (ticks <= 0) return;
            _delayTicks += ticks;
        }

        public void GoNext()
            => Go(_frameManager.GetNextMarker(Frame));

        public void GoPrevious()
            => Go(_frameManager.GetPreviousMarker(Frame));

        public void GoLoop()
            => Go(_frameManager.GetLoopMarker(Frame));

        public void InsertFrame()
        {
            // TODO: Implement score recording frame duplication
        }

        public void DeleteFrame()
        {
            // TODO: Implement frame deletion during score recording
        }

        public void UpdateFrame()
        {
            // TODO: Finalize changes for current frame during recording
        }
        // Go to a specific frame and stop
        public void GoToAndStop(int frame)
        {
            if (frame >= 1 && frame <= FrameCount)
            {
                // Jump directly to the requested frame while ensuring sprite
                // lifecycle events are fired. The existing AdvanceFrame logic
                // already handles begin/end sprite events when the playhead
                // moves to a new frame, so reuse it by setting the next frame
                // and manually advancing once.
                _NextFrame = frame;
                AdvanceFrame();
                _isPlaying = false;
                PlayStateChanged?.Invoke(false);
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



        public LingoMovieEnvironment GetEnvironment() => _environment;
        public IServiceProvider GetServiceProvider() => _environment.GetServiceProvider();

        public void StartTimer() => Timer = 0;

        public void SetScoreLabel(int frameNumber, string? name)
            => _frameManager.SetScoreLabel(frameNumber, name);

        public IReadOnlyDictionary<string, int> GetScoreLabels() => _frameManager.ScoreLabels;
        public IReadOnlyDictionary<int, LingoSprite> GetFrameSpriteBehaviors() => _spriteManager.FrameSpriteBehaviors;

        public void MoveFrameBehavior(int previousFrame, int newFrame)
            => _spriteManager.MoveFrameBehavior(previousFrame, newFrame);

        public int GetNextLabelFrame(int frame)
            => _frameManager.GetNextLabelFrame(frame);

        public int GetNextSpriteStart(int channel, int frame)
            => _frameManager.GetNextSpriteStart(channel, frame);

        public int GetPrevSpriteEnd(int channel, int frame)
            => _frameManager.GetPrevSpriteEnd(channel, frame);

        public IReadOnlyList<LingoMovieAudioClip> GetAudioClips() => _audioClips;

        public LingoMovieAudioClip AddAudioClip(int channel, int frame, LingoMemberSound sound)
        {
            int lengthFrames = (int)Math.Ceiling(sound.Length * Tempo);
            int end = Math.Clamp(frame + lengthFrames - 1, frame, FrameCount);
            var clip = new LingoMovieAudioClip(channel, frame, end, sound);
            _audioClips.Add(clip);
            RaiseAudioClipListChanged();
            return clip;
        }

        public void MoveAudioClip(LingoMovieAudioClip clip, int newFrame)
        {
            if (!_audioClips.Contains(clip)) return;
            int lengthFrames = clip.EndFrame - clip.BeginFrame;
            clip.BeginFrame = newFrame;
            clip.EndFrame = Math.Clamp(newFrame + lengthFrames, newFrame, FrameCount);
            RaiseAudioClipListChanged();
        }

        public int GetMaxLocZ() => _spriteManager.GetMaxLocZ();

        public ILingoMemberFactory New => _memberFactory;

        public LingoMember? MouseMemberUnderMouse() // todo : implement
            => null;

    }
}
