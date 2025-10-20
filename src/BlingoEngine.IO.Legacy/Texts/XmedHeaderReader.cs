
using Microsoft.Extensions.Logging;
using static BlingoEngine.IO.Legacy.Texts.Data.XmedStyleDescriptor;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedHeaderReader
    {
        private XmedDocument _document;
        private BlXmedTokenStyleParser _styleParser;
        private readonly XmedSpacingReader _spacingReader;
        private readonly BlXmedTokenReader _reader;
        private readonly ILogger _logger;

        public XmedHeaderReader(XmedDocument document, BlXmedTokenStyleParser styleParser, XmedSpacingReader spacingReader, BlXmedTokenReader reader, ILogger logger)
        {
            _document = document;
            _styleParser = styleParser;
            _spacingReader = spacingReader;
            _reader = reader;
            _logger = logger;
        }


        public void ReadHeader()
        {
            //while (!_reader.IsAtEnd)
            //{
            //    var token = _reader.Peek();
            //    if (token is null)
            //        break;

            //    if (token.IsTextBlock())
            //        break;

            //    if (token.IsPrefixedHex02() && token.Ascii is { } numeric)
            //    {
            //        if (numeric.Equals("40001", StringComparison.OrdinalIgnoreCase) || numeric.Equals("40000", StringComparison.OrdinalIgnoreCase))
            //            LogUnknown("Header", "02:40001");
            //        else if (numeric.Equals("-7FFF6FE0", StringComparison.OrdinalIgnoreCase))
            //            LogUnknown("Header", "02:-7FFF6FE0");

            //        _reader.Skip();
            //        continue;
            //    }

            //    if (token.IsPrefixedHex01() && token.Ascii is { } literal && literal.Equals("FFFF", StringComparison.OrdinalIgnoreCase))
            //    {
            //        LogUnknown("Header", "01:FFFF");
            //        _reader.Skip();
            //        continue;
            //    }

            //    if (token.IsC2())
            //    {
            //        switch (token.TypeValue)
            //        {
            //            case 0x03:
            //                _spacingReader.ReadParagraphSpacing(_reader);
            //                continue;
            //            case 0x04:
            //                ReadHeaderSpacing(_reader);
            //                continue;
            //            case 0x06:
            //                LogUnknown("Header", "C206");
            //                _reader.Skip();
            //                continue;
            //            case 0x07:
            //                ReadTabs(_reader);
            //                continue;
            //            case 0x08:
            //                LogUnknown("Header", "C208");
            //                _reader.Skip();
            //                continue;
            //            case 0x0A:
            //                TryReadSize(_reader);
            //                continue;
            //            case 0x0B:
            //                ReadEditable(_reader);
            //                continue;
            //            case 0x0F:
            //                LogUnknown("Header", "C20F");
            //                _reader.Skip();
            //                continue;
            //            case 0x12:
            //                LogUnknown("Header", "C212");
            //                _reader.Skip();
            //                continue;
            //        }
            //    }

            //    if (token.IsC1())
            //    {
            //        switch (token.TypeValue)
            //        {
            //            case 0x03:
            //                LogUnknown("Header", "C1C3");
            //                _reader.Skip();
            //                continue;
            //            case 0x04:
                            
            //                _styleParser.ReadHeaderColor(_reader);
            //                continue;
            //            case 0x1C:
            //                _styleParser.MarkHeaderStyleFlag(style =>
            //                {
            //                    style.Underline = true;
            //                    style.ApplyStyleFlag(XmedStyleFlags.Underline, true);
            //                });
            //                _reader.Skip();
            //                continue;
            //            case 0x1D:
            //                _styleParser.MarkHeaderStyleFlag(style =>
            //                {
            //                    style.Italic = true;
            //                    style.ApplyStyleFlag(XmedStyleFlags.Italic, true);
            //                });
            //                _reader.Skip();
            //                continue;
            //        }
            //    }

            //    LogUnknown("Header", $"Skipped token {_reader.Peek()}");
            //    _reader.Skip();
            //}
        }


        private bool TryReadSize(BlXmedTokenReader reader)
        {
            if (!reader.TryReadNumericPairInC2(0x0A, out var w, out var h,
                tok => LogUnknown("Size", tok.ToString() ??""))) return false;
            _document.Width = w;
            _document.Height = h;
            return true;
        }

        private void ReadTabs(BlXmedTokenReader reader)
        {
            if (!reader.TryReadBooleansInC2(0x07, out var hasTabs, out var wrapOn,
                    tok => LogUnknown("Tabs", tok.ToString())))
                return;

            _document.AllowTabs = hasTabs;
            _document.IsWrapOff = !wrapOn; // Director: wrapOn → invert
        }

        private void ReadEditable(BlXmedTokenReader reader)
        {
            if (!reader.TryReadBooleanInC2(0x0B, out var editable, tok => LogUnknown("Editable", tok.ToString())))
                return;

            _document.IsEditable = editable;
        }
        public void ReadHeaderSpacing(BlXmedTokenReader reader)
        {
            if (!TryReadPair(reader, 0x04, out var line, out var extra)) return;
            if (line >= 0) _document.LineSpacing = line;
            // if needed later: _document.ExtraSpacing = extra;
        }
        private bool TryReadPair(BlXmedTokenReader reader, byte type, out int a, out int b)
               => reader.TryReadNumericPairInC2(type, out a, out b, tok => LogUnknown($"C2({type:X2})", $"{(tok?.IsPrefixedHex02() == true ? "02" : "..")}:{tok?.Ascii ?? "?"}"));
        private void LogUnknown(string category, string token)
        {
            _logger.LogDebug("XMED: {Category} unknown header token {Token}", category, token);
        }
    }
}
