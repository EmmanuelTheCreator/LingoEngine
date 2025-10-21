
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedSpacingReader
    {
        private readonly XmedDocument _document;
        private readonly ILogger _logger;
        private readonly List<(int Before, int After)> _paragraphSpacing = new();

        public XmedSpacingReader(XmedDocument document, ILogger logger)
        {
            _document = document;
            _logger = logger;
        }
        internal void Reset()
        {
            _paragraphSpacing.Clear();
        }


       


    }
}
