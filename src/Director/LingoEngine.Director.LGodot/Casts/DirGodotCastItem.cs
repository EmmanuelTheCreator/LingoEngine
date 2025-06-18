using Godot;
using LingoEngine.Core;
using LingoEngine.LGodot.Pictures;
using LingoEngine.Pictures;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastItem : VBoxContainer
    {
        private readonly ColorRect _bg;
        private readonly ColorRect _selectionBg;
        //private readonly CenterContainer _spriteContainer;
        private readonly Sprite2D _Sprite2D;
        private readonly ILingoMember _lingoMember;
        private readonly Action<DirGodotCastItem> _onSelect;
        private readonly Label _caption;
        public int LabelHeight { get; set; } = 18;
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;
        public ILingoMember LingoMember => _lingoMember;
        public void SetSelected(bool selected)
        {
            _selectionBg.Visible = selected;
        }
        public DirGodotCastItem(ILingoMember element, int number, Action<DirGodotCastItem> onSelect, Color selectedColor)
        {
            _lingoMember = element;
            _onSelect = onSelect;
            CustomMinimumSize = new Vector2(50, 50);

            // Selection background - slightly larger than the item itself
            _selectionBg = new ColorRect { Color = selectedColor, Visible = false };
            _selectionBg.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _selectionBg.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _selectionBg.AnchorLeft = 0;
            _selectionBg.AnchorTop = 0;
            _selectionBg.AnchorRight = 1;
            _selectionBg.AnchorBottom = 1;
            _selectionBg.OffsetLeft = -1;
            _selectionBg.OffsetTop = -1;
            _selectionBg.OffsetRight = 1;
            _selectionBg.OffsetBottom = 1;
            AddChild(_selectionBg);

            // Solid background
            _bg = new ColorRect { Color = Colors.DimGray };
            _bg.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _bg.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            AddChild(_bg);

            // Sprite centered
            _Sprite2D = new Sprite2D();
            //_spriteContainer = new CenterContainer();
            //_spriteContainer.AddChild(_Sprite2D);
            _Sprite2D.Position = new Vector2(+Width/2, LabelHeight-1);
            AddChild(_Sprite2D);


            // Bottom label
            _caption = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _caption.LabelSettings = new LabelSettings
            {
                FontSize = 8,
            };
            AddChild(_caption);
            _caption.Text = !string.IsNullOrWhiteSpace(element.Name)? element.NumberInCast+"."+ element.Name: number.ToString();
            
        }
        public void SetPosition(int x, int y)
        {
            Position = new Vector2(x, y);
        }
        private bool wasClicked;
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (!wasClicked && mouseEvent.Pressed)
                {
                    Vector2 mousePos = GetGlobalMousePosition();

                    Rect2 bounds = new Rect2(GlobalPosition - CustomMinimumSize * 0.5f, CustomMinimumSize);

                    if (bounds.HasPoint(mousePos))
                    {
                        _onSelect(this);
                        wasClicked = true;
                    }
                    return;
                } 
                else if (wasClicked && !mouseEvent.Pressed)
                {
                    _onSelect(this);
                    wasClicked = false;
                }
            }
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

    }
}
