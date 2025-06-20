using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngine.Core;

internal static class CommandManagerExtensions
{
    public static void DiscoverAndSubscribe(this ILingoCommandManager manager, IServiceProvider provider)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var type in assemblies.SelectMany(a =>
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }))
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                manager.Register(type);
            }
        }
    }
}
