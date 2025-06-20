using LingoEngine.Director.Core.Events;
using LingoEngine;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Director.Core.Menus;
using LingoEngine.Director.Core.Windows;

namespace LingoEngine.Director.Core
{
    public static class DirectorSetup
    {
        public static ILingoEngineRegistration WithDirectorEngine(this ILingoEngineRegistration engineRegistration)
        {
            engineRegistration.Services(s => s
                    .AddSingleton<IDirectorEventMediator, DirectorEventMediator>()
                    .AddSingleton<IDirectorShortCutManager, DirectorShortCutManager>()
                    .AddSingleton<IDirectorWindowManager, DirectorWindowManager>()
                    .AddSingleton<DirectorProjectManager>()
                    );
            engineRegistration.AddBuildAction(
                (serviceProvider) =>
                {
                    DirectorWindowRegistrator.RegisterDirectorWindows(serviceProvider);
                   
                });
            return engineRegistration;
        }

    }
}
