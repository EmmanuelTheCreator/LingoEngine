using Godot;
using LingoEngine.Director.Core;
using LingoEngine.LGodot;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.LGodot.Gfx;

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
                s.AddSingleton<Theme>(p => p.GetRequiredService<DirectorStyle>().Theme);
                s.AddSingleton<DirGodotProjectSettingsWindow>();
            
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

                //      var inspector = new Inspector.DirGodotObjectInspector(p.GetRequiredService<IDirectorEventMediator>()) { Visible = false };
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
