using LingoEngine.Commands;
using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Director.Core.Commands;

/// <summary>
/// Adjusts the current stage scale via the framework stage implementation.
/// </summary>
public sealed class SetStageScaleCommandHandler : ICommandHandler<SetStageScaleCommand>
{
    private ILingoFrameworkStage? _stage;

    public void SetStage(ILingoFrameworkStage stage) => _stage = stage;

    public bool CanExecute(SetStageScaleCommand command) => _stage != null;

    public void Handle(SetStageScaleCommand command)
    {
        if (_stage == null) return;

        var prop = _stage.GetType().GetProperty("Scale");
        if (prop == null) return;
        var vectorType = prop.PropertyType;
        var ctor = vectorType.GetConstructor(new[] { typeof(float), typeof(float) });
        if (ctor == null) return;
        var vector = ctor.Invoke(new object[] { command.Scale, command.Scale });
        prop.SetValue(_stage, vector);
    }
}

