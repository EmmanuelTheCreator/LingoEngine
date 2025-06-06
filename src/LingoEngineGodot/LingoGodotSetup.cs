using Godot;
using LingoEngine;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngineGodot
{
    public static class LingoGodotSetup
    {
        public static ILingoEngineRegistration WithLingoGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode, Action<GodotFactory>? setup = null)
        {
            engineRegistration
                .Services(s => s
                        .AddSingleton<ILingoFrameworkFactory,GodotFactory>()
                        .AddSingleton(p => new LingoGodotRootNode(rootNode))
                        )
                .WithFrameworkFactory(setup)
                ;
            return engineRegistration;
        }
       
    }
}
