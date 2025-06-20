using Godot;
using LingoEngine.Casts;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Pictures;
using LingoEngine.LGodot.Texts;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngine.LGodot
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
        private float _x;
        private float _y;
        public float X { get => _x; set { _x = value; IsDirty = true; } }
        public float Y { get => _y; set { _y = value; IsDirty = true; } }
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
                UpdateSprite2DName();
            }
        }

        private void UpdateSprite2DName()
        {
            var fullName = _lingoSprite.GetFullName();
            _Sprite2D.Name = fullName + ".sprite";
            _Container2D.Name = fullName;
        }

        public float Width { get; private set; }
        public float Height { get; private set; }
        private float _DesiredWidth;
        private float _DesiredHeight;
        public float SetDesiredWidth { get => _DesiredWidth; set { _DesiredWidth = value; IsDirty = true; } }
        public float SetDesiredHeight { get => _DesiredHeight; set { _DesiredHeight = value; IsDirty = true; } }

        public float Rotation {
            get => Mathf.RadToDeg(_Sprite2D.Rotation);
            set => _Sprite2D.Rotation = Mathf.DegToRad(value);
        }
        public float Skew { get; set; }


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
            _x = lingoPoint.X;
            _y = lingoPoint.Y;
            IsDirty = true;
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
            Height = _Sprite2D.Texture.GetHeight();
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
                        Width = _DesiredWidth;
                        Height = _DesiredHeight;
                        Resize(_DesiredWidth, _DesiredHeight);
                    }
                }
                IsDirty = false;
            }
            var offset = GetRegPointOffset();
            _Sprite2D.Position = new Vector2(_x + offset.X, _y + offset.Y);
        }


        private void UpdateMember()
        {
            if (!IsDirtyMember) return;
            IsDirtyMember = false;


            switch (_lingoSprite.Member)
            {
                case LingoMemberPicture pictureMember:
                    UpdateMemberPicture(pictureMember.Framework<LingoGodotMemberPicture>());
                    UpdateSizeFromTexture();
                    if (_DesiredWidth == 0) _DesiredWidth = Width;
                    if (_DesiredHeight == 0) _DesiredHeight = Height;
                    IsDirty = true;
                    return;
                case LingoMemberText textMember:
                    UpdateMemberText(textMember.Framework<LingoGodotMemberText>());
                    break;
                case LingoMemberField fieldMember:
                    UpdateMemberField(fieldMember.Framework<LingoGodotMemberField>());
                    break;
            }
            UpdateSprite2DName();
        }

        private void UpdateMemberPicture(LingoGodotMemberPicture godotPicture)
        {
            godotPicture.Preload();

            // Set the texture using the ImageTexture from the picture member
            if (godotPicture.Texture == null)
                return;
            _Sprite2D.Texture = godotPicture.Texture;
        }
        private Node? _previousElement;
        private void UpdateMemberText(LingoGodotMemberText godotElement)
        {
            if (_previousElement == godotElement.Node2D) return;
            godotElement.Preload();
            _Sprite2D.AddChild(godotElement.Node2D);
            _previousElement = godotElement.Node2D;
        }
        private void UpdateMemberField(LingoGodotMemberField godotElement)
        {
            if (_previousElement == godotElement.Node2D) return;
            godotElement.Preload();
            _Sprite2D.AddChild(godotElement.Node2D);
            _previousElement = godotElement.Node2D;
        }

        public void Resize(float targetWidth, float targetHeight)
        {
            var width = _Sprite2D.Texture.GetWidth();
            var height = _Sprite2D.Texture.GetHeight();
            float scaleFactorW = targetWidth / width;
            float scaleFactorH = targetHeight / _Sprite2D.Texture.GetHeight();
            _Sprite2D.Scale = new Vector2(scaleFactorW, scaleFactorH);
        }

        private LingoPoint GetRegPointOffset()
        {
            if (_lingoSprite.Member is { } member)
            {
                var baseOffset = member.CenterOffsetFromRegPoint();
                if (member.Width != 0 && member.Height != 0)
                {
                    float scaleX = _Sprite2D.Scale.X;
                    float scaleY = _Sprite2D.Scale.Y;
                    return new LingoPoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
                }
                return baseOffset;
            }
            return new LingoPoint();
        }
    }
}
