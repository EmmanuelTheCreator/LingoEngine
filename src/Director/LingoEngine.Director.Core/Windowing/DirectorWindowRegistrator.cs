using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Texts;
using LingoEngine.Director.Core.Tools;
using LingoEngine.Director.Core.Tools.Commands;
using LingoEngine.Director.Core.UI;
using LingoEngine.Director.Core.Bitmaps;

namespace LingoEngine.Director.Core.Windowing
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
                .Register<DirectorBitmapEditWindow>(DirectorMenuCodes.PictureEditWindow, shortCutManager.CreateShortCut(DirectorMenuCodes.PictureEditWindow, "Ctrl+5", sc => new ExecuteShortCutCommand(sc)))
                .Register<DirectorImportExportWindow>(DirectorMenuCodes.ImportExportWindow)
                ;

            return serviceProvider;
        }


    }
}
