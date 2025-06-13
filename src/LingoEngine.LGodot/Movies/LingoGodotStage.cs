using Godot;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Inputs;

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

        public LingoGodotStage(Node rootNode, LingoClock lingoClock, LingoDebugOverlay overlay)
        {
            _lingoClock = lingoClock;
            _overlay = overlay;
            rootNode.AddChild(this);
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
                _overlay.Update((float)delta, _player);
                bool f1 = _player.Key.KeyPressed((int)Key.F1);
                if (_player.Key.ControlDown && f1 && !_f1Down)
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
            AddChild(lingoGodotMovie.GetNode2D());
        }
        internal void HideMovie(LingoGodotMovie lingoGodotMovie)
        {
            RemoveChild(lingoGodotMovie.GetNode2D());
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
    }
}
