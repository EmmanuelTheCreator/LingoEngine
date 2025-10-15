
using Microsoft.Extensions.Logging;

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
            while (!_reader.IsAtEnd)
            {
                var token = _reader.Peek();
                if (token is null)
                    break;

                if (token.IsTextBlock())
                    break;

                if (token.IsPrefixedHex02() && token.Ascii is { } numeric)
                {
                    if (numeric.Equals("40001", StringComparison.OrdinalIgnoreCase) || numeric.Equals("40000", StringComparison.OrdinalIgnoreCase))
                        LogUnknown("Header", "02:40001");
                    else if (numeric.Equals("-7FFF6FE0", StringComparison.OrdinalIgnoreCase))
                        LogUnknown("Header", "02:-7FFF6FE0");

                    _reader.Skip();
                    continue;
                }

                if (token.IsPrefixedHex01() && token.Ascii is { } literal && literal.Equals("FFFF", StringComparison.OrdinalIgnoreCase))
                {
                    LogUnknown("Header", "01:FFFF");
                    _reader.Skip();
                    continue;
                }

                if (token.IsC2())
                {
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _spacingReader.ReadParagraphSpacing(_reader);
                            continue;
                        case 0x04:
                            _spacingReader.ReadHeaderSpacing(_reader);
                            continue;
                        case 0x06:
                            LogUnknown("Header", "C206");
                            _reader.Skip();
                            continue;
                        case 0x07:
                            _styleParser.ReadTabs(_reader);
                            continue;
                        case 0x08:
                            LogUnknown("Header", "C208");
                            _reader.Skip();
                            continue;
                        case 0x0A:
                            if (TryReadSize(_reader, out var size))
                            {
                                _document.Width = size!.Value.Width;
                                _document.Height = size!.Value.Height;
                            }

                            continue;
                        case 0x0B:
                            _styleParser.ReadEditable(_reader);
                            continue;
                        case 0x0F:
                            LogUnknown("Header", "C20F");
                            _reader.Skip();
                            continue;
                        case 0x12:
                            LogUnknown("Header", "C212");
                            _reader.Skip();
                            continue;
                    }
                }

                if (token.IsC1())
                {
                    _styleParser.TrackStyleMarker(token);
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            LogUnknown("Header", "C1C3");
                            _reader.Skip();
                            continue;
                        case 0x04:
                            
                            _styleParser.ReadHeaderColor(_reader);
                            continue;
                        case 0x1C:
                            _styleParser.MarkHeaderStyleFlag(style =>
                            {
                                style.Underline = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                            });
                            _reader.Skip();
                            continue;
                        case 0x1D:
                            _styleParser.MarkHeaderStyleFlag(style =>
                            {
                                style.Italic = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x02);
                            });
                            _reader.Skip();
                            continue;
                    }
                }

                LogUnknown("Header", $"Skipped token {_reader.Peek()}");
                _reader.Skip();
            }
        }


        public bool TryReadSize(BlXmedTokenReader reader, out (int Width, int Height)? size)
        {
            size = null;
            if (!reader.TryReadNumericPairInC2(0x0A, out var w, out var h,
                tok => LogUnknown("Size", tok.ToString() ??""))) return false;
            size = (w, h);
            return true;
        }
        private void LogUnknown(string category, string token)
        {
            _logger.LogDebug("XMED: {Category} unknown header token {Token}", category, token);
        }
    }
}
