using Godot;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Movies
{
    public class LingoGodotPlayerControler : IHasStepFrameEvent, IDisposable
    {
        private readonly ILingoMovie _lingoMovie;
        private DirGodotCastWindow _castViewer;
        private DirGodotScoreWindow _overlay;

        public Node2D Container { get; set; } = new Node2D();
        //public Label LabelNode { get; set; } = new Label();

        public LingoGodotPlayerControler(Node2D parent, ILingoMovie lingoMovie)
        {
            parent.AddChild(Container);
            //Container.AddChild(LabelNode);
            Container.Position = new Vector2(800, 50);
            Container.ZIndex = 99999;
            _lingoMovie = lingoMovie;
            _lingoMovie.ActorList.Add(this);
            //LabelNode.Text = "Frame 1";
            //var labelSettings = new LabelSettings
            //{
            //    Font = GD.Load<FontFile>($"res://Media\\Fonts\\Earth.ttf"),
            //    FontColor = new Color(1, 0, 0),
            //    FontSize = 10,
            //};
            //LabelNode.LabelSettings = labelSettings;
            _castViewer = new DirGodotCastWindow(parent, lingoMovie) { Visible = false };
            _overlay = new DirGodotScoreWindow { Visible = false };
            _overlay.SetMovie((LingoMovie)lingoMovie);
            parent.AddChild(_overlay);
        }

        public void StepFrame()
        {
            //LabelNode.Text = "Frame " + _lingoMovie.Frame;
        }

        public void Dispose()
        {
            _lingoMovie.ActorList.Remove(this);
            _overlay.Dispose();
            _castViewer.Dispose();
        }
    }
}
