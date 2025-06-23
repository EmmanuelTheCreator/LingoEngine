using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class ImportDirCstFilesStep : VBoxContainer
{
    private readonly FileDialog _fileDialog = new();
    private readonly VBoxContainer _fileList = new();
    private readonly Button _importButton = new();
    private readonly Button _backButton = new();
    private readonly List<string> _files = new();

    public event Action? Back;

    public ImportDirCstFilesStep()
    {
        Visible = false;

        var selectBtn = new Button { Text = "Select Files" };
        selectBtn.Pressed += () => _fileDialog.PopupCentered();
        AddChild(selectBtn);

        _fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        _fileDialog.FileMode = FileDialog.FileModeEnum.OpenFiles; 
        _fileDialog.Filters = new string[] { "*.dir; *.cst; *.cct; *.cxt" };
        _fileDialog.FilesSelected += paths => AddFiles(paths);
        AddChild(_fileDialog);

        var scroll = new ScrollContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        scroll.AddChild(_fileList);
        AddChild(scroll);

        var opts = new VBoxContainer();
        opts.AddChild(new Label { Text = "Import:" });
        opts.AddChild(new CheckBox { Text = "Score", ButtonPressed = true });
        opts.AddChild(new CheckBox { Text = "Pictures", ButtonPressed = true });
        opts.AddChild(new CheckBox { Text = "Text", ButtonPressed = true });
        opts.AddChild(new CheckBox { Text = "Audio", ButtonPressed = true });
        opts.AddChild(new CheckBox { Text = "Fields", ButtonPressed = true });
        AddChild(opts);

        _importButton.Text = "Import";
        AddChild(_importButton);

        _backButton.Text = "Back";
        _backButton.Pressed += () => Back?.Invoke();
        AddChild(_backButton);
    }

    private void AddFiles(IEnumerable<string> paths)
    {
        bool hasDir = _files.Exists(f => f.EndsWith(".dir", StringComparison.OrdinalIgnoreCase));
        foreach (var p in paths)
        {
            if (string.IsNullOrEmpty(p))
                continue;
            var ext = Path.GetExtension(p).ToLowerInvariant();
            if (ext == ".dir")
            {
                if (hasDir)
                    continue;
                hasDir = true;
            }
            else if (ext != ".cst" && ext != ".cxt" && ext != ".cct")
            {
                continue;
            }

            if (_files.Contains(p))
                continue;
            _files.Add(p);
            AddFileRow(p);
        }
    }

    private void AddFileRow(string path)
    {
        var row = new HBoxContainer();
        row.AddChild(new Label { Text = Path.GetFileName(path), CustomMinimumSize = new Vector2(200,16) });
        var trash = new Button { Text = "🗑" };
        trash.Pressed += () => RemoveFile(row, path);
        row.AddChild(trash);
        _fileList.AddChild(row);
    }

    private void RemoveFile(HBoxContainer row, string path)
    {
        _fileList.RemoveChild(row);
        row.QueueFree();
        _files.Remove(path);
    }
}

