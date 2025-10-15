using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenParser
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<BlXmedToken> _tokens;
        private readonly IReadOnlyList<int> _lastNumbers;
        private readonly byte[] _buffer;
        private readonly XmedDocument _document = new();

        private readonly BlXmedTokenStyleParser _styleParser;
        private readonly XmedSpacingReader _spacingReader;
        private readonly XmedTextBuilder _textBuilder;
        private readonly XmedParagraphSliceBuilder _paragraphSliceBuilder;
        private readonly XmedParagraphDescriptorReader _descriptorReader;
        private readonly XmedRunSliceBuilder _slicerBuilder;
        private readonly XmedHeaderReader _headerReader;
        private readonly BlXmedTokenReader _reader;

        public BlXmedTokenParser(ILogger logger, byte[] buffer, IReadOnlyList<BlXmedToken> tokens, IReadOnlyList<int> lastNumbers)
        {
            _logger = logger;
            _buffer = buffer;
            _tokens = tokens ?? Array.Empty<BlXmedToken>();
            _lastNumbers = lastNumbers ?? Array.Empty<int>();
            _reader = new BlXmedTokenReader(_tokens);
            _styleParser = new BlXmedTokenStyleParser(logger, _tokens);
            _spacingReader = new XmedSpacingReader(_document);
            _textBuilder = new XmedTextBuilder(_document);
            _paragraphSliceBuilder = new XmedParagraphSliceBuilder();
            _descriptorReader = new XmedParagraphDescriptorReader(_document, _styleParser, _spacingReader);
            _slicerBuilder = new XmedRunSliceBuilder(_document, _styleParser, _descriptorReader);
            _headerReader = new XmedHeaderReader(_document, _styleParser, _spacingReader, _descriptorReader, logger);
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;

            ReadHeader();
            _styleParser.GetOrCreateStyle(0);
            ParseBody();
            var textBuilder = new XmedTextBuilder(_document);
            textBuilder.BuildText();
            _descriptorReader.CollectParagraphDescriptorsFromTokens(_reader);
            _styleParser.CollectFontsFromTokens();
            _styleParser.FinalizeStyles(_document);
            _slicerBuilder.FinalizeRunsAndParagraphs();

            return _document;
        }

       
        private void ParseBody()
        {
            while (!_reader.IsAtEnd)
            {
                var token = _reader.Peek();
                if (token is null)
                {
                    break;
                }

                if (token.Type == TokenType.Block00)
                {
                    if (token.Value == 40)
                    {
                        _styleParser.ReadFonts(_reader);
                        continue;
                    }

                    if (token.Value == 44)
                    {
                        LogUnknown("Block", "0044");
                        _reader.Skip();
                        continue;
                    }

                    _textBuilder.AddTextToken(token);
                    _reader.Skip();
                    continue;
                }

                if (token.IsPrefixedHex03())
                {
                    var ascii = token.Ascii ?? string.Empty;
                    var type = ascii.Length >= 4 ? ascii.Substring(0, 4) : string.Empty;
                    switch (type)
                    {
                        case "0004":
                            _slicerBuilder.ReadRuns(_reader);
                            continue;
                        case "0005":
                            _paragraphSliceBuilder.ReadParagraphFlags(_reader);
                            continue;
                        case "0006":
                            _styleParser.ReadStyles(_reader);
                            continue;
                        case "0007":
                            LogUnknown("Block", "0007");
                            _reader.Skip();
                            continue;
                        case "0013":
                            LogUnknown("Block", "0013");
                            _reader.Skip();
                            continue;
                    }
                }

                if (token.IsC2())
                {
                    switch (token.TypeValue)
                    {
                        case 0x03:
                            _spacingReader.ReadParagraphSpacing(_reader);
                            continue;
                        case 0x04:
                            _spacingReader.ReadSpacing(_reader);
                            continue;
                        case 0x06:
                            LogUnknown("Block", "C206");
                            _reader.Skip();
                            continue;
                        case 0x07:
                            _styleParser.ReadTabs(_reader);
                            continue;
                        case 0x08:
                            LogUnknown("Block", "C208");
                            _reader.Skip();
                            continue;
                        case 0x0A:
                            _headerReader.ReadBox(_reader);
                            continue;
                        case 0x0B:
                            _styleParser.ReadEditable(_reader);
                            continue;
                        case 0x0F:
                            LogUnknown("Block", "C20F");
                            _reader.Skip();
                            continue;
                        case 0x12:
                            LogUnknown("Block", "C212");
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
                            _descriptorReader.ReadParagraphDescriptor(_reader);
                            continue;
                        case 0x04:
                            //_styleParser.ReadColor(_reader);
                            // TODO
                            continue;
                        case 0x1C:
                            _styleParser.MarkStyleFlag(style =>
                            {
                                style.Underline = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x04);
                            });
                            _reader.Skip();
                            continue;
                        case 0x1D:
                            _styleParser.MarkStyleFlag(style =>
                            {
                                style.Italic = true;
                                style.StyleFlags = (byte)(style.StyleFlags | 0x02);
                            });
                            _reader.Skip();
                            continue;
                    }
                }

                _reader.Skip();
            }
        }

        private void LogUnknown(string category, string token)
        {
            _logger?.LogDebug("XMED: {Category} unknown token {Token}", category, token);
        }
    }
}
