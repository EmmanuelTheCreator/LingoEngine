using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenParser
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<BlXmedToken> _tokens;
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
            _tokens = tokens ?? Array.Empty<BlXmedToken>();
            _reader = new BlXmedTokenReader(_tokens);
            _styleParser = new BlXmedTokenStyleParser(logger,_document);
            _spacingReader = new XmedSpacingReader(_document,logger);
            _textBuilder = new XmedTextBuilder(_document);
            _paragraphSliceBuilder = new XmedParagraphSliceBuilder();
            _descriptorReader = new XmedParagraphDescriptorReader(_document, _styleParser, _spacingReader, logger);
            _slicerBuilder = new XmedRunSliceBuilder(_document, _styleParser, _descriptorReader,logger);
            _headerReader = new XmedHeaderReader(_document, _styleParser, _spacingReader, _reader, logger);
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;
            _headerReader.ReadHeader();
            
            _styleParser.GetOrCreateStyle(0);
            ParseBody();
            var textBuilder = new XmedTextBuilder(_document);
            textBuilder.BuildText();
            _styleParser.FinalizeStyles(_document);
            _slicerBuilder.FinalizeRunsAndParagraphs();

            return _document;
        }


        private void ParseBody()
        {
            while (!_reader.IsAtEnd)
            {
                var t = _reader.Peek();
                if (t is null) break;

                // Text
                if (t.Type == TokenType.Block00)
                {
                    if (t.Value == 40) { _styleParser.ReadFonts(_reader); continue; }
                    if (t.Value == 44) { LogUnknown("Block", "0044"); _reader.Skip(); continue; }
                    _textBuilder.AddTextToken(t); _reader.Skip(); continue;
                }

                // Routed by 03-prefixed ids
                if (t.IsPrefixedHex03())
                {
                    var s = t.Ascii ?? string.Empty;
                    var id = s.Length >= 4 ? s[..4] : string.Empty;
                    if (id == "0004") { _slicerBuilder.ReadRuns(_reader); continue; }
                    if (id == "0005") { _paragraphSliceBuilder.ReadParagraphFlags(_reader); continue; }
                    if (id == "0006") { _styleParser.ReadStyles(_reader); continue; }
                    LogUnknown("Block", id); _reader.Skip(); continue;
                }

                // Direct composites
                if (t.IsCompositeC1(0x03)) { _descriptorReader.TryExtractParagraphDescriptor(_reader, out _); continue; }
                if (t.IsCompositeC1(0x04)) { _slicerBuilder.ReadRuns(_reader); continue; }
                if (t.IsCompositeC2(0x03)) { _spacingReader.ReadParagraphSpacing(_reader); continue; }

                // Fallback handlers
                if (t.IsC2() && !ParseC2(t)) continue;
                if (t.IsC1() && !ParseC1(t)) continue;

                LogUnknown("Body", $"Skipped token {_reader.Peek()}");
                _reader.Skip();
            }
        }


        private bool ParseC1(BlXmedToken t)
        {
            if (t == null) return false;

            if (t.IsCompositeC1(0x03))
            {
                _descriptorReader.TryExtractParagraphDescriptor(_reader, out _);
                return true;
            }

            if (t.IsCompositeC1(0x04))
            {
                _slicerBuilder.ReadRuns(_reader);
                return true;
            }
            switch (t.TypeValue)
            {
                case 0x03: _descriptorReader.TryExtractParagraphDescriptor(_reader, out _); return false;
                case 0x04: _slicerBuilder.ReadRuns(_reader); return false;
            }

            LogUnknown("C1", t.ToString());
            _reader.Skip();
            return true;
        }


        private bool ParseC2(BlXmedToken token)
        {
            switch (token.TypeValue)
            {
                case 0x03:
                    _spacingReader.ReadParagraphSpacing(_reader);
                    return false; // handled

                case 0x04:
                    LogUnknown("C2", "04 (unused spacing)");
                    return true;

                case 0x06:
                    LogUnknown("C2", "06");
                    _reader.Skip();
                    return false;

                case 0x07:
                    LogUnknown("C2", "07 (tabs not implemented)");
                    return true;

                case 0x08:
                    LogUnknown("C2", "08");
                    _reader.Skip();
                    return false;

                case 0x0A:
                    LogUnknown("C2", "0A (header box not implemented)");
                    return true;

                case 0x0B:
                    LogUnknown("C2", "0B (editable flag not implemented)");
                    return true;

                case 0x0F:
                    LogUnknown("C2", "0F");
                    _reader.Skip();
                    return false;

                case 0x12:
                    LogUnknown("C2", "12");
                    _reader.Skip();
                    return false;
            }

            LogUnknown("C2", $"Unhandled {token}");
            return true;
        }


        private void LogUnknown(string category, string token)
        {
            _logger?.LogDebug("XMED: {Category} unknown token {Token}", category, token);
        }
    }
}
