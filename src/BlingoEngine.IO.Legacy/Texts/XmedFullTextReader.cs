using System.Text;
using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedFullTextReader
    {
        private readonly XmedDocument _document;

        public XmedFullTextReader(XmedDocument document)
        {
            _document = document;
        }

        public void Reset()
        {
            _document.Text = string.Empty;
            _document.TextLength = 0;
        }

        public void ReadText(XmedTokenGroup? group)
        {
            Reset();

            if (group == null || group.Items.Count == 0)
                return;

            var candidate = ExtractToken(group, 1) ?? ExtractToken(group, 0);
            if (candidate == null)
                return;

            if (!string.IsNullOrEmpty(candidate.Ascii))
                _document.Text = candidate.Ascii!;
            else if (candidate.Data is { Length: > 0 })
                _document.Text = Encoding.ASCII.GetString(candidate.Data);

            _document.TextLength = _document.Text.Length;
        }

        private static BlXmedToken? ExtractToken(XmedTokenGroup group, int index)
        {
            if (index < 0 || index >= group.Items.Count)
                return null;

            return group.Items[index] as BlXmedToken;
        }
    }
}
