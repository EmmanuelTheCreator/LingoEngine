using LingoEngine.Movies;
using LingoEngine.Xtras.BuddyApi;
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
                // Xtras
                .AddScoped<IBuddyAPI, BuddyAPI>()
                ;
            //container.AddScoped<ILingoMovieEnvironment>();
            return container;
        }
    }
}
