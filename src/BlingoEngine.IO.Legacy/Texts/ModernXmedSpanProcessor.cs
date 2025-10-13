using System;
using System.Globalization;
using System.Text;
using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts
{

    internal class AIBullshit
    {
        internal sealed class ModernXmedSpanProcessor
        {
            private readonly XMEDByteReader _reader;
            private readonly int _bodyEnd;
            private readonly XmedDocument _document;
            private readonly StringBuilder _textBuilder;
            private readonly XmedStyleDescriptor _baseStyle;

            private XmedStyleDescriptor? _currentStyle;
            private BlLegacyColor _activeColor;
            private bool _colorFromBlock;
            private ushort _blockFontSize;
            private XmedAlignment _blockAlignment;
            private bool _blockWrapOff;
            private bool _blockHasTabs;
            private int _nextStyleId;

            public ModernXmedSpanProcessor(byte[] buffer, int offset, int length, XmedDocument document,
                StringBuilder textBuilder, XmedStyleDescriptor baseStyle, XmedStyleDescriptor? currentStyle,
                BlLegacyColor activeColor, bool colorFromBlock, ushort blockFontSize,
                XmedAlignment blockAlignment, bool blockWrapOff, bool blockHasTabs)
            {
                _reader = new XMEDByteReader(buffer);
                _reader.Skip(offset);
                _bodyEnd = Math.Min(buffer.Length, offset + length);
                _document = document;
                _textBuilder = textBuilder;
                _baseStyle = baseStyle;
                _currentStyle = currentStyle;
                _activeColor = activeColor;
                _colorFromBlock = colorFromBlock;
                _blockFontSize = blockFontSize;
                _blockAlignment = blockAlignment;
                _blockWrapOff = blockWrapOff;
                _blockHasTabs = blockHasTabs;
                _nextStyleId = Math.Max(1, document.Styles.Count);
            }

            public XmedStyleDescriptor? CurrentStyle => _currentStyle;
            public BlLegacyColor ActiveColor => _activeColor;
            public bool ColorFromBlock => _colorFromBlock;
            public ushort BlockFontSize => _blockFontSize;
            public XmedAlignment BlockAlignment => _blockAlignment;
            public bool BlockWrapOff => _blockWrapOff;
            public bool BlockHasTabs => _blockHasTabs;

            public bool Process()
            {
                bool consumed = false;

                while (!_reader.EOF && _reader.Position < _bodyEnd)
                {
                    byte current = _reader.Peek();
                    if (current == 0)
                    {
                        _reader.Skip(1);
                        continue;
                    }

                    if (current == XMEDByteReader.RUN)
                    {
                        if (_reader.TryReadC1Opcode(out var opcode) &&
                            _reader.TryReadBlockContent(_bodyEnd, out var block, out _))
                        {
                            HandleC1Block(opcode, block);
                            consumed = true;
                            continue;
                        }

                        _reader.Skip(1);
                        continue;
                    }

                    if (current == XMEDByteReader.REND)
                    {
                        _reader.TryReadC2Tail(out _);
                        continue;
                    }

                    if (current >= (byte)'0' && current <= (byte)'9')
                    {
                        int digitsStart = _reader.Position;
                        if (_reader.TryReadAsciiDigits(_bodyEnd, out var digitsSpan))
                        {
                            string digits = Encoding.ASCII.GetString(digitsSpan);
                            if (TryHandleLengthPrefixedText(digits))
                            {
                                consumed = true;
                                continue;
                            }

                            if (TryHandleRunMap(digits, digitsStart))
                            {
                                consumed = true;
                                continue;
                            }

                            int consumedDigits = _reader.Position - digitsStart;
                            if (consumedDigits > 0)
                            {
                                _reader.Rewind(consumedDigits);
                            }
                        }

                        _reader.Skip(1);
                        continue;
                    }

                    _reader.Skip(1);
                }

                return consumed;
            }

            private void HandleC1Block(byte opcode, ReadOnlySpan<byte> block)
            {
                switch (opcode)
                {
                    case 0x03:
                        ParseStyleRun(block);
                        break;
                    case 0x04:
                        ParseAlignmentBlock(block);
                        break;
                    case 0x0A:
                    case 0x0B:
                    case 0x1C:
                        ApplyToggleBlock(opcode, block);
                        break;
                }
            }

            private void ParseStyleRun(ReadOnlySpan<byte> block)
            {
                ParseStyleBlock(block);

                if (_reader.Position < _bodyEnd && _reader.Peek() == (byte)',')
                {
                    _reader.Skip(1);
                    var textSpan = _reader.ReadAsciiSequence();
                    if (textSpan.Length > 0)
                    {
                        string text = Encoding.Latin1.GetString(textSpan);
                        int runStart = _textBuilder.Length;
                        _textBuilder.Append(text);

                        var run = new XmedTextRun
                        {
                            Start = runStart,
                            Length = text.Length,
                            Text = text,
                            FontName = _currentStyle?.FontName ?? _baseStyle.FontName,
                            FontSize = _blockFontSize.ResolveFontSize(_currentStyle, _baseStyle.FontSize),
                            ForeColor = _currentStyle.ResolveRunColor(_baseStyle, _activeColor, _colorFromBlock),
                            Bold = _currentStyle?.Bold ?? _baseStyle.Bold,
                            Italic = _currentStyle?.Italic ?? _baseStyle.Italic,
                            Underline = _currentStyle?.Underline ?? _baseStyle.Underline
                        };

                        _document.Runs.Add(run);

                        while (!_reader.EOF)
                        {
                            byte tail = _reader.Peek();
                            if (tail == 0x00 || tail == 0x03)
                            {
                                _reader.Skip(1);
                                continue;
                            }

                            break;
                        }
                    }
                }

                if (_currentStyle != null && _currentStyle.StyleId == 0)
                {
                    _currentStyle.StyleId = (ushort)Math.Clamp(_nextStyleId++, 0, ushort.MaxValue);
                }
            }

            private void ParseStyleBlock(ReadOnlySpan<byte> span)
            {
                int index = 0;
                bool foundFont = false;
                int colorChannel = 0;
                byte r = _activeColor.R;
                byte g = _activeColor.G;
                byte b = _activeColor.B;

                while (index < span.Length)
                {
                    byte token = span[index];
                    switch (token)
                    {
                        case 0x01:
                            index++;
                            var literal = span.ReadDelimitedAscii(ref index);
                            if (!foundFont)
                            {
                                foundFont = literal.TryParseFontDescriptor(_document, ref _currentStyle, _baseStyle,
                                    ref _activeColor, ref _colorFromBlock);
                                if (foundFont)
                                {
                                    colorChannel = 0;
                                    r = _activeColor.R;
                                    g = _activeColor.G;
                                    b = _activeColor.B;
                                }
                            }
                            else if (literal.TryParseColorComponent(out byte component))
                            {
                                switch (colorChannel)
                                {
                                    case 0:
                                        r = component;
                                        break;
                                    case 1:
                                        g = component;
                                        break;
                                    case 2:
                                        b = component;
                                        break;
                                }

                                colorChannel = (colorChannel + 1) % 3;
                                _activeColor = new BlLegacyColor(r, g, b);
                                _colorFromBlock = true;
                            }
                            break;
                        case 0x02:
                            index++;
                            literal = span.ReadDelimitedAscii(ref index);
                            if (literal.TryParseHexInt(out var value))
                            {
                                if (!foundFont)
                                {
                                    _blockFontSize = (ushort)Math.Clamp(value, 0, ushort.MaxValue);
                                    foundFont = true;
                                }
                                else
                                {
                                    _blockFontSize = (ushort)Math.Clamp(value, 0, ushort.MaxValue);
                                }
                            }
                            break;
                        case 0xC1:
                            index++;
                            if (index < span.Length)
                            {
                                byte nestedType = span[index];
                                index++;
                                int depth = 1;
                                int start = index;
                                while (index < span.Length && depth > 0)
                                {
                                    if (span[index] == 0xC1)
                                    {
                                        depth++;
                                        index += 2;
                                        continue;
                                    }

                                    if (span[index] == 0xC2)
                                    {
                                        depth--;
                                        index += 2;
                                        continue;
                                    }

                                    index++;
                                }

                                if (depth == 0 && start < index - 2)
                                {
                                    var nestedSpan = span[start..(index - 2)];
                                    ParseNestedBlock(nestedType, nestedSpan);
                                }
                            }
                            break;
                        default:
                            index++;
                            break;
                    }
                }
            }

            private void ParseNestedBlock(byte blockType, ReadOnlySpan<byte> span)
            {
                switch (blockType)
                {
                    case 0x03:
                        ParseStyleBlock(span);
                        break;
                    case 0x04:
                        ParseAlignmentBlock(span);
                        break;
                    case 0x0A:
                    case 0x0B:
                    case 0x1C:
                        ApplyToggleBlock(blockType, span);
                        break;
                }
            }

            private void ParseAlignmentBlock(ReadOnlySpan<byte> span)
            {
                int index = 0;
                bool alignmentSet = false;

                while (index < span.Length)
                {
                    byte token = span[index];
                    switch (token)
                    {
                        case 0x01:
                            index++;
                            var literal = span.ReadDelimitedAscii(ref index);
                            if (literal.Length == 1)
                            {
                                if (literal[0] == '0')
                                {
                                    _blockWrapOff = false;
                                }
                                else if (literal[0] == '1')
                                {
                                    _blockWrapOff = true;
                                }
                                else if (literal[0] == '2')
                                {
                                    _blockHasTabs = true;
                                }
                            }
                            break;
                        case 0x02:
                            index++;
                            literal = span.ReadDelimitedAscii(ref index);
                            if (!alignmentSet && literal.TryParseHexInt(out var value))
                            {
                                _blockAlignment = value switch
                                {
                                    1 => XmedAlignment.Right,
                                    2 => XmedAlignment.Left,
                                    3 => XmedAlignment.Justify,
                                    _ => XmedAlignment.Center
                                };
                                alignmentSet = true;
                            }
                            break;
                        default:
                            index++;
                            break;
                    }
                }
            }

            private void ApplyToggleBlock(byte blockType, ReadOnlySpan<byte> span)
            {
                bool? state = null;
                int index = 0;
                while (index < span.Length)
                {
                    byte token = span[index];
                    if (token == 0x01)
                    {
                        index++;
                        var literal = span.ReadDelimitedAscii(ref index);
                        if (literal == "33")
                        {
                            state = true;
                        }
                        else if (literal == "30")
                        {
                            state = false;
                        }
                    }
                    else
                    {
                        index++;
                    }
                }

                if (state == null)
                {
                    return;
                }

                switch (blockType)
                {
                    case 0x0A:
                        _baseStyle.Superscript = state.Value;
                        _baseStyle.Subscript &= !state.Value;
                        if (_currentStyle != null)
                        {
                            _currentStyle.Superscript = state.Value;
                            if (state.Value)
                            {
                                _currentStyle.Subscript = false;
                            }
                        }
                        break;
                    case 0x0B:
                        _baseStyle.Subscript = state.Value;
                        _baseStyle.Superscript &= !state.Value;
                        if (_currentStyle != null)
                        {
                            _currentStyle.Subscript = state.Value;
                            if (state.Value)
                            {
                                _currentStyle.Superscript = false;
                            }
                        }
                        break;
                    case 0x1C:
                        _baseStyle.Underline = state.Value;
                        if (_currentStyle != null)
                        {
                            _currentStyle.Underline = state.Value;
                        }
                        break;
                }
            }

            private bool TryHandleLengthPrefixedText(string digits)
            {
                if (digits.Length == 0)
                {
                    return false;
                }

                if (_reader.EOF || _reader.Peek() != (byte)',')
                {
                    return false;
                }

                if (!int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out int runLength) || runLength < 0)
                {
                    return false;
                }

                _reader.Skip(1);
                var textBytes = _reader.ReadBytes(runLength);
                if (textBytes.Length != runLength)
                {
                    return false;
                }

                for (int i = 0; i < textBytes.Length; i++)
                {
                    if (!XMEDByteReader.IsPrintableOrWhitespace(textBytes[i]))
                    {
                        return false;
                    }
                }

                string text = Encoding.Latin1.GetString(textBytes);
                int runStart = _textBuilder.Length;
                _textBuilder.Append(text);

                var run = new XmedTextRun
                {
                    Start = runStart,
                    Length = text.Length,
                    Text = text,
                    FontName = _currentStyle?.FontName ?? _baseStyle.FontName,
                    FontSize = _blockFontSize.ResolveFontSize(_currentStyle, _baseStyle.FontSize),
                    ForeColor = _currentStyle.ResolveRunColor(_baseStyle, _activeColor, _colorFromBlock),
                    Bold = _currentStyle?.Bold ?? _baseStyle.Bold,
                    Italic = _currentStyle?.Italic ?? _baseStyle.Italic,
                    Underline = _currentStyle?.Underline ?? _baseStyle.Underline
                };

                _document.Runs.Add(run);

                while (!_reader.EOF)
                {
                    byte tail = _reader.Peek();
                    if (tail == 0x00 || tail == 0x03)
                    {
                        _reader.Skip(1);
                        continue;
                    }

                    break;
                }

                return true;
            }

            private bool TryHandleRunMap(string digits, int position)
            {
                if (digits.Length != 20)
                {
                    return false;
                }

                if (!digits.TryParseRunMapEntry(position, out var entry))
                {
                    return false;
                }

                _document.RunMap.Add(entry);
                return true;
            }
        }
    }
}
