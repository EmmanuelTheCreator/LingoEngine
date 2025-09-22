using AbstUI;
using BlingoEngine.Director.Core.Bitmaps;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Behaviors;
using BlingoEngine.Director.Core.Importer;
using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Texts;
using BlingoEngine.Director.Core.Tools.Commands;
using BlingoEngine.Director.Core.UI;

namespace BlingoEngine.Director.Core.Windowing
{
    public static class DirectorWindowRegistrator
    {
        internal static IAbstFameworkWindowRegistrator RegisterDirectorWindows(this IAbstFameworkWindowRegistrator registrator)
        {
            registrator
                .AddSingletonWindow<DirectorProjectSettingsWindow>(DirectorMenuCodes.ProjectSettingsWindow)
                .AddSingletonWindow<DirectorToolsWindow>(DirectorMenuCodes.ToolsWindow, s => s.CreateShortCut(DirectorMenuCodes.ToolsWindow, "Ctrl+7", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorCastWindow>(DirectorMenuCodes.CastWindow, s => s.CreateShortCut(DirectorMenuCodes.CastWindow, "Ctrl+3", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorScoreWindow>(DirectorMenuCodes.ScoreWindow, s => s.CreateShortCut(DirectorMenuCodes.ScoreWindow, "Ctrl+4", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorPropertyInspectorWindow>(DirectorMenuCodes.PropertyInspector, s => s.CreateShortCut(DirectorMenuCodes.PropertyInspector, "Ctrl+Alt+S", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirBehaviorInspectorWindow>(DirectorMenuCodes.BehaviorInspectorWindow)
                .AddSingletonWindow<DirectorBinaryViewerWindow>(DirectorMenuCodes.BinaryViewerWindow)
                .AddSingletonWindow<DirectorBinaryViewerWindowV2>(DirectorMenuCodes.BinaryViewerWindowV2)
                .AddSingletonWindow<DirectorStageWindow>(DirectorMenuCodes.StageWindow, s => s.CreateShortCut(DirectorMenuCodes.StageWindow, "Ctrl+1", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorTextEditWindowV2>(DirectorMenuCodes.TextEditWindow, s => s.CreateShortCut(DirectorMenuCodes.TextEditWindow, "Ctrl+T", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorBitmapEditWindow>(DirectorMenuCodes.PictureEditWindow, s => s.CreateShortCut(DirectorMenuCodes.PictureEditWindow, "Ctrl+5", sc => new ExecuteShortCutCommand(sc)))
                .AddSingletonWindow<DirectorImportExportWindow>(DirectorMenuCodes.ImportExportWindow)
                //.AddSingletonWindow<DirectorImportExportWindow>(DirectorMenuCodes.MainMenu)
                // .Register<DirectorMainMenu>(DirectorMenuCodes.MainMenu)
                ;

            return registrator;
        }
        internal static IAbstFameworkComponentRegistrator RegisterDirectorComponents(this IAbstFameworkComponentRegistrator registrator)
        {
            return registrator;
        }

    }
}

