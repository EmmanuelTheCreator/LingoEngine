using System;
using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.Stages;

namespace BlingoEngine.Director.LGodot.Movies;

/// <summary>
/// Minimal Godot framework window that hosts the cross-platform stage UI.
/// </summary>
internal partial class DirGodotStageWindowV2 : BaseGodotWindow, IDirFrameworkStageWindow, IFrameworkFor<DirectorStageWindow>
{
    public DirGodotStageWindowV2(IServiceProvider serviceProvider)
        : base("Stage", serviceProvider)
    {
    }

    public void UpdateBoundingBoxes()
    {
    }

    public void UpdateSelectionBox()
    {
    }
}

/// <summary>
/// Retains the legacy class name for dependency injection registrations.
/// </summary>
internal partial class DirGodotStageWindow : DirGodotStageWindowV2
{
    public DirGodotStageWindow(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
