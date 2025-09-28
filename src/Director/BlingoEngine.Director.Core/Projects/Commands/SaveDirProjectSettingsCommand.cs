using AbstUI.Commands;
using BlingoEngine.Projects;

namespace BlingoEngine.Director.Core.Projects.Commands;

public sealed record SaveDirProjectSettingsCommand(
    DirectorProjectSettings? DirSettings = null,
    BlingoProjectSettings? ProjectSettings = null,
    bool StartNewProjectAfterSave = false) : IAbstCommand;

