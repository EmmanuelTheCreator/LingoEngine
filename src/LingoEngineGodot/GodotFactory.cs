using ArkGodot.GodotLinks;
using Godot;
using LingoEngine;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Sounds;
using LingoEngineGodot.Sounds;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngineGodot
{
    public class GodotFactory : ILingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly ILingoEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;

        public GodotFactory(ILingoEnvironment environment, IServiceProvider serviceProvider)
        {
            _environment = environment;
            _serviceProvider = serviceProvider;
        }

        #region Sound
        public LingoMemberSound CreateMemberSound(int number, string name = "")
        {
            var lingoMemberSound = new LingoGodotMemberSound();
            var memberSound = new LingoMemberSound(lingoMemberSound, number, name);
            lingoMemberSound.Init(memberSound);
            _disposables.Add(lingoMemberSound);
            return memberSound;
        }
        public LingoSound CreateSound()
        {
            var lingoSound = new LingoGodotSound();
            var soundChannel = new LingoSound(lingoSound, _environment);
            lingoSound.Init(soundChannel);
            return soundChannel;
        }
        public LingoSoundChannel CreateSoundChannel(int number)
        {
            var lingoSoundChannel = new LingoGodotSoundChannel(number);
            var soundChannel = new LingoSoundChannel(lingoSoundChannel, number);
            lingoSoundChannel.Init(soundChannel);
            _disposables.Add(lingoSoundChannel);
            return soundChannel;
        } 
        #endregion


        public T CreateSprite<T>(ILingoScore score) where T : LingoSprite
        {
            var lingoSprite = _serviceProvider.GetRequiredService<T>();
            var node2d = new LingoGodotSprite(new Node2D(), lingoSprite);
            return lingoSprite;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}
