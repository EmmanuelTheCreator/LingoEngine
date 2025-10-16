using System;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;
using static BlingoEngine.IO.Legacy.Texts.XmedDiagnostics;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class BlXmedTokenParser
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<BlXmedToken> _tokens;
        private readonly XmedDocument _document = new();

        private const XmedDiagnosticArea DiagnosticArea = XmedDiagnosticArea.TokenParser;
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
            _slicerBuilder = new XmedRunSliceBuilder(_document, _styleParser, _descriptorReader, _paragraphSliceBuilder, logger);
            _headerReader = new XmedHeaderReader(_document, _styleParser, _spacingReader, _reader, logger);
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;
            _headerReader.ReadHeader();
            
            _styleParser.GetOrCreateStyle(0);
            ReadTextSection();
            ReadRunSection();
            ReadStyleSection();
            ReadFooterSection();
            _textBuilder.BuildText();
            _styleParser.FinalizeStyles(_document);
            _slicerBuilder.FinalizeRunsAndParagraphs();

            return _document;
        }

        private void ReadTextSection()
        {
            while (!_reader.IsAtEnd)
            {
                var token = _reader.Peek();
                if (token is null)
                    break;

                if (IsRunBlock(token) || IsStyleBlock(token) || IsFooterBlock(token))
                    return;

                if (token.Type == TokenType.Block00)
                {
                    if (token.Value == 40) { _styleParser.ReadFonts(_reader); continue; }
                    if (token.Value == 44) { LogUnknown("Text", "0044"); _reader.Skip(); continue; }
                    _textBuilder.AddTextToken(token); _reader.Skip(); continue;
                }

                if (token.IsCompositeC1(0x03)) { _descriptorReader.TryExtractParagraphDescriptor(_reader, out _); continue; }
                if (token.IsCompositeC2(0x03)) { _spacingReader.ReadParagraphSpacing(_reader); continue; }

                if (token.IsC2())
                {
                    LogC2Fallback(token, "Text");
                    continue;
                }

                LogUnknown("Text", $"Skipped token {token}");
                _reader.Skip();
            }
        }

        private void ReadRunSection()
        {
            while (!_reader.IsAtEnd)
            {
                var token = _reader.Peek();
                if (token is null)
                    break;

                if (IsStyleBlock(token) || IsFooterBlock(token))
                    return;

                if (IsRunBlock(token))
                {
                    _slicerBuilder.ReadRuns(_reader);
                    continue;
                }

                if (token.IsPrefixedHex03())
                {
                    var id = token.Ascii ?? string.Empty;
                    if (id.StartsWith("0005", StringComparison.OrdinalIgnoreCase))
                    {
                        _paragraphSliceBuilder.ReadParagraphFlags(_reader);
                        continue;
                    }

                    LogUnknown("Runs", id.Length >= 4 ? id[..4] : id);
                    _reader.Skip();
                    continue;
                }

                LogUnknown("Runs", $"Skipped token {token}");
                _reader.Skip();
            }
        }

        private void ReadStyleSection()
        {
            while (!_reader.IsAtEnd)
            {
                var token = _reader.Peek();
                if (token is null)
                    break;

                if (IsFooterBlock(token))
                    return;

                if (IsStyleBlock(token))
                {
                    _styleParser.ReadStyles(_reader);
                    continue;
                }

                if (token.IsPrefixedHex03())
                {
                    var id = token.Ascii ?? string.Empty;
                    LogUnknown("Styles", id.Length >= 4 ? id[..4] : id);
                    _reader.Skip();
                    continue;
                }

                LogUnknown("Styles", $"Skipped token {token}");
                _reader.Skip();
            }
        }

        private void ReadFooterSection()
        {
            while (!_reader.IsAtEnd)
            {
                var token = _reader.Peek();
                if (token is null)
                    break;

                if (IsFooterBlock(token))
                {
                    _reader.Skip();
                    continue;
                }

                if (TryConsumeFooterStyleColors())
                    continue;

                LogUnknown("Footer", $"Skipped token {token}");
                _reader.Skip();
            }
        }

        private bool TryConsumeFooterStyleColors()
        {
            var token = _reader.Peek();
            if (token is null)
                return false;

            if (!token.IsPrefixedHex01() || !token.TryGetNumericValue(out var styleId) || styleId < 0 || styleId > byte.MaxValue)
                return false;

            var next = _reader.Peek(1);
            if (next is null || !next.IsCompositeC1(0x03) && !next.IsCompositeC1(0x04))
                return false;

            LogTrace(DiagnosticArea, _logger, "XMED footer: dispatching trailing inline colors for style {StyleId}", styleId);
            _styleParser.GetOrCreateStyle(styleId);
            _styleParser.ConsumeTrailingInlineColors(_reader, styleId);
            return true;
        }

        private void LogC2Fallback(BlXmedToken token, string section)
        {
            switch (token.TypeValue)
            {
                case 0x03:
                    _spacingReader.ReadParagraphSpacing(_reader);
                    return;
                case 0x04:
                    LogUnknown(section, "C2:04 (unused spacing)");
                    _reader.Skip();
                    return;
                case 0x06:
                    LogUnknown(section, "C2:06");
                    _reader.Skip();
                    return;
                case 0x07:
                    LogUnknown(section, "C2:07 (tabs not implemented)");
                    _reader.Skip();
                    return;
                case 0x08:
                    LogUnknown(section, "C2:08");
                    _reader.Skip();
                    return;
                case 0x0A:
                    LogUnknown(section, "C2:0A (header box not implemented)");
                    _reader.Skip();
                    return;
                case 0x0B:
                    LogUnknown(section, "C2:0B (editable flag not implemented)");
                    _reader.Skip();
                    return;
                case 0x0F:
                    LogUnknown(section, "C2:0F");
                    _reader.Skip();
                    return;
                case 0x12:
                    LogUnknown(section, "C2:12");
                    _reader.Skip();
                    return;
            }

            LogUnknown(section, $"Unhandled C2 {token}");
            _reader.Skip();
        }

        private static bool IsRunBlock(BlXmedToken token)
        {
            return token.IsPrefixedHex03() && token.Ascii is { Length: > 0 } ascii && ascii.StartsWith("0004", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStyleBlock(BlXmedToken token)
        {
            return token.IsPrefixedHex03() && token.Ascii is { Length: > 0 } ascii && ascii.StartsWith("0006", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFooterBlock(BlXmedToken token)
        {
            return token.IsPrefixedHex03() && token.Ascii is { Length: > 0 } ascii && ascii.StartsWith("0008", StringComparison.OrdinalIgnoreCase);
        }

        private void LogUnknown(string category, string token)
        {
            LogTrace(DiagnosticArea, _logger, "XMED: {Category} unknown token {Token}", category, token);
        }
    }
}
