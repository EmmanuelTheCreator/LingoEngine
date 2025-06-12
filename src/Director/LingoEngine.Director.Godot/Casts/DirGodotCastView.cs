using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngineGodot.Pictures;

namespace LingoEngine.Director.Godot.Casts
{
    internal class DirGodotCastViewer : IDisposable
    {
        private readonly Node2D _castsViewerNode2D;
        private readonly ILingoMovie _lingoMovie;

        public ILingoCast? ActiveCastLib { get; private set; }
        public DirGodotCastView CastLibViewer { get; }

        public DirGodotCastViewer(Node2D parent,ILingoMovie lingoMovie)
        {
            _castsViewerNode2D = new Node2D();
            _castsViewerNode2D.Position = new Vector2(645, 20);
            parent.AddChild(_castsViewerNode2D);
            _lingoMovie = lingoMovie;
            
            CastLibViewer = new DirGodotCastView(_castsViewerNode2D);
            Activate(2);
        }
        public void Activate(int castlibNum)
        {
            ActiveCastLib = _lingoMovie.CastLib.GetCast(castlibNum);
            if (ActiveCastLib != null)
                CastLibViewer.Show(ActiveCastLib);
        }

        public void Dispose()
        {
            CastLibViewer.Hide();
            _castsViewerNode2D.Dispose();
        }
    }
    internal class DirGodotCastView : IDirFrameworkCast
    {
        
        private readonly List<DirGodotCastItem> _elements = new List<DirGodotCastItem>();
        private readonly Node _parent;

        public DirGodotCastView(Node parent)
        {
            _parent = parent;
        }

        public void Show(ILingoCast cast)
        {
            Hide();
            var x = 0;
            var y = 0;
            var i = 0;
            foreach (var castItem in cast.GetAll())
            {
                var dirCastItem = new DirGodotCastItem(castItem);
                dirCastItem.Init();
                _elements.Add(dirCastItem);
                _parent.AddChild(dirCastItem.Node);
                if ((i - 20 + 1) % 12 == 0)
                {
                    y += dirCastItem.Height+5;
                    x = 0;
                }
                x += dirCastItem.Width;
                dirCastItem.SetPosition(x, y);
                i++;
            }
        }
        public void Hide()
        {
            foreach (var element in _elements)
            {
                _parent.RemoveChild(element.Node);
                element.Dispose();
            }
            _elements.Clear();
        }
       

    }
    internal class DirGodotCastItem : IDisposable
    {
        private readonly VBoxContainer _root;      // holds everything
        private readonly ColorRect _bg;
        private readonly CenterContainer _spriteContainer;
        private readonly Sprite2D _Sprite2D;
        private readonly ILingoMember _lingoMember;
        private readonly Label _caption;
        public int LabelHeight { get; set; } = 18;
        public Node Node => _root;
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;
        public DirGodotCastItem(ILingoMember element)
        {
            _lingoMember = element;
            _root = new VBoxContainer { CustomMinimumSize = new Vector2(50, 50) };

            // Solid background
            _bg = new ColorRect { Color = Colors.DimGray };
            _bg.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _bg.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _root.AddChild(_bg);

            // Sprite centered
            _Sprite2D = new Sprite2D();
            _spriteContainer = new CenterContainer();
            _spriteContainer.AddChild(_Sprite2D);
            _spriteContainer.Position = new Vector2(0, LabelHeight);
            _root.AddChild(_spriteContainer);

            // Bottom label
            _caption = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _caption.LabelSettings = new LabelSettings
            {
                FontSize = 8,
            };
            _root.AddChild(_caption);
            _caption.Text = element.Name;

            
            
        }
        public void SetPosition(int x, int y)
        {
            _root.Position = new Vector2(x, y);
        }
      

        public void Init()
        {
            switch (_lingoMember)
            {
                case LingoMemberPicture pic:
                    var godotPicture = pic.Framework<LingoGodotMemberPicture>();
                    godotPicture.Preload();

                    // Set the texture using the ImageTexture from the picture member
                    if (godotPicture.Texture == null)
                        return;
                    _Sprite2D.Texture = godotPicture.Texture;
                    Resize(Width - 1, Height - LabelHeight);
                    break;
                default:
                    break;
            }
        }

        public void Resize(float targetWidth, float targetHeight)
        {
            if (_Sprite2D.Texture == null) return;
            var width = _Sprite2D.Texture.GetWidth();
            var height = _Sprite2D.Texture.GetHeight();
            float scaleFactorW = targetWidth / width;
            float scaleFactorH = targetHeight / _Sprite2D.Texture.GetHeight();
            _Sprite2D.Scale = new Vector2(scaleFactorW, scaleFactorH);
        }

        public void Dispose()
        {
            _Sprite2D.Dispose();
            _root.Dispose();
        }
    }
}
