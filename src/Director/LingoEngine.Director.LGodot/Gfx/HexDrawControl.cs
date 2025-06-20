using Godot;
using System;
using System.Collections.Generic;

namespace LingoEngine.Director.LGodot.Gfx
{
    public partial class HexDrawControl : Control
    {
        private readonly byte[] _data;
        private readonly Dictionary<int, string> _knownOffsets;
        private readonly Dictionary<int, Color> _colors;
        private readonly HashSet<int> _styleBlocks;

        private const int BytesPerRow = 32;
        private const int ByteSpacing = 24;
        private const int GroupGap = 8;
        private const int RowHeight = 22; // taller to fit label
        private const int LeftMargin = 10;
        private const int AsciiOffsetX = 900;

        private readonly Dictionary<int, int> _groupMap = new();

        public HexDrawControl(
            byte[] data,
            Dictionary<int, string> knownOffsets,
            Dictionary<int, Color> colors,
            HashSet<int> styleBlocks)
        {
            _data = data;
            _knownOffsets = knownOffsets;
            _colors = colors;
            _styleBlocks = styleBlocks;

            // Build group map from style blocks
            int groupId = 0;
            var sortedOffsets = new List<int>(_styleBlocks);
            foreach (var kv in _knownOffsets)
                sortedOffsets.Add(kv.Key);

            sortedOffsets.Sort();


            for (int i = 0; i < sortedOffsets.Count;)
            {
                int start = sortedOffsets[i];
                int end = start;

                // Walk contiguous block
                while (i + 1 < sortedOffsets.Count && sortedOffsets[i + 1] == end + 1)
                {
                    i++;
                    end = sortedOffsets[i];
                }

                // Assign groupId to all offsets in [start, end]
                for (int off = start; off <= end; off++)
                    _groupMap[off] = groupId;

                groupId++;
                i++;
            }


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

            int colorIndex = 0;

            for (int i = 0; i < _data.Length; i += BytesPerRow)
            {
                float y = (i / BytesPerRow) * RowHeight;
                string ascii = "";

                for (int j = 0; j < BytesPerRow && i + j < _data.Length; j++)
                {
                    int offset = i + j;
                    byte b = _data[offset];

                    // Column position with group spacing
                    int group = j / GroupGap;
                    float hexX = LeftMargin + (j + group) * ByteSpacing;

                    // Background highlight
                    string tag = "";
                    Color? bg = null;

                    if (_groupMap.TryGetValue(offset, out int groupId))
                    {
                        if (!_colors.ContainsKey(groupId))
                        {
                            float hue = (groupId * 0.1f) % 1f;
                            _colors[groupId] = Color.FromHsv(hue, 0.3f, 1f);
                        }

                        bg = _colors[groupId];
                    }

                    if (_knownOffsets.TryGetValue(offset, out var desc))
                    {
                        tag = desc.Length > 6 ? desc.Substring(0, 6) : desc;
                    }

                    if (bg != null)
                    {
                        DrawRect(new Rect2(hexX, y, ByteSpacing - 2, RowHeight), bg.Value, true);
                        if (_styleBlocks.Contains(offset))
                            DrawRect(new Rect2(hexX, y, ByteSpacing - 2, RowHeight), Colors.Black, false, 1.0f);
                    }


                    // Hex byte (top)
                    DrawString(font, new Vector2(hexX, y + 12), $"{b:X2}", HorizontalAlignment.Center, -1, 12, Colors.Black);

                    // Tag label (bottom)
                    if (!string.IsNullOrEmpty(tag))
                        DrawString(font, new Vector2(hexX, y + 20), tag,HorizontalAlignment.Left,-1,8, Colors.Black);

                    // Build ASCII string
                    ascii += (b >= 32 && b <= 126) ? (char)b : '.';
                }

                // ASCII column
                DrawString(font, new Vector2(AsciiOffsetX, y + 16), ascii, HorizontalAlignment.Left, -1, 12, Colors.Black);
            }
        }
    }
}
