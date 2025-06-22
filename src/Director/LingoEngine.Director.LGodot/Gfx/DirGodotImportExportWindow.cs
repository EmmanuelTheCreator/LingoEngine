using Godot;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotImportExportWindow : BaseGodotWindow, IDirFrameworkImportExportWindow
{
    private readonly VBoxContainer _home = new();
    private readonly ImportLingoFilesStep _importLingoStep;
    private readonly ImportDirCstFilesStep _importDirStep;
    private readonly Button _scriptsButton = new();
    private readonly Button _dirButton = new();
    private readonly Button _exportButton = new();

    public DirGodotImportExportWindow(ProjectSettings settings, DirectorImportExportWindow directorWindow, IDirGodotWindowManager windowManager)
        : base(DirectorMenuCodes.ImportExportWindow, "Import / Export", windowManager)
    {
        directorWindow.Init(this);
        Size = new Vector2(400, 300);
        CustomMinimumSize = Size;

        _home.Position = new Vector2(5, TitleBarHeight + 5);
        _home.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _home.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_home);

        _scriptsButton.Text = "Import Lingo scripts";
        _scriptsButton.Pressed += () => ShowStep(_importLingoStep);
        _home.AddChild(_scriptsButton);

        _dirButton.Text = "Import dir/cst file";
        _dirButton.Pressed += () => ShowStep(_importDirStep);
        _home.AddChild(_dirButton);

        _exportButton.Text = "Export/Optimize code through AI";
        _home.AddChild(_exportButton);

        _importLingoStep = new ImportLingoFilesStep(settings);
        _importLingoStep.Back += ShowHome;
        AddChild(_importLingoStep);

        _importDirStep = new ImportDirCstFilesStep();
        _importDirStep.Back += ShowHome;
        AddChild(_importDirStep);

        ShowHome();
    }

    private void ShowHome()
    {
        _home.Visible = true;
        _importLingoStep.Visible = false;
        _importDirStep.Visible = false;
    }

    private void ShowStep(Control step)
    {
        _home.Visible = false;
        step.Visible = true;
    }
}
