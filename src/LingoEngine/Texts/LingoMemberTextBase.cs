using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Texts
{
    public interface ILingoMemberTextBaseInteral: ILingoMemberTextBase
    {
        void LoadFile();
    }
    public abstract class LingoMemberTextBase<TFrameworkType> : LingoMember, ILingoMemberTextBase, ILingoMemberTextBaseInteral
        where TFrameworkType : ILingoFrameworkMemberTextBase
    {
        protected readonly TFrameworkType _frameworkMember;
        protected int _selectionStart;
        protected int _selectionEnd;
        protected string _selectedText = "";

        protected LingoLines _Line;
        protected LingoWords _word = new LingoWords("");
        protected LingoChars _char = new LingoChars();
        protected LingoParagraphs _Paragraph = new LingoParagraphs();

        public T Framework<T>() where T : class, TFrameworkType => (T)_frameworkMember;

       /// <inheritdoc/>
        public string Text
        {
            get => _frameworkMember.Text;
            set
            {
                UpdateText(value);
                _frameworkMember.Text = value;
            }
        }
       
        /// <inheritdoc/>
        public LingoLines Line => _Line;
        /// <inheritdoc/>
        public LingoWords Word => _word;
        /// <inheritdoc/>
        public LingoParagraphs Paragraph => _Paragraph;
        /// <inheritdoc/>
        public LingoChars Char => _char;

        public LingoMemberTextBase(LingoMemberType type, LingoCast cast, TFrameworkType frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(frameworkMember, type, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkMember = frameworkMember;
            _Line = new LingoLines(LineTextChanged); 
        }
        private void LineTextChanged() => Text = _Line.ToString();

        private void UpdateText(string text)
        {
            _char.SetText(text);
            _word.SetText(text);
            _Line.SetText(text);
            _Paragraph.SetText(text);
        }



        public virtual void LoadFile()
        {
#if DEBUG
            if (FileName.Contains("blockT"))
            {
            }
#endif
            Text = _frameworkMember.ReadText();
           
        }


        public virtual void Clear()
        {
            Text = "";
        }

        public virtual void Copy()
        {
            if (_selectionStart > 0 && _selectionEnd >= _selectionStart)
                _frameworkMember.Copy(_selectedText);
        }

        public virtual void Cut()
        {
            if (_selectionStart > 0 && _selectionEnd >= _selectionStart)
            {
                _frameworkMember.Copy(_selectedText);
                Text = Text.Remove(_selectionStart - 1, _selectionEnd - _selectionStart);
                _selectedText = "";
                _selectionEnd = _selectionStart;
            }
        }

        public virtual void Paste()
        {
            var pasteText = _frameworkMember.PasteClipboard();
            InsertText(pasteText);
        }

        public virtual void InsertText(string text)
        {
            var caret = _selectionEnd;
            Text = Text.Insert(caret - 1, text);
            _selectionStart = caret + text.Length;
            _selectionEnd = _selectionStart;
        }

        public virtual void ReplaceSelection(string replacement)
        {
            if (_selectionStart > 0 && _selectionEnd >= _selectionStart)
            {
                Text = Text.Remove(_selectionStart - 1, _selectionEnd - _selectionStart)
                           .Insert(_selectionStart - 1, replacement);
                _selectionStart += replacement.Length;
                _selectionEnd = _selectionStart;
            }
        }

        public virtual void SetSelection(int start, int end)
        {
            _selectionStart = start;
            _selectionEnd = end;
            _selectedText = Text.Substring(start - 1, end - start);
        }
    }

}

