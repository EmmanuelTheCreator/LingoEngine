using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;
using LingoEngine.Texts.FrameworkCommunication;
using LingoEngine.Tools;

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


        #region Properties

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
        public int ScrollTop
        {
            get => _frameworkMember.ScrollTop;
            set => _frameworkMember.ScrollTop = value;
        }
        /// <inheritdoc/>
        public bool Editable { get; set; }
        /// <inheritdoc/>
        public bool WordWrap
        {
            get => _frameworkMember.WordWrap;
            set => _frameworkMember.WordWrap = value;
        }
        /// <inheritdoc/>
        public string Font
        {
            get => _frameworkMember.FontName;
            set => _frameworkMember.FontName = value;
        }
        /// <inheritdoc/>
        public int FontSize
        {
            get => _frameworkMember.FontSize;
            set => _frameworkMember.FontSize = value;
        }

        /// <inheritdoc/>
        public LingoColor TextColor
        {
            get => _frameworkMember.TextColor;
            set => _frameworkMember.TextColor = value;
        }
        /// <inheritdoc/>
        public LingoTextStyle FontStyle
        {
            get => _frameworkMember.FontStyle;
            set => _frameworkMember.FontStyle = value;
        }
        /// <inheritdoc/>
        public bool Bold
        {
            get => (_frameworkMember.FontStyle & LingoTextStyle.Bold) != 0;
            set
            {
                var style = _frameworkMember.FontStyle;
                if (value)
                    style |= LingoTextStyle.Bold;
                else
                    style &= ~LingoTextStyle.Bold;
                _frameworkMember.FontStyle = style;
            }
        }
        /// <inheritdoc/>
        public bool Italic
        {
            get => (_frameworkMember.FontStyle & LingoTextStyle.Italic) != 0;
            set
            {
                var style = _frameworkMember.FontStyle;
                if (value)
                    style |= LingoTextStyle.Italic;
                else
                    style &= ~LingoTextStyle.Italic;
                _frameworkMember.FontStyle = style;
            }
        }
        /// <inheritdoc/>
        public bool Underline
        {
            get => (_frameworkMember.FontStyle & LingoTextStyle.Underline) != 0;
            set
            {
                var style = _frameworkMember.FontStyle;
                if (value)
                    style |= LingoTextStyle.Underline;
                else
                    style &= ~LingoTextStyle.Underline;
                _frameworkMember.FontStyle = style;
            }
        }
        /// <inheritdoc/>
        public LingoTextAlignment Alignment
        {
            get => _frameworkMember.Alignment;
            set => _frameworkMember.Alignment = value;
        }
        /// <inheritdoc/>
        public int Margin
        {
            get => _frameworkMember.Margin;
            set => _frameworkMember.Margin = value;
        }

        /// <inheritdoc/>
        public LingoLines Line => _Line;
        /// <inheritdoc/>
        public LingoWords Word => _word;
        /// <inheritdoc/>
        public LingoParagraphs Paragraph => _Paragraph;
        /// <inheritdoc/>
        public LingoChars Char => _char;
        #endregion



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
            var rtf = _frameworkMember.ReadTextRtf();
            if (!string.IsNullOrWhiteSpace(rtf))
            {
                var rtfInfo = RtfExtracter.Parse(rtf);
                if (rtfInfo != null)
                {
                    if (rtfInfo.Size > 0) FontSize = rtfInfo.Size;
                    if (rtfInfo.Color != null) TextColor = rtfInfo.Color.Value;
                    if (!string.IsNullOrWhiteSpace(rtfInfo.FontName)) Font = rtfInfo.FontName;
                }
            }
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

