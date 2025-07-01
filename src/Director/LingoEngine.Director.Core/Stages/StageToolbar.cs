using LingoEngine.Commands;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Bitmaps.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Director.Core.Stages.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Stages;

public class StageToolbar : DirectorToolbar<StageTool>
{

    public LingoColor SelectedColor { get; private set; } = new LingoColor(0, 0, 0);
    public StageToolbar(IDirectorIconManager iconManager, ILingoCommandManager commandManager, ILingoFrameworkFactory factory) : base("StageToolbarRoot", iconManager, commandManager, factory)
    {
        AddToolButton(DirectorIcon.Pencil);
        AddToolButton(DirectorIcon.PaintRotateFree);
        AddToolButton(DirectorIcon.Hand);
        //AddToolButton(DirectorIcon.Magnifier);
        //AddToolButton(DirectorIcon.MemberTypeText);
        //AddToolButton(DirectorIcon.PaintFreeLine);
        AddColorPickerForeground(LingoColorList.Black);
        AddColorPickerBackground(LingoColorList.White);

        SelectTool(StageTool.Pointer);
    }
    protected override StageTool ConvertToTool(DirectorIcon icon)
    {
        return icon switch
        {
            DirectorIcon.Pencil => StageTool.Pointer,
            DirectorIcon.PaintRotateFree => StageTool.Rotate,
            DirectorIcon.Hand => StageTool.Move,
            _ => throw new ArgumentOutOfRangeException(nameof(icon), icon.ToString())
        };
    }
    protected void AddToolButton(DirectorIcon icon) => AddToolButton(icon, tool => new StageToolSelectCommand(tool));

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
