using LingoEngine.Director.Core.Menus;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Core.Windows
{
    public static class DirectorWindowRegistrator
    {
        internal static IServiceProvider RegisterDirectorWindows(this IServiceProvider serviceProvider)
        {
            var windowManager = serviceProvider.GetRequiredService<IDirectorWindowManager>();
            //windowManager.Register<DirectorMainMenu>("MainMenu", (sp) => new DirectorMainMenu(sp));
            //windowManager.Register<DirectorProjectSettingsWindow>("ProjectSettings", (sp) => new DirectorProjectSettingsWindow(sp));
            //windowManager.Register<DirectorShortCutWindow>("ShortCuts", (sp) => new DirectorShortCutWindow(sp));
            return serviceProvider;
        }

        
    }
}
