using Godot;
using LingoEngine.Director.Core.Events;
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

                IServiceCollection serviceCollection = s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()
                    .AddSingleton<ILingoFrameworkStageWindow>(p => new DirGodotStageWindow(rootNode))
                    .AddSingleton(p =>
                    {
                        var overlay = new DirGodotScoreWindow(p.GetRequiredService<IDirectorEventMediator>()) { Visible = false };
                        rootNode.AddChild(overlay);
                        return overlay;
                    });

            });
            return engineRegistration;
        }

    }
}
