using Godot;
using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Primitives;
using LingoEngineGodot.Pictures;

namespace LingoEngineGodot
{

    public partial class LingoGodotSprite : ILingoFrameworkSprite, IDisposable
    {
        private readonly CenterContainer _Container2D;
        private readonly Node2D _parentNode2D;
        private readonly Sprite2D _Sprite2D;
        private readonly Action<LingoGodotSprite> _showMethod;
        private readonly Action<LingoGodotSprite> _removeMethod;
        private readonly Action<LingoGodotSprite> _hideMethod;
        private readonly LingoSprite _lingoSprite;
        private bool _wasShown;

        internal LingoSprite LingoSprite => _lingoSprite;
        internal bool IsDirty { get; set; } = true;
        internal bool IsDirtyMember { get; set; } = true;
        public float X { get => _Sprite2D.Position.X; set { _Sprite2D.Position = new Vector2(value, _Sprite2D.Position.Y);  } }
        public float Y { get => _Sprite2D.Position.Y; set { _Sprite2D.Position = new Vector2(_Sprite2D.Position.X, value); } }
        public int ZIndex { get => _Sprite2D.ZIndex; set { _Sprite2D.ZIndex = value; } }
        public LingoPoint RegPoint { get => (_Container2D.Position.X, _Container2D.Position.Y); set { _Container2D.Position = new Vector2(value.X, value.Y); IsDirty = true; } }

        public bool Visibility { get => _Container2D.Visible; set => _Container2D.Visible = value; }
        public ILingoCast? Cast { get; private set; }

        public float Blend
        {
            get => _Sprite2D.SelfModulate.A;
            set
            {
                _Sprite2D.SelfModulate = new Color(_Sprite2D.SelfModulate, value);
                IsDirty = true;
            }
        }
        private string _name;
        public string Name
        {
            get => _name.ToString();
            set
            {
                _name = value;
                _Container2D.Name = value;
                _Sprite2D.Name = value + "_Sprite";
            }
        }
        public float Width { get; private set; }
        public float Height { get; private set; }
        private float _DesiredWidth;
        private float _DesiredHeight;
        public float SetDesiredHeight { get => _DesiredWidth; set { _DesiredWidth = value; IsDirty = true; } }
        public float SetDesiredWidth { get => _DesiredHeight; set { _DesiredHeight = value; IsDirty = true; } }


#pragma warning disable CS8618
        public LingoGodotSprite(LingoSprite lingoSprite, Node2D parentNode, Action<LingoGodotSprite> showMethod, Action<LingoGodotSprite> hideMethod, Action<LingoGodotSprite> removeMethod)
#pragma warning restore CS8618
        {
            _parentNode2D = parentNode;
            _lingoSprite = lingoSprite;
            _showMethod = showMethod;
            _hideMethod = hideMethod;
            _removeMethod = removeMethod;
            _Sprite2D = new Sprite2D();
            _Container2D = new CenterContainer();
            _Container2D.AddChild(_Sprite2D);
            lingoSprite.Init(this);
            ZIndex = lingoSprite.SpriteNum;
        }

        public void RemoveMe()
        {
            _removeMethod(this);
            Dispose();
        }
        public void Dispose()
        {
            _Container2D.GetParent().RemoveChild(_Container2D);
            _Sprite2D.Dispose();
            _Container2D.Dispose();
        }

        public void Show()
        {
            if (!_wasShown)
            {
                _wasShown = true;
                _parentNode2D.AddChild(_Container2D);
                _showMethod(this);
            }
            Update();
        }
        public void Hide()
        {
            if (!_wasShown)
                return;
            _wasShown = false;
            _hideMethod(this);
            _Container2D.GetParent().RemoveChild(_Container2D);
        }

        public void SetPosition(LingoPoint lingoPoint)
        {
            _Sprite2D.Position = new Vector2(lingoPoint.X, lingoPoint.Y);
            //IsDirty = true;
        }

        public void MemberChanged()
        {
            IsDirtyMember = true;
        }
        private void UpdateSizeFromTexture()
        {
            if (_Sprite2D.Texture == null)
            {
                Width = 0;
                Height = 0;
                return;
            }
            Width = _Sprite2D.Texture.GetWidth();
            Height = _Sprite2D.Texture.GetWidth();
        }
        internal void Update()
        {
            if (IsDirtyMember)
                UpdateMember();

            if (IsDirty)
            {
                // update complex properties
                if (_Sprite2D.Texture != null)
                {
                    if (_DesiredWidth != Width || _DesiredHeight != Height)
                    {
                        UpdateSizeFromTexture();
                        _DesiredWidth = Width;
                        _DesiredHeight = Height;
                    }
                }
                IsDirty = false;
            }
        }


        private void UpdateMember()
        {
            if (!IsDirtyMember) return;
            IsDirtyMember = false;

            // Only handle picture members
            if (!(_lingoSprite.Member is LingoMemberPicture pictureMember)) return;
            UpdateMemberPicture(pictureMember.Framework<LingoGodotMemberPicture>());
            UpdateSizeFromTexture();
            IsDirty = true;
        }

        private void UpdateMemberPicture(LingoGodotMemberPicture godotPicture)
        {
            godotPicture.Preload();

            // Set the texture using the ImageTexture from the picture member
            if (godotPicture.Texture == null)
                return;
            _Sprite2D.Texture = godotPicture.Texture;
        }

        public void Resize(float targetWidth, float targetHeight)
        {
            float scaleFactorW = targetWidth / _Sprite2D.Texture.GetWidth();
            float scaleFactorH = targetHeight / _Sprite2D.Texture.GetHeight();
            _Sprite2D.Scale = new Vector2(scaleFactorW, scaleFactorH);
        }
    }
}
