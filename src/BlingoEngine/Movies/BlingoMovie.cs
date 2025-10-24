using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using BlingoEngine.Projects;
using BlingoEngine.Transitions;
using BlingoEngine.Tempos;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Scripts;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Components;

namespace BlingoEngine.Movies
{


    public class BlingoMovie : IBlingoMovie, IBlingoClockListener, IDisposable
    {
        private readonly IBlingoMemberFactory _memberFactory;
        private IBlingoFrameworkMovie _frameworkMovie;
        private readonly BlingoMovieEnvironment _environment;
        private readonly Action<BlingoMovie> _onRemoveMe;
        private readonly BlingoClock _blingoClock;
        private BlingoStageMouse _blingoMouse;
        private readonly BlingoStage _stage;
        private readonly IBlingoTransitionPlayer _transitionPlayer;
        private bool _skipStepFrame;
        private int _currentFrame = 0;
        private int _nextFrame = -1;
        private int _lastFrame = 0;
        private bool _isPlaying = false;
        private readonly BlingoEventMediator _eventMediator;

        private bool _needToRaiseStartMovie = false;
        private BlingoCastLibsContainer _castLibContainer;
        private readonly BlingoFrameLabelManager _frameLabelManager;
        private readonly BlingoSprite2DManager _sprite2DManager;
        private bool _isManualUpdateStage;
        public event Action<int>? Sprite2DListChanged { add => _sprite2DManager.SpriteListChanged += value; remove => _sprite2DManager.SpriteListChanged -= value; }

        private readonly BlingoSpriteAudioManager _audioManager;
        private readonly BlingoSpriteTransitionManager _transitionManager;
        private readonly BlingoTempoSpriteManager _tempoManager;
        private readonly BlingoSpriteColorPaletteSpriteManager _paletteManager;
        private readonly BlingoFrameScriptSpriteManager _frameScriptManager;

        // Movie Script subscriptions
        private readonly ActorList _actorList = new ActorList();
        private readonly BlingoMovieScriptContainer _movieScripts;
        private readonly List<BlingoSpriteManager> _spriteManagers = new();


        #region Properties

        
        public T Framework<T>() where T : IAbstFrameworkNode => (T)_frameworkMovie;

        public float X { get; set; }
        public float Y { get; set; }
        public float Width
        {
            get => _stage.Width;
            set
            {
                if (_stage.Width == value) return;
                _stage.Width = value;
                _frameworkMovie.Width = value;
            }
        }
        public float Height
        {
            get => _stage.Height;
            set
            {
                if (_stage.Height == value) return;
                _stage.Height = value;
                _frameworkMovie.Height = value;
            }
        }
        public bool Visibility { get => _frameworkMovie.Visibility; set => _frameworkMovie.Visibility = value; }
        public AMargin Margin { get => _frameworkMovie.Margin; set => _frameworkMovie.Margin = value; }
        public int ZIndex { get => _frameworkMovie.ZIndex; set => _frameworkMovie.ZIndex = value; }
        public IAbstFrameworkNode FrameworkObj { get => _frameworkMovie; set => throw new NotImplementedException(); } // not allowed to set.

        public IBlingoSpriteAudioManager Audio => _audioManager;
        public IBlingoSpriteTransitionManager Transitions => _transitionManager;
        public IBlingoTempoSpriteManager Tempos => _tempoManager;
        public IBlingoSpriteColorPaletteSpriteManager ColorPalettes => _paletteManager;
        public IBlingoFrameScriptSpriteManager FrameScripts => _frameScriptManager;
        public BlingoSprite2DManager Sprite2DManager => _sprite2DManager;
        public IBlingoFrameLabelManager FrameLabels => _frameLabelManager;

        public BlingoSpriteManager? GetSpriteManager(BlingoSpriteType spriteType) => spriteType switch
        {
            BlingoSpriteType.Sprite2D => _sprite2DManager,
            BlingoSpriteType.Tempo => _tempoManager,
            BlingoSpriteType.ColorPalette => _paletteManager,
            BlingoSpriteType.FrameScript => _frameScriptManager,
            BlingoSpriteType.Transition => _transitionManager,
            BlingoSpriteType.Sound => _audioManager,
            _ => null,
        };

        public BlingoSprite? GetSprite(BlingoSpriteRef spriteRef)
        {
            if (spriteRef.SpriteType != BlingoSpriteType.Unknown)
            {
                var manager = GetSpriteManager(spriteRef.SpriteType);
                return manager?.GetSprite(spriteRef);
            }

            var resolved = _sprite2DManager.GetSprite(spriteRef);
            if (resolved != null)
                return resolved;

            foreach (var manager in _spriteManagers)
            {
                resolved = manager.GetSprite(spriteRef);
                if (resolved != null)
                    return resolved;
            }

            return null;
        }

        public string Name { get; set; }

        public int Number { get; private set; }


        private string _about = string.Empty;
        private string _copyright = string.Empty;
        private string _userName = string.Empty;
        private string _companyName = string.Empty;

        public string About
        {
            get => _about;
            set => SetProperty(ref _about, value);
        }
        public string Copyright
        {
            get => _copyright;
            set => SetProperty(ref _copyright, value);
        }
        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }
        public string CompanyName
        {
            get => _companyName;
            set => SetProperty(ref _companyName, value);
        }

        

        public int Frame => _currentFrame;
        public int CurrentFrame => _currentFrame;
        private int _frameCount = 620;
        public int FrameCount
        {
            get => _frameCount;
            private set => SetProperty(ref _frameCount, value);
        }
        public int Timer { get; private set; }
        public int SpriteTotalCount => _sprite2DManager.SpriteTotalCount;
        public int SpriteMaxNumber => _sprite2DManager.SpriteMaxNumber;
        public int LastChannel => _sprite2DManager.MaxSpriteChannelCount;
        public int LastFrame => FrameCount;
        public IReadOnlyDictionary<int, string> MarkerList =>
            _frameLabelManager.MarkerList;
        // Tempo (Frame Rate)
        public int Tempo
        {
            get => _tempoManager.Tempo;
            set => _tempoManager.ChangeTempo(value);
        }
        public int MaxSpriteChannelCount
        {
            get => _sprite2DManager.MaxSpriteChannelCount;
            set => _sprite2DManager.MaxSpriteChannelCount = value;
        }
        public bool IsPlaying => _isPlaying;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<bool>? PlayStateChanged;
        public event Action<int>? CurrentFrameChanged;

        public ActorList ActorList => _actorList;
        public BlingoTimeOutList TimeOutList { get; private set; } = new BlingoTimeOutList();

        #endregion


#pragma warning disable CS8618
        protected internal BlingoMovie(BlingoMovieEnvironment environment, BlingoStage movieStage, IBlingoTransitionPlayer transitionPlayer, BlingoCastLibsContainer castLibContainer, IBlingoMemberFactory memberFactory, string name, int number, BlingoEventMediator mediator, Action<BlingoMovie> onRemoveMe, BlingoProjectSettings projectSettings, IBlingoFrameLabelManager blingoFrameLabelManager)
#pragma warning restore CS8618
        {
            _castLibContainer = castLibContainer;
            _environment = environment;
            _memberFactory = memberFactory;
            _environment = environment;
            _onRemoveMe = onRemoveMe;
            Name = name;
            Number = number;
            _eventMediator = mediator;
            _movieScripts = new(environment, mediator);
            _blingoMouse = (BlingoStageMouse)environment.Mouse;
            _blingoClock = (BlingoClock)environment.Clock;
            _stage = movieStage;
            _transitionPlayer = transitionPlayer;

            _sprite2DManager = new BlingoSprite2DManager(this, environment);
            MaxSpriteChannelCount = projectSettings.MaxSpriteChannelCount;
            _frameLabelManager = (BlingoFrameLabelManager)blingoFrameLabelManager;
            _audioManager = new BlingoSpriteAudioManager(this, environment);
            _transitionManager = new BlingoSpriteTransitionManager(this, environment);
            _tempoManager = new BlingoTempoSpriteManager(this, environment);
            _paletteManager = new BlingoSpriteColorPaletteSpriteManager(this, environment);
            _frameScriptManager = new BlingoFrameScriptSpriteManager(this, environment);

            _spriteManagers.Add(_tempoManager);
            _spriteManagers.Add(_paletteManager);
            _spriteManagers.Add(_transitionManager);
            _spriteManagers.Add(_audioManager);
            _spriteManagers.Add(_frameScriptManager);
        }
        public void Init(IBlingoFrameworkMovie frameworkMovie)
        {
            _frameworkMovie = frameworkMovie;
        }
        public void Dispose()
        {

            RemoveMe();
            _transitionPlayer.Dispose();
        }
        public void RemoveMe()
        {
            Hide();
            _onRemoveMe(this);
        }

        internal void Show()
        {
            _blingoClock.Subscribe(this);
        }
        internal void Hide()
        {
            _blingoClock.Unsubscribe(this);
        }






        #region Sprite2Ds
        public IBlingoSprite? GetSprite(int channelNumber) => _sprite2DManager.Channel(channelNumber).Sprite;
        public IBlingoSpriteChannel Channel(int channelNumber) => _sprite2DManager.Channel(channelNumber);
        public void PuppetSprite(int number, bool isPuppetSprite) => Channel(number).Puppet = isPuppetSprite;
        public IBlingoSpriteChannel GetActiveSprite(int number) => _sprite2DManager.GetActiveSprite(number);
        public BlingoSprite2D AddSprite(string name, Action<BlingoSprite2D>? configure = null) => _sprite2DManager.AddSprite(name, configure);
        public BlingoSprite2D AddSprite(int num, Action<BlingoSprite2D>? configure = null) => _sprite2DManager.AddSprite(num, configure);
        public BlingoFrameScriptSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<BlingoFrameScriptSprite>? configure = null) where TBehaviour : BlingoSpriteBehavior
            => _frameScriptManager.Add(frameNumber, configureBehaviour, configure);
        public BlingoSprite2D AddSprite(int num, string name, Action<BlingoSprite2D>? configure = null) => _sprite2DManager.AddSprite(num, name, configure);
        public BlingoSprite? AddSpriteByChannelNum(int spriteNumWithChannel, int begin, int end, IBlingoMember? member)
        {
            if (spriteNumWithChannel < _spriteManagers.Count)
            {
                var sprite = _spriteManagers[spriteNumWithChannel].Add(spriteNumWithChannel, begin, end, member);
                return sprite;
            }
            var sprite2D = _sprite2DManager.Add(spriteNumWithChannel, begin, end, member);
            return sprite2D;
        }
        public BlingoSprite2D AddSprite(int num, int begin, int end, float x, float y, Action<BlingoSprite2D>? configure = null)
            => _sprite2DManager.AddSprite(num, begin, end, x, y, configure);
        public bool RemoveSprite(string name) => _sprite2DManager.RemoveSprite(name);
        public bool RemoveSprite(BlingoSprite2D sprite) => _sprite2DManager.RemoveSprite(sprite);
        public bool TryGetAllTimeSprite(string name, out BlingoSprite2D? sprite) => _sprite2DManager.TryGetAllTimeSprite(name, out sprite);
        public bool TryGetAllTimeSprite(int number, out BlingoSprite2D? sprite) => _sprite2DManager.TryGetAllTimeSprite(number, out sprite);
        public void SetSpriteMember(int number, string memberName) => _sprite2DManager.SetSpriteMember(number, memberName);
        public void SetSpriteMember(int number, int memberNumber) => _sprite2DManager.SetSpriteMember(number, memberNumber);
        public void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour) where T : IBlingoSpriteBehavior => _sprite2DManager.SendSprite(spriteNumber, actionOnSpriteBehaviour);
        public bool TrySendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour) where T : IBlingoSpriteBehavior => _sprite2DManager.TrySendSprite(spriteNumber, actionOnSpriteBehaviour);
        public TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour) where T : IBlingoSpriteBehavior => _sprite2DManager.SendSprite<T, TResult>(spriteNumber, actionOnSpriteBehaviour);
        public void SendSprite(string name, Action<IBlingoSpriteChannel> actionOnSprite) => _sprite2DManager.SendSprite(name, actionOnSprite);
        public void SendSprite(int spriteNumber, Action<IBlingoSpriteChannel> actionOnSprite) => _sprite2DManager.SendSprite(spriteNumber, actionOnSprite);
        public void SendAllSprites(Action<IBlingoSpriteChannel> actionOnSprite) => _sprite2DManager.SendAllSprites(actionOnSprite);
        public void SendAllSprites<T>(Action<T> actionOnSprite) where T : BlingoSpriteBehavior => _sprite2DManager.SendAllSprites(actionOnSprite);
        public IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : BlingoSpriteBehavior => _sprite2DManager.SendAllSprites<T, TResult>(actionOnSprite);
        public bool RollOver(int spriteNumber) => _sprite2DManager.RollOver(spriteNumber);
        public int RollOver() => _sprite2DManager.RollOver();
        public int ConstrainH(int spriteNumber, int pos) => _sprite2DManager.ConstrainH(spriteNumber, pos);
        public int ConstrainV(int spriteNumber, int pos) => _sprite2DManager.ConstrainV(spriteNumber, pos);
        public BlingoSprite2D? GetSpriteUnderMouse(bool skipLockedSprites = false) => _sprite2DManager.GetSpriteUnderMouse(skipLockedSprites);
        public IEnumerable<BlingoSprite2D> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false) => _sprite2DManager.GetSpritesAtPoint(x, y, skipLockedSprites);
        public BlingoSprite2D? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false) => _sprite2DManager.GetSpriteAtPoint(x, y, skipLockedSprites);
        public void ChangeSpriteChannel(BlingoSprite sprite, int newChannel)
        {
            if (sprite is BlingoSprite2D sprite2D)
                _sprite2DManager.ChangeSpriteChannel(sprite2D, newChannel);
        }

        #endregion



        #region Playhead

        public void GoTo(string label) => Go(label);
        public void Go(string label)
        {
            if (_frameLabelManager.ScoreLabels.TryGetValue(label, out var scoreLabel))
                _nextFrame = scoreLabel;
        }

        public void GoTo(int frame) => Go(frame);

        public void Go(int frame)
        {
            if (frame <= 0)
                throw new ArgumentOutOfRangeException(nameof(frame));
            _nextFrame = frame;
        }

        public void OnTick()
        {
            if (_isPlaying)
            {
                if (_transitionPlayer.IsActive)
                {
                    _transitionPlayer.Tick();
                    return;
                }

                if (_waitingForInput || _waitingForCuePoint)
                    return;

                if (_delayTicks > 0)
                {
                    _delayTicks--;
                    return;
                }
                if (_isManualUpdateStage)
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

                var frameChanged = false;
                if (_nextFrame < 0)
                {
                    var newFrame = _currentFrame + 1;
                    frameChanged = SetProperty(ref _currentFrame, newFrame, nameof(CurrentFrame));
                }
                else
                {
                    frameChanged = SetProperty(ref _currentFrame, _nextFrame, nameof(CurrentFrame));
                    _nextFrame = -1;
                }
                if (frameChanged)
                {
                    OnPropertyChanged(nameof(Frame));

                    var transitionSprite = _transitionManager.GetFrameSprite(_currentFrame);
                    if (transitionSprite != null)
                    {
                        if (_transitionPlayer.Start(transitionSprite))
                            _skipStepFrame = true;
                    }

                    // update the list with all ended, and all the new started sprites.
                    _sprite2DManager.UpdateActiveSprites(_currentFrame, _lastFrame);
                    _spriteManagers.ForEach(x => x.UpdateActiveSprites(_currentFrame, _lastFrame));

                    // End the sprites first, the frame has change, start by ending all sprites, that are not on this frame anymore.
                    _sprite2DManager.EndSprites();
                    _spriteManagers.ForEach(x => x.EndSprites());

                    // Begin the new sprites
                    _sprite2DManager.BeginSprites();
                    _spriteManagers.ForEach(x => x.BeginSprites());
                }
                else
                {
                    // Are there new puppet sprites set.
                    _sprite2DManager.DoPuppetSprites();
                }
                _lastFrame = _currentFrame;

                if (_needToRaiseStartMovie)
                {
                    _eventMediator.RaiseStartMovie();
                    _needToRaiseStartMovie = false;
                }

                _blingoMouse.UpdateMouseState();
                _sprite2DManager.PreStepFrame();
                if (!_skipStepFrame)
                    _eventMediator.RaiseStepFrame();
                _eventMediator.RaisePrepareFrame();
                _eventMediator.RaiseEnterFrame();

                OnUpdateStage();
                _skipStepFrame = false;
                if (frameChanged)
                    CurrentFrameChanged?.Invoke(_currentFrame);

                _eventMediator.RaiseExitFrame();
            }
            finally
            {
                //_sprite2DManager.EndSprites();
                //_spriteManagers.ForEach(x => x.EndSprites());
                _isAdvancing = false;
            }

        }



        // Play the movie
        public void Play()
        {
            _eventMediator.RaisePrepareMovie();
            _needToRaiseStartMovie = true;
            // prepareMovie
            // PrepareFrame
            // BeginSprite
            // StartMovie
            SetProperty(ref _isPlaying, true, nameof(IsPlaying));
            PlayStateChanged?.Invoke(true);
            //OnTick();
            //_needToRaiseStartMovie = false;

        }

        private void OnStop()
        {
            // on stop always restore the mouse to arrow
            _blingoMouse.SetCursor(AMouseCursor.Arrow);
            SetProperty(ref _isPlaying, false, nameof(IsPlaying));
            PlayStateChanged?.Invoke(false);
            _environment.Sound.StopAll();
            //_spriteManager.EndSprites();
            _eventMediator.RaiseStopMovie();
            // EndSprite
            // StopMovie
        }
        // Halt the movie
        public void Halt()
        {
            OnStop();
        }

        public void Rewind()
        {
            RemoveAllPuppetSprites();

            if (FrameCount == 0)
            {
                Halt();
                return;
            }

            _nextFrame = 1;
            AdvanceFrame();
            Halt();
        }

        private void RemoveAllPuppetSprites()
        {
            for (int channelIndex = 0; channelIndex < _sprite2DManager.MaxSpriteChannelCount; channelIndex++)
            {
                var channel = _sprite2DManager.Channel(channelIndex);
                if (channel.Puppet)
                    channel.Puppet = false;
            }
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
        private bool _waitingForInput;
        private bool _waitingForCuePoint;
        private int _waitCueChannel;
        private int _waitCuePoint;
        public void Delay(int ticks)
        {
            if (ticks <= 0) return;
            _delayTicks += ticks;
        }

        public void WaitForInput()
        {
            _waitingForInput = true;
        }

        public void ContinueAfterInput()
        {
            _waitingForInput = false;
        }

        public void WaitForCuePoint(int channel, int point)
        {
            _waitingForCuePoint = true;
            _waitCueChannel = channel;
            _waitCuePoint = point;
        }

        public void CuePointReached(int channel, int point)
        {
            if (_waitingForCuePoint && channel == _waitCueChannel && point == _waitCuePoint)
                _waitingForCuePoint = false;
        }

        public void GoNext()
            => Go(_frameLabelManager.GetNextMarker(Frame));

        public void GoPrevious()
            => Go(_frameLabelManager.GetPreviousMarker(Frame));

        public void GoLoop()
            => Go(_frameLabelManager.GetLoopMarker(Frame));

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
                _nextFrame = frame;
                AdvanceFrame();
                SetProperty(ref _isPlaying, false, nameof(IsPlaying));
                PlayStateChanged?.Invoke(false);
                _environment.Sound.StopAll();
            }
        }


        public void UpdateStage()
        {
            // a manual update stage needs to run on same framerate, it means that the head player will not advance
            _isManualUpdateStage = true;
        }
        private void OnUpdateStage()
        {

            Timer++;
            _actorList.Invoke();
            _frameworkMovie.UpdateStage();
            _isManualUpdateStage = false;
        }
        #endregion



        // PuppetTransition (for special effects/animations, implementation is up to you)
        public void PuppetTransition(int effectNumber)
        {
            // Implement specific logic for puppet transition effects (if any)
        }


        #region CastLibs
        public IBlingoCastLibsContainer CastLib => _castLibContainer;
        public IBlingoMembersContainer Member => _castLibContainer.Member;
        public T? GetMember<T>(int number) where T : class, IBlingoMember => _castLibContainer.GetMember<T>(number);
        public T? GetMember<T>(string name) where T : class, IBlingoMember => _castLibContainer.GetMember<T>(name);
        #endregion


        #region MovieScripts
        public IBlingoMovie AddMovieScript<T>()
            where T : BlingoMovieScript
        {
            _movieScripts.Add<T>();
            return this;
        }
        public void CallMovieScript<T>(Action<T> action) where T : IBlingoMovieScript
            => _movieScripts.Call(action);
        public TResult? CallMovieScript<T, TResult>(Func<T, TResult> action) where T : IBlingoMovieScript
            => _movieScripts.Call(action);

        private void CallOnAllMovieScripts(Action<IBlingoMovieScript> actionOnAll)
            => _movieScripts.CallAll(actionOnAll);

        #endregion



        public BlingoMovieEnvironment GetEnvironment() => _environment;
        public IBlingoServiceProvider GetServiceProvider() => _environment.GetServiceProvider();
        public T GetRequiredService<T>() where T : notnull => _environment.GetServiceProvider().GetRequiredService<T>();

        public void StartTimer() => Timer = 0;

        public void SetScoreLabel(int frameNumber, string? name)
            => _frameLabelManager.SetScoreLabel(frameNumber, name);

        public int GetNextSpriteStart(int channel, int frame)
            => _sprite2DManager.GetNextSpriteStart(channel, frame);

        public int GetPrevSpriteEnd(int channel, int frame)
            => _sprite2DManager.GetPrevSpriteEnd(channel, frame);


        public int GetMaxLocZ() => _sprite2DManager.GetMaxLocZ();

        public IBlingoMemberFactory New => _memberFactory;

       

        public BlingoMember? MouseMemberUnderMouse() // todo : implement
            => null;

        internal void SetMouse(BlingoStageMouse newMouse)
        {
            _blingoMouse = newMouse;
            _sprite2DManager.SetMouse(newMouse);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable<BlingoSprite2D> GetAll2DSpritesToStore()
        {
            return Sprite2DManager.AllTimeSprites.Where(x => !x.Puppet && !x.IsDeleted);
        }

       
    }
}

