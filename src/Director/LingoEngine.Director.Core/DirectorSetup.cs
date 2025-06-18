using LingoEngine.Director.Core.Events;
using LingoEngine;
using LingoEngine.Director.Core.Commands;
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
                    .AddSingleton<DirectorProjectManager>()
                    .AddSingleton<SetStageScaleCommandHandler>()
                    ;

            });
            return engineRegistration;
        }

    }
}
