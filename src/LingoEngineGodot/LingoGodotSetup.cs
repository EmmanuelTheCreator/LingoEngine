using ArkGodot.DirectorProxy;
using LingoEngineGodot;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine
{
    public static class LingoGodotSetup
    {
        public static IServiceCollection RegisterLingoGodotEngine(this IServiceCollection container)
        {
            container.AddSingleton<ILingoFrameworkFactory, GodotFactory>();
            return container;
        }
    }
}
