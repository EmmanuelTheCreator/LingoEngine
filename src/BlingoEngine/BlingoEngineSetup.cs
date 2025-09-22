using AbstUI.Commands;
using BlingoEngine.Casts;
using BlingoEngine.Casts.Commands;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Core;
using BlingoEngine.Events;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Members.Commands;
using BlingoEngine.Movies;
using BlingoEngine.Projects;
using BlingoEngine.Transitions;
using BlingoEngine.Transitions.TransitionLibrary;
using BlingoEngine.Sprites.BehaviorLibrary;
using BlingoEngine.Sprites.Commands;
using BlingoEngine.Xtras.BuddyApi;
using Microsoft.Extensions.DependencyInjection;
using AbstUI;

namespace BlingoEngine
{
    internal static class BlingoEngineSetup
    {
        public static IServiceCollection WithBlingoEngineBase(this IServiceCollection services)
        {

            services
                   .AddSingleton<BlingoPlayer>()
                   .AddSingleton<BlingoProjectSettings>()
                   .AddSingleton<BlingoCastLibsContainer>()
                   .AddSingleton<BlingoWindow>()
                   .AddSingleton<BlingoClock>()
                   .AddSingleton<BlingoSystem>()
                   .AddSingleton<BlingoFrameLabelManager>()
                   .AddSingleton<IBlingoColorPaletteDefinitions, BlingoColorPaletteDefinitions>()
                   .AddSingleton<IBlingoTransitionLibrary, BlingoTransitionLibrary>()
                   .AddSingleton<IBlingoBehaviorLibrary, BlingoBehaviorLibrary>()
                   .AddTransient<IBlingoTransitionPlayer, BlingoTransitionPlayer>()

                   .AddTransient<IBlingoPlayer>(p => p.GetRequiredService<BlingoPlayer>())
                   .AddTransient<IBlingoCastLibsContainer>(p => p.GetRequiredService<BlingoCastLibsContainer>())
                   .AddTransient<IBlingoWindow>(p => p.GetRequiredService<BlingoWindow>())
                   .AddTransient<IBlingoClock>(p => p.GetRequiredService<BlingoClock>())
                   .AddTransient<IBlingoSystem>(p => p.GetRequiredService<BlingoSystem>())
                   .AddTransient<IBlingoFrameLabelManager>(p => p.GetRequiredService<BlingoFrameLabelManager>())
                   .AddTransient<IBlingoMemberFactory, BlingoMemberFactory>()
                   .AddTransient(p => new Lazy<IBlingoMemberFactory>(() => p.GetRequiredService<IBlingoMemberFactory>()))
                   .AddTransient<BlingoSpriteCommandHandler>()
                   .AddTransient<BlingoMemberCommandHandler>()
                   .AddTransient<BlingoCastCommandHandler>()
                   .AddScoped<IBlingoMovieEnvironment, BlingoMovieEnvironment>()
                   .AddScoped<IBlingoEventMediator, BlingoEventMediator>()
                   // Xtras
                   .AddScoped<IBuddyAPI, BuddyAPI>()
                   ;

            return services;
        }
    }
}

