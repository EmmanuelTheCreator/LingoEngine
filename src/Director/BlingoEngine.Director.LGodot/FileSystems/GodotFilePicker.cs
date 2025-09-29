using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.LGodot;

namespace BlingoEngine.Director.LGodot.FileSystems;

public partial class GodotFilePicker : IDirFilePicker
{
    private readonly BlingoGodotRootNode _directorRoot;

    public GodotFilePicker(BlingoGodotRootNode directorRoot)
    {
        _directorRoot = directorRoot;
    }

    public void PickFile(Action<string> onPicked, string filter, string? currentFile = null)
    {
#if USE_WINDOWS_FEATURES
        var dialog = CreateDialog(FileDialog.FileModeEnum.OpenFile, filter, currentFile, treatCurrentPathAsFile: true);
        dialog.FileSelected += onPicked;
        ShowDialog(dialog);
#else
        GD.PushWarning("File picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
    }

    public void PickFiles(Action<IReadOnlyList<string>> onPicked, string filter, string? currentPath = null)
    {
#if USE_WINDOWS_FEATURES
        var dialog = CreateDialog(FileDialog.FileModeEnum.OpenFiles, filter, currentPath, treatCurrentPathAsFile: false);
        dialog.FilesSelected += files =>
        {
            if (files.Length > 0)
                onPicked(Array.AsReadOnly(files));
        };
        ShowDialog(dialog);
#else
        GD.PushWarning("File picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
    }

#if USE_WINDOWS_FEATURES
    private FileDialog CreateDialog(FileDialog.FileModeEnum mode, string filter, string? currentPath, bool treatCurrentPathAsFile)
    {
        var dialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = mode,
            Filters = new[] { filter }
        };

        ConfigureInitialPath(dialog, currentPath, treatCurrentPathAsFile);
        return dialog;
    }

    private void ConfigureInitialPath(FileDialog dialog, string? currentPath, bool treatCurrentPathAsFile)
    {
        if (string.IsNullOrWhiteSpace(currentPath))
            return;

        try
        {
            if (treatCurrentPathAsFile && !string.IsNullOrWhiteSpace(currentPath))
            {
                dialog.CurrentFile = currentPath;
                var directory = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                    dialog.CurrentDir = directory;
                return;
            }

            if (Directory.Exists(currentPath))
            {
                dialog.CurrentDir = currentPath;
                return;
            }

            var pathDirectory = Path.GetDirectoryName(currentPath);
            if (!string.IsNullOrEmpty(pathDirectory) && Directory.Exists(pathDirectory))
                dialog.CurrentDir = pathDirectory;
        }
        catch (Exception)
        {
            // Ignore invalid paths and fall back to default dialog location.
        }
    }

    private void ShowDialog(FileDialog dialog)
    {
        _directorRoot.RootNode.AddChild(dialog);
        dialog.PopupCentered();
    }
#endif
}
