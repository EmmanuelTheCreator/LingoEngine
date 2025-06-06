using Godot;
using LingoEngine;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngineGodot
{
    public static class LingoGodotSetup
    {
        public static ILingoEngineRegistration WithLingoGodotEngine(this ILingoEngineRegistration engineRegistration, Action<GodotFactory>? setup = null)
        {
            engineRegistration.WithFrameworkFactory(setup);
            return engineRegistration;
        }
       
        public static IServiceProvider SetupLingoGodotEngine(this IServiceProvider provider, Node2D rootNode)
        {
            ((GodotFactory)provider.GetRequiredService<ILingoFrameworkFactory>()).SetRootNode(rootNode);
            return provider;
        }
    }
}
