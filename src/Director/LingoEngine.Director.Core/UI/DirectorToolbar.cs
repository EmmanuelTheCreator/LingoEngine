using LingoEngine.Commands;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Bitmaps.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.UI;

public abstract class DirectorToolbar<TToolEnumType>
    where TToolEnumType : Enum
{
    protected readonly IDirectorIconManager _iconManager;
    protected readonly ILingoCommandManager _commandManager;
    protected readonly ILingoFrameworkFactory _factory;
    protected readonly LingoGfxPanel _panel;
    protected readonly LingoGfxWrapPanel _container;
    protected LingoGfxStateButton? _selectedButton;

    public event Action<TToolEnumType>? ToolSelected;
    public LingoGfxPanel Panel => _panel;
    public TToolEnumType SelectedTool { get; protected set; }

    public DirectorToolbar(string name, IDirectorIconManager iconManager, ILingoCommandManager commandManager, ILingoFrameworkFactory factory)
    {
        _iconManager = iconManager;
        _commandManager = commandManager;
        _factory = factory;

        _panel = factory.CreatePanel(name);
        _panel.BackgroundColor = DirectorColors.BG_WhiteMenus;
        _panel.Width = 52;   // fallback size similar to Godot implementation
        _panel.Height = 200;

        _container = factory.CreateWrapPanel(LingoOrientation.Vertical, "PaintToolbarContainer");
        _container.Width = _panel.Width - 2;
        _container.Height = _panel.Height - 2;
        // TODO: custom minimum size (48,100) when supported
        // TODO: size flags ExpandFill/ShrinkBegin when supported

        /*
        _container = new HFlowContainer();
            _container.CustomMinimumSize = new Vector2(48, 100);
            _container.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _container.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            _container.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            _container.SizeFlagsVertical = SizeFlags.ShrinkBegin;
            _container.AddThemeConstantOverride("margin_left", 2);
            _container.AddThemeConstantOverride("margin_top", 2);
            AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = BGColor }); 
        */

        _container.ItemMargin = new LingoMargin(2, 2, 0, 0); // margin_left/top
        _panel.AddItem(_container);

        ToolSelected?.Invoke(SelectedTool);
    }
   
    protected void AddToolButton(DirectorIcon icon, Func<TToolEnumType, ILingoCommand> toCommand)
    {
        var btn = _factory.CreateStateButton(icon.ToString(), _iconManager.Get(icon));
        btn.Width = 20; // approximate size
        btn.Height = 20;
        btn.ValueChanged += () =>
        {
            if (!btn.IsOn) return;
            SelectButton(btn);
            TToolEnumType tool = ConvertToTool(icon);

            SelectedTool = tool;
            _commandManager.Handle(toCommand(tool));
            ToolSelected?.Invoke(tool);
        };

        _container.AddItem(btn);
    }

    protected abstract TToolEnumType ConvertToTool(DirectorIcon icon);
   

    public void SelectTool(TToolEnumType tool)
    {
        if (SelectedTool.Equals(tool)) return;
        SelectedTool = tool;
        ToolSelected?.Invoke(tool);
    }

    private void SelectButton(LingoGfxStateButton btn)
    {
        if (_selectedButton == btn) return;
        if (_selectedButton != null)
            _selectedButton.IsOn = false;

        btn.IsOn = true;
        _selectedButton = btn;
    }


}
