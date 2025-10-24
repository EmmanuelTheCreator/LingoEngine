using AbstUI.Commands;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Stages.Commands;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Director.Core.Windowing;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Movies;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;

namespace BlingoEngine.Director.Core.Stages
{
    /// <summary>
    /// Legacy implementation kept for reference while the V2 window is validated.
    /// </summary>
    public class DirectorStageWindowLegacy : DirectorWindow<IDirFrameworkStageWindow>,
        IAbstCommandHandler<StageToolSelectCommand>,
        IAbstCommandHandler<MoveSpritesCommand>,
        IAbstCommandHandler<RotateSpritesCommand>
    {
        private readonly IBlingoPlayer _player;
        private readonly IHistoryManager _historyManager;
        private readonly IAbstMouseSubscription _mouseMoveSub;
        private bool _lastInside = false;
        private AMouseCursor _lastMouseCursor = AMouseCursor.Arrow;
        private IBlingoMovie? _currentMovie;

        public StageTool SelectedTool { get; private set; }

        public StageIconBar IconBar { get; }

        public DirectorStageWindowLegacy(IServiceProvider serviceProvider, IHistoryManager historyManager, IBlingoFrameworkFactory factory, IAbstCommandManager commandManager, IBlingoPlayer player, IDirectorEventMediator mediator, IDirStageManager stageManager) : base(serviceProvider, DirectorMenuCodes.StageWindow)
        {
            _player = player;
            _historyManager = historyManager;
            IconBar = new StageIconBar(factory, commandManager, player, mediator, stageManager);
            MinimumWidth = 200;
            MinimumHeight = 150;
            Width = 650;
            Height = 520;
            X = 70;
            Y = 22;

            _mouseMoveSub = MouseT.OnMouseMove(OnMouseMove);
            player.ActiveMovieChanged += Player_ActiveMovieChanged;
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            Title = "Stage";
        }

        private void Player_ActiveMovieChanged(IBlingoMovie? obj)
        {
             _currentMovie = obj;
        }

        protected override void OnDispose()
        {
            _player.ActiveMovieChanged -= Player_ActiveMovieChanged;
            _mouseMoveSub.Release();
            base.OnDispose();
        }

      

        private void UpdateSelectionBox() => Framework.UpdateSelectionBox();
        private void UpdateBoundingBoxes() => Framework.UpdateBoundingBoxes();

        public bool CanExecute(StageToolSelectCommand command) => true;
        public bool Handle(StageToolSelectCommand command)
        {
            SelectedTool = command.Tool;
            return true;
        }

        public bool CanExecute(MoveSpritesCommand command) => true;
        public bool Handle(MoveSpritesCommand command)
        {
            foreach (var kv in command.EndPositions)
            {
                kv.Key.LocH = kv.Value.X;
                kv.Key.LocV = kv.Value.Y;
            }
            _historyManager.Push(command.ToUndo(UpdateSelectionBox), command.ToRedo(UpdateSelectionBox));
            UpdateSelectionBox();
            UpdateBoundingBoxes();
            return true;
        }

        public bool CanExecute(RotateSpritesCommand command) => true;
        public bool Handle(RotateSpritesCommand command)
        {
            foreach (var kv in command.EndRotations)
                kv.Key.Rotation = kv.Value;
            _historyManager.Push(command.ToUndo(UpdateSelectionBox), command.ToRedo(UpdateSelectionBox));
            UpdateSelectionBox();
            UpdateBoundingBoxes();
            return true;
        }

      

        private void OnMouseMove(AbstMouseEvent e)
        {
            var windowRect = ARect.New(20, 20, Width-40, Height-40);
            var isInside = windowRect.Contains(new APoint(e.MouseH, e.MouseV));
            if (_lastInside == isInside) return;
            _lastInside = isInside;
            if (_currentMovie != null && !_currentMovie.IsPlaying)
            {
                MouseT.SetCursor(AMouseCursor.Arrow);
                return;
            }
            //Console.WriteLine($"Swap Isinside {isInside}");
            var cursor = MouseT.GetCursor();
            if (!isInside)
            {
                _lastMouseCursor = cursor;
                MouseT.SetCursor(AMouseCursor.Arrow);
            }
            else
            {
                // inside
                MouseT.SetCursor(_lastMouseCursor);
            }
        }
    }
}

