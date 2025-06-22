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

            windowManager
                .Register<DirectorProjectSettingsWindow>(DirectorMenuCodes.ProjectSettingsWindow)
                .Register<DirectorToolsWindow>(DirectorMenuCodes.ToolsWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.ToolsWindow, "Ctrl+7", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorCastWindow>(DirectorMenuCodes.CastWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.CastWindow, "Ctrl+3", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorScoreWindow>(DirectorMenuCodes.ScoreWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.ScoreWindow, "Ctrl+4", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorPropertyInspectorWindow>(DirectorMenuCodes.PropertyInspector, shortCutManager.CreateShortCut(DirectorMenuCodes.PropertyInspector, "Ctrl+Alt+S", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorBinaryViewerWindow>(DirectorMenuCodes.BinaryViewerWindow)
                .Register<DirectorStageWindow>(DirectorMenuCodes.StageWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.StageWindow, "Ctrl+1", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorTextEditWindow>(DirectorMenuCodes.TextEditWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.TextEditWindow, "Ctrl+T", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorPictureEditWindow>(DirectorMenuCodes.PictureEditWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.PictureEditWindow, "Ctrl+5", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorImportExportWindow>(DirectorMenuCodes.ImportExportWindow)
                ;

            return serviceProvider;
        }

        
    }
}
