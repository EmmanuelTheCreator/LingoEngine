using LingoEngine.FrameworkCommunication;
using LingoEngine.Setup;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.L3D.Core;

/// <summary>
/// Extension helpers to register the 3D engine services.
/// </summary>
public static class Lingo3dSetup
{
    public static ILingoEngineRegistration WithLingo3d(this ILingoEngineRegistration reg)
    {
        //reg.Services(s => s.AddSingleton<ILingoFrameworkFactory, Lingo3dFactory>());
        return reg;
    }
}
