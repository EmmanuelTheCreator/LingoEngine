using System;
using System.Collections.Generic;
using System.Linq;
using BlingoEngine.IO.Legacy.Core;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenParser
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<BlXmedTokenizer.Token> _tokens;
        private readonly IReadOnlyList<int> _lastNumbers;
        private readonly byte[] _buffer;
        private readonly XmedDocument _document = new();

        private readonly BlXmedTokenStyleParser _styleParser;
        private readonly BlXmedTokenRunParser _runParser;

        private int _index;

        public BlXmedTokenParser(ILogger logger, byte[] buffer, IReadOnlyList<BlXmedTokenizer.Token> tokens, IReadOnlyList<int> lastNumbers)
        {
            _logger = logger;
            _buffer = buffer;
            _tokens = tokens ?? Array.Empty<BlXmedTokenizer.Token>();
            _lastNumbers = lastNumbers ?? Array.Empty<int>();
            _styleParser = new BlXmedTokenStyleParser(logger, _tokens);
            _runParser = new BlXmedTokenRunParser(logger, _tokens, buffer, _document, _styleParser, _lastNumbers);
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;

            ReadHeader();
            _styleParser.GetOrCreateStyle(0);
            ParseBody();

            _runParser.BuildText();
            _runParser.CollectParagraphDescriptorsFromTokens();
            _styleParser.CollectFontsFromTokens();
            _styleParser.FinalizeStyles(_document);
            _runParser.FinalizeRunsAndParagraphs();

            return _document;
        }

        private void ReadHeader()
        {
            while (_index < _tokens.Count)
            {
                var token = _tokens[_index];

                if (token.IsTextBlock())
                {
                    break;
                }

                if (token.IsPrefixedHex02() && token.Ascii is { } numeric)
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
                        if (token.TryGetNumericValue(out var spacing) && spacing > 0)
                        {
                            _document.LineSpacing = (uint)spacing;
                        }
                    }
                    else if (_document.Width == 0 && token.TryGetNumericValue(out var widthValue) && widthValue > 0)
                    {
                        _document.Width = (uint)widthValue;
                    }

                    _index++;
                    continue;
                }

                if (token.IsPrefixedHex01() &&
                    token.Ascii is { } literal && literal.Equals("FFFF", StringComparison.OrdinalIgnoreCase))
                {
                    LogUnknown("Header", "01:FFFF");
                    _index++;
                    continue;
                }

                if (token.IsC2())
                {
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _runParser.ReadParagraphSpacing(ref _index);
                            continue;
                        case 0x04:
                            _runParser.ReadSpacing(ref _index);
                            continue;
                        case 0x06:
                            LogUnknown("Header", "C206");
                            _index++;
                            continue;
                        case 0x07:
                            _styleParser.ReadTabs(ref _index);
                            continue;
                        case 0x08:
                            LogUnknown("Header", "C208");
                            _index++;
                            continue;
                        case 0x0A:
                            _runParser.ReadBox(ref _index);
                            continue;
                        case 0x0B:
                            _styleParser.ReadEditable(ref _index);
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

                if (token.IsC1())
                {
                    _styleParser.TrackStyleMarker(token);
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _runParser.ReadParagraphDescriptor(ref _index);
                            continue;
                        case 0x04:
                            _styleParser.ReadColor(ref _index);
                            continue;
                        case 0x1C:
                            _styleParser.MarkStyleFlag(style =>
                            {
                                style.Underline = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                            });
                            _index++;
                            continue;
                        case 0x1D:
                            _styleParser.MarkStyleFlag(style =>
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
                        _styleParser.ReadFonts(ref _index);
                        continue;
                    }

                    if (token.Value == 44)
                    {
                        LogUnknown("Block", "0044");
                        _index++;
                        continue;
                    }

                    _runParser.AddTextToken(token);
                    _index++;
                    continue;
                }

                if (token.IsPrefixedHex03())
                {
                    var ascii = token.Ascii ?? string.Empty;
                    var type = ascii.Length >= 4 ? ascii.Substring(0, 4) : string.Empty;
                    switch (type)
                    {
                        case "0004":
                            _runParser.ReadRuns(ref _index);
                            continue;
                        case "0005":
                            _runParser.ReadParagraphFlags(ref _index);
                            continue;
                        case "0006":
                            _styleParser.ReadStyles(ref _index);
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

                if (token.IsC2())
                {
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _runParser.ReadParagraphSpacing(ref _index);
                            continue;
                        case 0x04:
                            _runParser.ReadSpacing(ref _index);
                            continue;
                        case 0x06:
                            LogUnknown("Block", "C206");
                            _index++;
                            continue;
                        case 0x07:
                            _styleParser.ReadTabs(ref _index);
                            continue;
                        case 0x08:
                            LogUnknown("Block", "C208");
                            _index++;
                            continue;
                        case 0x0A:
                            _runParser.ReadBox(ref _index);
                            continue;
                        case 0x0B:
                            _styleParser.ReadEditable(ref _index);
                            continue;
                        case 0x0F:
                            LogUnknown("Block", "C20F");
                            _index++;
                            continue;
                        case 0x12:
                            LogUnknown("Block", "C212");
                            _index++;
                            continue;
                    }
                }

                if (token.IsC1())
                {
                    _styleParser.TrackStyleMarker(token);
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _runParser.ReadParagraphDescriptor(ref _index);
                            continue;
                        case 0x04:
                            _styleParser.ReadColor(ref _index);
                            continue;
                        case 0x1C:
                            _styleParser.MarkStyleFlag(style =>
                            {
                                style.Underline = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                            });
                            _index++;
                            continue;
                        case 0x1D:
                            _styleParser.MarkStyleFlag(style =>
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

        private void LogUnknown(string category, string token)
        {
            _logger?.LogDebug("XMED: {Category} unknown token {Token}", category, token);
        }
    }
}
