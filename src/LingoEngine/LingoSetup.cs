using LingoEngine.Movies;
using LingoEngineGodot;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine
{
    public static class LingoSetup
    {
        public static IServiceCollection RegisterLingoEngine(this IServiceCollection container)
        {
            container
                .AddTransient<LingoSprite>()
                .AddTransient<ILingoMemberFactory,LingoMemberFactory>()
                .AddScoped<ILingoMovieEnvironment, LingoMovieEnvironment>()
                ;
            //container.AddScoped<ILingoMovieEnvironment>();
            return container;
        }
    }
}
