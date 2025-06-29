using Godot;
using LingoEngine.Director.Core;
using LingoEngine.LGodot;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.Core;
using LingoEngine.Director.LGodot.Pictures;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.LGodot.FileSystems;
using LingoEngine.Director.LGodot.Windowing;
using LingoEngine.Director.LGodot.Icons;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Importer;
using LingoEngine.Director.Core.Inspector;
using LingoEngine.Director.Core.Projects;
using LingoEngine.Director.Core.Scores;
using LingoEngine.Director.Core.Stages;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.LGodot.UI;
using LingoEngine.Director.Core.Icons;
using Microsoft.Extensions.Logging;
using LingoEngine.Styles;

namespace LingoEngine.Director.LGodot
{
    public static class DirGodotSetup
    {
        public static ILingoEngineRegistration WithDirectorGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode)
        {
            engineRegistration
                .WithLingoGodotEngine(rootNode,true)
                .WithDirectorEngine()
                ;
            engineRegistration.Services(s =>
            {
                s.AddSingleton<DirectorGodotStyle>();
                s.AddSingleton<DirGodotProjectSettingsWindow>();
                s.AddSingleton<DirGodotToolsWindow>();
                s.AddSingleton<DirGodotCastWindow>();
                s.AddSingleton<DirGodotScoreWindow>();
                s.AddSingleton<DirGodotStageWindow>();
                s.AddSingleton<DirGodotBinaryViewerWindow>();
                s.AddSingleton<DirGodotImportExportWindow>();
                s.AddSingleton<DirGodotPropertyInspector>();
                s.AddSingleton<DirGodotTextableMemberWindow>();
                s.AddSingleton<DirGodotPictureMemberEditorWindow>();
                s.AddSingleton<DirGodotMainMenu>();
                s.AddSingleton<DirGodotWindowManager>();
                s.AddSingleton<DirGodotWindowManager>();
                s.AddSingleton<IExecutableFilePicker, GodotFilePicker>();
                s.AddSingleton<IDirectorIconManager>(p =>
                {
                    var iconManager = new DirGodotIconManager(p.GetRequiredService<ILogger<DirGodotIconManager>>());
                    iconManager.LoadSheet("Media/Icons/General_Icons.png", 20,16, 16,8);
                    iconManager.LoadSheet("Media/Icons/Painter_Icons.png", 20,16, 16,8);
                    return iconManager;
                });

                s.AddTransient<IDirFrameworkProjectSettingsWindow>(p => p.GetRequiredService<DirGodotProjectSettingsWindow>());
                s.AddTransient<IDirFrameworkToolsWindow>(p => p.GetRequiredService<DirGodotToolsWindow>());
                s.AddTransient<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirGodotCastWindow>());
                s.AddTransient<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirGodotScoreWindow>());
                s.AddTransient<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirGodotStageWindow>());
                s.AddTransient<IDirFrameworkBinaryViewerWindow>(p => p.GetRequiredService<DirGodotBinaryViewerWindow>());
                s.AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirGodotPropertyInspector>());
                s.AddTransient<IDirFrameworkBitmapEditWindow>(p => p.GetRequiredService<DirGodotPictureMemberEditorWindow>());
                s.AddTransient<IDirFrameworkImportExportWindow>(p => p.GetRequiredService<DirGodotImportExportWindow>());
                s.AddTransient<IDirGodotWindowManager>(p => p.GetRequiredService<DirGodotWindowManager>());


            });
            engineRegistration.AddBuildAction(p =>
            {
                p.GetRequiredService<ILingoFontManager>().SetDefaultFont(DirectorGodotStyle.DefaultFont);
               new LingoGodotDirectorRoot(p.GetRequiredService<LingoPlayer>(), p);
            });
            return engineRegistration;
        }

    }
}
