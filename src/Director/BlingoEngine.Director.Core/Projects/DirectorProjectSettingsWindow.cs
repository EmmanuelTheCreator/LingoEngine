using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;
using AbstUI.Commands;
using BlingoEngine.Projects;
using BlingoEngine.Director.Core.Projects.Commands;
using System.IO;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Components.Containers;

namespace BlingoEngine.Director.Core.Projects;

/// <summary>
/// Framework independent implementation of the project settings window.
/// </summary>
public class DirectorProjectSettingsWindow : DirectorWindow<IDirFrameworkProjectSettingsWindow>
{
    private readonly ProjectSettingsEditorState _state;
    private readonly IIdePathResolver _resolver;
    private readonly IDirFolderPicker _folderPicker;
    private readonly IDirFilePicker _filePicker;
    private readonly IAbstWindowManager _windowManager;
    private readonly IAbstCommandManager _commandManager;
    private readonly BlingoProjectSettings _settings;
    private readonly DirectorProjectSettings _dirSettings;
    private bool _startNewProjectOnNextSave;

    private readonly AbstWrapPanel _root;
    private AbstWrapPanel? _vsCodePathRow;
    private AbstWrapPanel? _vsPathRow;
    private AbstLabel? _slnPreviewLabel;
    private AbstInputText? _csProjEdit;
    private string _projectName = "";
    private string _folderName = "";
    private string _csProjFile = "";
    private List<KeyValuePair<string, string>> _ideTypes = [
        new KeyValuePair<string, string>(DirectorIdeType.VisualStudio.ToString(), "Visual Studio"),
        new KeyValuePair<string, string>(DirectorIdeType.VisualStudioCode.ToString(), "Visual Studio Code"),
        ];
    private string visualStudioPath = "";
    private string visualStudioCodePath = "";
    private AbstInputText? _VsPathEdit;
    private AbstInputText? _VSCodePathEdit;

    public AbstWrapPanel RootPanel => _root;
    public string FolderName
    {
        get => _folderName;
        set
        {
            _folderName = value;
            UpdateSlnPreview();
        }
    }
    public string SolutionPreviewName { get; set; } = "";
    public string VisualStudioPath
    {
        get => visualStudioPath; set
        {
            visualStudioPath = value;
            if (_VsPathEdit != null)
                _VsPathEdit.Text = visualStudioPath;
        }
    }
    public string VisualStudioCodePath
    {
        get => visualStudioCodePath; set
        {
            visualStudioCodePath = value;
            if (_VSCodePathEdit != null)
                _VSCodePathEdit.Text = visualStudioCodePath;
        }
    }
    public string CsProjFile
    {
        get => _csProjFile;
        set
        {
            _csProjFile = value;
            if (_csProjEdit != null)
                _csProjEdit.Text = _csProjFile;
        }
    }
    public string ProjectName
    {
        get => _projectName;
        set
        {
            _projectName = value;
            UpdateSlnPreview();
        }
    }
    public DirectorIdeType SelectedIde { get; set; }

    public DirectorProjectSettingsWindow(
        IServiceProvider serviceProvider,
        ProjectSettingsEditorState state,
        IIdePathResolver resolver,
        IDirFolderPicker folderPicker,
        IDirFilePicker filePicker,
        IAbstWindowManager windowManager,
        IAbstCommandManager commandManager,
        BlingoProjectSettings settings,
        DirectorProjectSettings dirSettings,
        IBlingoFrameworkFactory factory) : base(serviceProvider, DirectorMenuCodes.ProjectSettingsWindow)
    {
        Width = 500;
        Height = 200;
        MinimumWidth = 200;
        MinimumHeight = 150;
        X = 100;
        Y = 100;
        _state = state;
        _resolver = resolver;
        _folderPicker = folderPicker;
        _filePicker = filePicker;
        _windowManager = windowManager;
        _commandManager = commandManager;
        _settings = settings;
        _dirSettings = dirSettings;
        _state.LoadFrom(_settings, _dirSettings);
        LoadState(state);

        _root = factory.CreateWrapPanel(AOrientation.Vertical, "ProjectSettingsRoot");
        _root.ItemMargin = new APoint(0, 4);
        _root.Width = Width;
        _root.Height = Height;

        // Project name
        _root.Compose()
            .NewLine("NameRow")
            .AddLabel("NameLabel", "Project Name:", 11, 100)
            .AddTextInput("NameEdit", this, s => s.ProjectName, 200)
            // Project folder
            .NewLine("FolderRow")
            .AddLabel("FolderLabel", "Project Folder:", 11, 100)
            .AddTextInput("FolderEdit", this, s => s.FolderName, 200)
            // Csproj file
            .NewLine("CsProjRow")
            .AddLabel("CsProjLabel", "C# Project:", 11, 100)
            .AddTextInput("CsProjEdit", this, s => s.CsProjFile, 200, c => _csProjEdit = c)
            .AddButton("CsProjBrowse", "Browse...", () => _filePicker.PickFile(path =>
            {
                if (!string.IsNullOrWhiteSpace(FolderName))
                    path = Path.GetRelativePath(FolderName, path);
                CsProjFile = path;
            }, "*.csproj ; C# Project Files", CsProjFile), c => c.Width = 80)
            // Preview
            .NewLine("PeviewRow")
            .AddLabel("PreviewLabel", GetSlnPreview(), 11, null, c => { _slnPreviewLabel = c; })
             // IDE selection
             .NewLine("IdeRow")
            .AddLabel("IdeLabel", "IDE", 11, 100)
            .AddCombobox("ComboIdeTypes", _ideTypes, 100, state.SelectedIde.ToString(), s => { SelectedIde = Enum.Parse<DirectorIdeType>(s!); UpdateIdePathVisibility(); })
            // Visual Studio path
            .NewLine("VsPathRow")
            .Configure(c => _vsPathRow = c)
            .AddLabel("VsPathLabel", "VS Path", 11, 100)
            .AddTextInput("VsPathEdit", this, s => s.VisualStudioPath, 200, c => _VsPathEdit = c)
            .AddButton("VsBrowse", "Browse...", () => _folderPicker.PickFolder(path => VisualStudioPath = path,VisualStudioPath), c => c.Width = 80)
            .AddButton("VsAuto", "Auto", () => VisualStudioPath = _resolver.AutoDetectVisualStudioPath() ?? string.Empty, c => c.Width = 80)
            // VS Code path
            .NewLine("VSCodePathRow")
            .Configure(c => _vsCodePathRow = c)
            .AddLabel("VSCodePathLabel", "VS Code Path", 11, 100)
            .AddTextInput("VSCodePathEdit", this, s => s.VisualStudioCodePath, 200, c => _VSCodePathEdit = c)
            .AddButton("CodeBrowse", "Browse...", () => _folderPicker.PickFolder(path => VisualStudioCodePath = path, VisualStudioCodePath), c => c.Width = 80)
            .AddButton("CodeAuto", "Auto", () => VisualStudioCodePath = _resolver.AutoDetectVSCodePath() ?? string.Empty, c => c.Width = 80)
            // Save & Apply buttons
            .NewLine("ButtonRow")
            .AddButton("SaveButton", "Save", OnSavePressed, c => c.Width = 90)
            .AddButton("ApplyButton", "Apply", () =>
            {
                if (!ValidateSettings())
                    return;
                SaveState();
                _state.SaveTo(_settings, _dirSettings);
                _commandManager.Handle(new SaveDirProjectSettingsCommand(_dirSettings, _settings));
            }, c => c.Width = 90)
            .Finalize();

        UpdateIdePathVisibility();
    }

    protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
    {
        base.OnInit(frameworkWindow);
        Title = "Project Settings";
    }

    public void PrepareForNewProject()
    {
        _startNewProjectOnNextSave = true;
        ProjectName = string.Empty;
        FolderName = string.Empty;
        CsProjFile = string.Empty;
    }

    public override void CloseWindow()
    {
        base.CloseWindow();
        _startNewProjectOnNextSave = false;
    }


    private void UpdateSlnPreview()
    {
        SolutionPreviewName = GetSlnPreview();
        _slnPreviewLabel!.Text = SolutionPreviewName;
    }
    private string GetSlnPreview()
    {
        var name = ProjectName.Trim();
        var folder = FolderName.Trim();
        return string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(folder)
            ? "(Solution path preview unavailable)"
            : $"Solution: {System.IO.Path.Combine(folder, $"{name}.sln")}";
    }

    private void OnSavePressed()
    {
        if (!ValidateSettings())
            return;

        SaveState();
        _state.SaveTo(_settings, _dirSettings);
        var handled = _commandManager.Handle(new SaveDirProjectSettingsCommand(_dirSettings, _settings, _startNewProjectOnNextSave));
        if (handled)
            CloseWindow();
    }
    private void LoadState(ProjectSettingsEditorState state)
    {
        _projectName = state.ProjectName;
        _folderName = state.ProjectFolder;
        _csProjFile = state.CsProjFile;
        SelectedIde = state.SelectedIde;
        VisualStudioPath = state.VisualStudioPath;
        VisualStudioCodePath = state.VisualStudioCodePath;
    }
    private void SaveState()
    {
        _state.ProjectName = ProjectName.Trim();
        _state.ProjectFolder = FolderName.Trim();
        _state.CsProjFile = CsProjFile.Trim();
        _state.SelectedIde = SelectedIde;
        _state.VisualStudioPath = VisualStudioPath.Trim();
        _state.VisualStudioCodePath = VisualStudioCodePath.Trim();
    }

    private void UpdateIdePathVisibility()
    {
        var showVs = SelectedIde == DirectorIdeType.VisualStudio;
        _vsPathRow!.Visibility = showVs;
        _vsCodePathRow!.Visibility = !showVs;
    }

    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            _windowManager.ShowNotification("Project name is required.", AbstUINotificationType.Error);
            return false;
        }

        if (string.IsNullOrWhiteSpace(FolderName))
        {
            _windowManager.ShowNotification("Project folder is required.", AbstUINotificationType.Error);
            return false;
        }

        if (SelectedIde == DirectorIdeType.VisualStudio && string.IsNullOrWhiteSpace(VisualStudioPath))
        {
            _windowManager.ShowNotification("Visual Studio path is required.", AbstUINotificationType.Error);
            return false;
        }

        if (SelectedIde == DirectorIdeType.VisualStudioCode && string.IsNullOrWhiteSpace(VisualStudioCodePath))
        {
            _windowManager.ShowNotification("VS Code path is required.", AbstUINotificationType.Error);
            return false;
        }

        return true;
    }
}


