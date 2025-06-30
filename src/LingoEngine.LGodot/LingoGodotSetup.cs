using Godot;
using LingoEngine;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Core;
using LingoEngine.LGodot.Stages;
using LingoEngine.LGodot.Styles;
using LingoEngine.Stages;
using LingoEngine.Styles;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine.LGodot
{
    public static class LingoGodotSetup
    {
        public static ILingoEngineRegistration WithLingoGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode, bool withStageInWindow = false, Action<GodotFactory>? setup = null)
        {
            engineRegistration
                .Services(s => s
                        .AddGodotLogging()
                        .AddSingleton<LingoGodotStyle>()
                        .AddSingleton<ILingoFrameworkFactory, GodotFactory>()
                        .AddSingleton<ILingoFrameworkStageContainer, LingoGodotStageContainer>()
                        .AddSingleton<ILingoFontManager, LingoGodotFontManager>()
                        .AddSingleton(p => new LingoGodotRootNode(rootNode, withStageInWindow))
                        )
                .WithFrameworkFactory(setup)
                ;
            return engineRegistration;
        }

    }
}
