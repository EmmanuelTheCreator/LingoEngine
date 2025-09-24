using AbstUI.Components;
using AbstUI.Inputs;
using AbstUI.LGodot.Components;
using AbstUI.LGodot.Inputs;
using AbstUI.LGodot.Styles;
using AbstUI.LGodot.Windowing;
using AbstUI.Styles;
using AbstUI.Resources;
using AbstUI.LGodot.Resources;
using AbstUI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using AbstUI.Core;

namespace AbstUI.LGodot
{
    public static class AbstUIGodotSetup
    {
        public static bool IsRegisteredServices = false;
        public static bool IsRegistered = false;
        public static IServiceCollection WithAbstUIGodot(this IServiceCollection services, Action<IAbstFameworkComponentWinRegistrator>? windowRegistrations = null)
        {
            if (IsRegisteredServices) return services;
            AbstEngineGlobal.RunFramework = AbstEngineRunFramework.Godot;
            IsRegisteredServices = true;
            services
                .AddSingleton<GodotComponentFactory>()
                .AddTransient<IAbstComponentFactory>(p => p.GetRequiredService<GodotComponentFactory>())
                .AddTransient<IAbstFrameworkDialog, AbstGodotDialog>()
                .AddTransient<AbstGodotDialog>(p => (AbstGodotDialog)p.GetRequiredService<IAbstFrameworkDialog>())

                .AddSingleton<IAbstGlobalMouse, GlobalGodotAbstMouse>()
                .AddSingleton<IAbstGlobalKey, GlobalAbstKey>()
                .AddSingleton<IAbstGodotWindowManager, AbstGodotWindowManager>()
                .AddSingleton<IAbstFrameworkMainWindow, AbstGodotMainWindow>()


                .AddSingleton<IAbstFontManager, AbstGodotFontManager>()
                .AddSingleton<IAbstStyleManager, AbstGodotStyleManager>()
                .AddSingleton<IAbstResourceManager, AbstGodotResourceManager>()
                .AddTransient(p => (AbstGodotFontManager)p.GetRequiredService<IAbstFontManager>())
                .AddTransient(p => (AbstGodotStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .AddTransient(p => (IAbstGodotStyleManager)p.GetRequiredService<IAbstStyleManager>())
                .WithAbstUI(windowRegistrations)
                ;

            return services;
        }
        
        public static IServiceProvider WithAbstUIGodot(this IServiceProvider services)
        {
            if (IsRegistered) return services;
            IsRegistered = true;
            services.WithAbstUI(); // need to be first to register all the windows in the windows factory.
            var factory = services.GetRequiredService<IAbstComponentFactory>();
            factory

                .Register<AbstMouse, AbstGodotMouse>()
                .Register<GlobalGodotAbstMouse, AbstGodotGlobalMouse>()
                .Register<AbstKey, AbstGodotKey>()
                .Register<AbstDialog, AbstGodotDialog>()
                .Register<AbstMainWindow, AbstGodotMainWindow>()
                .Register<AbstWindowManager, AbstGodotWindowManager>()
                ;

            var windowManager = services.GetRequiredService<IAbstGodotWindowManager>();// we need to resolve the framework window manager to link him
            services.GetRequiredService<IAbstGlobalKey>(); 
            services.GetRequiredService<IAbstGlobalMouse>(); 
            return services;
        }
    }
}
