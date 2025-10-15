
namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedSpacingReader
    {
        private readonly XmedDocument _document;
        private readonly List<(int Before, int After)> _paragraphSpacing = new();

        public XmedSpacingReader(XmedDocument document)
        {
            _document = document;
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

        public void ReadSpacing(BlXmedTokenReader reader)
        {
            reader.Skip();
            var values = reader.GetNumericValues();
            if (values.Count > 0 && values[0] >= 0)
                _document.LineSpacing = (uint)values[0];
        }

        public void ReadParagraphSpacing(BlXmedTokenReader reader)
        {
            reader.Skip();
            ReadParagraphSpacingInternal(reader);
        }

        private void ReadParagraphSpacingInternal(BlXmedTokenReader reader)
        {
            var values = reader.GetNumericValues();
            if (values.Count == 0) return;

            int before = values.ElementAtOrDefault(0);
            int after = values.ElementAtOrDefault(1);

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

       
    }
}
