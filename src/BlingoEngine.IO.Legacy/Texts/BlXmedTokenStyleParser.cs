using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;
using static BlingoEngine.IO.Legacy.Texts.Data.XmedStyleDescriptor;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenStyleParser
    {
        private readonly HashSet<string> _fontNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, XmedStyleDescriptor> _stylesById = new();
        private readonly Dictionary<int, int> _styleParents = new();
        private readonly Queue<int> _styleOrder = new();
        private readonly ILogger _logger;
        private readonly XmedDocument _document;
        private int _nextStyleId = 1;
        private BlLegacyColor _activeColor = new(0, 0, 0);

        public bool ItalicMarkerSeen { get; private set; }
        public bool UnderlineMarkerSeen { get; private set; }
        public IReadOnlyDictionary<int, XmedStyleDescriptor> StylesById => _stylesById;

        public BlXmedTokenStyleParser(ILogger logger, XmedDocument document)
        {
            _logger = logger;
            _document = document;
        }

        #region Header

        public void ReadTabs(BlXmedTokenReader reader)
        {
            if (!reader.TryReadBooleansInC2(0x07, out var hasTabs, out var wrapOn,
                    tok => LogUnknown("Tabs", tok.ToString())))
                return;

            _document.AllowTabs = hasTabs;
            _document.IsWrapOff = !wrapOn; // Director: wrapOn → invert
        }

        public void ReadEditable(BlXmedTokenReader reader)
        {
            if (!reader.TryReadBooleanInC2(0x0B, out var editable, tok => LogUnknown("Editable", tok.ToString())))
                return;

            _document.IsEditable = editable;
        }
        internal void ReadHeaderColor(BlXmedTokenReader reader)
        {
            if (reader.TryGetColor(out var color))
            {
                var baseStyle = GetOrCreateStyle(0);
                baseStyle.Color = color!.Value;
            }
        }
        public void MarkHeaderStyleFlag(Action<XmedStyleDescriptor> mutator)
        {
            var target = GetOrCreateStyle(0);
            mutator(target);
        }
        #endregion




        public void TrackStyleMarker(BlXmedToken token)
        {
            if (token.Type != TokenType.C1)
                return;

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
        
        public void ReadStyles(BlXmedTokenReader reader)
        {
            reader.Skip();
            XmedStyleDescriptor? current = null;
            int fieldIndex = 0;
            int blockDepth = 1;

            while (!reader.IsAtEnd && blockDepth > 0)
            {
                var lookahead = reader.Peek();
                if (lookahead is null)
                    break;

                if (lookahead.Type == TokenType.Block00 || (lookahead.IsPrefixedHex03() && fieldIndex > 0) || lookahead.IsCompositeOpen()) break;
                if (lookahead.Type == TokenType.Block00 || (lookahead.IsPrefixedHex03() && fieldIndex > 0)) break;
                if (lookahead.IsCompositeC1(0x04)) {

                    if (reader.TryGetColor(out var color))
                        _activeColor = color!.Value;
                    if (current != null) current.Color = _activeColor;
                    continue; 
                } // color triplet
                if (lookahead.IsCompositeOpen()) { reader.Skip(); continue; } // ignore other composites


                var token = reader.ReadNext();
                if (token is null)
                    break;

                if (token.IsPrefixedHex01() && current == null)
                {
                    if (token.TryGetNumericValue(out var styleId))
                    {
                        current = GetOrCreateStyle(styleId);
                        fieldIndex = 0;
                    }

                    continue;
                }

                if (token.IsFieldSeparator())
                {
                    fieldIndex++;
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    blockDepth--;
                    if (blockDepth <= 0)
                        break;
                    continue;
                }

                if (current is { } descriptor)
                {
                    if (token.IsPrefixedHex01())
                    {
                        if (fieldIndex == 1)
                        {
                            if (token.TryGetNumericValue(out var parent) && parent >= 0)
                                _styleParents[descriptor.StyleId] = parent;
                            continue;
                        }

                        if (fieldIndex == 2)
                        {
                            if (token.TryGetNumericValue(out var color) && color >= 0 && color <= 0xFF)
                                descriptor.ColorIndex = (byte)color;
                            continue;
                        }

                        if (fieldIndex == 3)
                        {
                            if (token.TryGetNumericValue(out var size) && size >= 0)
                                descriptor.FontSize = (ushort)Math.Clamp(size, 0, ushort.MaxValue);
                            continue;
                        }
                    }

                    if (token.IsBoolean()) {
                        ApplyStyleFlag(descriptor, XmedStyleFlags.Bold, token.GetBool());
                        continue; 
                    }
                }
            }

            if (current != null)
                _stylesById[current.StyleId] = current;
        }

        public void ReadFonts(BlXmedTokenReader reader)
        {
            var token = reader.Peek();
            if (token is null)
                return;

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

            reader.Skip();
        }

        public void CollectFontsFromTokens()
        {
            var pending = new Queue<int>(_stylesById
                .Where(pair => string.IsNullOrEmpty(pair.Value.FontName))
                .Select(pair => pair.Key)
                .OrderBy(id => id));

            foreach (var token in _tokens)
            {
                if (token.Type != TokenType.Block00 || token.Value != 40)
                    continue;

                string name = token.Ascii ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name) || !_fontNames.Add(name))
                    continue;

                int styleId = pending.Count > 0 ? pending.Dequeue() : _nextStyleId++;
                var descriptor = GetOrCreateStyle(styleId);
                descriptor.FontName = name;
            }
        }

      

        //public BlLegacyColor ReadColor(BlXmedTokenReader reader)
        //{
        //    reader.Skip();
        //    var components = reader.GetColorComponents();

        //    byte r = components.Count > 0 ? components[0] : (byte)0;
        //    byte g = components.Count > 1 ? components[1] : (byte)0;
        //    byte b = components.Count > 2 ? components[2] : (byte)0;

        //    return new BlLegacyColor(r, g, b);
        //}

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
                document.Styles.Add(descriptor);

            if (document.Styles.Count == 0)
                document.Styles.Add(baseStyle);
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

            return _activeColor;
        }

        private int? TakeNextStyleNeedingFont()
        {
            while (_styleOrder.Count > 0)
            {
                int styleId = _styleOrder.Dequeue();
                if (_stylesById.TryGetValue(styleId, out var descriptor) && string.IsNullOrEmpty(descriptor.FontName))
                    return styleId;
            }

            return null;
        }

        public static void ApplyStyleFlag(XmedStyleDescriptor style, XmedStyleFlags flag, bool enabled)
        {
            style.Flags = enabled ? style.Flags | flag : style.Flags & ~flag;

            style.Bold = style.Flags.HasFlag(XmedStyleFlags.Bold);
            style.Italic = style.Flags.HasFlag(XmedStyleFlags.Italic);
            style.Underline = style.Flags.HasFlag(XmedStyleFlags.Underline);
            style.Strikeout = style.Flags.HasFlag(XmedStyleFlags.Strikeout);
            style.Subscript = style.Flags.HasFlag(XmedStyleFlags.Subscript);
            style.Superscript = style.Flags.HasFlag(XmedStyleFlags.Superscript);
            style.TabbedField = style.Flags.HasFlag(XmedStyleFlags.TabbedField);
        }

        private void ApplyParentChain(int styleId, HashSet<int> visited)
        {
            if (!_styleParents.TryGetValue(styleId, out var parentId))
                return;

            if (parentId == styleId)
                return;

            if (!visited.Add(styleId))
                return;

            if (!_stylesById.TryGetValue(parentId, out var parent))
                return;

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
            if (child.Alignment == XmedAlignment.Center && parent.Alignment != XmedAlignment.Center)
            {
                child.Alignment = parent.Alignment;
            }
            if (child.ColorIndex == 0) child.ColorIndex = parent.ColorIndex;
        }

        private void LogUnknown(string category, string token)
        {
            _logger.LogDebug("XMED: {Category} unknown style parsing token {Token}", category, token);
        }

       
    }
}
