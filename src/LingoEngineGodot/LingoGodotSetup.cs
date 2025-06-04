using Godot;
using LingoEngine.FrameworkCommunication;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngineGodot
{
    public static class LingoGodotSetup
    {
        public static IServiceCollection RegisterLingoGodotEngine(this IServiceCollection container)
        {
            container
                .AddSingleton<ILingoFrameworkFactory, GodotFactory>()

                ;
            return container;
        }
        public static IServiceProvider SetupLingoGodotEngine(this IServiceProvider provider, Node2D rootNode)
        {
            ((GodotFactory)provider.GetRequiredService<ILingoFrameworkFactory>()).SetRootNode(rootNode);
            return provider;
        }
    }
}
