using LingoEngine.Movies;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine
{
    public static class LingoSetup
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container)
        {
            container.AddTransient<LingoSprite>();
            //container.AddScoped<ILingoMovieEnvironment>();
            return container;
        }
    }
}
