using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Texts
{
    public abstract class LingoMemberTextBase<TFrameworkType> : LingoMember, ILingoMemberTextBase
        where TFrameworkType : ILingoFrameworkMemberTextBase
    {
        protected readonly TFrameworkType _frameworkMember;
        public T Framework<T>() where T : class, TFrameworkType => (T)_frameworkMember;

       /// <inheritdoc/>
        public string Text
        {
            get => _frameworkMember.Text;
            set
            {
                _frameworkMember.Text = value;
                UpdateText(value);
            }
        }
        private LingoLine _Line;
        private LingoWord _word = new LingoWord("");
        private LingoChar _char = new LingoChar();
        private LingoParagraph _Paragraph = new LingoParagraph();
        /// <inheritdoc/>
        public ILingoLine Line => _Line;
        /// <inheritdoc/>
        public ILingoWord Word => _word;
        /// <inheritdoc/>
        public LingoParagraph Paragraph => _Paragraph;
        /// <inheritdoc/>
        public ILingoChar Char => _char;

        public LingoMemberTextBase(LingoMemberType type, LingoCast cast, TFrameworkType frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(frameworkMember, type, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkMember = frameworkMember;
            _Line = new LingoLine(LineTextChanged); 
        }
        private void LineTextChanged() => Text = _Line.ToString();

        private void UpdateText(string text)
        {
            _char.SetText(text);
            _word.SetText(text);
            _Line.SetText(text);
            _Paragraph.SetText(text);
        }
    }

}

