using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly XmedParagraphDescriptorReader _descriptorReader;
        private readonly XmedSliceBuilder _styleSliceBuilder;
        private readonly XmedSliceBuilder _paragraphSliceBuilder;
        private readonly XmedHeaderReader _headerReader;
        private readonly XmedFontTableReader _fontTableReader;
        private readonly XmedFullTextReader _fullTextReader;
        private readonly BlXmedTokenReader _reader;

        public BlXmedTokenParser(ILogger logger, byte[] buffer, IReadOnlyList<BlXmedToken> tokens, IReadOnlyList<int> lastNumbers)
        {
            _logger = logger;
            _tokens = tokens ?? Array.Empty<BlXmedToken>();
            _reader = new BlXmedTokenReader(_tokens);
            _styleParser = new BlXmedTokenStyleParser(logger, _document);
            _spacingReader = new XmedSpacingReader(_document, logger);
            _descriptorReader = new XmedParagraphDescriptorReader(_document, _spacingReader, logger);
            _styleSliceBuilder = new XmedSliceBuilder();
            _paragraphSliceBuilder = new XmedSliceBuilder();
            _headerReader = new XmedHeaderReader(_document, _styleParser, _spacingReader, _reader, logger);
            _fontTableReader = new XmedFontTableReader(_document);
            _fullTextReader = new XmedFullTextReader(_document);
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;
            _fontTableReader.Reset();

            _descriptorReader.Reset();
            _styleSliceBuilder.Reset();
            _paragraphSliceBuilder.Reset();
            _fullTextReader.Reset();
            _styleParser.Reset();

            var tokenList = _tokens as List<BlXmedToken> ?? _tokens.ToList();
            var groups = BlXmedTokenizer.CreateGroups(tokenList);
            foreach (var group in groups)
            {
                switch (group.MainType)
                {
                    case XmedMainTokenGroup.MainGroupType.RunHeaderFFFF:
                        break;
                    case XmedMainTokenGroup.MainGroupType.RunHeader:
                    case XmedMainTokenGroup.MainGroupType.Layout:
                        break;
                    case XmedMainTokenGroup.MainGroupType.FullText:
                        _fullTextReader.ReadText(group);
                        break;
                    case XmedMainTokenGroup.MainGroupType.RunStyles:
                        _styleSliceBuilder.LoadBoundaries(group);
                        break;
                    case XmedMainTokenGroup.MainGroupType.RunParagraphs:
                        _paragraphSliceBuilder.LoadBoundaries(group);
                        break;
                    case XmedMainTokenGroup.MainGroupType.Styles:
                        _styleParser.LoadStyles(group);
                        break;
                    case XmedMainTokenGroup.MainGroupType.Paragraphs:
                        break;
                    case XmedMainTokenGroup.MainGroupType.Fonts:
                        _fontTableReader.ReadFontTable(group);
                        break;
                    case XmedMainTokenGroup.MainGroupType.SpacingDescriptor:
                    case XmedMainTokenGroup.MainGroupType.SpacingDescriptor2:
                    case XmedMainTokenGroup.MainGroupType.UnknownB:
                    case XmedMainTokenGroup.MainGroupType.UnknownC:
                    case XmedMainTokenGroup.MainGroupType.UnknownF:
                    case XmedMainTokenGroup.MainGroupType.Unknown13:
                    case XmedMainTokenGroup.MainGroupType.Unknown128:
                    case XmedMainTokenGroup.MainGroupType.Unknown129:
                        break;
                    default:
                        break;
                }
            }

            var paragraphSlices = _paragraphSliceBuilder.BuildSlices(_document.Text);
            _descriptorReader.ApplyParagraphRuns(paragraphSlices);
            _descriptorReader.BuildParagraphs();

            var runSlices = _styleSliceBuilder.BuildSlices(_document.Text);
            _styleParser.BuildRuns(_document, runSlices);
            _styleParser.FinalizeStyles(_document);

            return _document;
        }

      

      

     

        

      
       
        private void LogUnknown(string category, string token)
        {
            LogTrace(DiagnosticArea, _logger, "XMED: {Category} unknown token {Token}", category, token);
        }
    }
}
