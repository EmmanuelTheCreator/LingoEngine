using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly HashSet<int> _inlineColorStyles = new();
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
                _inlineColorStyles.Add(baseStyle.StyleId);
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
            bool inlineColorRead = false;
            bool parentCaptured = false;
            byte? firstPaletteIndex = null;
            byte? selectedPaletteIndex = null;

            void FinalizeCurrentStyle()
            {
                if (current == null)
                    return;

                if (selectedPaletteIndex is null && firstPaletteIndex.HasValue && current.ColorIndex is null)
                    current.ColorIndex = firstPaletteIndex;

                _stylesById[current.StyleId] = current;
                string inlineHex = inlineColorRead ? current.Color.ToHex() : "<null>";
                string colorIndexText = current.ColorIndex is { } colorIdx ? $"0x{colorIdx:X2}" : "<null>";
                _logger.LogInformation(
                    "XMED style {StyleId}: finalize colorIndex {ColorIndex} inline {InlineColor} font '{FontName}'",
                    current.StyleId,
                    colorIndexText,
                    inlineHex,
                    current.FontName);

                if (inlineColorRead)
                    _inlineColorStyles.Add(current.StyleId);
                else
                    _inlineColorStyles.Remove(current.StyleId);

                current = null;
                fieldIndex = 0;
                inlineColorRead = false;
                parentCaptured = false;
                firstPaletteIndex = null;
                selectedPaletteIndex = null;
            }

            while (!reader.IsAtEnd && depth > 0)
            {
                var t = reader.Peek(); if (t is null) break;

                if (current != null && t.IsCompositeC1(0x03))
                    LogInlineColorPreview(reader, current.StyleId);

                if (current != null && (t.IsCompositeC1(0x04) || t.IsCompositeC1(0x03)))
                {
                    if (reader.TryGetColor(out var col))
                    {
                        current.Color = col.GetValueOrDefault();
                        inlineColorRead = true;
                        _logger.LogInformation(
                            "XMED style {StyleId}: inline color {InlineColor} from C1({CompositeId:X2})",
                            current.StyleId,
                            current.Color.ToHex(),
                            t.TypeValue.GetValueOrDefault());
                    }
                    else
                        _logger.LogInformation(
                            "XMED style {StyleId}: encountered color composite C1({CompositeId:X2}) without components",
                            current.StyleId,
                            t.TypeValue.GetValueOrDefault());
                    continue;
                }

                if (t.IsFieldTerminator())
                {
                    reader.ReadNext();
                    depth--;

                    int? finalizedStyleId = current?.StyleId;
                    FinalizeCurrentStyle();
                    if (finalizedStyleId.HasValue)
                        ConsumeTrailingInlineColors(reader, finalizedStyleId.Value);

                    if (depth <= 0)
                        break;
                    continue;
                }
                if (t.IsFieldSeparator())
                {
                    reader.ReadNext();
                    fieldIndex++;
                    continue;
                }

                var tok = reader.ReadNext(); if (tok is null) break;

                if (current != null)
                    _logger.LogInformation("XMED style {StyleId}: field {FieldIndex} tokenType {TokenType} token {Token}", current.StyleId, fieldIndex, tok.Type, tok.ToString());

                if (tok.IsPrefixedHex01() && current == null && tok.TryGetNumericValue(out var sid))
                {
                    current = GetOrCreateStyle(sid);
                    fieldIndex = 0;
                    inlineColorRead = false;
                    parentCaptured = false;
                    firstPaletteIndex = null;
                    selectedPaletteIndex = null;
                    _logger.LogInformation("XMED style {StyleId}: begin 03:0006 entry", sid);
                    continue;
                }
                if (current is null)
                    continue;

                if (!parentCaptured && fieldIndex == 0 && tok.IsPrefixedHex01() && tok.TryGetNumericValue(out var parent) && parent >= 0)
                {
                    _styleParents[current.StyleId] = parent;
                    parentCaptured = true;
                    _logger.LogInformation("XMED style {StyleId}: parent {ParentStyleId}", current.StyleId, parent);
                    continue;
                }

                if (tok.IsPrefixedHex01() && fieldIndex == 2 && tok.TryGetNumericValue(out var ci) && ci >= 0 && ci <= 0xFF)
                {
                    byte paletteCandidate = (byte)ci;
                    if (!firstPaletteIndex.HasValue)
                        firstPaletteIndex = paletteCandidate;

                    if (paletteCandidate != 0 && selectedPaletteIndex is null)
                    {
                        selectedPaletteIndex = paletteCandidate;
                        current.ColorIndex = paletteCandidate;
                        _logger.LogInformation("XMED style {StyleId}: color index 0x{ColorIndex:X2} (selected)", current.StyleId, paletteCandidate);
                    }
                    else if (selectedPaletteIndex is null)
                    {
                        current.ColorIndex = paletteCandidate;
                        _logger.LogInformation("XMED style {StyleId}: color index candidate 0x{ColorIndex:X2}", current.StyleId, paletteCandidate);
                    }
                    else
                        _logger.LogInformation("XMED style {StyleId}: ignoring color index candidate 0x{ColorIndex:X2} (selected 0x{Selected:X2})", current.StyleId, paletteCandidate, selectedPaletteIndex.Value);
                    continue;
                }

                if (tok.IsPrefixedHex01() && fieldIndex == 3 && tok.TryGetNumericValue(out var fs) && fs >= 0)
                {
                    current.FontSize = (ushort)Math.Clamp(fs, 0, ushort.MaxValue);
                    _logger.LogInformation("XMED style {StyleId}: font size {FontSize}", current.StyleId, current.FontSize);
                    continue;
                }

                if (tok.IsBoolean())
                {
                    switch (fieldIndex)
                    {
                        case 4: current.ApplyStyleFlag(XmedStyleFlags.Bold, tok.GetBool()); _logger.LogInformation("XMED style {StyleId}: bold {Value}", current.StyleId, current.Bold); break;
                        case 5: current.ApplyStyleFlag(XmedStyleFlags.Italic, tok.GetBool()); _logger.LogInformation("XMED style {StyleId}: italic {Value}", current.StyleId, current.Italic); break;
                        case 6: current.ApplyStyleFlag(XmedStyleFlags.Underline, tok.GetBool()); _logger.LogInformation("XMED style {StyleId}: underline {Value}", current.StyleId, current.Underline); break;
                        case 7: current.ApplyStyleFlag(XmedStyleFlags.Strikeout, tok.GetBool()); _logger.LogInformation("XMED style {StyleId}: strikeout {Value}", current.StyleId, current.Strikeout); break;
                        case 8: current.ApplyStyleFlag(XmedStyleFlags.Subscript, tok.GetBool()); _logger.LogInformation("XMED style {StyleId}: subscript {Value}", current.StyleId, current.Subscript); break;
                        case 9: current.ApplyStyleFlag(XmedStyleFlags.Superscript, tok.GetBool()); _logger.LogInformation("XMED style {StyleId}: superscript {Value}", current.StyleId, current.Superscript); break;
                    }
                    continue;
                }
            }

            FinalizeCurrentStyle();
            _activeColor = prevColor;
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
            if (_inlineColorStyles.Contains(descriptor.StyleId))
                return descriptor.Color;

            if (descriptor.ColorIndex.HasValue)
                return new BlLegacyColor(descriptor.ColorIndex.Value);

            if (_inlineColorStyles.Contains(baseStyle.StyleId))
                return baseStyle.Color;

            if (baseStyle.ColorIndex.HasValue)
                return new BlLegacyColor(baseStyle.ColorIndex.Value);

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

        internal void ConsumeTrailingInlineColors(BlXmedTokenReader reader, int styleId)
        {
            int currentStyleId = styleId;
            bool consumed = false;

            while (!reader.IsAtEnd)
            {
                var token = reader.Peek();
                if (token == null)
                    break;

                if (token.IsFieldSeparator())
                {
                    reader.ReadNext();
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    reader.ReadNext();
                    continue;
                }

                if (token.IsPrefixedHex03() || token.Type == BlXmedToken.TokenType.Block00)
                    break;

                if (token.IsPrefixedHex01() && token.TryGetNumericValue(out var retargetStyle))
                {
                    reader.ReadNext();

                    if (retargetStyle < 0 || retargetStyle > byte.MaxValue)
                    {
                        _logger.LogInformation(
                            "XMED style {StyleId}: ignoring trailing style retarget 0x{RawValue:X4}",
                            currentStyleId,
                            retargetStyle);
                        continue;
                    }

                    int previousStyleId = currentStyleId;
                    currentStyleId = retargetStyle;
                    GetOrCreateStyle(currentStyleId);
                    _logger.LogInformation(
                        "XMED style {StyleId}: retargeting trailing inline colors to style {TargetStyleId}",
                        previousStyleId,
                        currentStyleId);
                    continue;
                }

                if (token.Type == BlXmedToken.TokenType.C1)
                {
                    if (token.IsCompositeC1(0x04))
                    {
                        _logger.LogInformation(
                            "XMED style {StyleId}: skipping trailing sentinel C1(04)",
                            currentStyleId);
                        reader.ReadNext();
                        consumed = true;
                        continue;
                    }

                    if (token.IsCompositeC1(0x03))
                    {
                        var descriptor = GetOrCreateStyle(currentStyleId);
                        if (reader.TryGetColor(out var color) && color.HasValue)
                        {
                            descriptor.Color = color.Value;
                            _inlineColorStyles.Add(descriptor.StyleId);
                            _logger.LogInformation(
                                "XMED style {StyleId}: trailing inline color {InlineColor} from C1(03)",
                                currentStyleId,
                                descriptor.Color.ToHex());
                        }
                        else
                        {
                            _logger.LogInformation(
                                "XMED style {StyleId}: trailing C1(03) without usable color",
                                currentStyleId);
                        }

                        consumed = true;
                        continue;
                    }

                    _logger.LogInformation(
                        "XMED style {StyleId}: skipping trailing composite C1({CompositeId:X2})",
                        currentStyleId,
                        token.TypeValue.GetValueOrDefault());
                    reader.ReadNext();
                    consumed = true;
                    continue;
                }

                if (token.Type == BlXmedToken.TokenType.C2)
                {
                    _logger.LogInformation(
                        "XMED style {StyleId}: skipping trailing composite C2({CompositeId:X2})",
                        currentStyleId,
                        token.TypeValue.GetValueOrDefault());
                    reader.ReadNext();
                    consumed = true;
                    continue;
                }

                if (token.Type == BlXmedToken.TokenType.PrefixedHex)
                {
                    _logger.LogInformation(
                        "XMED style {StyleId}: skipping trailing prefixed hex token {Token}",
                        currentStyleId,
                        token.ToString());
                    reader.ReadNext();
                    consumed = true;
                    continue;
                }

                if (token.Type == BlXmedToken.TokenType.Ascii || token.Type == BlXmedToken.TokenType.Byte)
                {
                    reader.ReadNext();
                    consumed = true;
                    continue;
                }

                break;
            }

            if (!consumed)
                _logger.LogInformation("XMED style {StyleId}: no trailing inline colors consumed", styleId);
        }

        private void LogInlineColorPreview(BlXmedTokenReader reader, int styleId)
        {
            var token = reader.Peek();
            if (token == null)
                return;

            _logger.LogInformation("XMED style {StyleId}: inspecting C1(03) inline color composite", styleId);

            for (int offset = 0; offset < 16; offset++)
            {
                var preview = reader.Peek(offset);
                if (preview == null)
                    break;

                string ascii = preview.Ascii ?? "<null>";
                string value = preview.Value.HasValue ? preview.Value.Value.ToString(CultureInfo.InvariantCulture) : "<null>";
                string typeValue = preview.TypeValue.HasValue ? $"0x{preview.TypeValue.Value:X2}" : "<null>";

                _logger.LogInformation(
                    "XMED style {StyleId}: C1(03) preview[{Offset:D2}] type {TokenType} ascii {Ascii} value {Value} typeValue {TypeValue}",
                    styleId,
                    offset,
                    preview.Type,
                    ascii,
                    value,
                    typeValue);

                if (offset > 0 && preview.IsBlockBoundary())
                    break;
            }
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
