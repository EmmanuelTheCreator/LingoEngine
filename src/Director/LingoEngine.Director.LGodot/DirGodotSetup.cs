using Godot;
using LingoEngine.Director.LGodot.Scores;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.LGodot
{
    public static class DirGodotSetup
    {
        public static ILingoEngineRegistration WithDirectorGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode)
        {
            //engineRegistration
            //    .Services(s => s
            //            .AddSingleton<ILingoFrameworkFactory, GodotFactory>()
            //            .AddSingleton<ILingoFontManager, LingoGodotFontManager>()
            //            .AddSingleton(p => new LingoGodotRootNode(rootNode))
            //            )
            //    .WithFrameworkFactory(setup)
            //    ;

            engineRegistration.Services(s =>
            {
                s.AddSingleton<DirGodotScoreWindow>(p =>
                {
                    var overlay = new DirGodotScoreWindow { Visible = false };
                    rootNode.AddChild(overlay);
                    return overlay;
                });
            });
            return engineRegistration;
        }

    }
}
