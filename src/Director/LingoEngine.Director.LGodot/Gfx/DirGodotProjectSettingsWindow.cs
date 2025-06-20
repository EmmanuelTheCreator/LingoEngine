using Godot;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Windows;
using LingoEngine;
using LingoEngine.Director.LGodot;
using LingoEngine.Director.Core.Gfx;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotProjectSettingsWindow : BaseGodotWindow, IDirFrameworkProjectSettingsWindow
{
    private readonly ProjectSettings _settings;
    private readonly LineEdit _nameEdit = new LineEdit();
    private readonly LineEdit _folderEdit = new LineEdit();
    private readonly OptionButton _ideSelect = new OptionButton();

    public DirGodotProjectSettingsWindow(ProjectSettings settings, IDirGodotWindowManager windowManager, DirectorProjectSettingsWindow directorProjectSettingsWindow)
        : base(DirectorMenuCodes.ProjectSettingsWindow, "Project Settings", windowManager)
    {
        _settings = settings;
        directorProjectSettingsWindow.Init(this);
        Size = new Vector2(300, 150);
        CustomMinimumSize = Size;

        var vbox = new VBoxContainer();
        vbox.Position = new Vector2(5, TitleBarHeight + 5);
        AddChild(vbox);

        var h1 = new HBoxContainer();
        h1.AddChild(new Label { Text = "Project Name", CustomMinimumSize = new Vector2(100, 16) });
        _nameEdit.Text = settings.ProjectName;
        h1.AddChild(_nameEdit);
        vbox.AddChild(h1);

        var h2 = new HBoxContainer();
        h2.AddChild(new Label { Text = "Project Folder", CustomMinimumSize = new Vector2(100, 16) });
        _folderEdit.Text = settings.ProjectFolder;
        h2.AddChild(_folderEdit);
        vbox.AddChild(h2);

        var h3 = new HBoxContainer();
        h3.AddChild(new Label { Text = "IDE", CustomMinimumSize = new Vector2(100, 16) });
        _ideSelect.AddItem("Visual Studio", (int)IdeType.VisualStudio);
        _ideSelect.AddItem("Visual Studio Code", (int)IdeType.VisualStudioCode);
        _ideSelect.Selected = (int)settings.PreferredIde;
        h3.AddChild(_ideSelect);
        vbox.AddChild(h3);

        var save = new Button { Text = "Save" };
        save.Pressed += OnSavePressed;
        vbox.AddChild(save);
    }

    private void OnSavePressed()
    {
        _settings.ProjectName = _nameEdit.Text.Trim();
        _settings.ProjectFolder = _folderEdit.Text.Trim();
        _settings.PreferredIde = (IdeType)_ideSelect.Selected;
        Visible = false;
    }

    
}
