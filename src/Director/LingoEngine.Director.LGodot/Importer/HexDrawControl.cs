using Godot;
using LingoEngine.Director.LGodot.Importer;
using System;
using System.Collections.Generic;

namespace LingoEngine.Director.LGodot.Gfx
{
    internal partial class HexDrawControl : Control
    {
        private readonly byte[] _data;
        private readonly IReadOnlyList<XmedBlock> _blocks;
        private readonly Dictionary<int, Color> _blockColors;
        private readonly Dictionary<int, int> _blockIndexByOffset = new();
        private readonly HashSet<int> _styleOffsets = new();

        private const int BytesPerRow = 32;
        private const int ByteSpacing = 24;
        private const int GroupGap = 8;
        private const int RowHeight = 22;
        private const int LeftMargin = 10;
        private const int AsciiOffsetX = 900;

        public HexDrawControl(
            byte[] data,
            IReadOnlyList<XmedBlock> blocks,
            Dictionary<int, Color> blockColors,
            Dictionary<int, int> blockIndexByOffset,
            HashSet<int> styleOffsets)
        {
            _data = data;
            _blocks = blocks;
            _blockColors = blockColors;

            foreach (var kv in blockIndexByOffset)
                _blockIndexByOffset[kv.Key] = kv.Value;
            foreach (var s in styleOffsets)
                _styleOffsets.Add(s);

            int totalRows = (int)Math.Ceiling(data.Length / (float)BytesPerRow);
            CustomMinimumSize = new Vector2(AsciiOffsetX + 300, totalRows * RowHeight);
            Size = CustomMinimumSize;
        }

        public override void _Draw()
        {
            var font = GetThemeDefaultFont();
            if (font == null)
            {
                GD.PushWarning("HexDrawControl: No default font found.");
                return;
            }

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

                        var block = _blocks[blockId];
                        if (offset == block.Start)
                            tag = block.Description.Length > 6 ? block.Description.Substring(0, 6) : block.Description;
                    }

                    if (bg != null)
                    {
                        DrawRect(new Rect2(hexX, y, ByteSpacing - 2, RowHeight), bg.Value, true);
                        if (_styleOffsets.Contains(offset))
                            DrawRect(new Rect2(hexX, y, ByteSpacing - 2, RowHeight), Colors.Black, false, 1.0f);
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
