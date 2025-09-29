using Godot;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.LGodot;

namespace BlingoEngine.Director.LGodot.FileSystems
{
    public partial class GodotFolderPicker : IDirFolderPicker
    {
        private readonly BlingoGodotRootNode _directorRoot;

        public GodotFolderPicker(BlingoGodotRootNode directorRoot)
        {
            _directorRoot = directorRoot;
        }

        public void PickFolder(Action<string> onPicked, string? currentFolder = null)
        {
#if USE_WINDOWS_FEATURES
        var dialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenDir,
            CurrentPath = currentFolder,
        };
        dialog.DirSelected += h =>
        {
            onPicked(h);
            dialog.QueueFree();
        };
        PrepareDialog(dialog);
        _directorRoot.RootNode.AddChild(dialog);
        dialog.PopupCentered();
#else
            GD.PushWarning("Executable folder picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
        }

#if USE_WINDOWS_FEATURES
        private static void PrepareDialog(FileDialog dialog)
        {
            dialog.Exclusive = false;
            dialog.Transient = true;
            dialog.TransientToFocused = true;

            dialog.CloseRequested += dialog.QueueFree;
            dialog.Canceled += dialog.QueueFree;
        }
#endif
    }
}

