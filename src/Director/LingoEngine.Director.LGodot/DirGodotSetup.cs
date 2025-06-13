using Godot;
using LingoEngine.Director.LGodot.Scores;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.LGodot
{
    public static class DirGodotSetup
    {
        public static ILingoEngineRegistration WithDirectorGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode)
        {
            //engineRegistration.Services(s =>
            //{
            //    IServiceCollection serviceCollection = s.AddSingleton(p =>
            //    {
            //        var overlay = new DirGodotScoreWindow { Visible = false };
            //        rootNode.AddChild(overlay);
            //        return overlay;
            //    });
            //});
            return engineRegistration;
        }

    }
}
