using ArkGodot.DirectorProxy;
using ArkGodot.GodotLinks;
using Godot;
using LingoEngine;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LingoEngineGodot
{
    public class GodotFactory : ILingoFrameworkFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GodotFactory(ILingoEnvironment environment, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public T CreateSprite<T>(ILingoScore score) where T : LingoSprite
        {
            var lingoSprite = _serviceProvider.GetRequiredService<T>();
            var node2d = new LingoGodotSprite(new Node2D(), lingoSprite);
            return lingoSprite;
        }

    }
}
