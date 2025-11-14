using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Components.Menus;
using AbstUI.Components.Texts;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.FilmLoops;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Sprites;
using BlingoEngine.Stages;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlingoEngine.Tests.Fakes;

internal sealed class FakeBlingoMovieBuilder
{
    private FakeBlingoMovieBuilder(BlingoMovie movie, RecordingTransitionPlayer transitionPlayer)
    {
        Movie = movie;
        TransitionPlayer = transitionPlayer;
    }

    internal BlingoMovie Movie { get; }

    internal RecordingTransitionPlayer TransitionPlayer { get; }

    internal static FakeBlingoMovieBuilder Create(
        BlingoEventMediator mediator,
        List<string> timeline,
        Action<Options>? configure = null)
    {
        var options = new Options();
        configure?.Invoke(options);

        var movie = (BlingoMovie)FormatterServices.GetUninitializedObject(typeof(BlingoMovie));

        PrivateFieldSetter.SetField(movie, "_eventMediator", mediator);
        PrivateFieldSetter.SetField(movie, "_actorList", new ActorList());
        PrivateFieldSetter.SetField(movie, "_spriteManagers", new List<BlingoSpriteManager>());
        PrivateFieldSetter.SetField(movie, "_onRemoveMe", (Action<BlingoMovie>)(_ => { }));
        PrivateFieldSetter.SetField(movie, "_idleHandlerPeriod", 1);
        PrivateFieldSetter.SetField(movie, "_idleIntervalSeconds", 1f / 60f);

        var frameworkStage = new StubFrameworkStage();
        var stage = new BlingoStage(frameworkStage);
        frameworkStage.AttachStage(stage);
        PrivateFieldSetter.SetField(movie, "_stage", stage);

        var frameworkMovie = new StubFrameworkMovie();
        PrivateFieldSetter.SetField(movie, "_frameworkMovie", frameworkMovie);

        var frameworkMouse = new StubFrameworkMouse();
        var stageMouse = new BlingoStageMouse(stage, frameworkMouse);
        PrivateFieldSetter.SetField(movie, "_blingoMouse", stageMouse);

        var factory = new StubFrameworkFactory();
        var environment = CreateEnvironment(movie, mediator, stageMouse, factory);
        var spritesPlayer = new StubSpritesPlayer();

        var spriteManager = CreateSprite2DManager(movie, mediator, stageMouse, environment, spritesPlayer, timeline);
        PrivateFieldSetter.SetField(movie, "_sprite2DManager", spriteManager);

        var transitionManagerObject = CreateTransitionManager(movie, mediator, timeline, options);
        var transitionManager = (BlingoSpriteManager)transitionManagerObject;

        var managers = new List<BlingoSpriteManager> { transitionManager };
        PrivateFieldSetter.SetField(movie, "_spriteManagers", managers);
        PrivateFieldSetter.SetField(movie, "_transitionManager", transitionManagerObject);

        var transitionPlayer = new RecordingTransitionPlayer(timeline);
        PrivateFieldSetter.SetField(movie, "_transitionPlayer", transitionPlayer);

        PrivateFieldSetter.SetField(movie, "_lastFrame", 0);
        PrivateFieldSetter.SetField(movie, "_currentFrame", 0);
        PrivateFieldSetter.SetField(movie, "_nextFrame", -1);

        return new FakeBlingoMovieBuilder(movie, transitionPlayer);
    }

    internal sealed class Options
    {
        internal bool RecordTransitionLifecycle { get; set; }
        internal int? TransitionActivationFrame { get; set; }
    }

    internal sealed class RecordingTransitionPlayer : IBlingoTransitionPlayer
    {
        internal RecordingTransitionPlayer(List<string> timeline) => Timeline = timeline;

        private List<string> Timeline { get; }

        internal int StartCallCount { get; private set; }

        internal bool StartResult { get; set; } = true;

        public bool Start(BlingoTransitionSprite sprite)
        {
            StartCallCount++;
            return StartResult;
        }

        public void Tick()
        {
        }

        public bool IsActive => false;

        public void Dispose()
        {
        }
    }

    private static IBlingoMovieEnvironment CreateEnvironment(
        BlingoMovie movie,
        BlingoEventMediator mediator,
        BlingoStageMouse mouse,
        IBlingoFrameworkFactory factory)
    {
        var environmentType = typeof(BlingoMovie).Assembly.GetType("BlingoEngine.Movies.BlingoMovieEnvironment", throwOnError: true)!;
        var environment = (IBlingoMovieEnvironment)FormatterServices.GetUninitializedObject(environmentType);

        PrivateFieldSetter.SetField(environment, "_eventMediator", mediator);
        PrivateFieldSetter.SetField(environment, "_movie", movie);
        PrivateFieldSetter.SetField(environment, "_factory", factory);
        PrivateFieldSetter.SetField(environment, "_mouse", mouse);
        PrivateFieldSetter.SetField(environment, "_clock", new BlingoClock());
        PrivateFieldSetter.SetField(environment, "_logger", NullLogger<BlingoMovieEnvironment>.Instance);
        PrivateFieldSetter.SetField(environment, "_globals", new BlingoGlobalVars());
        PrivateFieldSetter.SetField(environment, "_castLibsContainer", new BlingoCastLibsContainer(factory));

        return environment;
    }

    private static BlingoSprite2DManager CreateSprite2DManager(
        BlingoMovie movie,
        BlingoEventMediator mediator,
        BlingoStageMouse mouse,
        IBlingoMovieEnvironment environment,
        IBlingoSpritesPlayer spritesPlayer,
        List<string> timeline)
    {
        var manager = (BlingoSprite2DManager)FormatterServices.GetUninitializedObject(typeof(BlingoSprite2DManager));

        PrivateFieldSetter.SetField(manager, "_movie", movie);
        PrivateFieldSetter.SetField(manager, "_environment", environment);
        PrivateFieldSetter.SetField(manager, "_mutedSprites", new List<int>());
        PrivateFieldSetter.SetField(manager, "_spriteChannels", new Dictionary<int, BlingoSpriteChannel>());
        var spritesByName = new Dictionary<string, BlingoSprite2D>();
        PrivateFieldSetter.SetField(manager, "_spritesByName", spritesByName);
        var allTimeSprites = new List<BlingoSprite2D>();
        PrivateFieldSetter.SetField(manager, "_allTimeSprites", allTimeSprites);
        PrivateFieldSetter.SetField(manager, "_newPuppetSprites", new List<BlingoSprite2D>());
        PrivateFieldSetter.SetField(manager, "_activeSprites", new Dictionary<int, BlingoSprite2D>());
        PrivateFieldSetter.SetField(manager, "_activeSpritesOrdered", new List<BlingoSprite2D>());
        PrivateFieldSetter.SetField(manager, "_enteredSprites", new List<BlingoSprite2D>());
        PrivateFieldSetter.SetField(manager, "_exitedSprites", new List<BlingoSprite2D>());
        PrivateFieldSetter.SetField(manager, "_changedMembers", new List<BlingoMember>());
        PrivateFieldSetter.SetField(manager, "_deletedPuppetSpritesCache", new Dictionary<int, BlingoSprite2D>());
        PrivateFieldSetter.SetField(manager, "<SpriteNumChannelOffset>k__BackingField", BlingoSprite2D.SpriteNumOffset);
        PrivateFieldSetter.SetField(manager, "_blingoMouse", mouse);

        var channel = new BlingoSpriteChannel(1, movie);
        var channels = new Dictionary<int, BlingoSpriteChannel> { [1] = channel };
        PrivateFieldSetter.SetField(manager, "_spriteChannels", channels);

        var frameworkSprite = new StubFrameworkSprite
        {
            Name = "Sprite_1"
        };

        var sprite = new RecordingSprite2D(environment, spritesPlayer, timeline);
        sprite.Init(frameworkSprite);
        PrivateFieldSetter.SetField(sprite, "<SpriteNum>k__BackingField", 1);
        sprite.BeginFrame = 1;
        sprite.EndFrame = 1;
        spritesByName[frameworkSprite.Name] = sprite;
        allTimeSprites.Add(sprite);

        return manager;
    }

    private static object CreateTransitionManager(
        BlingoMovie movie,
        BlingoEventMediator mediator,
        List<string> timeline,
        Options options)
    {
        var managerType = typeof(BlingoMovie).Assembly.GetType("BlingoEngine.Transitions.BlingoSpriteTransitionManager", throwOnError: true)!;
        var manager = FormatterServices.GetUninitializedObject(managerType);

        PrivateFieldSetter.SetField(manager, "_movie", movie);
        PrivateFieldSetter.SetField(manager, "_environment", null);
        PrivateFieldSetter.SetField(manager, "_mutedSprites", new List<int>());
        PrivateFieldSetter.SetField(manager, "_spriteChannels", new Dictionary<int, BlingoSpriteChannel>());
        var allTimeSprites = new List<BlingoTransitionSprite>();
        PrivateFieldSetter.SetField(manager, "_allTimeSprites", allTimeSprites);
        PrivateFieldSetter.SetField(manager, "_newPuppetSprites", new List<BlingoTransitionSprite>());
        PrivateFieldSetter.SetField(manager, "_activeSprites", new Dictionary<int, BlingoTransitionSprite>());
        PrivateFieldSetter.SetField(manager, "_activeSpritesOrdered", new List<BlingoTransitionSprite>());
        PrivateFieldSetter.SetField(manager, "_enteredSprites", new List<BlingoTransitionSprite>());
        PrivateFieldSetter.SetField(manager, "_exitedSprites", new List<BlingoTransitionSprite>());
        PrivateFieldSetter.SetField(manager, "_deletedPuppetSpritesCache", new Dictionary<int, BlingoTransitionSprite>());
        PrivateFieldSetter.SetField(manager, "<SpriteNumChannelOffset>k__BackingField", BlingoTransitionSprite.SpriteNumOffset);

        if (options.RecordTransitionLifecycle && options.TransitionActivationFrame.HasValue)
        {
            var cast = new StubCast();
            var sprite = new RecordingTransitionSprite(mediator, cast, _ => { }, timeline);
            PrivateFieldSetter.SetField(sprite, "<SpriteNum>k__BackingField", 1);
            sprite.BeginFrame = options.TransitionActivationFrame.Value;
            sprite.EndFrame = options.TransitionActivationFrame.Value;
            allTimeSprites.Add(sprite);
        }

        return manager;
    }

    private sealed class RecordingSprite2D : BlingoSprite2D
    {
        private readonly List<string> _timeline;

        internal RecordingSprite2D(IBlingoMovieEnvironment environment, IBlingoSpritesPlayer spritesPlayer, List<string> timeline)
            : base(environment, spritesPlayer)
        {
            _timeline = timeline;
        }

        protected override void BeginSprite()
        {
            _timeline.Add("beginSprite");
        }

        protected override void EndSprite()
        {
            _timeline.Add("endSprite");
        }
    }

    private sealed class RecordingTransitionSprite : BlingoTransitionSprite
    {
        private readonly List<string> _timeline;

        internal RecordingTransitionSprite(IBlingoEventMediator mediator, IBlingoCast cast, Action<BlingoTransitionSprite> removeMe, List<string> timeline)
            : base(mediator, cast, removeMe)
        {
            _timeline = timeline;
        }

        protected override void BeginSprite()
        {
            _timeline.Add("transition.beginSprite");
        }

        protected override void EndSprite()
        {
            _timeline.Add("transition.endSprite");
        }
    }

    private sealed class StubSpritesPlayer : IBlingoSpritesPlayer
    {
        public int CurrentFrame => 1;
        public int GetMaxLocZ() => 0;
    }

    private sealed class StubFrameworkFactory : IBlingoFrameworkFactory
    {
        public IAbstComponentFactory ComponentFactory => throw new NotImplementedException();
        public BlingoStage CreateStage(BlingoPlayer blingoPlayer) => throw new NotImplementedException();
        public BlingoMovie AddMovie(BlingoStage stage, BlingoMovie blingoMovie) => throw new NotImplementedException();
        public T CreateMember<T>(IBlingoCast cast, int numberInCast, string name = "") where T : BlingoMember => throw new NotImplementedException();
        public BlingoMemberBitmap CreateMemberBitmap(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoMemberSound CreateMemberSound(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoFilmLoopMember CreateMemberFilmLoop(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoMemberShape CreateMemberShape(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoMemberField CreateMemberField(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoMemberText CreateMemberText(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoMember CreateScript(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoMember CreateEmpty(IBlingoCast cast, int numberInCast, string name = "", string? fileName = null, APoint regPoint = default) => throw new NotImplementedException();
        public BlingoSound CreateSound(IBlingoCastLibsContainer castLibsContainer) => throw new NotImplementedException();
        public BlingoSoundChannel CreateSoundChannel(int number) => throw new NotImplementedException();
        public BlingoStageMouse CreateMouse(BlingoStage stage) => throw new NotImplementedException();
        public BlingoKey CreateKey() => throw new NotImplementedException();
        public AbstGfxCanvas CreateGfxCanvas(string name, int width, int height) => throw new NotImplementedException();
        public AbstWrapPanel CreateWrapPanel(AOrientation orientation, string name) => throw new NotImplementedException();
        public AbstPanel CreatePanel(string name) => throw new NotImplementedException();
        public AbstLayoutWrapper CreateLayoutWrapper(IAbstNode content, float? x, float? y) => throw new NotImplementedException();
        public AbstTabContainer CreateTabContainer(string name) => throw new NotImplementedException();
        public AbstTabItem CreateTabItem(string name, string title) => throw new NotImplementedException();
        public AbstScrollContainer CreateScrollContainer(string name) => throw new NotImplementedException();
        public AbstInputSlider<float> CreateInputSliderFloat(AOrientation orientation, string name, float? min = null, float? max = null, float? step = null, Action<float>? onChange = null) => throw new NotImplementedException();
        public AbstInputSlider<int> CreateInputSliderInt(AOrientation orientation, string name, int? min = null, int? max = null, int? step = null, Action<int>? onChange = null) => throw new NotImplementedException();
        public AbstInputText CreateInputText(string name, int maxLength = 0, Action<string>? onChange = null, bool multiLine = false) => throw new NotImplementedException();
        public AbstInputNumber<float> CreateInputNumberFloat(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
        public AbstInputNumber<int> CreateInputNumberInt(string name, int? min = null, int? max = null, Action<int>? onChange = null) => throw new NotImplementedException();
        public AbstInputSpinBox CreateSpinBox(string name, float? min = null, float? max = null, Action<float>? onChange = null) => throw new NotImplementedException();
        public AbstInputCheckbox CreateInputCheckbox(string name, Action<bool>? onChange = null) => throw new NotImplementedException();
        public AbstInputCombobox CreateInputCombobox(string name, Action<string?>? onChange = null) => throw new NotImplementedException();
        public AbstItemList CreateItemList(string name, Action<string?>? onChange = null) => throw new NotImplementedException();
        public AbstColorPicker CreateColorPicker(string name, Action<AColor>? onChange = null) => throw new NotImplementedException();
        public AbstLabel CreateLabel(string name, string text = "") => throw new NotImplementedException();
        public AbstButton CreateButton(string name, string text = "") => throw new NotImplementedException();
        public AbstStateButton CreateStateButton(string name, IAbstTexture2D? texture = null, string text = "", Action<bool>? onChange = null) => throw new NotImplementedException();
        public AbstMenu CreateMenu(string name) => throw new NotImplementedException();
        public AbstMenuItem CreateMenuItem(string name, string? shortcut = null) => throw new NotImplementedException();
        public AbstMenu CreateContextMenu(object window) => throw new NotImplementedException();
        public AbstHorizontalLineSeparator CreateHorizontalLineSeparator(string name) => throw new NotImplementedException();
        public AbstVerticalLineSeparator CreateVerticalLineSeparator(string name) => throw new NotImplementedException();
        public BlingoSprite2D CreateSprite2D(IBlingoMovie movie, Action<BlingoSprite2D> onRemoveMe) => throw new NotImplementedException();
    }

    private sealed class StubCast : IBlingoCast
    {
        public string Name { get; set; } = "Cast";
        public string FileName { get; set; } = string.Empty;
        public int Number => 1;
        public PreLoadModeType PreLoadMode { get; set; }
        public bool IsInternal => true;
        public CastMemberSelection? Selection { get; set; }
        public event Action<IBlingoMember>? MemberAdded
        {
            add { }
            remove { }
        }
        public event Action<IBlingoMember>? MemberDeleted
        {
            add { }
            remove { }
        }
        public event Action<IBlingoMember>? MemberNameChanged
        {
            add { }
            remove { }
        }
        public T? GetMember<T>(int number) where T : IBlingoMember => default;
        public T? GetMember<T>(string name) where T : IBlingoMember => default;
        public IBlingoMembersContainer Member => throw new NotImplementedException();
        public int FindEmpty() => 0;
        public IBlingoMember Add(BlingoMemberType type, int numberInCast, string name, string fileName = "", APoint regPoint = default) => throw new NotImplementedException();
        public T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T : IBlingoMember => throw new NotImplementedException();
        public IEnumerable<IBlingoMember> GetAll() => Array.Empty<IBlingoMember>();
        public void SwapMembers(int slot1, int slot2)
        {
        }
        public void Save()
        {
        }
        public void Dispose()
        {
        }
    }

    private sealed class StubFrameworkMovie : IBlingoFrameworkMovie
    {
        public string Name { get; set; } = "Movie";
        public bool Visibility { get; set; } = true;
        public float Width { get; set; }
        public float Height { get; set; }
        public AMargin Margin { get; set; }
        public int ZIndex { get; set; }
        public object FrameworkNode => this;
        public void UpdateStage()
        {
        }
        public void RemoveMe()
        {
        }
        public APoint GetGlobalMousePosition() => default;
        public void Dispose()
        {
        }
    }

    private sealed class StubFrameworkStage : IBlingoFrameworkStage
    {
        private BlingoStage? _stage;
        private readonly StubTexture2D _texture = new();

        public string Name { get; set; } = "Stage";
        public bool Visibility { get; set; } = true;
        public float Width { get; set; } = 640f;
        public float Height { get; set; } = 480f;
        public AMargin Margin { get; set; }
        public int ZIndex { get; set; }
        public object FrameworkNode => this;
        public BlingoStage BlingoStage => _stage ?? throw new InvalidOperationException();
        public float Scale { get; set; } = 1f;
        public void AttachStage(BlingoStage stage) => _stage = stage;
        public void SetActiveMovie(BlingoMovie? blingoMovie)
        {
        }
        public void ApplyPropertyChanges()
        {
        }
        public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
            => onCaptured(_texture);
        public IAbstTexture2D GetScreenshot() => _texture;
        public void ShowTransition(IAbstTexture2D startTexture)
        {
        }
        public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        {
        }
        public void HideTransition()
        {
        }
        public void Dispose()
        {
        }
    }

    private sealed class StubFrameworkMouse : IBlingoFrameworkMouse
    {
        private AMouseCursor _cursor = AMouseCursor.Arrow;
        public void HideMouse(bool state)
        {
        }
        public void Release()
        {
        }
        public void ReplaceMouseObj(IAbstMouse blingoMouse)
        {
        }
        public void SetCursor(AMouseCursor cursor) => _cursor = cursor;
        public AMouseCursor GetCursor() => _cursor;
        public void SetCursor(BlingoMemberBitmap? image)
        {
        }
        public void SetOffset(int x, int y)
        {
        }
    }

    private sealed class StubFrameworkSprite : IBlingoFrameworkSprite
    {
        public string Name { get; set; } = string.Empty;
        public bool Visibility { get; set; } = true;
        public float Width { get; set; }
        public float Height { get; set; }
        public AMargin Margin { get; set; }
        public int ZIndex { get; set; }
        public object FrameworkNode => this;
        public float Blend { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public APoint RegPoint { get; set; }
        public float DesiredHeight { get; set; }
        public float DesiredWidth { get; set; }
        public float Rotation { get; set; }
        public float Skew { get; set; }
        public bool FlipH { get; set; }
        public bool FlipV { get; set; }
        public bool DirectToStage { get; set; }
        public int Ink { get; set; }
        public void MemberChanged()
        {
        }
        public void RemoveMe()
        {
        }
        public void Show()
        {
        }
        public void Hide()
        {
        }
        public void SetPosition(APoint point)
        {
            X = point.X;
            Y = point.Y;
        }
        public void ApplyMemberChangesOnStepFrame()
        {
        }
        public void SetTexture(IAbstTexture2D texture)
        {
        }
        public void Dispose()
        {
        }
    }

    private sealed class StubTexture2D : IAbstTexture2D
    {
        public int Width => 0;
        public int Height => 0;
        public bool IsDisposed => false;
        public string Name { get; set; } = "Texture";
        public IAbstUITextureUserSubscription AddUser(object user) => new StubTextureSubscription(this);
        public byte[] GetPixels() => Array.Empty<byte>();
        public void SetARGBPixels(byte[] argbPixels)
        {
        }
        public void SetRGBAPixels(byte[] rgbaPixels)
        {
        }
        public IAbstTexture2D Clone() => this;
        public void Dispose()
        {
        }

        private sealed class StubTextureSubscription : IAbstUITextureUserSubscription
        {
            internal StubTextureSubscription(IAbstTexture2D texture) => Texture = texture;
            public IAbstTexture2D Texture { get; }
            public void Release()
            {
            }
        }
    }
}

internal static class PrivateFieldSetter
{
    internal static void SetField(object target, string fieldName, object? value)
    {
        var type = target.GetType();
        while (type != null)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType()}'.");
    }
}
