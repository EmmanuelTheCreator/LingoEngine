using Godot;
using LingoEngine.Director.Core;
using LingoEngine.LGodot;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.LGodot.Inspector;
using LingoEngine.Director.Core.Windows;
using LingoEngine.Director.LGodot.Movies;
using System.IO;
using System;
using LingoEngine.Core;
using LingoEngine.Director.LGodot.Pictures;
using LingoEngine.Director.Core.FileSystems;
using LingoEngine.Director.LGodot.FileSystems;

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
                s.AddSingleton<DirectorStyle>();
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
                s.AddSingleton<IDirGodotIconManager>(p =>
                {
                    var iconManager = new DirGodotIconManager();
                    iconManager.LoadSheet("res://Media/Icons/General_Icons.png", 20,16, 16,8);
                    iconManager.LoadSheet("res://Media/Icons/Painter_Icons.png", 20,16, 16,8);
                    return iconManager;
                });

                s.AddTransient<IDirFrameworkProjectSettingsWindow>(p => p.GetRequiredService<DirGodotProjectSettingsWindow>());
                s.AddTransient<IDirFrameworkToolsWindow>(p => p.GetRequiredService<DirGodotToolsWindow>());
                s.AddTransient<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirGodotCastWindow>());
                s.AddTransient<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirGodotScoreWindow>());
                s.AddTransient<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirGodotStageWindow>());
                s.AddTransient<IDirFrameworkBinaryViewerWindow>(p => p.GetRequiredService<DirGodotBinaryViewerWindow>());
                s.AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirGodotPropertyInspector>());
                s.AddTransient<IDirFrameworkPictureEditWindow>(p => p.GetRequiredService<DirGodotPictureMemberEditorWindow>());
                s.AddTransient<IDirFrameworkImportExportWindow>(p => p.GetRequiredService<DirGodotImportExportWindow>());
                s.AddTransient<IDirGodotWindowManager>(p => p.GetRequiredService<DirGodotWindowManager>());


            });
            engineRegistration.AddBuildAction(p =>
            {
               new LingoGodotDirectorRoot(p.GetRequiredService<LingoPlayer>(), p);
            });
            return engineRegistration;
        }

    }
}
