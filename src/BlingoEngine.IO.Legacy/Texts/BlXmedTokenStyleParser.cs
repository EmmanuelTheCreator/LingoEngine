using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
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
        private BlLegacyColor _activeColor = new(0);

        public IReadOnlyDictionary<int, XmedStyleDescriptor> StylesById => _stylesById;


        public BlXmedTokenStyleParser(ILogger logger, XmedDocument document)
        {
            _logger = logger;
            _document = document;
        }

        #region Header
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


        public void MarkStyleFlag(int styleIndex, Action<XmedStyleDescriptor> mutator)
        {
            var target = GetOrCreateStyle(styleIndex);
            mutator(target);
        }

        public void ReadStyles(BlXmedTokenReader reader)
        {
            reader.Skip(); // enter C1(..)
            XmedStyleDescriptor? current = null;
            int fieldIndex = 0, depth = 1;
            var prevColor = _activeColor;

            while (!reader.IsAtEnd && depth > 0)
            {
                var t = reader.Peek(); if (t is null) break;

                if (t.IsCompositeC1(0x04))
                {
                    if (reader.TryGetColor(out var col) && current != null)
                        current.Color = col.Value;
                    continue;
                }

                if (t.IsFieldTerminator()) { reader.ReadNext(); depth--; continue; }
                if (t.IsFieldSeparator()) { reader.ReadNext(); fieldIndex++; continue; }

                var tok = reader.ReadNext(); if (tok is null) break;

                if (tok.IsPrefixedHex01() && current == null && tok.TryGetNumericValue(out var sid))
                {
                    current = GetOrCreateStyle(sid);
                    fieldIndex = 0;
                    continue;
                }
                if (current is null) continue;

                if (tok.IsPrefixedHex01() && fieldIndex == 1 && tok.TryGetNumericValue(out var parent) && parent >= 0)
                {
                    _styleParents[current.StyleId] = parent;
                    continue;
                }

                if (tok.IsPrefixedHex01() && fieldIndex == 2 && tok.TryGetNumericValue(out var ci) && ci >= 0 && ci <= 0xFF)
                {
                    current.ColorIndex = (byte?)ci;
                    continue;
                }

                if (tok.IsPrefixedHex01() && fieldIndex == 3 && tok.TryGetNumericValue(out var fs) && fs >= 0)
                {
                    current.FontSize = (ushort)Math.Clamp(fs, 0, ushort.MaxValue);
                    continue;
                }

                // handle style booleans
                if (tok.IsBoolean())
                {
                    switch (fieldIndex)
                    {
                        case 4: current.ApplyStyleFlag(XmedStyleFlags.Bold, tok.GetBool()); break;
                        case 5: current.ApplyStyleFlag(XmedStyleFlags.Italic, tok.GetBool()); break;
                        case 6: current.ApplyStyleFlag(XmedStyleFlags.Underline, tok.GetBool()); break;
                        case 7: current.ApplyStyleFlag(XmedStyleFlags.Strikeout, tok.GetBool()); break;
                        case 8: current.ApplyStyleFlag(XmedStyleFlags.Subscript, tok.GetBool()); break;
                        case 9: current.ApplyStyleFlag(XmedStyleFlags.Superscript, tok.GetBool()); break;
                    }
                    continue;
                }
            }

            _activeColor = prevColor;
            if (current != null) _stylesById[current.StyleId] = current;
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
                    descriptor = GetOrCreateStyle(styleId.Value);
                else
                    descriptor = GetOrCreateStyle(_nextStyleId++);

                descriptor.FontName = name;
            }

            reader.Skip();
        }


        public void FinalizeStyles(XmedDocument document)
        {
            var baseStyle = GetOrCreateStyle(0);

            foreach (var styleId in _stylesById.Keys.ToArray())
                ApplyParentChain(styleId, new HashSet<int>());

            document.Styles.Clear();
            foreach (var descriptor in _stylesById.Values.OrderBy(s => s.StyleId))
                document.Styles.Add(descriptor);

            if (document.Styles.Count == 0)
                document.Styles.Add(baseStyle);
        }

        public BlLegacyColor ResolveColor(XmedStyleDescriptor descriptor, XmedStyleDescriptor baseStyle)
        {
            if (descriptor.ColorIndex.HasValue && descriptor.ColorIndex != 0)
                return new BlLegacyColor(descriptor.ColorIndex!.Value);

            if (descriptor.ColorIndex.HasValue && baseStyle.ColorIndex != 0)
                return new BlLegacyColor(baseStyle.ColorIndex!.Value);

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
                    _styleOrder.Enqueue(styleId);
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
                parent.ApplyStyleInheritanceToChild(child);
        }

       
    }
}
