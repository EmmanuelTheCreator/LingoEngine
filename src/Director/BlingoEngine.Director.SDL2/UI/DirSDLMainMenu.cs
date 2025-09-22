using System;
using AbstUI.Commands;
using AbstUI.Components;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Menus;
using AbstUI.FrameworkCommunication;
using AbstUI.Inputs;
using AbstUI.SDL2.Components;
using AbstUI.SDL2.Components.Buttons;
using AbstUI.SDL2.Components.Containers;
using AbstUI.SDL2.Styles;
using AbstUI.SDL2.Windowing;
using AbstUI.Tools;
using AbstUI.Windowing;
using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.LGodot;

/// <summary>
/// Godot wrapper for <see cref="DirectorMainMenu"/>.
/// </summary>
internal partial class DirSDLMainMenu : AbstSdlWindow, IDirFrameworkMainMenuWindow, IFrameworkFor<DirectorMainMenu>
{
    private readonly AbstSdlWrapPanel _menuBar;
    private readonly AbstSdlWrapPanel _iconBar;
    private readonly DirectorMainMenu _directorMainMenu;
    private readonly AbstPanel _root;

    IAbstMouse IAbstFrameworkWindow.Mouse => _directorMainMenu.Mouse;

    public DirectorMainMenu MainMenu => _directorMainMenu;
    public DirSDLMainMenu(
        DirectorProjectManager projectManager, IServiceProvider services,
        BlingoPlayer player,
        IAbstShortCutManager shortCutManager,
        IHistoryManager historyManager,
        IAbstWindowManager windowManager,
        IAbstCommandManager commandManager,
        DirectorMainMenu directorMainMenu, IBlingoFrameworkFactory factory)
        :base((AbstSdlComponentFactory)services.GetRequiredService<IAbstComponentFactory>())
    {
        ComponentContext.AlwaysOnTop = true;
        _directorMainMenu = directorMainMenu;
        _directorMainMenu.Init(this);
        _menuBar = directorMainMenu.MenuBar.Framework<AbstSdlWrapPanel>();
        _iconBar = directorMainMenu.IconBar.Framework<AbstSdlWrapPanel>();
        Borderless = true;
        //CreateBgColor();

        //AddChild(_bgColorPanel);
        //AddChild(_menuBar);
        //AddChild(_iconBar);
        _root = factory.CreatePanel("MenuBarRoot");
        _root.Width = _menuBar.Width + _iconBar.Width + _iconBar.X;
        _root.Height = 50;
        Title = "Main menu";
        Width = _root.Width;
        Height = _root.Height;
        _directorMainMenu.Width = (int)Width;
        _directorMainMenu.Height = (int)Height;
        _root.BackgroundColor = DirectorColors.BG_TopMenu;
        _root.AddItem(directorMainMenu.MenuBar, _menuBar.X, _menuBar.Y);
        _root.AddItem(directorMainMenu.IconBar, _iconBar.X, _iconBar.Y);
        StyleTopMenuButtons();
        //_root.Compose(Factory).NextRow().Finalize();
        //_root.Compose(Factory).NextRow().AddButton("hallo", "Halo", () => { }).AddLabel("rzer", "tedft").Finalize();
        //directorMainMenu.CallOnAllTopMenus(btn =>
        //{
        //    AddChild(btn.Framework<AbstGodotMenu>());
        //});

        //StyleTopMenu(directorMainMenu);
        //foreach (var childItem in _iconBar.GetChild(0).GetChildren())
        //{
        //    if (childItem is Button btn)
        //    {
        //        StyleIconButton(btn);
        //    }
        //}

    }
    public override void Init(IAbstWindow instance)
    {
        base.Init(instance);
        Content = _root.FrameworkObj;
    }

    public void RegisterTopMenu(AbstMenu menu)
    {
        _root.AddItem(menu);
    }

    private void StyleTopMenuButtons()
    {
        var baseColor = DirectorColors.BG_TopMenu;
        var hoverColor = baseColor.Lighten(0.1f);
        var fontManager = _componentFactory.FontManagerTyped;

        _directorMainMenu.CallOnAllTopMenuButtons(btn =>
        {
            btn.Margin = AMargin.Zero;
            btn.BackgroundColor = baseColor;
            btn.BackgroundHoverColor = hoverColor;
            btn.BorderColor = AColor.Transparent();
            if (btn.FrameworkObj is IHasButtonBorderStates borderStates)
            {
                borderStates.SetUniformBorderColor(AColor.Transparent());
            }
            btn.TextColor = DirectorColors.TextColorLabels;

            var text = btn.Text ?? string.Empty;
            var textWidth = fontManager.MeasureTextWidth(text, SdlFontManager.DefaultFontName, 12);
            const float horizontalPadding = 12f;
            btn.Width = MathF.Ceiling(textWidth + horizontalPadding);
        });
    }

    //private static void StyleIconButton(Button btn)
    //{
    //    var topMenuBtnStyle = new StyleBoxFlat
    //    {
    //        BorderWidthLeft = 0,
    //        BorderWidthRight = 0,
    //        BorderWidthTop = 0,
    //        BorderWidthBottom = 0,
    //        CornerRadiusBottomLeft = 0,
    //        CornerRadiusBottomRight = 0,
    //        CornerRadiusTopLeft = 0,
    //        CornerRadiusTopRight = 0,
    //        BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
    //        ContentMarginLeft = 2,
    //        ContentMarginRight = 2,
    //    };
    //    btn.AddThemeStyleboxOverride("normal", topMenuBtnStyle);
    //    btn.Size = new Vector2(18, 18);
    //}
    //private static void StyleTopMenu(DirectorMainMenu directorMainMenu)
    //{
    //    var topMenuBtnStyle = new StyleBoxFlat
    //    {
    //        BorderWidthLeft = 0,
    //        BorderWidthRight = 0,
    //        BorderWidthTop = 0,
    //        BorderWidthBottom = 0,
    //        CornerRadiusBottomLeft = 0,
    //        CornerRadiusBottomRight = 0,
    //        CornerRadiusTopLeft = 0,
    //        CornerRadiusTopRight = 0,
    //        BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
    //        ContentMarginLeft = 5,
    //        ContentMarginRight = 5,
    //    };
    //    var topMenuBtnStyle_hover = new StyleBoxFlat
    //    {
    //        BorderWidthLeft = 1,
    //        BorderWidthRight = 1,
    //        BorderWidthTop = 1,
    //        BorderWidthBottom = 0,
    //        CornerRadiusBottomLeft = 0,
    //        CornerRadiusBottomRight = 0,
    //        CornerRadiusTopLeft = 5,
    //        CornerRadiusTopRight = 5,
    //        BgColor = DirectorColors.BG_TopMenu.ToGodotColor(),
    //        ContentMarginLeft = 5,
    //        ContentMarginRight = 5,
    //    };
        
    //    directorMainMenu.CallOnAllTopMenuButtons(btn =>
    //    {
    //        var btnG = (Button)btn.Framework<AbstGodotButton>().FrameworkNode;
    //        btnG.AddThemeStyleboxOverride("normal", topMenuBtnStyle);
    //        btnG.AddThemeStyleboxOverride("hover", topMenuBtnStyle_hover);
    //        btnG.CustomMinimumSize = new Vector2(30, 18);
    //    });
    //}

    //private void CreateBgColor()
    //{
    //    StyleBoxFlat panelStyle = new StyleBoxFlat();
    //    _bgColorPanel = new Panel();
    //    _bgColorPanel.Size = new Vector2(3000, 20);
    //    //_bgColorPanel.CustomMinimumSize = new Vector2(3000, 20);
    //    _bgColorPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
    //    _bgColorPanel.GrowHorizontal = GrowDirection.End;
    //    _bgColorPanel.Name = "MainMenuBackgroundColorPanel";
    //    panelStyle.BgColor = DirectorColors.BG_TopMenu.ToGodotColor(); ;
    //    _bgColorPanel.AddThemeStyleboxOverride("panel", panelStyle);
    //}


    public override void OpenWindow()
    {
        // not allowed
        base.OpenWindow();
    }
    public override void CloseWindow()
    {
        // not allowed
    }
    public override void MoveWindow(int x, int y)
    {
        // not allowed
    }

    public override void SetPositionAndSize(int x, int y, int width, int height)
    {
        // not allowed
    }
    public override void SetSize(int width, int height)
    {
        // not allowed
    }

    //public new APoint GetPosition() => Position.ToAbstPoint();

    //public new APoint GetSize() => Size.ToAbstPoint();

  
}

