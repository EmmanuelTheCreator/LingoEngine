using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Movies;
using LingoEngine.Commands;
using LingoEngine.Xtras.BuddyApi;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine
{
    internal static class LingoEngineSetup
    {
        public static IServiceCollection WithGodotEngine(this IServiceCollection services)
        {

            services
                   .AddSingleton<LingoPlayer>()
                   .AddSingleton<ProjectSettings>()
                   .AddTransient<ILingoPlayer>(p => p.GetRequiredService<ILingoPlayer>())
                   .AddTransient<LingoSprite>()
                   .AddTransient<ILingoMemberFactory, LingoMemberFactory>()
                   .AddTransient(p => new Lazy<ILingoMemberFactory>(() => p.GetRequiredService<ILingoMemberFactory>()))
                   .AddScoped<ILingoMovieEnvironment, LingoMovieEnvironment>()
                   .AddScoped<ILingoEventMediator, LingoEventMediator>()
                   // Xtras
                   .AddScoped<IBuddyAPI, BuddyAPI>()
                   .AddSingleton<ICommandManager, CommandManager>()
                   ;

            return services;
        }
    }
}
