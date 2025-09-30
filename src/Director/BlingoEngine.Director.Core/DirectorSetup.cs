using AbstUI.Commands;
using AbstUI.Components;
using AbstUI.Windowing;
using BlingoEngine.Casts.Commands;
using BlingoEngine.Director.Core.Bitmaps;
using BlingoEngine.Director.Core.Bitmaps.Commands;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Casts.Commands;
using BlingoEngine.Director.Core.Movies.Commands;
using BlingoEngine.Director.Core.Compilers;
using BlingoEngine.Director.Core.Compilers.Commands;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.Director.Core.Importer;
using BlingoEngine.Director.Core.Importer.Commands;
using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.Inspector.Commands;
using BlingoEngine.Director.Core.Members.Commands;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Projects.Commands;
using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Director.Core.Scripts;
using BlingoEngine.Director.Core.Scripts.Commands;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Director.Core.Sprites.Commands;
using BlingoEngine.Director.Core.Sprites.Behaviors;
using BlingoEngine.Director.Core.Behaviors;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.Core.Stages.Commands;
using BlingoEngine.Director.Core.Texts;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.Director.Core.Remote;
using BlingoEngine.Director.Core.Remote.Commands;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using BlingoEngine.Net.RNetProjectClient;
using BlingoEngine.Net.RNetPipeClient;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using BlingoEngine.Sprites.BehaviorLibrary;
using BlingoEngine.Members.Commands;
using BlingoEngine.Sprites.Commands;

namespace BlingoEngine.Director.Core
{
    public static class DirectorSetup
    {
        public static IBlingoEngineRegistration WithDirectorEngine(this IBlingoEngineRegistration engineRegistration, Action<DirectorProjectSettings>? directorSettingsConfig = null)
        {
            engineRegistration
                .RegisterWindows(r => DirectorWindowRegistrator.RegisterDirectorWindows(r))
                .RegisterComponents(r => DirectorWindowRegistrator.RegisterDirectorComponents(r))
                .ServicesMain(s => s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()


                    .AddSingleton<DirectorProjectManager>()
                    .AddSingleton<BlingoScriptCompiler>()
                    .AddSingleton<DirectorProjectSettings>()
                    .AddSingleton<DirectorProjectSettingsRepository>()
                    .AddSingleton<BlingoProjectSettingsRepository>()
                    .AddTransient<IDirectorBehaviorDescriptionManager, DirectorBehaviorDescriptionManager>()

                    // File system
                    .AddSingleton<IIdePathResolver, IdePathResolver>()
                    .AddSingleton<IIdeLauncher, IdeLauncher>()
                    .AddSingleton<ProjectSettingsEditorState, ProjectSettingsEditorState>()

                    .AddSingleton<DirectorScriptsManager>()

                    .AddTransient(p => new Lazy<IDirectorScriptsManager>(() => p.GetRequiredService<DirectorScriptsManager>()))

                    .AddTransient<IDirCastImportService, DirCastImportService>()
                    .AddTransient<DirCastImportDialog>()
                    .AddTransient<DirCastImportDialogHandler>()

                    // Windows
                    .AddSingleton<DirectorMainMenu>()

                    .AddSingleton<DirectorProjectSettingsWindow>()
                    .AddSingleton<DirectorToolsWindow>()
                    .AddSingleton<DirectorCastWindow>()
                    .AddSingleton<DirectorScoreWindow>()
                    .AddSingleton<DirectorPropertyInspectorWindow>()
                    .AddSingleton<DirBehaviorInspectorWindow>()
                    .AddSingleton<DirectorStageGuides>()
                    .AddSingleton<DirectorBinaryViewerWindow>()
                    .AddSingleton<DirectorBinaryViewerWindowV2>()
                    .AddSingleton<DirectorStageWindow>()
                    .AddSingleton<DirectorTextEditWindowV2>()
                    .AddSingleton<DirectorBitmapEditWindow>()
                    .AddSingleton<DirectorImportExportWindow>()
                    .AddSingleton<RNetConfiguration>()
                    .AddSingleton<IRNetConfiguration>(p => p.GetRequiredService<RNetConfiguration>())
                    .AddTransient<RNetSettingsDialog>()
                    .AddSingleton<RNetSettingsDialogHandler>()
                    .AddDirectorDummyProjectServer()
                    .AddSingleton<IBlingoRNetProjectClient, BlingoRNetProjectClient>()
                    .AddSingleton<IBlingoRNetPipeClient, RNetPipeClient>()
                    .AddSingleton<DirectorRNetServer>()
                    .AddSingleton<DirectorRNetClient>()
                    .AddSingleton<DirStageManager>()
                    .AddTransient<IDirStageManager>(p => p.GetRequiredService<DirStageManager>())
                    .AddSingleton<DirScoreManager>()
                    .AddSingleton<DirSpritesManager>()
                    .AddSingleton<DirCastManager>()
                    .AddTransient<IDirSpritesManager>(p => p.GetRequiredService<DirSpritesManager>())
                    .AddTransient(p => new Lazy<IDirSpritesManager>(() => p.GetRequiredService<DirSpritesManager>()))
                    // Handlers
                    .AddTransient<CompileProjectCommandHandler>()
                    .AddTransient<BlingoCSharpConverterPopup>()
                    .AddTransient<BlingoCSharpConverterPopupHandler>()
                    .AddTransient<BlingoCodeImporterPopup>()
                    .AddTransient<BlingoCodeImporterPopupHandler>()
                    .AddTransient<DirectorSpriteCommandHandler>()
                    .AddTransient<DirectorMemberCommandHandler>()
                    .AddTransient<BlingoMovieCommandHandler>()
                    .AddTransient<DirectorCastCommandHandler>()

                );
            engineRegistration.AddBuildAction(
                (serviceProvider) =>
                {
                    //serviceProvider.RegisterDirectorWindows();
                    //serviceProvider.GetRequiredService<IAbstComponentFactory>();
                    //serviceProvider.GetRequiredService< BlingoCSharpConverterPopupHandler>();
                    var behaviorLib = serviceProvider.GetRequiredService<IBlingoBehaviorLibrary>();
                    behaviorLib.Register(new BlingoBehaviorDefinition("GetServerIP", typeof(Sprites.Behaviors.GetServerIPBehavior), "Network"));

                    if (directorSettingsConfig != null)
                    {
                        //var settings = new DirectorProjectSettings();
                        //directorSettingsConfig(settings)
                        //serviceProvider.GetRequiredService<IAbstWindowManager>()


                    }
                    var cmdManager = serviceProvider.GetRequiredService<IAbstCommandManager>();
                    cmdManager
                        .Register<DirStageManager, StageChangeBackgroundColorCommand>()
                        .Register<DirectorStageWindow, StageToolSelectCommand>()
                        .Register<DirectorStageWindow, MoveSpritesCommand>()
                        .Register<DirectorStageWindow, RotateSpritesCommand>()
                        .Register<DirCastManager, CreateFilmLoopCommand>()
                        .Register<DirectorScoreWindow, FindCastMemberInScoreCommand>()
                        .Register<DirectorScriptsManager, OpenScriptCommand>()
                        .Register<BlingoCodeImporterPopupHandler, OpenBlingoCodeImporterCommand>()
                        .Register<BlingoCSharpConverterPopupHandler, OpenBlingoCSharpConverterCommand>()
                        .Register<DirCastImportDialogHandler, OpenCastImportDialogCommand>()
                        .Register<DirectorProjectManager, SaveDirProjectSettingsCommand>()
                        .Register<DirectorPropertyInspectorWindow, OpenBehaviorPopupCommand>()
                        .Register<CompileProjectCommandHandler, CompileProjectCommand>()
                        .Register<DirSpritesManager, ChangeSpriteRangeCommand>()
                        .Register<DirSpritesManager, AddSpriteCommand>()
                        .Register<DirSpritesManager, RemoveSpriteCommand>()
                        .Register<DirectorSpriteCommandHandler, BlingoUpdateSpritePropertiesCommand>(replace: true)
                        .Register<DirectorMemberCommandHandler, BlingoUpdateMemberPropertiesCommand>(replace: true)
                        .Register<BlingoMovieCommandHandler, BlingoUpdateMoviePropertiesCommand>()
                        .Register<DirectorCastCommandHandler, BlingoUpdateCastPropertiesCommand>(replace: true)
                        .Register<DirectorBitmapEditWindow, PainterToolSelectCommand>()
                        .Register<DirectorBitmapEditWindow, PainterDrawPixelCommand>()
                        .Register<DirectorBitmapEditWindow, PainterFillCommand>()
                        .Register<DirectorRNetServer, ConnectRNetServerCommand>()
                        .Register<DirectorRNetServer, DisconnectRNetServerCommand>()
                        .Register<DirectorRNetClient, ConnectRNetClientCommand>()
                        .Register<DirectorRNetClient, DisconnectRNetClientCommand>()
                        .Register<RNetSettingsDialogHandler, OpenRNetSettingsCommand>();

                });
            return engineRegistration;
        }

    }
}

