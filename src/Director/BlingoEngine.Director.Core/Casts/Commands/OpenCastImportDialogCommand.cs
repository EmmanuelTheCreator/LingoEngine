using AbstUI.Commands;
using BlingoEngine.Casts;
using BlingoEngine.Director.Core.Casts;

namespace BlingoEngine.Director.Core.Casts.Commands;

public sealed record OpenCastImportDialogCommand(
    IBlingoCast Cast,
    int StartSlot) : IAbstCommand;
