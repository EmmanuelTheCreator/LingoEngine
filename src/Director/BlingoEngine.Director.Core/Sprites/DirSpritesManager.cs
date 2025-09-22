using AbstUI.Commands;
using BlingoEngine.Director.Core.Scores;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Director.Core.Stages.Commands;
using BlingoEngine.Tempos;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Scripts;
using BlingoEngine.Sounds;
using BlingoEngine.Transitions;
using BlingoEngine.Members;
using BlingoEngine.Director.Core.Casts.Commands;
using System.Collections.Generic;
using System.Linq;
using BlingoEngine.Bitmaps;
using BlingoEngine.Texts;
using BlingoEngine.FilmLoops;
using BlingoEngine.Shapes;

namespace BlingoEngine.Director.Core.Sprites
{
    public interface IDirSpritesManager
    {
        
        IDirectorEventMediator Mediator { get; }
        IBlingoFrameworkFactory Factory { get; }
        IAbstCommandManager CommandManager { get; }
        DirSpritesSelection SpritesSelection { get; }
        IBlingoKey Key { get; }
        DirScoreManager ScoreManager { get; }

        void SelectSprite(BlingoSprite sprite);
        void DeselectSprite(BlingoSprite sprite);
        void DeleteSelected(BlingoMovie movie);
        void CreateFilmLoop(BlingoMovie movie, string name);
    }

    public class DirSpritesManager : IDirSpritesManager, IDisposable,
            IAbstCommandHandler<ChangeSpriteRangeCommand>,
            IAbstCommandHandler<AddSpriteCommand>,
            IAbstCommandHandler<RemoveSpriteCommand>
    {
        private readonly IHistoryManager _historyManager;

        public IDirectorEventMediator Mediator { get; }
        public IBlingoFrameworkFactory Factory { get; }
        public IAbstCommandManager CommandManager { get; }
        public DirSpritesSelection SpritesSelection { get; } = new();
        public DirScoreManager ScoreManager { get; }

        public IBlingoKey Key { get; }

        public DirSpritesManager(IDirectorEventMediator mediator, IBlingoFrameworkFactory factory, IAbstCommandManager commandManager, DirScoreManager scoreManager, IHistoryManager historyManager)
        {
            Mediator = mediator;
            Mediator.Subscribe(this);
            Factory = factory;
            CommandManager = commandManager;
            Key = factory.CreateKey();
            ScoreManager = scoreManager;
            _historyManager = historyManager;
            ScoreManager.SetSpritesManager(this);
        }
        public void Dispose()
        {
            Mediator.Unsubscribe(this);
            SpritesSelection.Clear();
        }

        public void SelectSprite(BlingoSprite sprite)
        {
            if (SpritesSelection.AlreadySelected(sprite))
                return;
            if (!Key.ControlDown && !Key.ShiftDown)
                SpritesSelection.Clear();
            SpritesSelection.Add(sprite);
            ScoreManager.SelectSprite(sprite);
            Mediator.RaiseSpriteSelected(sprite);
        }
        public void DeselectSprite(BlingoSprite sprite)
        {
            SpritesSelection.Remove(sprite);
            ScoreManager.DeselectSprite(sprite);
        }

        public void CreateFilmLoop(BlingoMovie movie, string name)
        {
            var sprites = SpritesSelection.Sprites.ToArray();
            CommandManager.Handle(new CreateFilmLoopCommand(movie, sprites, name));
        }

        public void DeleteSelected(BlingoMovie movie)
        {
            var sprites = SpritesSelection.Sprites.Where(x => !x.Lock).ToArray();
            foreach (var s in sprites)
                CommandManager.Handle(new RemoveSpriteCommand(movie, s));
        }

        public void ChannelChanged(int spriteNumWithChannel)
        {

        }

        #region Commands

        public bool CanExecute(ChangeSpriteRangeCommand command) => true;
        public bool Handle(ChangeSpriteRangeCommand command)
        {
            if (command.EndChannel != command.Sprite.SpriteNum - 1)
                command.Movie.ChangeSpriteChannel(command.Sprite, command.EndChannel);
            command.Sprite.BeginFrame = command.EndBegin;
            command.Sprite.EndFrame = command.EndEnd;
            _historyManager.Push(command.ToUndo(() => ChannelChanged(command.Sprite.SpriteNumWithChannel)), command.ToRedo(() => ChannelChanged(command.Sprite.SpriteNumWithChannel)));
            ChannelChanged(command.Sprite.SpriteNumWithChannel);
            return true;
        }

        public bool CanExecute(AddSpriteCommand command) => true;
        public bool Handle(AddSpriteCommand command)
        {
            var sprite = CreateSprite(command.Movie, command.Member, command.SpriteNumWithChannel, command.BeginFrame, command.EndFrame);
            if (sprite == null) return true;
            _historyManager.Push(command.ToUndo(sprite, () => ChannelChanged(sprite.SpriteNumWithChannel)), command.ToRedo(() => ChannelChanged(sprite.SpriteNumWithChannel)));
            ChannelChanged(sprite.SpriteNumWithChannel);
            return true;
        }

        public bool CanExecute(RemoveSpriteCommand command) => true;
        public bool Handle(RemoveSpriteCommand command)
        {
            var movie = command.Movie;
            var sprite = command.Sprite;

            int channel = sprite.SpriteNum;
            int begin = sprite.BeginFrame;
            int end = sprite.EndFrame;
            BlingoMemberSound? memberSound = null;
            if (sprite is BlingoSpriteSound sound)
                memberSound = sound.Sound;
          
            Action<BlingoSprite> action = sprite.GetCloneAction();

            sprite.RemoveMe();

            BlingoSprite current = sprite;
            void refresh() => ChannelChanged(command.Sprite.SpriteNumWithChannel);

            Action undo = () =>
            {
                current = CreateSprite(movie, sprite, channel, begin,end, memberSound, action, current);
                refresh();
            };

            Action redo = () =>
            {
                current.RemoveMe();
                refresh();
            };

            _historyManager.Push(undo, redo);
            ChannelChanged(command.Sprite.SpriteNumWithChannel);
            return true;
        }


        private static BlingoSprite CreateSprite(BlingoMovie movie, BlingoSprite sprite, int spriteNumWithChannel, int begin,int end, BlingoMemberSound? memberSound, Action<BlingoSprite> action, BlingoSprite current)
        {
            switch (sprite)
            {
                case BlingoSprite2D:
                    {
                        var sprite2D = movie.Sprite2DManager.Add(spriteNumWithChannel - movie.Sprite2DManager.SpriteNumChannelOffset, begin,end);
                        action(sprite2D);
                        current = sprite2D;
                        break;
                    }
                case BlingoTempoSprite:
                    {
                        var tempoSprite1 = movie.Tempos.Add(begin);
                        action(tempoSprite1);
                        current = tempoSprite1;
                        break;
                    }
                case BlingoColorPaletteSprite:
                    {
                        var colorSprite = movie.ColorPalettes.Add(begin);
                        action(colorSprite);
                        current = colorSprite;
                        break;
                    }
                case BlingoFrameScriptSprite:
                    {
                        var scriptSprite = movie.FrameScripts.Add(begin);
                        action(scriptSprite);
                        current = scriptSprite;
                        break;
                    }
                case BlingoTransitionSprite:
                    {
                        var transitionSprite = movie.Transitions.Add(begin);
                        action(transitionSprite);
                        current = transitionSprite;
                        break;
                    }
                case BlingoSpriteSound:
                    {
                        if (memberSound != null)
                        {
                            var soundSprite = movie.Audio.Add(spriteNumWithChannel - movie.Audio.SpriteNumChannelOffset, begin, memberSound);
                            action(soundSprite);
                            current = soundSprite;
                        }
                        break;
                    }
            }

            return current;
        } 
        public static BlingoSprite? CreateSprite(BlingoMovie movie, IBlingoMember member, int spriteNumWithChannel, int begin, int end)
        {
            switch (member)
            {
                case BlingoMemberShape:
                case BlingoFilmLoopMember:
                case BlingoMemberBitmap:
                    return movie.Sprite2DManager.Add(spriteNumWithChannel - movie.Sprite2DManager.SpriteNumChannelOffset, begin,end, c => c.SetMember(member));
                case BlingoMemberText:
                case BlingoMemberField:
                    return movie.Sprite2DManager.Add(spriteNumWithChannel - movie.Sprite2DManager.SpriteNumChannelOffset, begin,end, c => c.SetMember(member));
                //case BlingoTempoSprite:
                case BlingoColorPaletteMember memberPalette:
                    {
                        var colorSprite = movie.ColorPalettes.Add(begin);
                        colorSprite.SetMember(memberPalette);
                        return colorSprite;
                    }
                case BlingoMemberScript blingoMemberScript:return movie.FrameScripts.Add(begin, c => c.SetMember(blingoMemberScript));
                case BlingoTransitionMember transitionMember:return movie.Transitions.Add(begin, transitionMember);
                case BlingoMemberSound memberSound:return movie.Audio.Add(spriteNumWithChannel - movie.Audio.SpriteNumChannelOffset, begin, memberSound);
            }

            return null;
        }


        #endregion
    }
}

