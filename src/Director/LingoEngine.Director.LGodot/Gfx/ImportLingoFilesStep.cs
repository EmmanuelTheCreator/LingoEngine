using Godot;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Lingo.Core;
using LingoEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LingoEngine.Director.LGodot.Gfx;

internal partial class ImportLingoFilesStep : VBoxContainer
{
    private readonly ProjectSettings _settings;
    private readonly VBoxContainer _fileList = new();
    private readonly List<HBoxContainer> _rows = new();
    private readonly HashSet<HBoxContainer> _selectedRows = new();
    private readonly StyleBoxFlat _normalStyle = new();
    private readonly StyleBoxFlat _selectedStyle = new() { BgColor = Colors.SkyBlue };
    private readonly FileDialog _folderDialog = new();
    private readonly Button _importButton = new();
    private readonly OptionButton _bulkType = new();
    private readonly Button _backButton = new();
    private string _folderPath = string.Empty;

    public event Action? Back;

    public ImportLingoFilesStep(ProjectSettings settings)
    {
        _settings = settings;
        Visible = false;

        var selectBtn = new Button { Text = "Select Folder" };
        selectBtn.Pressed += () => _folderDialog.PopupCentered();
        AddChild(selectBtn);

        _backButton.Text = "Back";
        _backButton.Pressed += () => Back?.Invoke();
        AddChild(_backButton);

        _folderDialog.Access = FileDialog.AccessEnum.Filesystem;
        _folderDialog.Mode = FileDialog.FileModeEnum.OpenDir;
        _folderDialog.DirSelected += dir => LoadFolder(dir);
        AddChild(_folderDialog);

        var scroll = new ScrollContainer();
        scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.AddChild(_fileList);
        AddChild(scroll);

        var bulkRow = new HBoxContainer();
        bulkRow.AddChild(new Label { Text = "Set type for selected" });
        _bulkType.AddItem("Parent", (int)LingoScriptType.Parent);
        _bulkType.AddItem("Movie", (int)LingoScriptType.Movie);
        _bulkType.AddItem("Behavior", (int)LingoScriptType.Behavior);
        _bulkType.Selected = 2;
        _bulkType.ItemSelected += idx => ApplyBulkType((int)idx);
        bulkRow.AddChild(_bulkType);
        AddChild(bulkRow);

        _importButton.Text = "Import";
        _importButton.Pressed += OnImportPressed;
        AddChild(_importButton);
    }

    private void LoadFolder(string path)
    {
        _folderPath = path;
        foreach (var child in _fileList.GetChildren())
        {
            if (child is Node n)
            {
                _fileList.RemoveChild(n);
                n.QueueFree();
            }
        }
        _rows.Clear();
        _selectedRows.Clear();
        _lastClickedIndex = -1;

        foreach (var file in Directory.GetFiles(path, "*.ls"))
        {
            var row = new HBoxContainer();
            row.SetMeta("path", file);

            var label = new Label { Text = Path.GetFileName(file), CustomMinimumSize = new Vector2(200, 16) };
            row.AddChild(label);

            var opt = new OptionButton();
            opt.AddItem("Parent", (int)LingoScriptType.Parent);
            opt.AddItem("Movie", (int)LingoScriptType.Movie);
            opt.AddItem("Behavior", (int)LingoScriptType.Behavior);
            opt.Selected = 2;
            row.AddChild(opt);
            row.SetMeta("opt", opt);

            row.AddThemeStyleboxOverride("panel", _normalStyle);
            row.MouseFilter = MouseFilterEnum.Stop;
            row.GuiInput += (InputEvent e) => OnRowGuiInput(row, e);

            _fileList.AddChild(row);
            _rows.Add(row);
        }
    }

    private int _lastClickedIndex = -1;

    private void OnRowGuiInput(HBoxContainer row, InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
        {
            int index = _rows.IndexOf(row);
            bool shift = Input.IsKeyPressed(Key.Shift);
            bool ctrl = Input.IsKeyPressed(Key.Ctrl);

            if (shift && _lastClickedIndex >= 0)
            {
                if (!ctrl)
                    ClearSelection();
                int start = Mathf.Min(_lastClickedIndex, index);
                int end = Mathf.Max(_lastClickedIndex, index);
                for (int i = start; i <= end; i++)
                    SelectRow(_rows[i], true);
            }
            else if (ctrl)
            {
                if (_selectedRows.Contains(row))
                    SelectRow(row, false);
                else
                    SelectRow(row, true);
                _lastClickedIndex = index;
            }
            else
            {
                ClearSelection();
                SelectRow(row, true);
                _lastClickedIndex = index;
            }
        }
    }

    private void ApplyBulkType(int id)
    {
        foreach (var row in _selectedRows)
        {
            if (row.GetMeta("opt") is OptionButton opt)
                opt.Selected = id;
        }
    }

    private void ClearSelection()
    {
        foreach (var row in _selectedRows.ToList())
            SelectRow(row, false);
    }

    private void SelectRow(HBoxContainer row, bool select)
    {
        if (select)
        {
            if (_selectedRows.Add(row))
                row.AddThemeStyleboxOverride("panel", _selectedStyle);
        }
        else
        {
            if (_selectedRows.Remove(row))
                row.AddThemeStyleboxOverride("panel", _normalStyle);
        }
    }

    private void OnImportPressed()
    {
        if (string.IsNullOrEmpty(_folderPath) || !_settings.HasValidSettings)
            return;

        var scripts = new List<LingoScriptFile>();
        foreach (var child in _fileList.GetChildren())
        {
            if (child is HBoxContainer row)
            {
                string path = (string)row.GetMeta("path");
                var opt = (OptionButton)row.GetMeta("opt");
                var type = (LingoScriptType)opt.GetSelectedId();
                scripts.Add(new LingoScriptFile
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    Source = File.ReadAllText(path),
                    Type = type
                });
            }
        }

        if (scripts.Count == 0)
            return;

        var result = LingoToCSharpConverter.Convert(scripts);
        foreach (var script in scripts)
        {
            if (!result.ConvertedScripts.TryGetValue(script.Name, out var code))
                continue;
            string folder = script.Type switch
            {
                LingoScriptType.Movie => Path.Combine(_settings.ProjectFolder, "MovieScripts"),
                LingoScriptType.Parent => Path.Combine(_settings.ProjectFolder, "ParentScripts"),
                LingoScriptType.Behavior => Path.Combine(_settings.ProjectFolder, "Sprites", "Behaviors"),
                _ => _settings.ProjectFolder
            };
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, script.Name + ".cs"), code);
        }
        Back?.Invoke();
    }
}
