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
        internal bool IsDirty { get; set; }
        internal bool IsDirtyMember { get; set; } = true;
        public float X { get => _Sprite2D.Position.X; set { _Sprite2D.Position = new Vector2(value, _Sprite2D.Position.Y); IsDirty = true; } }
        public float Y { get => _Sprite2D.Position.Y; set { _Sprite2D.Position = new Vector2(_Sprite2D.Position.X, value); IsDirty = true; } }
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

        internal void Update()
        {
            if (IsDirty)
            {
                // update complex properties
                IsDirty = false;
            }
            if (IsDirtyMember)
                UpdateMember();

        }


        private void UpdateMember()
        {
            if (!IsDirtyMember) return;
            IsDirtyMember = false;

            // Only handle picture members
            if (!(_lingoSprite.Member is LingoMemberPicture pictureMember)) return;
            UpdateMemberPicture(pictureMember.Framework<LingoGodotMemberPicture>());
        }

        private void UpdateMemberPicture(LingoGodotMemberPicture godotPicture)
        {
            godotPicture.Preload();

            // Set the texture using the ImageTexture from the picture member
            if (godotPicture.Texture == null)
                return;
            _Sprite2D.Texture = godotPicture.Texture;
        }


    }
}
