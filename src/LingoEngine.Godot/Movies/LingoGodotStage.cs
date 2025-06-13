using Godot;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;

namespace LingoEngine.Godot.Movies
{
    public partial class LingoGodotStage : Node2D, ILingoFrameworkStage, IDisposable
    {
        private LingoStage _LingoStage;
        private readonly LingoClock _lingoClock;

        private LingoGodotMovie? _activeMovie;

        public LingoGodotStage(Node rootNode, LingoClock lingoClock)
        {
            _lingoClock = lingoClock;
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
        }
        internal void Init(LingoStage lingoInstance)
        {
            _LingoStage = lingoInstance;
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
