using LingoEngine.Director.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.Core.Menus;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.Core.Projects;

namespace LingoEngine.Director.Core
{
    public static class DirectorSetup
    {
        public static ILingoEngineRegistration WithDirectorEngine(this ILingoEngineRegistration engineRegistration)
        {
            engineRegistration.Services(s => s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()
                    .AddSingleton<IDirectorShortCutManager, DirectorShortCutManager>()
                    .AddSingleton<IStageToolManager, StageToolManager>()
                    .AddSingleton<IHistoryManager, HistoryManager>()
                    .AddSingleton<DirectorWindowManager>()
                    .AddSingleton<DirectorProjectManager>()
                    .AddTransient<IDirectorWindowManager>(p => p.GetRequiredService<DirectorWindowManager>())

                    // File system
                    .AddSingleton<IIdePathResolver, IdePathResolver>()
                    .AddSingleton<IIdeLauncher, IdeLauncher>()
                    .AddSingleton<ProjectSettingsEditorState, ProjectSettingsEditorState>()

                    // Windows
                    .AddSingleton<DirectorMainMenu>()

                    .AddSingleton<DirectorProjectSettingsWindow>()
                    .AddSingleton<DirectorToolsWindow>()
                    .AddSingleton<DirectorCastWindow>()
                    .AddSingleton<DirectorScoreWindow>()
                    .AddSingleton<DirectorPropertyInspectorWindow>()
                    .AddSingleton<DirectorBinaryViewerWindow>()
                    .AddSingleton<DirectorStageWindow>()
                    .AddSingleton<DirectorTextEditWindow>()
                    .AddSingleton<DirectorPictureEditWindow>()
                    );
            engineRegistration.AddBuildAction(
                (serviceProvider) =>
                {
                    serviceProvider.RegisterDirectorWindows();
                   
                });
            return engineRegistration;
        }

    }
}
