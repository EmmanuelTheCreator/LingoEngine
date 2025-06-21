using Godot;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.Core.Stages;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotToolsWindow : BaseGodotWindow, IDirFrameworkToolsWindow
{
    private readonly GridContainer _grid = new GridContainer();
    private readonly IStageToolManager _toolManager;

    public event Action<int>? IconPressed;

    public DirGodotToolsWindow(DirectorToolsWindow directorToolsWindow, IStageToolManager toolManager, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.ToolsWindow, "Tools", windowManager)
    {
        directorToolsWindow.Init(this);
        _toolManager = toolManager;

        Size = new Vector2(80, 200);
        CustomMinimumSize = Size;

        _grid.Columns = 2;
        _grid.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(_grid);

        AddButton("P", StageTool.Pointer);
        AddButton("M", StageTool.Move);
        AddButton("R", StageTool.Rotate);
    }

    private void AddButton(string text, StageTool tool)
    {
        var btn = new Button { Text = text };
        btn.CustomMinimumSize = new Vector2(30, 20);
        btn.Pressed += () => _toolManager.CurrentTool = tool;
        _grid.AddChild(btn);
    }

  
}
