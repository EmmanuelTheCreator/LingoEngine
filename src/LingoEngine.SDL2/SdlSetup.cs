using LingoEngine;
using LingoEngine.FrameworkCommunication;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngineSDL2.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngineSDL2;

public static class SdlSetup
{
    public static ILingoEngineRegistration WithLingoSdlEngine(this ILingoEngineRegistration reg, Action<SdlFactory>? setup = null)
    {
        reg.Services(s => s
            .AddSingleton<ILingoFrameworkFactory, SdlFactory>()
            .AddSingleton<ILingoFontManager, SdlFontManager>()
        ).WithFrameworkFactory(setup);
        return reg;
    }
}
