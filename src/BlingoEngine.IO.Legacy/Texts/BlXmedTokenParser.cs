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
        private readonly XmedParagraphSliceBuilder _paragraphSliceBuilder;
        private readonly XmedParagraphDescriptorReader _descriptorReader;
        private readonly XmedRunSliceBuilder _slicerBuilder;
        private readonly XmedHeaderReader _headerReader;
        private readonly XmedFontTableReader _fontTableReader;
        private readonly BlXmedTokenReader _reader;

        public BlXmedTokenParser(ILogger logger, byte[] buffer, IReadOnlyList<BlXmedToken> tokens, IReadOnlyList<int> lastNumbers)
        {
            _logger = logger;
            _tokens = tokens ?? Array.Empty<BlXmedToken>();
            _reader = new BlXmedTokenReader(_tokens);
            _styleParser = new BlXmedTokenStyleParser(logger,_document);
            _spacingReader = new XmedSpacingReader(_document,logger);
            _paragraphSliceBuilder = new XmedParagraphSliceBuilder();
            _descriptorReader = new XmedParagraphDescriptorReader(_document, _spacingReader, logger);
            _slicerBuilder = new XmedRunSliceBuilder(_document, _styleParser, _descriptorReader, _paragraphSliceBuilder, logger);
            _headerReader = new XmedHeaderReader(_document, _styleParser, _spacingReader, _reader, logger);
            _fontTableReader = new XmedFontTableReader(_document);
        }

        public XmedDocument Parse(int directorVersion)
        {
            _document.DirectorVersion = directorVersion;
            _fontTableReader.Reset();

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
                    break;
                case XmedMainTokenGroup.MainGroupType.RunStyles:
                case XmedMainTokenGroup.MainGroupType.RunParagraphs:
                    break;
                case XmedMainTokenGroup.MainGroupType.Styles:
                    break;
                case XmedMainTokenGroup.MainGroupType.Paragraphs:
                    break;
                case XmedMainTokenGroup.MainGroupType.Fonts:
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

            return _document;
        }

      

      

     

        

      
       
        private void LogUnknown(string category, string token)
        {
            LogTrace(DiagnosticArea, _logger, "XMED: {Category} unknown token {Token}", category, token);
        }
    }
}
