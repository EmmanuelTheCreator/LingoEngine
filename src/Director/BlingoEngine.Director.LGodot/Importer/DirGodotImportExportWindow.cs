using Godot;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Importer;
using BlingoEngine.Projects;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BlingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotImportExportWindow : BaseGodotWindow, IDirFrameworkImportExportWindow, IFrameworkFor<DirectorImportExportWindow>
{
    private readonly VBoxContainer _home = new();
    private readonly ImportBlingoFilesStep _importBlingoStep;
    private readonly ImportDirCstFilesStep _importDirStep;
    private readonly Button _scriptsButton = new();
    private readonly Button _dirButton = new();
    private readonly Button _exportButton = new();

    public DirGodotImportExportWindow(BlingoProjectSettings settings, BlingoPlayer player, DirectorImportExportWindow directorWindow, IServiceProvider serviceProvider)
        : base("Import / Export", serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<DirGodotImportExportWindow>>();
        Init(directorWindow);
       
        CustomMinimumSize = Size;

        _home.Position = new Vector2(5, TitleBarHeight + 5);
        _home.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _home.SizeFlagsVertical = SizeFlags.ExpandFill;
        AddChild(_home);

        _scriptsButton.Text = "Import Lingo scripts";
        _scriptsButton.Pressed += () => ShowStep(_importBlingoStep);
        _home.AddChild(_scriptsButton);

        _dirButton.Text = "Import dir/cst file";
        _dirButton.Pressed += () => ShowStep(_importDirStep);
        _home.AddChild(_dirButton);

        _exportButton.Text = "Export/Optimize code through AI";
        _home.AddChild(_exportButton);

        _importBlingoStep = new ImportBlingoFilesStep(settings)
        {
            Position = new Vector2(5, TitleBarHeight + 5),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _importBlingoStep.Back += ShowHome;
        AddChild(_importBlingoStep);

        _importDirStep = new ImportDirCstFilesStep(player, logger)
        {
            Position = new Vector2(5, TitleBarHeight + 5),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _importDirStep.Back += ShowHome;
        AddChild(_importDirStep);

        ShowHome();
    }

    private void ShowHome()
    {
        _home.Visible = true;
        _importBlingoStep.Visible = false;
        _importDirStep.Visible = false;
    }

    private void ShowStep(Control step)
    {
        _home.Visible = false;
        step.Visible = true;
    }
}

