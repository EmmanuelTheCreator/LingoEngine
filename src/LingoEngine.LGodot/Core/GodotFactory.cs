using Godot;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
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
using LingoEngine.Sprites;
using LingoEngine.Stages;
using LingoEngine.Styles;
using Microsoft.Extensions.Logging;
using System;

namespace LingoEngine.LGodot.Core
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
                Type t when t == typeof(LingoMemberBitmap) => (CreateMemberBitmap(cast, numberInCast, name) as T)!,
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
        public LingoMemberBitmap CreateMemberBitmap(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberBitmap(_serviceProvider.GetRequiredService<ILogger<LingoGodotMemberBitmap>>());
            var lingoInstance = new LingoMemberBitmap((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberField CreateMemberField(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberField(_serviceProvider.GetRequiredService<ILingoFontManager>(), _serviceProvider.GetRequiredService<ILogger<LingoGodotMemberField>>());
            var lingoInstance = new LingoMemberField((LingoCast)cast, godotInstance, numberInCast, name, fileName ?? "", regPoint);
            godotInstance.Init(lingoInstance);
            _disposables.Add(godotInstance);
            return lingoInstance;
        }
        public LingoMemberText CreateMemberText(ILingoCast cast, int numberInCast, string name = "", string? fileName = null, LingoPoint regPoint = default)
        {
            var godotInstance = new LingoGodotMemberText(_serviceProvider.GetRequiredService<ILingoFontManager>(),_serviceProvider.GetRequiredService<ILogger<LingoGodotMemberText>>());
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


        #region Gfx elements
        public LingoGfxCanvas CreateGfxCanvas(string name, int width, int height)
        {
            var canvas = new LingoGfxCanvas();
            var impl = new LingoGodotGfxCanvas(canvas, _serviceProvider.GetRequiredService<ILingoFontManager>(), width, height);
            canvas.Width = width;
            canvas.Height = height;
            canvas.Name = name;
            return canvas;
        }

        public LingoGfxWrapPanel CreateWrapPanel(LingoOrientation orientation, string name)
        {
            var panel = new LingoGfxWrapPanel();
            var impl = new LingoGodotWrapPanel(panel, orientation);

            panel.Name = name;
            return panel;
        }

        public LingoGfxPanel CreatePanel(string name)
        {
            var panel = new LingoGfxPanel();
            var impl = new LingoGodotPanel(panel);

            panel.Name = name;
            return panel;
        }

        public LingoGfxTabContainer CreateTabContainer(string name)
        {
            var tab = new LingoGfxTabContainer();
            var impl = new LingoGodotTabContainer(tab);

            tab.Name = name;
            return tab;
        }
        public LingoGfxTabItem CreateTabItem(string name, string title)
        {
            var tab = new LingoGfxTabItem(title);
            var impl = new LingoGodotTabItem(tab);

            tab.Name = name;
            return tab;
        }

        public LingoGfxScrollContainer CreateScrollContainer(string name)
        {
            var scroll = new LingoGfxScrollContainer();
            var impl = new LingoGodotScrollContainer(scroll);
            scroll.Name = name;
            return scroll;
        }

        public LingoGfxInputText CreateInputText(string name, int maxLength = 0)
        {
            var input = new LingoGfxInputText();
            var impl = new LingoGodotInputText(input, _serviceProvider.GetRequiredService<ILingoFontManager>());
            input.MaxLength = maxLength;
            input.Name = name;
            return input;
        }

        public LingoGfxInputNumber CreateInputNumber(string name, float min = 0, float max = 100)
        {
            var input = new LingoGfxInputNumber();
            var impl = new LingoGodotInputNumber(input);
            input.Min = min;
            input.Max = max;
            input.Name = name;
            return input;
        }

        public LingoGfxSpinBox CreateSpinBox(string name, float min = 0, float max = 100)
        {
            var spin = new LingoGfxSpinBox();
            var impl = new LingoGodotSpinBox(spin);
            spin.Name = name;
            spin.Min = min;
            spin.Max = max;
            return spin;
        }

        public LingoGfxInputCheckbox CreateInputCheckbox(string name)
        {
            var input = new LingoGfxInputCheckbox();
            var impl = new LingoGodotInputCheckbox(input);

            input.Name = name;
            return input;
        }

        public LingoGfxInputCombobox CreateInputCombobox(string name)
        {
            var input = new LingoGfxInputCombobox();
            var impl = new LingoGodotInputCombobox(input);

            input.Name = name;
            return input;
        }

        public LingoGfxLabel CreateLabel(string name, string text = "")
        {
            var label = new LingoGfxLabel();
            var impl = new LingoGodotLabel(label, _serviceProvider.GetRequiredService<ILingoFontManager>());
            label.Text = text;

            label.Name = name;
            return label;
        }

        public LingoGfxButton CreateButton(string name, string text = "")
        {
            var button = new LingoGfxButton();
            var impl = new LingoGodotButton(button);
            button.Name = name;
            if (!string.IsNullOrWhiteSpace(text))
                button.Text = text;
            return button;
        }

        public LingoGfxMenu CreateMenu(string name)
        {
            var menu = new LingoGfxMenu();
            var impl = new LingoGodotMenu(menu, name);
            return menu;
        }

        public LingoGfxMenuItem CreateMenuItem(string name, string? shortcut = null)
        {
            var item = new LingoGfxMenuItem();
            var impl = new LingoGodotMenuItem(name, shortcut);
            return item;
        } 
        #endregion


    }
}
