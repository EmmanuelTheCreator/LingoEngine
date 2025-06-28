using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Inputs;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
using LingoEngine.SDL2.Movies;
using LingoEngine.SDL2.Pictures;
using LingoEngine.SDL2.Sounds;
using LingoEngine.SDL2.Texts;
using LingoEngine.SDL2.Shapes;
using LingoEngine.Gfx;
using LingoEngine.SDL2.Gfx;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Styles;
using LingoEngine.SDL2.Stages;
using LingoEngine.SDL2.Inputs;

namespace LingoEngine.SDL2.Core;

public class SdlFactory : ILingoFrameworkFactory, IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly SdlRootContext _rootContext;

    public SdlFactory(IServiceProvider serviceProvider, SdlRootContext rootContext)
    {
        _serviceProvider = serviceProvider;
        _rootContext = rootContext;
    }

    public T CreateBehavior<T>(LingoMovie movie) where T : LingoSpriteBehavior
        => movie.GetServiceProvider().GetRequiredService<T>();
    public T CreateMovieScript<T>(LingoMovie movie) where T : LingoMovieScript
        => movie.GetServiceProvider().GetRequiredService<T>();

    #region Sound
    public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer)
    {
        var impl = new SdlSound();
        var sound = new LingoSound(impl, castLibsContainer, this);
        impl.Init(sound);
        return sound;
    }
    public LingoSoundChannel CreateSoundChannel(int number)
    {
        var impl = new SdlSoundChannel(number);
        var channel = new LingoSoundChannel(impl, number);
        impl.Init(channel);
        _disposables.Add(impl);
        return channel;
    }
    #endregion

    #region Members
    public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember
    {
        return typeof(T) switch
        {
            Type t when t == typeof(LingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
            Type t when t == typeof(LingoMemberFilmLoop) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
            _ => throw new NotSupportedException()
        };
    }
    public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberSound();
        var member = new LingoMemberSound(impl, (LingoCast)cast, numberInCast, name, fileName ?? "");
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberFilmLoop CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberFilmLoop();
        var member = new LingoMemberFilmLoop(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberShape();
        var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberBitmap();
        var member = new LingoMemberBitmap((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberField(_serviceProvider.GetRequiredService<ILingoFontManager>());
        var member = new LingoMemberField((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlMemberText(_serviceProvider.GetRequiredService<ILingoFontManager>());
        var member = new LingoMemberText((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
        impl.Init(member);
        _disposables.Add(impl);
        return member;
    }
    public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
    {
        var impl = new SdlFrameworkMemberEmpty();
        var member = new LingoMember(impl, LingoMemberType.Empty, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
        return member;
    }
    #endregion

    public LingoStage CreateStage(LingoPlayer player)
    {
        _rootContext.Init(player);
        var impl = new SdlStage(_rootContext, (LingoClock)player.Clock);
        var stage = new LingoStage(impl);
        impl.Init(stage);
        _disposables.Add(impl);
        return stage;
    }
    public LingoMovie AddMovie(LingoStage stage, LingoMovie movie)
    {
        var sdlStage = stage.Framework<SdlStage>();
        var impl = new SdlMovie(sdlStage, movie, m => _disposables.Remove(m));
        movie.Init(impl);
        _disposables.Add(impl);
        return movie;
    }

    public T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite> onRemoveMe) where T : LingoSprite
    {
        var movieTyped = (LingoMovie)movie;
        var sprite = movieTyped.GetServiceProvider().GetRequiredService<T>();
        sprite.SetOnRemoveMe(onRemoveMe);
        movieTyped.Framework<SdlMovie>().CreateSprite(sprite);
        return sprite;
    }

    public void Dispose()
    {
        foreach (var d in _disposables)
            d.Dispose();
    }

    public LingoMouse CreateMouse(LingoStage stage)
    {
        var mouseImpl = new SdlMouse(new Lazy<LingoMouse>(() => null!));
        var mouse = new LingoMouse(stage, mouseImpl);
        mouseImpl.SetLingoMouse(mouse);
        return mouse;
    }

    public LingoKey CreateKey()
    {
        var impl = _rootContext.Key;
        var key = new LingoKey(impl);
        impl.SetLingoKey(key);
        return key;
    }


    #region Gfx elements

    public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
    {
        var canvas = new LingoGfxCanvas();
        var impl = new SdlGfxCanvas(_rootContext.Renderer, _serviceProvider.GetRequiredService<ILingoFontManager>(), width, height);
        canvas.Init(impl);
        canvas.Width = width;
        canvas.Height = height;
        canvas.Name = name;
        return canvas;
    }

    public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
    {
        var panel = new LingoGfxWrapPanel();
        var impl = new SdlGfxWrapPanel(orientation);
        panel.Init(impl);
        panel.Name = name;
        // Keep orientation in sync on creation
        panel.Orientation = orientation;
        return panel;
    }

    public LingoGfxPanel CreatePanel(string name)
    {
        var panel = new LingoGfxPanel();
        var impl = new SdlGfxPanel();
        panel.Init(impl);
        panel.Name = name;
        return panel;
    }

    public LingoGfxTabContainer CreateTabContainer(string name)
    {
        var tab = new LingoGfxTabContainer();
        var impl = new SdlGfxTabContainer();
        tab.Init(impl);
        tab.Name = name;
        return tab;
    }
    public LingoGfxTabItem CreateTabItem(string name, string title)
    {
        var tab = new LingoGfxTabItem(title);
        var impl = new SdlGfxTabItem(tab);

        tab.Name = name;
        return tab;
    }
    
    public LingoGfxScrollContainer CreateScrollContainer(string name)
    {
        var scroll = new LingoGfxScrollContainer();
        var impl = new SdlGfxScrollContainer();
        scroll.Init(impl);
        scroll.Name = name;
        return scroll;
    }

    public LingoGfxInputText CreateInputText(string name, int maxLength = 0)
    {
        var input = new LingoGfxInputText { MaxLength = maxLength };
        var impl = new SdlGfxInputText();
        input.Init(impl);
        input.Name = name;
        return input;
    }

    public LingoGfxInputNumber CreateInputNumber(string name, float min = 0, float max = 100)
    {
        var input = new LingoGfxInputNumber { Min = min, Max = max };
        var impl = new SdlGfxInputNumber();
        input.Init(impl);
        input.Name = name;
        return input;
    }

    public LingoGfxSpinBox CreateSpinBox(string name, float min = 0, float max = 100)
    {
        var spin = new LingoGfxSpinBox { Min = min, Max = max };
        var impl = new SdlGfxSpinBox();
        spin.Init(impl);
        spin.Name = name;
        return spin;
    }

    public LingoGfxInputCheckbox CreateInputCheckbox(string name)
    {
        var input = new LingoGfxInputCheckbox();
        var impl = new SdlGfxInputCheckbox();
        input.Init(impl);
        input.Name = name;
        return input;
    }

    public LingoGfxInputCombobox CreateInputCombobox(string name)
    {
        var input = new LingoGfxInputCombobox();
        var impl = new SdlGfxInputCombobox();
        input.Init(impl);
        input.Name = name;
        return input;
    }

    public LingoGfxLabel CreateLabel(string name, string text = "")
    {
        var label = new LingoGfxLabel { Text = text };
        var impl = new SdlGfxLabel();
        label.Init(impl);
        label.Name = name;
        return label;
    }

    public LingoGfxButton CreateButton(string name, string text = "")
    {
        var button = new LingoGfxButton { Text = text };
        var impl = new SdlGfxButton();
        button.Init(impl);
        button.Name = name;
        return button;
    }

    public LingoGfxMenu CreateMenu(string name)
    {
        var menu = new LingoGfxMenu();
        var impl = new SdlGfxMenu(name);
        menu.Init(impl);
        return menu;
    }

    public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
    {
        var item = new LingoGfxMenuItem();
        var impl = new SdlGfxMenuItem(name, shortcut);
        item.Init(impl);
        return item;
    }
    #endregion


}