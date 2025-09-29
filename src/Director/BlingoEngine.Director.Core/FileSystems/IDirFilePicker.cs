using System.Collections.Generic;

namespace BlingoEngine.Director.Core.FileSystems;

public interface IDirFilePicker
{
    void PickFile(Action<string> onPicked, string filter, string? currentFile = null);

    void PickFiles(Action<IReadOnlyList<string>> onPicked, string filter, string? currentPath = null);
}
