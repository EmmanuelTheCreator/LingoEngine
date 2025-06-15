using Godot;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.LGodot.Movies;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.LGodot
{
    public static class DirGodotSetup
    {
        public static ILingoEngineRegistration WithDirectorGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode)
        {
            engineRegistration.Services(s =>
            {
                s.AddSingleton<ILingoFrameworkStageWindow>(p => new DirGodotStageWindow(rootNode) { Visible = false });
            });
            return engineRegistration;
        }

    }
}
