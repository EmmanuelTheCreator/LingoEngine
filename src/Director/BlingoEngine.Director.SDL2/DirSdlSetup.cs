using AbstUI.SDL2;
using BlingoEngine.Core;
using BlingoEngine.Director.Core;
using BlingoEngine.Director.Core.Behaviors;
using BlingoEngine.Director.Core.Casts;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Inspector;
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Director.Core.Stages;
using BlingoEngine.Director.LGodot;
using BlingoEngine.Director.SDL2.Behaviors;
using BlingoEngine.Director.SDL2.Casts;
using BlingoEngine.Director.SDL2.Icons;
using BlingoEngine.Director.SDL2.Inspector;
using BlingoEngine.Director.SDL2.Scores;
using BlingoEngine.Director.SDL2.Stages;
using BlingoEngine.Director.SDL2.UI;
using BlingoEngine.Projects;
using BlingoEngine.SDL2;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Director.SDL2
{
    public static class DirSdlSetup
    {
        public static IBlingoEngineRegistration WithDirectorSdlEngine(this IBlingoEngineRegistration reg, string windowTitle, int width, int height, Action<DirectorProjectSettings>? directorSettings = null)
        {
            BlingoEngineGlobal.IsRunningDirector = true;
            reg.WithBlingoSdlEngine(windowTitle, width, height)
               .WithDirectorEngine(directorSettings)
               .RegisterComponents(c =>
                    c
                        .AddSingleton<DirectorCastWindow, DirSdlCastWindow>()
                        .AddSingleton<DirectorStageWindow, DirSdlStageWindow>()
                        .AddSingleton<DirectorScoreWindow, DirSdlScoreWindow>()
                        .AddSingleton<DirectorPropertyInspectorWindow, DirSdlPropertyInspectorWindow>()
                        .AddSingleton<DirBehaviorInspectorWindow, DirSdlBehaviorInspectorWindow>()
                    )
               .ServicesMain(s =>
               {
                   s
                        .AddSingleton<DirSDLMainMenu>()
                        .AddTransient<IDirFrameworkCastWindow>(p => p.GetRequiredService<DirSdlCastWindow>())
                        .AddTransient<IDirFrameworkStageWindow>(p => p.GetRequiredService<DirSdlStageWindow>())
                        .AddTransient<IDirFrameworkScoreWindow>(p => p.GetRequiredService<DirSdlScoreWindow>())
                        .AddTransient<IDirFrameworkPropertyInspectorWindow>(p => p.GetRequiredService<DirSdlPropertyInspectorWindow>())
                        .AddTransient<IDirFrameworkBehaviorInspectorWindow>(p => p.GetRequiredService<DirSdlBehaviorInspectorWindow>())
                    ;

                   s.AddSingleton<IDirectorIconManager>(p =>
                   {
                       var mgr = new DirSdlIconManager(p.GetRequiredService<ILogger<DirSdlIconManager>>(), p.GetRequiredService<BlingoSdlRootContext>());
                       mgr.LoadSheet("Media/Icons/General_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Painter_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Painter2_Icons.png", 20, 16, 16, 8);
                       mgr.LoadSheet("Media/Icons/Director_Icons.png", 20, 16, 16, 8);
                       return mgr;
                   });
               })
               .AddBuildAction(p =>
               {
                   new BlingoSdlDirectorRoot(p.GetRequiredService<IAbstSDLRootContext>(),p.GetRequiredService<BlingoPlayer>(), p, p.GetRequiredService<BlingoProjectSettings>());
               });
            return reg;
        }
    }
}

