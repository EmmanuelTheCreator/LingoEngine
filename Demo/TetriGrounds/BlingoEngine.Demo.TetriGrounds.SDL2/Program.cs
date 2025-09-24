using BlingoEngine.SDL2;
using BlingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if DEBUG_WITH_DIRECTOR
using BlingoEngine.Director.Core.Projects;
using BlingoEngine.Director.SDL2;
#endif
#if DEBUG
using BlingoEngine.Net.RNetProjectHost;
using BlingoEngine.Net.RNetPipeServer;
#endif

namespace BlingoEngine.Demo.TetriGrounds.SDL2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            IServiceProvider? serviceProvider = null;
            services
                // Add logging support
                .AddLogging(config =>
                {
                    config.AddConsole();   // log to console
                    config.SetMinimumLevel(LogLevel.Debug);
                })

                .RegisterBlingoEngine(c => c
#if DEBUG_WITH_DIRECTOR
                    .WithDirectorSdlEngine("TetriGrounds", 1600, 970, d =>
                    {
                        d.CsProjFile = "BlingoEngine.Demo.TetriGrounds.Core\\BlingoEngine.Demo.TetriGrounds.Core.csproj";
                    })
#else
                    .WithBlingoSdlEngine("TetriGrounds", 730, 500)
#endif
#if DEBUG
                    .WithRNetProjectHostServer(61699,true)
                    //.WithRNetPipeHostServer(61699,true)
#endif
                    .SetProjectFactory<BlingoEngine.Demo.TetriGrounds.Core.TetriGroundsProjectFactory>()
                    .BuildAndRunProject(sp => serviceProvider = sp)
                    );
            serviceProvider?.GetRequiredService<BlingoSdlRootContext>().Run();
            SdlSetup.Dispose();
        }
    }

}

