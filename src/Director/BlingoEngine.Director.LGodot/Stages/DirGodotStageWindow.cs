using AbstEngine.Director.LGodot;
using AbstUI.FrameworkCommunication;
using AbstUI.LGodot.Components;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.LGodot.Stages;
using BlingoEngine.Stages;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlingoEngine.Director.LGodot.Movies;

/// <summary>
/// Minimal Godot framework window that hosts the cross-platform stage UI.
/// </summary>
internal partial class DirGodotStageWindowV2 : BaseGodotWindow, IDirFrameworkStageWindow, IFrameworkFor<DirectorStageWindow>
{
    public DirGodotStageWindowV2(IServiceProvider serviceProvider, IBlingoFrameworkStageContainer stageContainer)
        : base("Stage", serviceProvider)
    {
        var stage = serviceProvider.GetRequiredService<DirectorStageWindow>();
        Init(stage);
        var godotStageContainer = (BlingoGodotStageContainer)stageContainer;
        stage.StageLayer.Framework<AbstGodotPanel>().AddChild(godotStageContainer.Container);
        stage.ComposeStageLayers();
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
    public DirGodotStageWindow(IServiceProvider serviceProvider, IBlingoFrameworkStageContainer stageContainer)
        : base(serviceProvider, stageContainer)
    {
    }
}
