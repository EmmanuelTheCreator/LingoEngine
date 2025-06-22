using Godot;
using LingoEngine.LGodot.Pictures;
using LingoEngine.LGodot.Texts;
using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Texts;
using System.Text;
using System;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Director.LGodot.Gfx;
using LingoEngine.Director.Core.Commands;
using LingoEngine.Director.LGodot.Helpers;
using LingoEngine.Director.Core.Inputs;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastItem : Control
    {
        private readonly ColorRect _bg;
        private readonly ColorRect _selectionBg;
        private readonly Color _selectedColor;
        private readonly StyleBoxFlat _selectedLabelStyle = new();
        private readonly StyleBoxFlat _normalLabelStyle = new();
        //private readonly CenterContainer _spriteContainer;
        private readonly DirGodotMemberThumbnail _thumb;
        private readonly ILingoMember _lingoMember;
        private readonly ColorRect _separator;
        private readonly Action<DirGodotCastItem> _onSelect;
        private readonly ILingoCommandManager _commandManager;
        private readonly Label _caption;
        private Control? _dragHelper;

        public int LabelHeight { get; set; } = 15;
        public int Width { get; set; } = 50;
        public int Height { get; set; } = 50;
        public ILingoMember LingoMember => _lingoMember;
        public void SetSelected(bool selected)
        {
            _selectionBg.Visible = selected;
            _bg.Color = selected ? _selectedColor : Colors.DimGray;
            // Labels use the "normal" stylebox for their background, not "panel"
            _caption.AddThemeStyleboxOverride("normal", selected ? _selectedLabelStyle : _normalLabelStyle);
        }
        public DirGodotCastItem(ILingoMember element, int number, Action<DirGodotCastItem> onSelect, Color selectedColor, ILingoCommandManager commandManager)
        {
            _lingoMember = element;
            _onSelect = onSelect;
            _commandManager = commandManager;
            _selectedColor = selectedColor;
            CustomMinimumSize = new Vector2(50, 50);
            MouseFilter = MouseFilterEnum.Stop;
            _selectedLabelStyle.BgColor = selectedColor;
            _normalLabelStyle.BgColor = Colors.DimGray;
            

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
            _selectionBg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(_selectionBg);

            // Solid background
            _bg = new ColorRect { Color = Colors.DimGray };
            _bg.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _bg.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _bg.AnchorLeft = 0;
            _bg.AnchorTop = 0;
            _bg.AnchorRight = 1;
            _bg.AnchorBottom = 1;
            _bg.OffsetLeft = 0;
            _bg.OffsetTop = 0;
            _bg.OffsetRight = 0;
            _bg.OffsetBottom = 0;
            _bg.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(_bg);


            _thumb = new DirGodotMemberThumbnail(Width - 1, Height - LabelHeight);
            AddChild(_thumb);

            // separator line above the caption
            _separator = new ColorRect
            {
                Color = Colors.DarkGray,
                CustomMinimumSize = new Vector2(Width, 1),
                MouseFilter = MouseFilterEnum.Ignore
            };
            AddChild(_separator);
            _separator.Position = new Vector2(0, Height - LabelHeight - 1);

            // Bottom label
            _caption = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _caption.LabelSettings = new LabelSettings
            {
                FontSize = 8,
                FontColor = Colors.Black,
            };
            AddChild(_caption);
            _caption.Text = !string.IsNullOrWhiteSpace(element.Name) ? element.NumberInCast + "." + element.Name : number.ToString();
            _caption.AddThemeColorOverride("font_color", Colors.Black);
            // Apply background style to the label using the "normal" stylebox
            _caption.AddThemeStyleboxOverride("normal", _normalLabelStyle);
            _caption.MouseFilter = MouseFilterEnum.Ignore;

            _caption.AnchorLeft = 0;
            _caption.AnchorRight = 1;
            _caption.AnchorTop = 1;
            _caption.AnchorBottom = 1;
            _caption.OffsetLeft = 0;
            _caption.OffsetRight = 0;
            _caption.OffsetBottom = 0;
            _caption.OffsetTop = -LabelHeight;

            
        }
        public void SetPosition(int x, int y)
        {
            Position = new Vector2(x, y);
        }
        private bool _wasClicked;
        private static bool _openingEditor;
        private static object _lock = new object();
        private bool _dragging;
        private Vector2 _dragStart;
        public override void _Input(InputEvent @event)
        {
            if (!IsVisibleInTree()) return;

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2 mousePos = GetGlobalMousePosition();

                Rect2 bounds = new Rect2(GlobalPosition, CustomMinimumSize);

                if (mouseEvent.Pressed && mouseEvent.DoubleClick && !_openingEditor && bounds.HasPoint(mousePos))
                {
                    
                    OpenEditor();
                    _onSelect(this);
                    return;
                }
                else if (_openingEditor && !mouseEvent.Pressed)
                {
                    _openingEditor = false;
                }


                if (!_wasClicked && mouseEvent.Pressed)
                {
                    if (bounds.HasPoint(mousePos))
                    {
                        _onSelect(this);
                        _wasClicked = true;
                    }
                    return;
                }
                else if (_wasClicked && !mouseEvent.Pressed)
                {
                    //_onSelect(this);
                    _wasClicked = false;

                }
            }
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (!IsVisibleInTree()) return;

            if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    _dragStart = mb.Position;
                    _dragging = false;
                }
                else
                {
                    _dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion motion)
            {
                if (Input.IsMouseButtonPressed(MouseButton.Left) && !_dragging)
                {
                    if (motion.Position.DistanceSquaredTo(_dragStart) > 16)
                    {
                        _dragging = true;
                        DirDragDropHolder.StartDrag(_lingoMember, "CastItem");
                        //AcceptEvent(); // Prevent default handling

                        //var preview = new ColorRect
                        //{
                        //    Color = new Color(1f, 1f, 1f, 0.5f),
                        //    Size = CustomMinimumSize
                        //};
                        //// Call start_drag with your data and preview
                        //this.StartDragWorkaround(Variant.From(_lingoMember), preview);
                    }
                }
            }
        }



        private void OpenEditor()
        {
            lock (_lock)
            {
                if (_openingEditor) return;
                _openingEditor = true;
            }
            string? windowCode = _lingoMember switch
            {
                ILingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                LingoMemberPicture => DirectorMenuCodes.PictureEditWindow,
                _ => null
            };

            if (windowCode != null)
                _commandManager.Handle(new OpenWindowCommand(windowCode));
        }

        public void Init()
        {
            _thumb.SetMember(_lingoMember);
        }


        //public override Variant _GetDragData(Vector2 atPosition)
        //{
        //    GD.Print($"CastMemberItem: _GetDragData called at {atPosition} with {_lingoMember.Name}");
        //    var preview = new ColorRect
        //    {
        //        Color = new Color(1f, 1f, 1f, 0.5f),
        //        Size = CustomMinimumSize
        //    };
        //    SetDragPreview(preview);
        //    return Variant.From(_lingoMember);
        //}

        public override Variant _GetDragData(Vector2 atPosition)
        {
            GD.Print("CastItem: drag triggered at " + atPosition);
            var label = new Label { Text = "Dragging " + _lingoMember.Name };
            label.CustomMinimumSize = new Vector2(100, 30);
            label.Modulate = new Color(1, 1, 0, 0.6f);
            SetDragPreview(label);
            return Variant.From(_lingoMember);
        }


    }
}
