
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


        public void InjectSpacings()
        {
            if (_paragraphSpacing.Count <= 0)
                return;
            
            var spacing = _paragraphSpacing.ToList();
            if (spacing.Count < _document.Paragraphs.Count)
            {
                int missing = _document.Paragraphs.Count - spacing.Count;
                spacing.InsertRange(0, Enumerable.Repeat((0, 0), missing));
            }
            else if (spacing.Count > _document.Paragraphs.Count)
                spacing = spacing.Skip(spacing.Count - _document.Paragraphs.Count).ToList();

            for (int i = 0; i < _document.Paragraphs.Count && i < spacing.Count; i++)
            {
                var (before, after) = spacing[i];
                _document.Paragraphs[i].SpacingBefore = before;
                _document.Paragraphs[i].SpacingAfter = after;
            }
        }

        private bool TryReadPair(BlXmedTokenReader reader, byte type, out int a, out int b)
                => reader.TryReadNumericPairInC2(type, out a, out b, tok => LogUnknown($"C2({type:X2})", $"{(tok?.IsPrefixedHex02() == true ? "02" : "..")}:{tok?.Ascii ?? "?"}"));

        public void ReadParagraphSpacing(BlXmedTokenReader reader)
        {
            if (!TryReadPair(reader, 0x03, out var before, out var after)) return;
            if (before >= -512 && before <= 0x2000 && after >= -512 && after <= 0x2000)
                _paragraphSpacing.Add((before, after));
        }

        private void LogUnknown(string category, string token)
        {
            _logger.LogDebug("XMED: {Category} unknown spacingReader token {Token}", category, token);
        }
    }
}
