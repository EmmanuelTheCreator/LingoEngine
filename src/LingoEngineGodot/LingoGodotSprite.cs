using Godot;
using LingoEngine;
using LingoEngine.FrameworkCommunication;

namespace ArkGodot.GodotLinks
{

    public partial class LingoGodotSprite : Node2D, ILingoFrameworkSprite
    {
        private Node2D _node2D;
        private ILingoSprite _lingoSprite;
        public float X {get => _node2D.Position.X; set => _node2D.Position = new Vector2(value, _node2D.Position.Y); }
        public float Y { get => _node2D.Position.Y; set => _node2D.Position = new Vector2(_node2D.Position.X,Y); }
        public ILingoCast? Cast { get; private set; }
        public ILingoScore Score { get; }

#pragma warning disable CS8618
        public LingoGodotSprite(Node2D node2D, LingoSprite lingoSprite)
#pragma warning restore CS8618
        {
            _node2D = node2D;
            _lingoSprite = lingoSprite;
            lingoSprite.Init(this);
        }
      
        public override void _Ready()
        {

        }

        public override void _Process(double delta)
        {
            
        }

        public override void _Input(InputEvent @event)
        {
            //// Mouse in viewport coordinates.
            //if (@event is InputEventMouseButton eventMouseButton)
            //    MousePosition = eventMouseButton.Position;
            //else if (@event is InputEventMouseMotion eventMouseMotion)
            //    MousePosition = eventMouseMotion.Position;

        }

        public void SetPositionX(float x) => _node2D.Position = new Vector2(x, _node2D.Position.Y);
        public void SetPositionY(float y) => _node2D.Position = new Vector2(_node2D.Position.X, y);

        System.Numerics.Vector2 ILingoFrameworkSprite.GetGlobalMousePosition()
        {
            throw new NotImplementedException();
        }

      

        public float Blend
        {
            get => _node2D.SelfModulate.A;
            set => _node2D.SelfModulate = new Color(_node2D.SelfModulate, value);
        }

         public new string Name
        {
            get => _node2D.Name.ToString();
            set => _node2D.Name = value;
        }
      
    }

}
