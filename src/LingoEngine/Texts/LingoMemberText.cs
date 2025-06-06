
using LingoEngine.Core;
using LingoEngine.Primitives;
using System.Data;

namespace LingoEngine.Texts
{
    public interface ILingoWord
    {
        string this[int index] { get; }

        public int Count { get; }
    }
    public interface ILingoLine
    {
        string this[int index] { get; }
        public int Count { get; }
    } 
    public interface ILingoParagraph
    {
        string this[int index] { get; }
        public int Count { get; }
    }
    public class LingoMemberText : LingoMember
    {
        protected readonly ILingoFrameworkMemberText _frameworkMemberText;
        public T Framework<T>() where T : class, ILingoFrameworkMemberText => (T)_frameworkMemberText;

        /// <summary>
        /// The raw text content of the member.
        /// Lingo: the text of member
        /// </summary>
        public string Text
        {
            get => _frameworkMemberText.Text;
            set
            {
                _frameworkMemberText.Text = value;
                UpdateParagraphs(value);
            }
        }

        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new LingoMemberText(_cast, _lingoFrameworkMember, newNumber, Name);
            //clone.Text = Text;
            //return clone;
        }
        /// <summary>
        /// List of parsed paragraphs.
        /// Lingo: the paragraphs of member
        /// </summary>
        private LingoLine _Line  = new LingoLine();
        private LingoWord _word = new LingoWord("");
        private LingoParagraph _Paragraph = new LingoParagraph();
        public ILingoLine Line => _Line;
        public ILingoWord Word => _word;
        public LingoParagraph Paragraph => _Paragraph;

        public LingoMemberText(LingoCast cast, ILingoFrameworkMemberText frameworkMember, int number, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(frameworkMember, LingoMemberType.Text, cast, number, name, fileName,regPoint)
        {
            _frameworkMemberText = frameworkMember;
        }

        public void UpdateParagraphs(string text)
        {
            _word.SetText(text);
            _Line.SetText(text);
            _Paragraph.SetText(text);
        }
        public class LingoParagraph : ILingoParagraph
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
        public class LingoWord : ILingoWord
        {
            private bool _hasParsed = false;
            private string _text = "";
            private string[] _words = [];
            public string this[int index]
            {
                get
                {
                    if (!_hasParsed) Parse();
                    return _words[index - 1];
                }
            }

            public int Count => _words.Length;
            public LingoWord(string text)
            {
                SetText(text);
            }

            public override string ToString() => string.Join(Environment.NewLine, _words);

            internal void SetText(string text)
            {
                _text = text;
                _hasParsed = false;
                
            }
            private void Parse()
            {
                _words = _text.Split([' ', '.', '?', '!', ';', ':', '=', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
                           .Where(word => word != "")
                           .ToArray();
                _hasParsed = true;
            }
        }

       
        public class LingoLine : ILingoLine
        {
            private bool _hasParsed = false;
            private string _text = "";
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

 
}
