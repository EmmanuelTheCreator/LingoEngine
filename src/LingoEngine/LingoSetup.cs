using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine
{
    public static class LingoSetup
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container)
        {
            container.AddTransient<LingoSprite>();
            container.AddSingleton<ILingoEnvironment,LingoProject>();
            return container;
        }
    }
}
