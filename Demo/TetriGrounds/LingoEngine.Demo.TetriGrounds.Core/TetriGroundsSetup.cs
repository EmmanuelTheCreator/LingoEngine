using Microsoft.Extensions.DependencyInjection;
using LingoEngine;
using LingoEngine.Movies;
using LingoEngine.Demo.TetriGrounds.Core.MovieScripts;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Globals;
using LingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using LingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;

namespace LingoEngine.Demo.TetriGrounds.Core
{
    public static class TetriGroundsSetup
    {
        public static IServiceCollection AddTetriGrounds(this IServiceCollection services, Action<ILingoEngineRegistration> registration)
        {
            services
                .RegisterLingoEngine(config =>
                {
                    config
                        //.AddFont("Arcade", Path.Combine("Media", "Fonts", "arcade.ttf"))
                        //.AddFont("Bikly", Path.Combine("Media", "Fonts", "bikly.ttf"))
                        //.AddFont("8Pin Matrix", Path.Combine("Media", "Fonts", "8PinMatrix.ttf"))
                        //.AddFont("Earth", Path.Combine("Media", "Fonts", "earth.ttf"))
                        //.AddFont("Tahoma", Path.Combine("Media", "Fonts", "Tahoma.ttf"))
                        .WithProjectSettings(s =>
                        {
                            s.ProjectFolder = "TetriGrounds";
                            s.ProjectName = "TetriGrounds";
                            s.MaxSpriteChannelCount = 300;
                        })
                        .ForMovie(TetriGroundsGame.MovieName, s => s
                            //.AddMovieScript<StartMoviesScript>()
                            //.AddMovieScript<StarMovieScript>()
                            .AddScriptsFromAssembly()
                            // Other
                        );
                    registration(config);
                }
                )
                .AddSingleton<IArkCore, TetriGroundsCore>()
                .AddSingleton<TetriGroundsGame, TetriGroundsGame>()
                .AddSingleton<GlobalVars>()
                ;
            return services;
        }
        public static ILingoMovie SetupGame(this IServiceProvider serviceProvider)
        {
            var game = serviceProvider.GetRequiredService<TetriGroundsGame>();
            return game.LoadMovie();
        }

        public static TetriGroundsGame StartGame(this IServiceProvider serviceProvider)
        {
            var game = serviceProvider.GetRequiredService<TetriGroundsGame>();
            game.Play();
            return game;
        }
    }
}
