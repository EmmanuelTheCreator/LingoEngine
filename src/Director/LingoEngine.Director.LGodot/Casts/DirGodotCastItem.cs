﻿using Godot;
using LingoEngine.LGodot.Pictures;
using LingoEngine.LGodot.Texts;
using LingoEngine.Members;
using LingoEngine.Pictures;
using LingoEngine.Texts;
using System.Text;
using System;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Director.LGodot.Gfx;

namespace LingoEngine.Director.LGodot.Casts
{
    internal partial class DirGodotCastItem : VBoxContainer
    {
        private readonly ColorRect _bg;
        private readonly ColorRect _selectionBg;
        private readonly Color _selectedColor;
        private readonly StyleBoxFlat _selectedLabelStyle = new();
        private readonly StyleBoxFlat _normalLabelStyle = new();
        //private readonly CenterContainer _spriteContainer;
        private readonly Sprite2D _Sprite2D;
        private readonly SubViewport _textViewport = new();
        private readonly ILingoMember _lingoMember;
        private readonly Action<DirGodotCastItem> _onSelect;
        private readonly ILingoCommandManager _commandManager;
        private readonly Label _caption;
        public int LabelHeight { get; set; } = 18;
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

            // Text viewport for text previews
            _textViewport.SetDisable3D(true);
            _textViewport.TransparentBg = true;
            _textViewport.SetUpdateMode(SubViewport.UpdateMode.Always);
            //_textViewport.MouseFilter = MouseFilterEnum.Ignore;
            AddChild(_textViewport);

            // Sprite centered
            _Sprite2D = new Sprite2D();
            //_spriteContainer = new CenterContainer();
            //_spriteContainer.AddChild(_Sprite2D);
            _Sprite2D.Position = new Vector2(+Width/2, LabelHeight-1);
            //_Sprite2D.MouseFilter = MouseFilterEnum.Ignore;
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
            _caption.AddThemeColorOverride("font_color", Colors.Black);
            // Apply background style to the label using the "normal" stylebox
            _caption.AddThemeStyleboxOverride("normal", _normalLabelStyle);
            _caption.MouseFilter = MouseFilterEnum.Ignore;
            
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
                        var preview = new ColorRect
                        {
                            Color = new Color(1f, 1f, 1f, 0.5f),
                            Size = CustomMinimumSize
                        };
                        SetDragPreview(preview);
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
                case ILingoMemberTextBase textMember:
                    var godotText = textMember.FrameworkObj;
                    godotText.Preload();

                    string prev = GetPreviewText(textMember);
                    var label = new Label
                    {
                        Text = prev,
                        LabelSettings = new LabelSettings { FontSize = 10,LineSpacing = 11, FontColor = Colors.Black },
                        AutowrapMode = TextServer.AutowrapMode.Word,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    _textViewport.SetSize(new Vector2I((int)(Width - 1), (int)(Height - LabelHeight)));
                    label.Size = new Vector2(Width - 1, Height - LabelHeight);
                    _textViewport.AddChild(label);
                    _Sprite2D.Texture = _textViewport.GetTexture();
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
        public override Variant _GetDragData(Vector2 atPosition)
        {
            var preview = new ColorRect
            {
                Color = new Color(1f, 1f, 1f, 0.5f),
                Size = CustomMinimumSize
            };
            SetDragPreview(preview);
            return Variant.From(_lingoMember);
        }



        private static string GetPreviewText(ILingoMemberTextBase text)
        {
            var lines = text.Text.Replace("\r", "").Split('\n');
            var sb = new StringBuilder();
            int count = Math.Min(4, lines.Length);
            for (int i = 0; i < count; i++)
            {
                var line = lines[i];
                if (line.Length > 14)
                    line = line.Substring(0, 14);
                sb.Append(line);
                if (i < count - 1)
                    sb.Append('\n');
            }
            return sb.ToString();
        }

    }
}
