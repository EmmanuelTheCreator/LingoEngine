using Godot;
using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.LGodot.Stages;
using LingoEngine.Stages;

namespace LingoEngine.LGodot.Movies
{
    public partial class LingoGodotStage : Node2D, ILingoFrameworkStage, IDisposable
    {
        private LingoStage _LingoStage;
        private readonly LingoClock _lingoClock;
        private readonly LingoDebugOverlay _overlay;
        private LingoPlayer? _player;
        private bool _f1Down;

        private LingoGodotMovie? _activeMovie;

        float ILingoFrameworkStage.Scale { get => base.Scale.X; set => base.Scale = new Vector2(value,value); }

        public LingoGodotStage(LingoPlayer lingoPlayer)
        {
            _lingoClock = (LingoClock)lingoPlayer.Clock;
            _overlay = new LingoDebugOverlay(new Core.LingoGodotDebugOverlay(this), lingoPlayer);
        }

        public override void _Ready()
        {
            base._Ready();
        }
        public override void _Process(double delta)
        {
            base._Process(delta);
            _lingoClock.Tick((float)delta);
            if (_player != null)
            {
                _overlay.Update((float)delta);
                bool f1 = _player.Key.KeyPressed((int)Key.F1);
                if (f1 && !_f1Down)
                    _overlay.Toggle();
                _f1Down = f1;
                _overlay.Render();
            }
        }
        internal void Init(LingoStage lingoInstance, LingoPlayer lingoPlayer)
        {
            _LingoStage = lingoInstance;
            _player = lingoPlayer;
        }


        internal void ShowMovie(LingoGodotMovie lingoGodotMovie)
        {
            var node = lingoGodotMovie.GetNode2D();
            // Avoid adding the same node multiple times which results in an error
            if (node.GetParent() != this)
            {
                AddChild(node);
            }
        }

        internal void HideMovie(LingoGodotMovie lingoGodotMovie)
        {
            var node = lingoGodotMovie.GetNode2D();
            if (node.GetParent() == this)
                RemoveChild(node);
        }

    public void SetActiveMovie(LingoMovie? lingoMovie)
        {
            if (_activeMovie != null)
                _activeMovie.Hide();
            if (lingoMovie == null) { 
                _activeMovie = null;
                return;
            }
            if (lingoMovie == null) return;
            var godotMovie = lingoMovie.Framework<LingoGodotMovie>();
            _activeMovie = godotMovie;
            godotMovie.Show();
        }

        internal void SetScale(float scale)
        {
            Scale = new Vector2(scale, scale);
        }
    }
}
