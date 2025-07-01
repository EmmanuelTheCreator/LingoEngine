using Godot;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.LGodot.Gfx;
using LingoEngine.Commands;
using LingoEngine.Director.Core.Icons;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotToolsWindow : BaseGodotWindow, IDirFrameworkToolsWindow
{
    private readonly IStageToolManager _toolManager;
    private StageToolbar _stageToolbar;

    public event Action<int>? IconPressed;

    public DirGodotToolsWindow(DirectorToolsWindow directorToolsWindow, IStageToolManager toolManager, IDirGodotWindowManager windowManager, IDirectorIconManager iconManager, ILingoCommandManager commandManager, ILingoFrameworkFactory factory)
        : base(DirectorMenuCodes.ToolsWindow, "Tools", windowManager)
    {
        directorToolsWindow.Init(this);
        _toolManager = toolManager;

        Size = new Vector2(80, 200);
        CustomMinimumSize = Size;

        _stageToolbar = new StageToolbar(iconManager, commandManager, factory);
        var toolbarPanel = _stageToolbar.Panel.Framework<LingoGodotPanel>();
        toolbarPanel.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(toolbarPanel);

        //AddButton("P", StageTool.Pointer);
        //AddButton("M", StageTool.Move);
        //AddButton("R", StageTool.Rotate);
    }

   

  
}
