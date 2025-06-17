using Godot;
using LingoEngine.Director.LGodot.Casts;
using LingoEngine.Director.LGodot.Scores;
using LingoEngine.Director.Core.Events;
using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Director.LGodot.Movies
{
    public class LingoGodotPlayerControler : IHasStepFrameEvent, IDisposable
    {
        private readonly ILingoMovie _lingoMovie;
        private readonly Node2D _directorParent;
        //private DirGodotCastWindow _castViewer;
        private DirGodotScoreWindow _overlay;
        private readonly IDirectorEventMediator _mediator;

        public Node2D Container { get; set; } = new Node2D();
        //public Label LabelNode { get; set; } = new Label();

        public LingoGodotPlayerControler(Node2D parent, ILingoMovie lingoMovie, IDirectorEventMediator mediator)
        {
            _directorParent = new Node2D();
            parent.AddChild(_directorParent);
            parent.AddChild(Container);
            //Container.AddChild(LabelNode);
            Container.Position = new Vector2(800, 50);
            Container.ZIndex = 99999;
            _lingoMovie = lingoMovie;
            _mediator = mediator;
            _lingoMovie.ActorList.Add(this);
            //LabelNode.Text = "Frame 1";
            //var labelSettings = new LabelSettings
            //{
            //    Font = GD.Load<FontFile>($"res://Media\\Fonts\\Earth.ttf"),
            //    FontColor = new Color(1, 0, 0),
            //    FontSize = 10,
            //};
            //LabelNode.LabelSettings = labelSettings;
            //_castViewer = new DirGodotCastWindow(_directorParent, lingoMovie, mediator) { Visible = false };
            _overlay = new DirGodotScoreWindow(mediator) { Visible = false };
            _overlay.SetMovie((LingoMovie)lingoMovie);
            _directorParent.AddChild(_overlay);
        }

        public void StepFrame()
        {
            //LabelNode.Text = "Frame " + _lingoMovie.Frame;
        }

        public void Dispose()
        {
            _lingoMovie.ActorList.Remove(this);
            _overlay.Dispose();
            //_castViewer.Dispose();
        }
    }
}
