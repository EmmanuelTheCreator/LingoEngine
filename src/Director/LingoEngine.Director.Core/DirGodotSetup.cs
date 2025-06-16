using LingoEngine.Director.Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Director.Core
{
    public static class DirSetup
    {
        public static ILingoEngineRegistration WithDirectorEngine(this ILingoEngineRegistration engineRegistration)
        {
            engineRegistration.Services(s =>
            {

                IServiceCollection serviceCollection = s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()
                    ;

            });
            return engineRegistration;
        }

    }
}
