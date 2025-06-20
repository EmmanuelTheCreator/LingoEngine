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
                s.AddSingleton<DirGodotPropertyInspector>();
                s.AddSingleton<DirGodotTextableMemberWindow>();
                s.AddSingleton<DirGodotPictureMemberEditorWindow>();
                s.AddSingleton<DirGodotMainMenu>();
                s.AddSingleton<IDirGodotWindowManager, DirGodotWindowManager>();

                s.AddSingleton<IDirFrameworkProjectSettingsWindow>(p => p.GetRequiredService<DirGodotProjectSettingsWindow>());
                s.AddSingleton<IDirFrameworkToolsWindow>(p => p.GetRequiredService<DirGodotToolsWindow>());
                s.AddSingleton<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirGodotCastWindow>());
                s.AddSingleton<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirGodotScoreWindow>());
                s.AddSingleton<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirGodotStageWindow>());
                s.AddSingleton<IDirFrameworkBinaryViewerWindow>(p => p.GetRequiredService<DirGodotBinaryViewerWindow>());
                s.AddSingleton<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirGodotPropertyInspector>());
                s.AddSingleton<IDirFrameworkPictureEditWindow>(p => p.GetRequiredService<DirGodotPictureMemberEditorWindow>());


            });
            engineRegistration.AddBuildAction(p =>
            {
               new LingoGodotDirectorRoot(p.GetRequiredService<LingoPlayer>(), p);
            });
            return engineRegistration;
        }

    }
}
