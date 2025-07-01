using LingoEngine.Commands;
using LingoEngine.Director.Core.Bitmaps.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Bitmaps;

public class PaintToolbar : DirectorToolbar<PainterToolType>
{

    public LingoColor SelectedColor { get; private set; } = new LingoColor(0, 0, 0);
    public PaintToolbar(IDirectorIconManager iconManager, ILingoCommandManager commandManager, ILingoFrameworkFactory factory) : base("PaintToolbarRoot", iconManager, commandManager, factory)
    {
        AddToolButton(DirectorIcon.Pencil);
        AddToolButton(DirectorIcon.PaintBrush);
        AddToolButton(DirectorIcon.Eraser);
        AddToolButton(DirectorIcon.PaintLasso);
        AddToolButton(DirectorIcon.RectangleSelect);
        AddToolButton(DirectorIcon.PaintBucket);
        AddColorPickerForeground(LingoColorList.Black);
        AddColorPickerBackground(LingoColorList.White);

        SelectTool(PainterToolType.Pencil);
    }

    protected void AddToolButton(DirectorIcon icon) => AddToolButton(icon, tool => new PainterToolSelectCommand(tool));
    protected override PainterToolType ConvertToTool(DirectorIcon icon)
    {
        return icon switch
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
    }
    private void AddColorPickerForeground(LingoColor color) 
        => AddColorPicker(c => new PainterChangeForegroundColorCommand(c), color);

    private void AddColorPickerBackground(LingoColor color) 
        => AddColorPicker(c => new PainterChangeBackgroundColorCommand(c), color);

    protected void AddColorPicker(Func<LingoColor, ILingoCommand> toCommand, LingoColor color)
    {
        var picker = _factory.CreateColorPicker("PaintColorPicker", color =>
        {
            SelectedColor = color;
            _commandManager.Handle(toCommand(color));
        });
        picker.Width = 20;
        picker.Height = 20;
        picker.Color = color;
        _container.AddItem(picker);
    }


}
