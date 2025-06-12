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
            _castsViewerNode2D.Position = new Vector2(800, 20);
            parent.AddChild(_castsViewerNode2D);
            _lingoMovie = lingoMovie;
            
            CastLibViewer = new DirGodotCastView(parent);
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
                    y += dirCastItem.Height;
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
        private readonly CenterContainer _Container2D;
        private readonly Sprite2D _Sprite2D;
        private readonly ILingoMember _lingoMember;
        public Node Node => _Container2D;
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;
        public DirGodotCastItem(ILingoMember element)
        {
            _Sprite2D = new Sprite2D();
            _Container2D = new CenterContainer();
            _Container2D.AddChild(_Sprite2D);
            _lingoMember = element;
            Resize(Width, Height- 18);
        }
        public void SetPosition(int x, int y)
        {
            _Container2D.Position = new Vector2(x, y);
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
            _Container2D.Dispose();
        }
    }
}
