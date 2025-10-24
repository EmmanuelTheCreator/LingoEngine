using AbstUI.Windowing;

namespace BlingoEngine.Director.Core.Scores
{
    public interface IDirFrameworkScoreWindow : IAbstFrameworkWindow
    {
        void ScrollTo(float horizontal, float vertical);
    }
}

