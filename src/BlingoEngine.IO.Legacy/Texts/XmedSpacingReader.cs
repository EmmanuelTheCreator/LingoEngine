
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

        #region Header
        public void ReadHeaderSpacing(BlXmedTokenReader reader)
        {
            if (!TryReadPair(reader, 0x04, out var line, out var extra)) return;
            if (line >= 0) _document.LineSpacing = line;
            // if needed later: _document.ExtraSpacing = extra;
        } 
        #endregion


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

       

        public bool TryReadParagraphSpacing(BlXmedTokenReader reader, out (int Before, int After)? spacing)
        {
            spacing = null;

            // Expect a spacing block: C2(03)
            if (!reader.Peek()?.IsCompositeC2(0x03) ?? true)
                return false;

            reader.ReadNext(); // consume C2(03)
            var values = new List<int>();

            while (!reader.IsAtEnd)
            {
                var t = reader.Peek();
                if (t is null) break;

                if (t.IsPrefixedHex02() && t.TryGetNumericValue(out var v))
                {
                    values.Add(v);
                    reader.ReadNext();
                    continue;
                }

                if (t.IsFieldSeparator()) { reader.ReadNext(); continue; }
                if (t.IsFieldTerminator()) { reader.ReadNext(); break; }
                if (t.IsBlockBoundary()) break;

                reader.ReadNext();
            }

            if (values.Count < 2)
                return false;

            int before = values[0];
            int after = values[1];
            if (before >= -512 && before <= 0x2000 && after >= -512 && after <= 0x2000)
                spacing = (before, after);

            return true;
        }

        private void LogUnknown(string category, string token)
        {
            _logger.LogDebug("XMED: {Category} unknown spacingReader token {Token}", category, token);
        }
    }
}
