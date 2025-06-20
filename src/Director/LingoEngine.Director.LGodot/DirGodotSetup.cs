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
                s.AddSingleton<TextableMemberWindow>();

                s.AddSingleton<IDirFrameworkProjectSettingsWindow>(p => p.GetRequiredService<DirGodotProjectSettingsWindow>());
                s.AddSingleton<IDirFrameworkToolsWindow>(p => p.GetRequiredService<DirGodotToolsWindow>());
                s.AddSingleton<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirGodotCastWindow>());
                s.AddSingleton<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirGodotScoreWindow>());
                s.AddSingleton<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirGodotStageWindow>());
                s.AddSingleton<IDirFrameworkBinaryViewerWindow>(p => p.GetRequiredService<DirGodotBinaryViewerWindow>());
                s.AddSingleton<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirGodotPropertyInspector>());
                s.AddSingleton<IDirFrameworkTextEditWindow>(p => p.GetRequiredService<TextableMemberWindow>());
            
                //IServiceCollection serviceCollection = s
                  //  .AddSingleton<ILingoFrameworkStageWindow>(p => new DirGodotStageWindow(rootNode))
                  //  .AddSingleton(p =>
                  //  {
                  //      var overlay = new DirGodotScoreWindow(p.GetRequiredService<IDirectorEventMediator>()) { Visible = false };
                  //      rootNode.AddChild(overlay);
                  //      return overlay;
                  //  })
                  //  .AddSingleton(p =>
                  //  {

                //      var inspector = new Inspector.DirGodotPropertyInspector(p.GetRequiredService<IDirectorEventMediator>()) { Visible = false };
                //      rootNode.AddChild(inspector);
                //      return inspector;
                //  })
                //.AddSingleton(p =>
                //  {
                //      var menu = new DirGodotMainMenu();
                //      rootNode.AddChild(menu);
                //      return menu;

                //  });

            });
            return engineRegistration;
        }

    }
}
