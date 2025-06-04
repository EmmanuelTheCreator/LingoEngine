using ArkGodot.GodotLinks;
using Godot;
using LingoEngine;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;
using LingoEngineGodot.Movies;
using LingoEngineGodot.Pictures;
using LingoEngineGodot.Sounds;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngineGodot
{
    public class GodotFactory : ILingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly LingoMovieEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;

        public GodotFactory(LingoMovieEnvironment environment, IServiceProvider serviceProvider)
        {
            _environment = environment;
            _serviceProvider = serviceProvider;
        }

        public LingoSpriteBehavior CreateBehavior<T>() where T : LingoSpriteBehavior => _serviceProvider.GetRequiredService<T>();

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


        public LingoMemberPicture CreateMemberPicture(int number, string name = "")
        {
            var godotInstance = new LingoGodotMemberPicture();
            var lingoInstance = new LingoMemberPicture(godotInstance, number, name);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoStage CreateStage()
        {
            var godotInstance = new LingoGodotStage();
            var lingoInstance = new LingoStage(godotInstance);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }

        /// <summary>
        /// Dependant on movie, because the behaviors are scoped and movie related.
        /// </summary>
        public T CreateSprite<T>(ILingoMovie movie, ILingoScore score) where T : LingoSprite
        {
            var movieTyped = (LingoMovie)movie;
            var lingoSprite = movieTyped.GetServiceProvider().GetRequiredService<T>();
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
