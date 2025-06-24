using Godot;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Core;
using LingoEngine.LGodot.Movies;
using LingoEngine.LGodot.Pictures;
using LingoEngine.LGodot.Sounds;
using LingoEngine.LGodot.Texts;
using LingoEngine.LGodot.Shapes;
using LingoEngine.Inputs;
using LingoEngine.Movies;
using LingoEngine.Primitives;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using Microsoft.Extensions.DependencyInjection;
using LingoEngine.Pictures;
using LingoEngine.LGodot.Stages;
using LingoEngine.Members;
using LingoEngine.Casts;
using LingoEngine.Shapes;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Gfx;

namespace LingoEngine.LGodot
{
    public class GodotFactory : ILingoFrameworkFactory, IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly IServiceProvider _serviceProvider;
        private Node _rootNode;

        public GodotFactory(IServiceProvider serviceProvider, LingoGodotRootNode rootNode)
        {
            _rootNode = rootNode.RootNode;
            _serviceProvider = serviceProvider;
        }

        public T CreateBehavior<T>(LingoMovie lingoMovie) where T : LingoSpriteBehavior => lingoMovie.GetServiceProvider().GetRequiredService<T>();
        public T CreateMovieScript<T>(LingoMovie lingoMovie) where T : LingoMovieScript => lingoMovie.GetServiceProvider().GetRequiredService<T>();

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
            var lingoSoundChannel = new LingoGodotSoundChannel(number, _rootNode);
            var soundChannel = new LingoSoundChannel(lingoSoundChannel, number);
            lingoSoundChannel.Init(soundChannel);
            _disposables.Add(lingoSoundChannel);
            return soundChannel;
        }
        #endregion


        #region Members
        public T CreateMember<T>(ILingoCast cast, int numberInCast, string name = "") where T : LingoMember
        {

            return typeof(T) switch
            {
                Type t when t == typeof(LingoMemberPicture) => (CreateMemberPicture(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberText) => (CreateMemberText(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberField) => (CreateMemberField(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberSound) => (CreateMemberSound(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberFilmLoop) => (CreateMemberFilmLoop(cast, numberInCast, name) as T)!,
                Type t when t == typeof(LingoMemberShape) => (CreateMemberShape(cast, numberInCast, name) as T)!,
            };
        }
        public LingoMemberSound CreateMemberSound(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var lingoMemberSound = new LingoGodotMemberSound();
            var memberSound = new LingoMemberSound(lingoMemberSound, (LingoCast)cast, numberInCast, name, fileName ?? "");
            lingoMemberSound.Init(memberSound);
            _disposables.Add(lingoMemberSound);
            return memberSound;
        }
        public LingoMemberFilmLoop CreateMemberFilmLoop(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var impl = new LingoGodotMemberFilmLoop();
            var member = new LingoMemberFilmLoop(impl, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            impl.Init(member);
            _disposables.Add(impl);
            return member;
        }
        public LingoMemberShape CreateMemberShape(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var impl = new LingoGodotMemberShape();
            var member = new LingoMemberShape((LingoCast)cast, impl, numberInCast, name, fileName ?? "", regPoint);
            _disposables.Add(impl);
            return member;
        }
        public LingoMemberPicture CreateMemberPicture(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberPicture();
            var lingoInstance = new LingoMemberPicture((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberField(_serviceProvider.GetRequiredService<ILingoFontManager>());
            var lingoInstance = new LingoMemberField((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberText(_serviceProvider.GetRequiredService<ILingoFontManager>());
            var lingoInstance = new LingoMemberText((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMember CreateEmpty(ILingoCast cast, int numberInCast, string name = "", string? fileName = null,
            LingoPoint regPoint = default)
        {
            var godotInstance = new LingoFrameworkMemberEmpty();
            var lingoInstance = new LingoMember(godotInstance, LingoMemberType.Empty, (LingoCast)cast, numberInCast, name, fileName ?? "", regPoint);
            return lingoInstance;
        }
        #endregion


        public LingoStage CreateStage(LingoPlayer lingoPlayer)
        {
            var stageContainer = (LingoGodotStageContainer)_serviceProvider.GetRequiredService<ILingoFrameworkStageContainer>();
            var godotInstance = new LingoGodotStage(lingoPlayer);
            var lingoInstance = new LingoStage(godotInstance);
            stageContainer.SetStage(godotInstance);
            godotInstance.Init(lingoInstance, lingoPlayer);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMovie AddMovie(LingoStage stage, LingoMovie lingoMovie)
        {
            var godotStage = stage.Framework<LingoGodotStage>();
            var godotInstance = new LingoGodotMovie(godotStage, lingoMovie, m => _disposables.Remove(m));
            lingoMovie.Init(godotInstance);
            _disposables.Add(godotInstance);

            return lingoMovie;
        }

        /// <summary>
        /// Dependant on movie, because the behaviors are scoped and movie related.
        /// </summary>
        public T CreateSprite<T>(ILingoMovie movie, Action<LingoSprite> onRemoveMe) where T : LingoSprite
        {
            var movieTyped = (LingoMovie)movie;
            var lingoSprite = movieTyped.GetServiceProvider().GetRequiredService<T>();
            lingoSprite.SetOnRemoveMe(onRemoveMe);
            movieTyped.Framework<LingoGodotMovie>().CreateSprite(lingoSprite);
            return lingoSprite;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        public LingoMouse CreateMouse(LingoStage stage)
        {
            LingoMouse? mouse = null;
            var godotInstance = new LingoGodotMouse(_rootNode, new Lazy<LingoMouse>(() => mouse!));
            mouse = new LingoMouse(stage, godotInstance);
            return mouse;
        }

        public LingoKey CreateKey()
        {
            LingoKey? key = null;
            var impl = new LingoGodotKey(_rootNode, new Lazy<LingoKey>(() => key!));
            key = new LingoKey(impl);
            impl.SetLingoKey(key);
            return key;
        }

        public LingoGfxCanvas CreateGfxCanvas(int width, int height)
        {
            var canvas = new LingoGfxCanvas();
            var impl = new LingoGodotGfxCanvas(canvas, _serviceProvider.GetRequiredService<ILingoFontManager>(), width, height);
            _rootNode.AddChild(impl);
            canvas.Width = width;
            canvas.Height = height;
            return canvas;
        }

        public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation)
        {
            var panel = new LingoGfxWrapPanel();
            var impl = new LingoGodotWrapPanel(panel, orientation);
            _rootNode.AddChild(impl);
            return panel;
        }

        public LingoGfxPanel CreatePanel()
        {
            var panel = new LingoGfxPanel();
            var impl = new LingoGodotPanel(panel);
            _rootNode.AddChild(impl);
            return panel;
        }

        public LingoGfxInputText CreateInputText(int maxLength = 0)
        {
            var input = new LingoGfxInputText { MaxLength = maxLength };
            var impl = new LingoGodotInputText(input, _serviceProvider.GetRequiredService<ILingoFontManager>());
            _rootNode.AddChild(impl);
            return input;
        }

        public LingoGfxInputNumber CreateInputNumber(float min = 0, float max = 100)
        {
            var input = new LingoGfxInputNumber { Min = min, Max = max };
            var impl = new LingoGodotInputNumber(input);
            _rootNode.AddChild(impl);
            return input;
        }

        public LingoGfxInputCheckbox CreateInputCheckbox()
        {
            var input = new LingoGfxInputCheckbox();
            var impl = new LingoGodotInputCheckbox(input);
            _rootNode.AddChild(impl);
            return input;
        }

        public LingoGfxInputCombobox CreateInputCombobox()
        {
            var input = new LingoGfxInputCombobox();
            var impl = new LingoGodotInputCombobox(input);
            _rootNode.AddChild(impl);
            return input;
        }
    }
}
