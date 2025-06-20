using LingoEngine.Director.Core.Menus;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Commands;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Casts;

namespace LingoEngine.Director.Core.Windows
{
    public static class DirectorWindowRegistrator
    {
        internal static IServiceProvider RegisterDirectorWindows(this IServiceProvider serviceProvider)
        {
            var windowManager = serviceProvider.GetRequiredService<IDirectorWindowManager>();
            var shortCutManager = serviceProvider.GetRequiredService<IDirectorShortCutManager>();

            windowManager.Register<DirectorProjectSettingsWindow>(DirectorMenuCodes.ProjectSettingsWindow,
                sp => new DirectorProjectSettingsWindow(sp.GetRequiredService<IDirFrameworkProjectSettingsWindow>()));

            windowManager.Register<DirectorToolsWindow>(DirectorMenuCodes.ToolsWindow,
                sp => new DirectorToolsWindow(sp.GetRequiredService<IDirFrameworkToolsWindow>()),
                shortCutManager.CreateShortCut(DirectorMenuCodes.ToolsWindow, "Ctrl+7", sc => new ExecuteShortCutCommand(sc)));

            windowManager.Register<DirectorCastWindow>(DirectorMenuCodes.CastWindow,
                sp => new DirectorCastWindow(sp.GetRequiredService<IDirFrameworkCastWindow>()),
                shortCutManager.CreateShortCut(DirectorMenuCodes.CastWindow, "Ctrl+3", sc => new ExecuteShortCutCommand(sc)));

            windowManager.Register<DirectorScoreWindow>(DirectorMenuCodes.ScoreWindow,
                sp => new DirectorScoreWindow(sp.GetRequiredService<IDirFrameworkScoreWindow>()),
                shortCutManager.CreateShortCut(DirectorMenuCodes.ScoreWindow, "Ctrl+4", sc => new ExecuteShortCutCommand(sc)));

            windowManager.Register<DirectorPropertyInspectorWindow>(DirectorMenuCodes.PropertyInspector,
                sp => new DirectorPropertyInspectorWindow(sp.GetRequiredService<IDirFrameworkPropertyInspectorWindow>()),
                shortCutManager.CreateShortCut(DirectorMenuCodes.PropertyInspector, "Ctrl+Alt+S", sc => new ExecuteShortCutCommand(sc)));

            windowManager.Register<DirectorBinaryViewerWindow>(DirectorMenuCodes.BinaryViewerWindow,
                sp => new DirectorBinaryViewerWindow(sp.GetRequiredService<IDirFrameworkBinaryViewerWindow>()));

            windowManager.Register<DirectorStageWindow>(DirectorMenuCodes.StageWindow,
                sp => new DirectorStageWindow(sp.GetRequiredService<IDirFrameworkStageWindow>()),
                shortCutManager.CreateShortCut(DirectorMenuCodes.StageWindow, "Ctrl+1", sc => new ExecuteShortCutCommand(sc)));

            windowManager.Register<DirectorTextEditWindow>(DirectorMenuCodes.TextEditWindow,
                sp => new DirectorTextEditWindow(sp.GetRequiredService<IDirFrameworkTextEditWindow>()),
                shortCutManager.CreateShortCut(DirectorMenuCodes.TextEditWindow, "Ctrl+T", sc => new ExecuteShortCutCommand(sc)));

            return serviceProvider;
        }

        
    }
}
