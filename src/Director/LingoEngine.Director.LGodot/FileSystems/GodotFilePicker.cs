using Godot;
using LingoEngine.Director.Core.FileSystems;

namespace LingoEngine.Director.LGodot.FileSystems
{
    public partial class GodotFilePicker : Node, IExecutableFilePicker
    {
        public void PickExecutable(Action<string> onPicked)
        {
#if USE_WINDOWS_FEATURES
        var dialog = new FileDialog
        {
            Access = FileDialog.AccessEnum.Filesystem,
            FileMode = FileDialog.FileModeEnum.OpenFile,
            Filters = new[] { "*.exe ; Executable Files" }
        };

        dialog.FileSelected += h => onPicked(h);
        GetTree().Root.AddChild(dialog);
        dialog.PopupCentered();
#else
            GD.PushWarning("Executable file picker not available. Define USE_WINDOWS_FEATURES in your Godot project to enable it.");
#endif
        }
    }
}
