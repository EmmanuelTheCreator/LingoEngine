using AbstUI.Components.Menus;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.Windowing;

namespace BlingoEngine.Director.Core.UI
{
    public interface IDirFrameworkMainMenuWindow : IAbstFrameworkWindow
    {
        void RegisterTopMenu(AbstMenu menu);
    }
}
