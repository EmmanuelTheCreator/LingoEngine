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
                        })
                        .ForMovie(TetriGroundsGame.MovieName, s => s
                            //.AddMovieScript<StartMoviesScript>()
                            //.AddMovieScript<StarMovieScript>()
                            // Globals
                            .AddBehavior<MouseDownNavigateBehavior>()
                            .AddBehavior<MouseDownNavigateWithStayBehavior>()
                            .AddBehavior<PauseBehaviour>()
                            .AddBehavior<StartGameBehavior>()
                            .AddBehavior<StayOnFrameFrameScript>()
                            .AddBehavior<WaiterFrameScript>()
                            // Beh
                            .AddBehavior<AnimationScriptBehavior>()
                            .AddBehavior<AppliBgBehavior>()
                            .AddBehavior<BgScriptBehavior>()
                            .AddBehavior<ExecuteBehavior>()
                            .AddBehavior<FontRollOverBehavior>()
                            .AddBehavior<GameStopBehavior>()
                            .AddBehavior<NewGameBehavior>()
                            .AddBehavior<StartBehavior>()
                            .AddBehavior<StopMenuBehavior>()
                            .AddBehavior<TextCounterBehavior>()

                            //.AddParentScript<BlockParentScript>()
                            //.AddParentScript<BlocksParentScript>()
                            //.AddParentScript<GfxParentScript>()
                            //.AddParentScript<InterestingEncryptoParentScript>()
                            //.AddParentScript<MousePointer>()
                            //.AddParentScript<OverScreenTextParentScript>()
                            //.AddParentScript<PlayerBlockParentScript>()
                            //.AddParentScript<ScoreManagerParentScript>()
                            //.AddParentScript<SpriteManagerParentScript>()
                            //.AddParentScript<ScoreGetParentScript>()
                            //.AddParentScript<ScoreSaveParentScript>()
                            //.AddParentScript<ClassSubscribeParentScript>()
                            //.AddParentScript<StartDataGetParentScript>()
                            //.AddParentScript<StartDataSaveParentScript>()
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
