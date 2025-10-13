using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BlingoEngine.IO.Legacy.Core;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenParser
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<BlXmedTokenizer.Token> _tokens;
        private readonly IReadOnlyList<int> _lastNumbers;
        private readonly XmedDocument _document = new();
        private readonly List<string> _textBlocks = new();
        private readonly List<(int End, int StyleId)> _runBoundaries = new();
        private readonly List<(int End, bool Flag)> _paragraphFlags = new();
        private readonly HashSet<string> _fontNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, XmedStyleDescriptor> _stylesById = new();
        private readonly Dictionary<int, int> _styleParents = new();
        private readonly Queue<int> _styleOrder = new();

        private int _index;
        private int _nextStyleId = 1;
        private BlLegacyColor _activeColor = new(0, 0, 0);
        private bool _italicMarkerSeen;
        private bool _underlineMarkerSeen;

        public BlXmedTokenParser(ILogger logger, IReadOnlyList<BlXmedTokenizer.Token> tokens, IReadOnlyList<int> lastNumbers)
        {
            _logger = logger;
            _tokens = tokens ?? Array.Empty<BlXmedTokenizer.Token>();
            _lastNumbers = lastNumbers ?? Array.Empty<int>();
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;

            ReadHeader();
            if (!_stylesById.ContainsKey(0))
            {
                _stylesById[0] = new XmedStyleDescriptor { StyleId = 0 };
            }
            ParseBody();
            CollectFontsFromTokens();
            FinalizeDocument();

            return _document;
        }

        private void ReadHeader()
        {
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];

                if (IsTextBlock(token))
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x02 && token.Ascii is { } numeric)
                {
                    if (numeric.Equals("40001", StringComparison.OrdinalIgnoreCase) || numeric.Equals("40000", StringComparison.OrdinalIgnoreCase))
                    {
                        LogUnknown("Header", "02:40001");
                    }
                    else if (numeric.Equals("-7FFF6FE0", StringComparison.OrdinalIgnoreCase))
                    {
                        LogUnknown("Header", "02:-7FFF6FE0");
                    }
                    else if (numeric.Equals("101", StringComparison.OrdinalIgnoreCase) && _document.LineSpacing == 0)
                    {
                        if (TryParseTokenValue(token, out var spacing) && spacing > 0)
                        {
                            _document.LineSpacing = (uint)spacing;
                        }
                    }
                    else if (_document.Width == 0 && TryParseTokenValue(token, out var widthValue) && widthValue > 0)
                    {
                        _document.Width = (uint)widthValue;
                    }

                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01 &&
                    token.Ascii is { } literal && literal.Equals("FFFF", StringComparison.OrdinalIgnoreCase))
                {
                    LogUnknown("Header", "01:FFFF");
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _index++;
                            continue;
                        case 0x04:
                            ReadSpacing_C204();
                            continue;
                        case 0x06:
                            LogUnknown("Header", "C206");
                            _index++;
                            continue;
                        case 0x07:
                            ReadTabs_C207();
                            continue;
                        case 0x08:
                            LogUnknown("Header", "C208");
                            _index++;
                            continue;
                        case 0x0A:
                            ReadBox_C20A();
                            continue;
                        case 0x0B:
                            ReadEditable_C20B();
                            continue;
                        case 0x0F:
                            LogUnknown("Header", "C20F");
                            _index++;
                            continue;
                        case 0x12:
                            LogUnknown("Header", "C212");
                            _index++;
                            continue;
                    }
                }

                if (token.Type == BlXmedTokenizer.TokenType.C1)
                {
                    TrackStyleMarker(token);
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            ReadPara_C003();
                            continue;
                        case 0x04:
                            ReadColor_C104();
                            continue;
                        case 0x1C:
                            MarkStyleFlag(style =>
                            {
                                style.Underline = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                            });
                            _index++;
                            continue;
                        case 0x1D:
                            MarkStyleFlag(style =>
                            {
                                style.Italic = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x02);
                            });
                            _index++;
                            continue;
                    }
                }

                _index++;
            }
        }

        private void ParseBody()
        {
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];

                if (token.Type == BlXmedTokenizer.TokenType.Block00)
                {
                    if (token.Value == 40)
                    {
                        ReadFonts_0040();
                        continue;
                    }

                    if (token.Value == 44)
                    {
                        LogUnknown("Block", "0044");
                        _index++;
                        continue;
                    }

                    var text = token.Ascii ?? string.Empty;
                    if (!string.IsNullOrEmpty(text))
                    {
                        _textBlocks.Add(text);
                    }
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03)
                {
                    var ascii = token.Ascii ?? string.Empty;
                    var type = ascii.Length >= 4 ? ascii.Substring(0, 4) : string.Empty;
                    switch (type)
                    {
                        case "0004":
                            ReadRuns_0004();
                            continue;
                        case "0005":
                            ReadParaFlags_0005();
                            continue;
                        case "0006":
                            ReadStyles_0006();
                            continue;
                        case "0007":
                            LogUnknown("Block", "0007");
                            _index++;
                            continue;
                        case "0013":
                            LogUnknown("Block", "0013");
                            _index++;
                            continue;
                    }
                }

                if (token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    switch (token.TypeValue)
                    {
                        case 0x0A:
                            ReadBox_C20A();
                            continue;
                        case 0x04:
                            ReadSpacing_C204();
                            continue;
                        case 0x07:
                            ReadTabs_C207();
                            continue;
                        case 0x0B:
                            ReadEditable_C20B();
                            continue;
                        case 0x06:
                            LogUnknown("Block", "C206");
                            _index++;
                            continue;
                        case 0x12:
                            LogUnknown("Block", "C212");
                            _index++;
                            continue;
                        case 0x0F:
                            LogUnknown("Block", "C20F");
                            _index++;
                            continue;
                        case 0x08:
                            LogUnknown("Block", "C208");
                            _index++;
                            continue;
                    }
                }

                if (token.Type == BlXmedTokenizer.TokenType.C1)
                {
                    TrackStyleMarker(token);
                    switch (token.TypeValue)
                    {
                        case 0x04:
                            ReadColor_C104();
                            continue;
                        case 0x03:
                            ReadPara_C003();
                            continue;
                        case 0x1C:
                            MarkStyleFlag(style =>
                            {
                                style.Underline = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                            });
                            _index++;
                            continue;
                        case 0x1D:
                            MarkStyleFlag(style =>
                            {
                                style.Italic = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x02);
                            });
                            _index++;
                            continue;
                    }
                }

                _index++;
            }
        }

        private void ReadRuns_0004()
        {
            _index++;
            int? pendingEnd = null;
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03)
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.Block00 ||
                    token.Type == BlXmedTokenizer.TokenType.C1 ||
                    token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x02 &&
                    TryParseTokenValue(token, out var end))
                {
                    pendingEnd = end;
                    _index++;
                    continue;
                }

                if (pendingEnd.HasValue)
                {
                    if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01 &&
                        TryParseTokenValue(token, out var styleId))
                    {
                        _runBoundaries.Add((pendingEnd.Value, styleId));
                        pendingEnd = null;
                        _index++;
                        continue;
                    }

                    if (token.Type == BlXmedTokenizer.TokenType.Boolean)
                    {
                        int boolStyle = token.BoolValue == true ? 1 : 0;
                        _runBoundaries.Add((pendingEnd.Value, boolStyle));
                        pendingEnd = null;
                        _index++;
                        continue;
                    }
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81 || token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    _index++;
                    continue;
                }

                _index++;
            }
        }

        private void ReadParaFlags_0005()
        {
            _index++;
            int? pendingEnd = null;
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03)
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.Block00 ||
                    token.Type == BlXmedTokenizer.TokenType.C1 ||
                    token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x02 &&
                    TryParseTokenValue(token, out var end))
                {
                    pendingEnd = end;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.Boolean && pendingEnd.HasValue)
                {
                    bool flag = token.BoolValue ?? false;
                    _paragraphFlags.Add((pendingEnd.Value, flag));
                    pendingEnd = null;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81 || token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    _index++;
                    continue;
                }

                _index++;
            }
        }

        private void ReadStyles_0006()
        {
            _index++;
            XmedStyleDescriptor? current = null;
            int boolIndex = 0;
            int fieldIndex = 0;
            int blockDepth = 1;

            while (_index < _tokens.Count && blockDepth > 0)
            {
                var token = _tokens[_index];

                if (token.Type == BlXmedTokenizer.TokenType.Block00 ||
                    (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03 && fieldIndex > 0) ||
                    token.Type == BlXmedTokenizer.TokenType.C1 ||
                    token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01 && current == null)
                {
                    if (TryParseTokenValue(token, out var styleId))
                    {
                        current = GetOrCreateStyle(styleId);
                        boolIndex = 0;
                        fieldIndex = 0;
                    }
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.Boolean && current != null && fieldIndex == 0)
                {
                    ApplyBooleanStyle(current, ref boolIndex, token.BoolValue ?? false);
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    fieldIndex++;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    blockDepth--;
                    _index++;
                    if (blockDepth <= 0)
                    {
                        break;
                    }
                    continue;
                }

                if (current != null)
                {
                    if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01)
                    {
                        if (fieldIndex == 1)
                        {
                            if (TryParseTokenValue(token, out var parent) && parent >= 0)
                            {
                                _styleParents[current.StyleId] = parent;
                            }
                            _index++;
                            continue;
                        }

                        if (fieldIndex == 2)
                        {
                            if (TryParseTokenValue(token, out var color) && color >= 0 && color <= 0xFF)
                            {
                                current.ColorIndex = (byte)color;
                            }
                            _index++;
                            continue;
                        }

                        if (fieldIndex == 3)
                        {
                            if (TryParseTokenValue(token, out var size) && size >= 0)
                            {
                                current.FontSize = (ushort)Math.Clamp(size, 0, ushort.MaxValue);
                            }
                            _index++;
                            continue;
                        }
                    }

                    if (token.Type == BlXmedTokenizer.TokenType.Boolean)
                    {
                        ApplyBooleanStyle(current, ref boolIndex, token.BoolValue ?? false);
                        _index++;
                        continue;
                    }
                }

                _index++;
            }

            if (current != null)
            {
                _stylesById[current.StyleId] = current;
            }
        }

        private void ReadFonts_0040()
        {
            if (_index >= _tokens.Count)
            {
                return;
            }

            var token = _tokens[_index];
            string name = token.Ascii ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(name) && _fontNames.Add(name))
            {
                XmedStyleDescriptor descriptor;
                int? styleId = TakeNextStyleNeedingFont();
                if (styleId.HasValue)
                {
                    descriptor = GetOrCreateStyle(styleId.Value);
                }
                else
                {
                    descriptor = GetOrCreateStyle(_nextStyleId++);
                }

                descriptor.FontName = name;
            }

            _index++;
        }

        private void CollectFontsFromTokens()
        {
            var pending = new Queue<int>(_stylesById
                .Where(pair => string.IsNullOrEmpty(pair.Value.FontName))
                .Select(pair => pair.Key)
                .OrderBy(id => id));

            foreach (var token in _tokens)
            {
                if (token.Type != BlXmedTokenizer.TokenType.Block00 || token.Value != 40)
                {
                    continue;
                }

                string name = token.Ascii ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name) || !_fontNames.Add(name))
                {
                    continue;
                }

                int styleId = pending.Count > 0 ? pending.Dequeue() : _nextStyleId++;
                var descriptor = GetOrCreateStyle(styleId);
                descriptor.FontName = name;
            }
        }

        private void ReadBox_C20A()
        {
            _index++;
            var numbers = new List<int>();
            int depth = 0;
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x02 &&
                    TryParseTokenValue(token, out var value))
                {
                    numbers.Add(value);
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.C1 || token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    if (token.Type == BlXmedTokenizer.TokenType.C1)
                    {
                        TrackStyleMarker(token);
                    }
                    depth++;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    if (depth == 0)
                    {
                        _index++;
                        break;
                    }

                    depth--;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81 || token.Type == BlXmedTokenizer.TokenType.Boolean)
                {
                    _index++;
                    continue;
                }

                _index++;
            }

            if (numbers.Count >= 2)
            {
                long width = numbers[1] - numbers[0];
                if (width < 0)
                {
                    width = 0;
                }

                _document.Width = (uint)width;
            }
        }

        private void ReadPara_C003()
        {
            _index++;
            int depth = 0;
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.C1)
                {
                    TrackStyleMarker(token);
                    if (token.TypeValue == 0x1C)
                    {
                        MarkStyleFlag(style =>
                        {
                            style.Underline = true;
                            style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                        });
                    }
                    else if (token.TypeValue == 0x1D)
                    {
                        MarkStyleFlag(style =>
                        {
                            style.Italic = true;
                            style.StyleFlags = (byte)(style.StyleFlags | 0x02);
                        });
                    }

                    depth++;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    if (depth == 0)
                    {
                        _index++;
                        break;
                    }

                    depth--;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    _index++;
                    continue;
                }

                _index++;
            }
        }

        private void ReadSpacing_C204()
        {
            _index++;
            if (_index < _tokens.Count &&
                _tokens[_index].Type == BlXmedTokenizer.TokenType.PrefixedHex &&
                _tokens[_index].TypeValue == 0x02 &&
                TryParseTokenValue(_tokens[_index], out var spacing) && spacing >= 0)
            {
                _document.LineSpacing = (uint)spacing;
            }

            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (IsBlockBoundary(token))
                {
                    break;
                }

                _index++;
            }
        }

        private void ReadTabs_C207()
        {
            _index++;
            bool? tabsEnabled = null;
            bool? wrapEnabled = null;

            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.Boolean)
                {
                    if (tabsEnabled == null)
                    {
                        tabsEnabled = token.BoolValue;
                    }
                    else if (wrapEnabled == null)
                    {
                        wrapEnabled = token.BoolValue;
                    }

                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_82 ||
                    token.Type == BlXmedTokenizer.TokenType.Block00 ||
                    token.Type == BlXmedTokenizer.TokenType.C1 ||
                    token.Type == BlXmedTokenizer.TokenType.C2 ||
                    (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03))
                {
                    break;
                }

                _index++;
            }

            var baseStyle = GetOrCreateStyle(0);
            if (tabsEnabled.HasValue)
            {
                baseStyle.HasTabs = tabsEnabled.Value;
            }

            if (wrapEnabled.HasValue)
            {
                baseStyle.WrapOff = !wrapEnabled.Value;
            }
        }

        private void ReadEditable_C20B()
        {
            _index++;
            bool? editable = null;
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.Boolean)
                {
                    editable = token.BoolValue;
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_82 ||
                    token.Type == BlXmedTokenizer.TokenType.Block00 ||
                    token.Type == BlXmedTokenizer.TokenType.C1 ||
                    token.Type == BlXmedTokenizer.TokenType.C2 ||
                    (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03))
                {
                    break;
                }

                _index++;
            }

            if (editable.HasValue)
            {
                var baseStyle = GetOrCreateStyle(0);
                baseStyle.EditableField = editable.Value;
            }
        }

        private void ReadColor_C104()
        {
            _index++;
            var components = new List<byte>();
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];
                if (token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    _index++;
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    _index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01 &&
                    TryParseColorComponent(token.Ascii, out var component))
                {
                    components.Add(component);
                    _index++;
                    continue;
                }

                _index++;
            }

            if (components.Count >= 3)
            {
                _activeColor = new BlLegacyColor(components[0], components[1], components[2]);
            }
        }

        private void FinalizeDocument()
        {
            if (_textBlocks.Count > 0)
            {
                var builder = new StringBuilder();
                foreach (var block in _textBlocks)
                {
                    builder.Append(block);
                }

                _document.Text = builder.ToString();
                _document.TextLength = _document.Text.Length;
            }

            var baseStyle = GetOrCreateStyle(0);

            if (_italicMarkerSeen && !baseStyle.Italic)
            {
                baseStyle.Italic = true;
                baseStyle.StyleFlags = (byte)(baseStyle.StyleFlags | 0x02);
            }

            if (_underlineMarkerSeen && !baseStyle.Underline)
            {
                baseStyle.Underline = true;
                baseStyle.StyleFlags = (byte)(baseStyle.StyleFlags | 0x04);
            }

            foreach (var styleId in _stylesById.Keys.ToArray())
            {
                ApplyParentChain(styleId, new HashSet<int>());
            }

            _document.Styles.Clear();
            foreach (var descriptor in _stylesById.Values.OrderBy(s => s.StyleId))
            {
                _document.Styles.Add(descriptor);
            }

            if (_document.Styles.Count == 0)
            {
                _document.Styles.Add(baseStyle);
            }

            if (_document.TextLength <= 0)
            {
                return;
            }

            var orderedBoundaries = _runBoundaries.OrderBy(b => b.End).ToList();
            if (orderedBoundaries.Count == 0)
            {
                orderedBoundaries.Add((_document.TextLength, 0));
            }

            int cursor = 0;
            int currentStyleId = 0;
            var runEntries = new List<XmedRunMapEntry>();
            foreach (var (end, styleId) in orderedBoundaries)
            {
                int clampedEnd = Math.Clamp(end, 0, _document.TextLength);
                int length = Math.Max(0, clampedEnd - cursor);
                currentStyleId = styleId;

                if (length > 0)
                {
                    var descriptor = _stylesById.TryGetValue(styleId, out var style) ? style : baseStyle;
                    runEntries.Add(new XmedRunMapEntry(0, 0, (ushort)Math.Clamp(length, 0, (int)ushort.MaxValue), 0,
                        (ushort)Math.Clamp((int)descriptor.StyleId, 0, (int)ushort.MaxValue), cursor));
                    cursor += length;
                }
            }

            if (cursor < _document.TextLength)
            {
                int length = _document.TextLength - cursor;
                var descriptor = _stylesById.TryGetValue(currentStyleId, out var style) ? style : baseStyle;
                runEntries.Add(new XmedRunMapEntry(0, 0, (ushort)Math.Clamp(length, 0, (int)ushort.MaxValue), 0,
                    (ushort)Math.Clamp((int)descriptor.StyleId, 0, (int)ushort.MaxValue), cursor));
            }

            _document.RunMap.Clear();
            _document.RunMap.AddRange(runEntries);

            _document.Runs.Clear();
            int primaryStyleId = runEntries.Count > 0 ? runEntries[0].StyleId : 0;
            var primaryDescriptor = _stylesById.TryGetValue(primaryStyleId, out var primary) ? primary : baseStyle;
            var mergedRun = CreateRun(0, _document.TextLength, _document.Text, primaryDescriptor, baseStyle);
            _document.Runs.Add(mergedRun);
        }

        private void ApplyParentChain(int styleId, HashSet<int> visited)
        {
            if (!_styleParents.TryGetValue(styleId, out var parentId))
            {
                return;
            }

            if (parentId == styleId)
            {
                return;
            }

            if (!visited.Add(styleId))
            {
                return;
            }

            if (!_stylesById.TryGetValue(parentId, out var parent))
            {
                return;
            }

            ApplyParentChain(parentId, visited);

            if (_stylesById.TryGetValue(styleId, out var child))
            {
                ApplyStyleInheritance(parent, child);
            }
        }

        private static void ApplyStyleInheritance(XmedStyleDescriptor parent, XmedStyleDescriptor child)
        {
            if (string.IsNullOrEmpty(child.FontName)) child.FontName = parent.FontName;
            if (child.FontSize == 0) child.FontSize = parent.FontSize;

            if (!child.Bold && parent.Bold) child.Bold = true;
            if (!child.Italic && parent.Italic) child.Italic = true;
            if (!child.Underline && parent.Underline) child.Underline = true;
            if (!child.Strikeout && parent.Strikeout) child.Strikeout = true;
            if (!child.Subscript && parent.Subscript) child.Subscript = true;
            if (!child.Superscript && parent.Superscript) child.Superscript = true;
            if (!child.TabbedField && parent.TabbedField) child.TabbedField = true;
            if (!child.EditableField && parent.EditableField) child.EditableField = true;
            if (!child.HasTabs && parent.HasTabs) child.HasTabs = true;
            if (!child.WrapOff && parent.WrapOff) child.WrapOff = true;
            if (child.Alignment == XmedAlignment.Center && parent.Alignment != XmedAlignment.Center)
            {
                child.Alignment = parent.Alignment;
            }
            if (child.ColorIndex == 0) child.ColorIndex = parent.ColorIndex;
        }

        private XmedTextRun CreateRun(int start, int length, string text, XmedStyleDescriptor descriptor, XmedStyleDescriptor baseStyle)
        {
            return new XmedTextRun
            {
                Start = start,
                Length = length,
                Text = text,
                FontName = !string.IsNullOrEmpty(descriptor.FontName) ? descriptor.FontName : baseStyle.FontName,
                FontSize = descriptor.FontSize != 0 ? descriptor.FontSize : baseStyle.FontSize,
                Bold = descriptor.Bold || baseStyle.Bold,
                Italic = descriptor.Italic || baseStyle.Italic,
                Underline = descriptor.Underline || baseStyle.Underline,
                ForeColor = ResolveColor(descriptor, baseStyle)
            };
        }

        private BlLegacyColor ResolveColor(XmedStyleDescriptor descriptor, XmedStyleDescriptor baseStyle)
        {
            if (descriptor.ColorIndex != 0)
            {
                byte c = descriptor.ColorIndex;
                return new BlLegacyColor(c, c, c);
            }

            if (baseStyle.ColorIndex != 0)
            {
                byte c = baseStyle.ColorIndex;
                return new BlLegacyColor(c, c, c);
            }

            return _activeColor;
        }

        private static bool IsTextBlock(BlXmedTokenizer.Token token)
        {
            return token.Type == BlXmedTokenizer.TokenType.Block00 && token.Value != 40 && token.Value != 44;
        }

        private static bool IsBlockBoundary(BlXmedTokenizer.Token token)
        {
            return token.Type == BlXmedTokenizer.TokenType.Block00 ||
                   (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03) ||
                   token.Type == BlXmedTokenizer.TokenType.C1 ||
                   token.Type == BlXmedTokenizer.TokenType.C2;
        }

        private static bool TryParseTokenValue(BlXmedTokenizer.Token token, out int value)
        {
            value = 0;
            if (token.Ascii is not { } ascii || ascii.Length == 0)
            {
                return false;
            }

            var text = ascii.Trim();
            bool negative = text.StartsWith("-", StringComparison.Ordinal);
            if (negative)
            {
                text = text[1..];
            }

            if (text.Length == 0)
            {
                return false;
            }

            if (!int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            value = negative ? -parsed : parsed;
            return true;
        }

        private static bool TryParseColorComponent(string? ascii, out byte component)
        {
            component = 0;
            if (string.IsNullOrWhiteSpace(ascii))
            {
                return false;
            }

            string text = ascii.Trim();
            if (text.Length > 2)
            {
                text = text[..2];
            }

            return byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out component);
        }

        private static void ApplyBooleanStyle(XmedStyleDescriptor style, ref int index, bool value)
        {
            switch (index)
            {
                case 0:
                    style.Bold = value;
                    break;
                case 1:
                    style.Italic = value;
                    break;
                case 2:
                    style.Underline = value;
                    break;
                case 3:
                    style.Strikeout = value;
                    break;
                case 4:
                    style.Subscript = value;
                    break;
                case 5:
                    style.Superscript = value;
                    break;
                case 6:
                    style.TabbedField = value;
                    break;
                case 7:
                    style.EditableField = value;
                    break;
            }

            index++;
        }

        private void LogUnknown(string category, string token)
        {
            _logger?.LogDebug("XMED: {Category} unknown token {Token}", category, token);
        }

        private XmedStyleDescriptor GetOrCreateStyle(int styleId)
        {
            if (!_stylesById.TryGetValue(styleId, out var descriptor))
            {
                descriptor = new XmedStyleDescriptor
                {
                    StyleId = (ushort)Math.Clamp(styleId, 0, ushort.MaxValue)
                };
                _stylesById[styleId] = descriptor;
                if (styleId != 0)
                {
                    _styleOrder.Enqueue(styleId);
                }
            }

            return descriptor;
        }

        private int? TakeNextStyleNeedingFont()
        {
            while (_styleOrder.Count > 0)
            {
                int styleId = _styleOrder.Dequeue();
                if (_stylesById.TryGetValue(styleId, out var descriptor) && string.IsNullOrEmpty(descriptor.FontName))
                {
                    return styleId;
                }
            }

            return null;
        }

        private void MarkStyleFlag(Action<XmedStyleDescriptor> mutator)
        {
            var target = GetOrCreateStyle(0);
            mutator(target);
        }

        private void TrackStyleMarker(BlXmedTokenizer.Token token)
        {
            if (token.Type != BlXmedTokenizer.TokenType.C1)
            {
                return;
            }

            switch (token.TypeValue)
            {
                case 0x1C:
                case 0x11:
                    _underlineMarkerSeen = true;
                    break;
                case 0x1D:
                case 0x07:
                case 0x13:
                    _italicMarkerSeen = true;
                    break;
            }
        }
    }
}

