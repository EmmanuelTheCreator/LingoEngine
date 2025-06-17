namespace LingoEngine
{
    public static class GodotEngineSetup
    {
        public static ILingoEngineRegistration WithGodotEngine(this ILingoEngineRegistration engineRegistration)
        {
            
            engineRegistration.Services(s =>
            {

                //IServiceCollection serviceCollection = s
                //  .AddSingleton<ILingoFrameworkStageWindow>(p => new DirGodotStageWindow(rootNode))
                //  });

            });
            return engineRegistration;
        }
    }
}
