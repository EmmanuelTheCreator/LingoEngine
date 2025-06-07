
using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Texts
{
    public class LingoMemberField : LingoMember
    {
        protected readonly ILingoFrameworkMemberField _frameworkMemberText;
        public T Framework<T>() where T : class, ILingoFrameworkMemberField => (T)_frameworkMemberText;

        /// <summary>
        /// The raw text content of the member.
        /// Lingo: the text of member
        /// </summary>
        public string Text
        {
            get => _frameworkMemberText.Text;
            set => _frameworkMemberText.Text = value;
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
        private LingoLine _Line = new LingoLine();
        private LingoWord _word = new LingoWord("");
        private LingoParagraph _Paragraph = new LingoParagraph();
        public ILingoLine Line => _Line;
        public ILingoWord Word => _word;
        public LingoParagraph Paragraph => _Paragraph;

        public LingoMemberField(LingoCast cast, ILingoFrameworkMemberField frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(frameworkMember, LingoMemberType.Text, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkMemberText = frameworkMember;
        }

        public void UpdateTextFromFW(string text)
        {
            _word.SetText(text);
            _Line.SetText(text);
            _Paragraph.SetText(text);
        }
    }

}

 

