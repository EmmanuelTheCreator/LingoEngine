using Godot;
using LingoEngine.Director.Core;
using LingoEngine.Director.Core.Windows;
using LingoEngine;
using LingoEngine.Director.LGodot;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class DirGodotProjectSettingsWindow : BaseGodotWindow, IDirFrameworkProjectSettingsWindow
{
    private readonly ProjectSettings _settings;
    private readonly LineEdit _nameEdit = new LineEdit();
    private readonly LineEdit _folderEdit = new LineEdit();

    public DirGodotProjectSettingsWindow(ProjectSettings settings, GodotWindowManager windowManager)
        : base("Project Settings", windowManager)
    {
        _settings = settings;

        Size = new Vector2(300, 120);
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

        var save = new Button { Text = "Save" };
        save.Pressed += OnSavePressed;
        vbox.AddChild(save);
    }

    private void OnSavePressed()
    {
        _settings.ProjectName = _nameEdit.Text.Trim();
        _settings.ProjectFolder = _folderEdit.Text.Trim();
        Visible = false;
    }

    
}
