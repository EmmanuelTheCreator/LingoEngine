using Godot;
using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngineGodot.Movies
{
    public class LingoGodotPlayerControler : IHasStepFrameEvent, IDisposable
    {
        private readonly ILingoMovie _lingoMovie;

        public Node2D Container { get; set; } = new Node2D();
        public Label LabelNode { get; set; } = new Label();

        public LingoGodotPlayerControler(Node2D parent, ILingoMovie lingoMovie)
        {
            parent.AddChild(Container);
            Container.AddChild(LabelNode);

            Container.ZIndex = 99999;
            _lingoMovie = lingoMovie;
            _lingoMovie.ActorList.Add(this);
            LabelNode.Text = "Frame 1";
        }

        public void StepFrame()
        {
            LabelNode.Text = "Frame "+_lingoMovie.Frame;
        }

        public void Dispose()
        {
            _lingoMovie.ActorList.Remove(this);
        }
    }
}
