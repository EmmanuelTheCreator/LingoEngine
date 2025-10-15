using BlingoEngine.IO.Legacy.Texts.Data;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedTextBuilder
    {
        private readonly List<BlXmedToken> _textTokens = new();
        private readonly XmedDocument _document;
        public List<BlXmedToken> TextTokens => _textTokens;

        public XmedTextBuilder(XmedDocument document)
        {
            _document = document;
        }

        public void AddTextToken(BlXmedToken token)
        {
            if (token.IsTextBlock())
                _textTokens.Add(token);
        }

        public void BuildText()
        {
            if (_textTokens.Count == 0)
            {
                _document.Text = string.Empty;
                _document.TextLength = 0;
                return;
            }

            var builder = new StringBuilder();
            foreach (var token in _textTokens)
            {
                if (token.Data is { Length: > 0 } data)
                {
                    builder.Append(Encoding.ASCII.GetString(data));
                }
                else if (!string.IsNullOrEmpty(token.Ascii))
                {
                    builder.Append(token.Ascii);
                }
            }

            _document.Text = builder.ToString();
            _document.TextLength = _document.Text.Length;
        }
    }
}
