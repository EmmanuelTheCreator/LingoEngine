
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
           
        }


       
    }
}
