using Godot;

namespace LingoEngine.Director.Godot
{
    public static class DirGodotSetup
    {
        public static ILingoEngineRegistration WithDirectorGodotEngine(this ILingoEngineRegistration engineRegistration, Node rootNode)
        {
            //engineRegistration
            //    .Services(s => s
            //            .AddSingleton<ILingoFrameworkFactory, GodotFactory>()
            //            .AddSingleton<ILingoFontManager, LingoGodotFontManager>()
            //            .AddSingleton(p => new LingoGodotRootNode(rootNode))
            //            )
            //    .WithFrameworkFactory(setup)
            //    ;
            return engineRegistration;
        }

    }
}
