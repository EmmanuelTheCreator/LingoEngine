using LingoEngine.Director.Core.Events;
using LingoEngine;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Core
{
    public static class DirectorSetup
    {
        public static ILingoEngineRegistration WithDirectorEngine(this ILingoEngineRegistration engineRegistration)
        {
            engineRegistration.Services(s =>
            {

                IServiceCollection serviceCollection = s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()
                    .AddSingleton<ProjectSettings>()
                    .AddSingleton<DirectorProjectManager>()
                    ;

            });
            return engineRegistration;
        }

    }
}
