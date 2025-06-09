namespace LingoEngine.Texts
{
    public class LingoParagraphs
    {
        private bool _hasParsed = false;
        private string _text;
        private string[] _lines = [];
        public string this[int index]
        {
            get
            {
                if (!_hasParsed) Parse();
                return _lines[index - 1];
            }
        }

        public override string ToString() => string.Join(Environment.NewLine, _lines);
        public int Count => _lines.Length;
        internal void SetText(string text)
        {
            _text = text;
            _hasParsed = false;
        }
        private void Parse()
        {
            _lines = _text.Split(new[] { '\n' }, StringSplitOptions.None);
            _hasParsed = true;
        }
    }

}

 

