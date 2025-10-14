using System;
using System.Collections.Generic;
using System.Linq;
using BlingoEngine.IO.Legacy.Core;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenStyleParser
    {
        private readonly IReadOnlyList<BlXmedTokenizer.Token> _tokens;
        private readonly HashSet<string> _fontNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, XmedStyleDescriptor> _stylesById = new();
        private readonly Dictionary<int, int> _styleParents = new();
        private readonly Queue<int> _styleOrder = new();

        private int _nextStyleId = 1;

        public BlXmedTokenStyleParser(ILogger logger, IReadOnlyList<BlXmedTokenizer.Token> tokens)
        {
            _ = logger;
            _tokens = tokens;
        }

        public BlLegacyColor ActiveColor { get; private set; } = new(0, 0, 0);
        public bool ItalicMarkerSeen { get; private set; }
        public bool UnderlineMarkerSeen { get; private set; }

        public IReadOnlyDictionary<int, XmedStyleDescriptor> StylesById => _stylesById;

        public void TrackStyleMarker(BlXmedTokenizer.Token token)
        {
            if (token.Type != BlXmedTokenizer.TokenType.C1)
            {
                return;
            }

            switch (token.TypeValue)
            {
                case 0x1C:
                case 0x11:
                    UnderlineMarkerSeen = true;
                    break;
                case 0x1D:
                case 0x07:
                case 0x13:
                    ItalicMarkerSeen = true;
                    break;
            }
        }

        public void MarkStyleFlag(Action<XmedStyleDescriptor> mutator)
        {
            var target = GetOrCreateStyle(0);
            mutator(target);
        }

        public XmedStyleDescriptor GetOrCreateStyle(int styleId)
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

        public bool TryGetStyle(int styleId, out XmedStyleDescriptor? descriptor)
        {
            if (styleId == 0)
            {
                descriptor = GetOrCreateStyle(0);
                return true;
            }

            if (_stylesById.TryGetValue(styleId, out var value))
            {
                descriptor = value;
                return true;
            }

            descriptor = null;
            return false;
        }

        public void ReadStyles(ref int index)
        {
            index++;
            XmedStyleDescriptor? current = null;
            int boolIndex = 0;
            int fieldIndex = 0;
            int blockDepth = 1;

            while (index < _tokens.Count && blockDepth > 0)
            {
                var token = _tokens[index];

                if (token.Type == BlXmedTokenizer.TokenType.Block00 ||
                    (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x03 && fieldIndex > 0) ||
                    token.Type == BlXmedTokenizer.TokenType.C1 ||
                    token.Type == BlXmedTokenizer.TokenType.C2)
                {
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01 && current == null)
                {
                    if (token.TryGetNumericValue(out var styleId))
                    {
                        current = GetOrCreateStyle(styleId);
                        boolIndex = 0;
                        fieldIndex = 0;
                    }
                    index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.Boolean && current != null && fieldIndex == 0)
                {
                    ApplyBooleanStyle(current, ref boolIndex, token.BoolValue ?? false);
                    index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    fieldIndex++;
                    index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    blockDepth--;
                    index++;
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
                            if (token.TryGetNumericValue(out var parent) && parent >= 0)
                            {
                                _styleParents[current.StyleId] = parent;
                            }
                            index++;
                            continue;
                        }

                        if (fieldIndex == 2)
                        {
                            if (token.TryGetNumericValue(out var color) && color >= 0 && color <= 0xFF)
                            {
                                current.ColorIndex = (byte)color;
                            }
                            index++;
                            continue;
                        }

                        if (fieldIndex == 3)
                        {
                            if (token.TryGetNumericValue(out var size) && size >= 0)
                            {
                                current.FontSize = (ushort)Math.Clamp(size, 0, ushort.MaxValue);
                            }
                            index++;
                            continue;
                        }
                    }

                    if (token.Type == BlXmedTokenizer.TokenType.Boolean)
                    {
                        ApplyBooleanStyle(current, ref boolIndex, token.BoolValue ?? false);
                        index++;
                        continue;
                    }
                }

                index++;
            }

            if (current != null)
            {
                _stylesById[current.StyleId] = current;
            }
        }

        public void ReadFonts(ref int index)
        {
            if (index >= _tokens.Count)
            {
                return;
            }

            var token = _tokens[index];
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

            index++;
        }

        public void CollectFontsFromTokens()
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

        public void ReadTabs(ref int index)
        {
            index++;
            bool? tabsEnabled = null;
            bool? wrapEnabled = null;

            while (index < _tokens.Count)
            {
                var token = _tokens[index];
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

                    index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    index++;
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

                index++;
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

        public void ReadEditable(ref int index)
        {
            index++;
            bool? editable = null;
            while (index < _tokens.Count)
            {
                var token = _tokens[index];
                if (token.Type == BlXmedTokenizer.TokenType.Boolean)
                {
                    editable = token.BoolValue;
                    index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    index++;
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

                index++;
            }

            if (editable.HasValue)
            {
                var baseStyle = GetOrCreateStyle(0);
                baseStyle.EditableField = editable.Value;
            }
        }

        public void ReadColor(ref int index)
        {
            index++;
            var components = new List<byte>();
            while (index < _tokens.Count)
            {
                var token = _tokens[index];
                if (token.Type == BlXmedTokenizer.TokenType.B_82)
                {
                    index++;
                    break;
                }

                if (token.Type == BlXmedTokenizer.TokenType.B_81)
                {
                    index++;
                    continue;
                }

                if (token.Type == BlXmedTokenizer.TokenType.PrefixedHex && token.TypeValue == 0x01 &&
                    token.TryGetColorComponent(out var component))
                {
                    components.Add(component);
                    index++;
                    continue;
                }

                index++;
            }

            if (components.Count >= 3)
            {
                ActiveColor = new BlLegacyColor(components[0], components[1], components[2]);
            }
        }

        public void FinalizeStyles(XmedDocument document)
        {
            var baseStyle = GetOrCreateStyle(0);

            if (ItalicMarkerSeen && !baseStyle.Italic)
            {
                baseStyle.Italic = true;
                baseStyle.StyleFlags = (byte)(baseStyle.StyleFlags | 0x02);
            }

            if (UnderlineMarkerSeen && !baseStyle.Underline)
            {
                baseStyle.Underline = true;
                baseStyle.StyleFlags = (byte)(baseStyle.StyleFlags | 0x04);
            }

            foreach (var styleId in _stylesById.Keys.ToArray())
            {
                ApplyParentChain(styleId, new HashSet<int>());
            }

            document.Styles.Clear();
            foreach (var descriptor in _stylesById.Values.OrderBy(s => s.StyleId))
            {
                document.Styles.Add(descriptor);
            }

            if (document.Styles.Count == 0)
            {
                document.Styles.Add(baseStyle);
            }
        }

        public BlLegacyColor ResolveColor(XmedStyleDescriptor descriptor, XmedStyleDescriptor baseStyle)
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

            return ActiveColor;
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
    }
}
