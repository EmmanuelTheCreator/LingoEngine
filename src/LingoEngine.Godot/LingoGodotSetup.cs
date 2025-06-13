using Godot;
using LingoEngine;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Godot.Core;
using Microsoft.Extensions.DependencyInjection;
namespace LingoEngine.Godot
{
    public static class LingoGodotSetup
    {
        public static ILingoEngineRegistration WithLingoGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode, Action<GodotFactory>? setup = null)
        {
            engineRegistration
                .Services(s => s
                        .AddSingleton<ILingoFrameworkFactory, GodotFactory>()
                        .AddSingleton<ILingoFontManager, LingoGodotFontManager>()
                        .AddSingleton(p => new LingoGodotRootNode(rootNode))
                        )
                .WithFrameworkFactory(setup)
                ;
            return engineRegistration;
        }

    }
}
