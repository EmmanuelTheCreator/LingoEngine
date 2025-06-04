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
    }
}
