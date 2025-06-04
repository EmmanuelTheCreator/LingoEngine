using ArkGodot.GodotLinks;
using Godot;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using LingoEngineGodot.Movies;
using LingoEngineGodot.Pictures;
using LingoEngineGodot.Sounds;
using Microsoft.Extensions.DependencyInjection;

namespace LingoEngineGodot
{
    public class GodotFactory : ILingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly IServiceProvider _serviceProvider;
        private Node2D _rootNode;
        private Node2D _stageNode2D;

        public GodotFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T CreateBehavior<T>() where T : LingoSpriteBehavior => _serviceProvider.GetRequiredService<T>();
        public T CreateMovieScript<T>() where T : LingoMovieScript => _serviceProvider.GetRequiredService<T>();

        #region Sound
       
        public LingoSound CreateSound(ILingoCastLibsContainer castLibsContainer)
        {
            var lingoSound = new LingoGodotSound();
            var soundChannel = new LingoSound(lingoSound, castLibsContainer, this);
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


        #region Members
        public T CreateMember<T>(ILingoCast cast, string name = "") where T : LingoMember
        {

            return typeof(T) switch
            {
                Type t when t == typeof(LingoMemberPicture) => (CreateMemberPicture(cast, name) as T)!,
                Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, name) as T)!,
                Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, name) as T)!,
            };
        }
        public LingoMemberSound CreateMemberSound(ILingoCast cast, string name = "")
        {
            var newNumber = cast.FindEmpty();
            var lingoMemberSound = new LingoGodotMemberSound();
            var memberSound = new LingoMemberSound(lingoMemberSound, (LingoCast)cast, newNumber, name);
            lingoMemberSound.Init(memberSound);
            _disposables.Add(lingoMemberSound);
            return memberSound;
        }
        public LingoMemberPicture CreateMemberPicture(ILingoCast cast, string name = "")
        {
            var newNumber = cast.FindEmpty();
            var godotInstance = new LingoGodotMemberPicture();
            var lingoInstance = new LingoMemberPicture((LingoCast)cast, godotInstance, newNumber, name);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberText CreateMemberText(ILingoCast cast, string name = "")
        {
            var newNumber = cast.FindEmpty();
            //var godotInstance = new LingoGodotMemberText();
            var lingoInstance = new LingoMemberText((LingoCast)cast, newNumber, name);
            //godotInstance.Init(lingoInstance);
            //_disposables.Add(godotInstance);
            return lingoInstance;
        } 
        #endregion


        public LingoStage CreateStage()
        {
            var godotInstance = new LingoGodotStage(_rootNode);
            var lingoInstance = new LingoStage(godotInstance);
            _stageNode2D = godotInstance.StageNode2D;
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }

        /// <summary>
        /// Dependant on movie, because the behaviors are scoped and movie related.
        /// </summary>
        public T CreateSprite<T>(ILingoMovie movie) where T : LingoSprite
        {
            var movieTyped = (LingoMovie)movie;
            var lingoSprite = movieTyped.GetServiceProvider().GetRequiredService<T>();
            var spriteNode2D = new Node2D();
            _stageNode2D.AddChild(spriteNode2D);
            var node2d = new LingoGodotSprite(spriteNode2D, lingoSprite);
            return lingoSprite;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        internal void SetRootNode(Node2D rootNode) => _rootNode = rootNode;
    }
}
