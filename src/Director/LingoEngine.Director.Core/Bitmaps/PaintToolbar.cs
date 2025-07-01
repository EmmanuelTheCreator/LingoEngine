using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Pictures.Commands;
using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using System;

namespace LingoEngine.Director.Core.Pictures;

public class PaintToolbar
{
    private readonly IDirectorIconManager _iconManager;
    private readonly ILingoCommandManager _commandManager;
    private readonly ILingoFrameworkFactory _factory;
    private readonly LingoGfxPanel _panel;
    private readonly LingoGfxWrapPanel _container;
    private LingoGfxStateButton? _selectedButton;

    public event Action<PainterToolType>? ToolSelected;

    public PainterToolType SelectedTool { get; private set; } = PainterToolType.Pencil;
    public LingoColor SelectedColor { get; private set; } = new LingoColor(0, 0, 0);
    public LingoColor BGColor { get; set; } = DirectorColors.BG_WhiteMenus;

    public PaintToolbar(IDirectorIconManager iconManager, ILingoCommandManager commandManager, ILingoFrameworkFactory factory)
    {
        _iconManager = iconManager;
        _commandManager = commandManager;
        _factory = factory;

        _panel = factory.CreatePanel("PaintToolbarRoot");
        _panel.BackgroundColor = BGColor;
        _panel.Width = 52;   // fallback size similar to Godot implementation
        _panel.Height = 200;

        _container = factory.CreateWrapPanel(LingoOrientation.Horizontal, "PaintToolbarContainer");
        // TODO: custom minimum size (48,100) when supported
        // TODO: size flags ExpandFill/ShrinkBegin when supported
        _container.ItemMargin = new LingoMargin(2,2,0,0); // margin_left/top
        _panel.AddItem(_container);

        AddToolButton(DirectorIcon.Pencil);
        AddToolButton(DirectorIcon.PaintBrush);
        AddToolButton(DirectorIcon.Eraser);
        AddToolButton(DirectorIcon.PaintLasso);
        AddToolButton(DirectorIcon.RectangleSelect);
        AddToolButton(DirectorIcon.PaintBucket);
        AddColorPickerForeground();
        AddColorPickerBackground();

        ToolSelected?.Invoke(SelectedTool);
    }

    public LingoGfxPanel Panel => _panel;

    private void AddToolButton(DirectorIcon icon)
    {
        var btn = _factory.CreateStateButton(icon.ToString(), _iconManager.Get(icon));
        btn.Width = 20; // approximate size
        btn.Height = 20;
        btn.ValueChanged += () =>
        {
            SelectButton(btn);
            var tool = icon switch
            {
                DirectorIcon.Pencil => PainterToolType.Pencil,
                DirectorIcon.PaintBrush => PainterToolType.PaintBrush,
                DirectorIcon.Eraser => PainterToolType.Eraser,
                DirectorIcon.PaintLasso => PainterToolType.SelectLasso,
                DirectorIcon.RectangleSelect => PainterToolType.SelectRectangle,
                DirectorIcon.ColorPicker => PainterToolType.ColorPicker,
                DirectorIcon.PaintBucket => PainterToolType.Fill,
                _ => throw new ArgumentOutOfRangeException(nameof(icon), icon.ToString())
            };

            SelectedTool = tool;
            _commandManager.Handle(new PainterToolSelectCommand(tool));
            ToolSelected?.Invoke(tool);
        };

        _container.AddItem(btn);
    }

    public void SelectTool(PainterToolType tool)
    {
        SelectedTool = tool;
        ToolSelected?.Invoke(tool);
    }

    private void SelectButton(LingoGfxStateButton btn)
    {
        if (_selectedButton != null)
            _selectedButton.IsOn = false;

        btn.IsOn = true;
        _selectedButton = btn;
    }

    private void AddColorPickerForeground() =>
        AddColorPicker(color => new PainterChangeForegroundColorCommand(color));

    private void AddColorPickerBackground() =>
        AddColorPicker(color => new PainterChangeBackgroundColorCommand(color));

    private void AddColorPicker(Func<LingoColor, ILingoCommand> toCommand)
    {
        var picker = _factory.CreateColorPicker("PaintColorPicker", color =>
        {
            SelectedColor = color;
            _commandManager.Handle(toCommand(color));
        });
        picker.Width = 20;
        picker.Height = 20;
        _container.AddItem(picker);
    }
}
