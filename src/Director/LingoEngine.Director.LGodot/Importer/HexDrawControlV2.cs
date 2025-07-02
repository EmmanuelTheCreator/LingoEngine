using Godot;
using ProjectorRays.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LingoEngine.Director.LGodot.Gfx
{
    internal partial class HexDrawControlV2 : Control
    {
        private readonly byte[] _data;
        private readonly RayStreamAnnotatorDecorator _annotator;
        private readonly Dictionary<int, Color> _blockColors = new();
        private readonly Dictionary<int, int> _blockIndexByOffset = new();

        private readonly Label _tooltip = new();
        public event Action<int>? ByteClicked;

        private const int BytesPerRow = 32;
        private const int ByteSpacing = 24;
        private const int GroupGap = 8;
        private const int RowHeight = 22;
        private const int LeftMargin = 10;
        private const int AsciiOffsetX = 900;

        public HexDrawControlV2(byte[] data, RayStreamAnnotatorDecorator annotator)
        {
            _data = data;
            _annotator = annotator;

            int colorIndex = 0;
            long baseOffset = annotator.StreamOffsetBase;
            for (int idx = 0; idx < annotator.Annotations.Count; idx++)
            {
                var ann = annotator.Annotations[idx];
                var hue = (colorIndex * 0.1f) % 1f;
                var color = Color.FromHsv(hue, 0.3f, 1f);
                _blockColors[idx] = color;
                int start = (int)(ann.Address - baseOffset);
                for (int i = 0; i < ann.Length; i++)
                {
                    int off = start + i;
                    if (off >= 0 && off < data.Length)
                        _blockIndexByOffset[off] = idx;
                }
                colorIndex++;
            }

            int totalRows = (int)Math.Ceiling(data.Length / (float)BytesPerRow);
            CustomMinimumSize = new Vector2(AsciiOffsetX + 300, totalRows * RowHeight);
            Size = CustomMinimumSize;
            MouseFilter = MouseFilterEnum.Pass;
        }

        public override void _Ready()
        {
            _tooltip.Visible = false;
            _tooltip.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(1f, 1f, 0.8f), BorderWidthAll = 1, BorderColor = Colors.Black });
            AddChild(_tooltip);
        }

        private int GetOffsetFromPosition(Vector2 pos)
        {
            if (pos.X < LeftMargin || pos.Y < 0)
                return -1;

            int row = (int)(pos.Y / RowHeight);
            int column = -1;
            for (int j = 0; j < BytesPerRow; j++)
            {
                int group = j / GroupGap;
                float hexX = LeftMargin + (j + group) * ByteSpacing;
                if (pos.X >= hexX && pos.X <= hexX + ByteSpacing - 2)
                {
                    column = j;
                    break;
                }
            }

            if (column == -1)
                return -1;

            int offset = row * BytesPerRow + column;
            return offset >= 0 && offset < _data.Length ? offset : -1;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion motion)
            {
                int offset = GetOffsetFromPosition(motion.Position);
                if (offset >= 0)
                {
                    string text = $"0x{_annotator.StreamOffsetBase + offset:X4}";
                    if (_blockIndexByOffset.TryGetValue(offset, out int blockId))
                    {
                        var ann = _annotator.Annotations[blockId];
                        text += $" {ann.Description}";
                        if (ann.Keys.Count > 0)
                        {
                            string keys = string.Join(", ", ann.Keys.Select(kv => $"{kv.Key}={kv.Value}"));
                            text += $" ({keys})";
                        }
                    }
                    _tooltip.Text = text;
                    _tooltip.Position = motion.Position + new Vector2(12, 12);
                    _tooltip.Visible = true;
                }
                else
                {
                    _tooltip.Visible = false;
                }
            }
            else if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                int offset = GetOffsetFromPosition(mb.Position);
                if (offset >= 0 && _blockIndexByOffset.TryGetValue(offset, out int blockId))
                    ByteClicked?.Invoke(blockId);
            }
        }

        public override void _Draw()
        {
            var font = GetThemeDefaultFont();
            if (font == null)
            {
                GD.PushWarning("HexDrawControlV2: No default font found.");
                return;
            }

            long baseOffset = _annotator.StreamOffsetBase;
            for (int i = 0; i < _data.Length; i += BytesPerRow)
            {
                float y = (i / BytesPerRow) * RowHeight;
                string ascii = string.Empty;

                for (int j = 0; j < BytesPerRow && i + j < _data.Length; j++)
                {
                    int offset = i + j;
                    byte b = _data[offset];

                    int group = j / GroupGap;
                    float hexX = LeftMargin + (j + group) * ByteSpacing;

                    string tag = string.Empty;
                    Color? bg = null;

                    if (_blockIndexByOffset.TryGetValue(offset, out int blockId))
                    {
                        if (_blockColors.TryGetValue(blockId, out var c))
                            bg = c;

                        var ann = _annotator.Annotations[blockId];
                        int relStart = (int)(ann.Address - baseOffset);
                        if (offset == relStart)
                            tag = ann.Description.Length > 6 ? ann.Description.Substring(0, 6) : ann.Description;
                    }

                    if (bg != null)
                    {
                        DrawRect(new Rect2(hexX, y, ByteSpacing - 2, RowHeight), bg.Value, true);
                    }

                    DrawString(font, new Vector2(hexX, y + 12), $"{b:X2}", HorizontalAlignment.Center, -1, 12, Colors.Black);
                    if (!string.IsNullOrEmpty(tag))
                        DrawString(font, new Vector2(hexX, y + 20), tag, HorizontalAlignment.Left, -1, 8, Colors.Black);

                    ascii += (b >= 32 && b <= 126) ? (char)b : '.';
                }

                DrawString(font, new Vector2(AsciiOffsetX, y + 16), ascii, HorizontalAlignment.Left, -1, 12, Colors.Black);
            }
        }
    }
}
