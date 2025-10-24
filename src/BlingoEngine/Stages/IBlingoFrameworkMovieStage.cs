using AbstUI.Components;
using AbstUI.Primitives;
using BlingoEngine.Core;
using BlingoEngine.Movies;

namespace BlingoEngine.Stages;

/// <summary>
/// Represents the top-level window or stage. Implementations update the
/// display when the active <see cref="BlingoMovie"/> changes.
/// </summary>
public interface IBlingoFrameworkStage : IAbstFrameworkLayoutNode
{
    BlingoStage BlingoStage { get; }
    /// <summary>Sets the currently active movie.</summary>
    void SetActiveMovie(BlingoMovie? blingoMovie);
    void ApplyPropertyChanges();

    float Scale { get; set; }
    void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured);

    /// <summary>Captures a screenshot of the current stage.</summary>
    IAbstTexture2D GetScreenshot();

    /// <summary>Shows the transition overlay above the sprite layer.</summary>
    void ShowTransition(IAbstTexture2D startTexture);

    /// <summary>Updates the displayed transition frame.</summary>
    void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect);

    /// <summary>Hides the transition overlay and returns rendering to sprites.</summary>
    void HideTransition();
}

