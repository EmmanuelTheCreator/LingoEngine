using Godot;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotProjectSettingsWindow : BaseGodotWindow, IDirFrameworkProjectSettingsWindow
{
    private readonly ProjectSettingsEditorState _state;
    private readonly IIdePathResolver _resolver;
    private readonly IExecutableFilePicker _picker;
    private readonly HBoxContainer _vsPathRow;
    private readonly HBoxContainer _vsCodePathRow;

    private readonly LineEdit _nameEdit = new LineEdit();
    private readonly LineEdit _folderEdit = new LineEdit();
    private readonly OptionButton _ideSelect = new OptionButton();
    private readonly LineEdit _vsPathEdit = new LineEdit();
    private readonly LineEdit _vscodePathEdit = new LineEdit();
    private readonly Label _slnPreviewLabel = new Label();




    public DirGodotProjectSettingsWindow(
        ProjectSettingsEditorState state,
        IIdePathResolver resolver,
        IExecutableFilePicker picker,
        IDirGodotWindowManager windowManager,
        DirectorProjectSettingsWindow directorProjectSettingsWindow)
        : base(DirectorMenuCodes.ProjectSettingsWindow, "Project Settings", windowManager)
    {
        _state = state;
        _resolver = resolver;
        _picker = picker;
        directorProjectSettingsWindow.Init(this);
        Size = new Vector2(420, 200);
        CustomMinimumSize = Size;

        var vbox = new VBoxContainer();
        vbox.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(vbox);

        // Project name
        var h1 = new HBoxContainer();
        h1.AddChild(new Label { Text = "Project Name", CustomMinimumSize = new Vector2(100, 16) });
        _nameEdit.Text = state.ProjectName;
        h1.AddChild(_nameEdit);
        vbox.AddChild(h1);
        _nameEdit.TextChanged += (_) => _slnPreviewLabel.Text = GetSlnPreview();
        _folderEdit.TextChanged += (_) => _slnPreviewLabel.Text = GetSlnPreview();


        // Project folder
        var h2 = new HBoxContainer();
        h2.AddChild(new Label { Text = "Project Folder", CustomMinimumSize = new Vector2(100, 16) });
        _folderEdit.Text = state.ProjectFolder;
        h2.AddChild(_folderEdit);
        vbox.AddChild(h2);

        _slnPreviewLabel.Text = GetSlnPreview();
        vbox.AddChild(_slnPreviewLabel);


        // IDE selection
        var h3 = new HBoxContainer();
        h3.AddChild(new Label { Text = "IDE", CustomMinimumSize = new Vector2(100, 16) });
        _ideSelect.AddItem("Visual Studio", (int)IdeType.VisualStudio);
        _ideSelect.AddItem("Visual Studio Code", (int)IdeType.VisualStudioCode);
        _ideSelect.Selected = (int)state.SelectedIde;
        _ideSelect.ItemSelected += OnIdeSelected;
        h3.AddChild(_ideSelect);
        vbox.AddChild(h3);

        // Visual Studio path row
        _vsPathRow = new HBoxContainer();
        _vsPathRow.AddChild(new Label { Text = "VS Path", CustomMinimumSize = new Vector2(100, 16) });
        _vsPathEdit.Text = state.VisualStudioPath;
        _vsPathRow.AddChild(_vsPathEdit);
        var vsBrowse = new Button { Text = "..." };
        vsBrowse.Pressed += () => _picker.PickExecutable(path => _vsPathEdit.Text = path);
        _vsPathRow.AddChild(vsBrowse);
        var vsAuto = new Button { Text = "Auto" };
        vsAuto.Pressed += () => _vsPathEdit.Text = _resolver.AutoDetectVisualStudioPath() ?? "";
        _vsPathRow.AddChild(vsAuto);
        vbox.AddChild(_vsPathRow);

        // VS Code path row
        _vsCodePathRow = new HBoxContainer();
        _vsCodePathRow.AddChild(new Label { Text = "VS Code Path", CustomMinimumSize = new Vector2(100, 16) });
        _vscodePathEdit.Text = state.VisualStudioCodePath;
        _vsCodePathRow.AddChild(_vscodePathEdit);
        var codeBrowse = new Button { Text = "..." };
        codeBrowse.Pressed += () => _picker.PickExecutable(path => _vscodePathEdit.Text = path);
        _vsCodePathRow.AddChild(codeBrowse);
        var codeAuto = new Button { Text = "Auto" };
        codeAuto.Pressed += () => _vscodePathEdit.Text = _resolver.AutoDetectVSCodePath() ?? "";
        _vsCodePathRow.AddChild(codeAuto);
        vbox.AddChild(_vsCodePathRow);

        // Save button
        var save = new Button { Text = "Save" };
        save.Pressed += OnSavePressed;
        vbox.AddChild(save);


        var apply = new Button { Text = "Apply" };
        apply.Pressed += () =>
        {
            if (ValidateSettings())
                SaveState();
        };
        vbox.AddChild(apply);


        UpdateIdePathVisibility();
    }

    private string GetSlnPreview()
    {
        var name = _nameEdit.Text.Trim();
        var folder = _folderEdit.Text.Trim();
        return string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(folder)
            ? "(Solution path preview unavailable)"
            : $"Solution: {System.IO.Path.Combine(folder, $"{name}.sln")}";
    }


    private void OnIdeSelected(long index) => UpdateIdePathVisibility();

    private void UpdateIdePathVisibility()
    {
        bool showVs = _ideSelect.Selected == (int)IdeType.VisualStudio;
        _vsPathRow.Visible = showVs;
        _vsCodePathRow.Visible = !showVs;
    }

    private void OnSavePressed()
    {
        if (!ValidateSettings())
            return;

        SaveState();
        Visible = false;
    }

    private void SaveState()
    {
        _state.ProjectName = _nameEdit.Text.Trim();
        _state.ProjectFolder = _folderEdit.Text.Trim();
        _state.SelectedIde = (IdeType)_ideSelect.Selected;
        _state.VisualStudioPath = _vsPathEdit.Text.Trim();
        _state.VisualStudioCodePath = _vscodePathEdit.Text.Trim();
    }



    private bool ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_nameEdit.Text))
        {
            GD.PrintErr("Project name is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_folderEdit.Text))
        {
            GD.PrintErr("Project folder is required.");
            return false;
        }

        var ide = (IdeType)_ideSelect.Selected;
        if (ide == IdeType.VisualStudio && string.IsNullOrWhiteSpace(_vsPathEdit.Text))
        {
            GD.PrintErr("Visual Studio path is required.");
            return false;
        }

        if (ide == IdeType.VisualStudioCode && string.IsNullOrWhiteSpace(_vscodePathEdit.Text))
        {
            GD.PrintErr("VS Code path is required.");
            return false;
        }

        return true;
    }




}
