
namespace LingoEngine.Texts
{
    public class LingoMemberText : LingoMember
    {
        /// <summary>
        /// The raw text content of the member.
        /// Lingo: the text of member
        /// </summary>
        public string Text
        {
            get => string.Join("\n", Paragraphs.Select(p => p.Text));
            set => ParseParagraphs(value);
        }

        /// <summary>
        /// List of parsed paragraphs.
        /// Lingo: the paragraphs of member
        /// </summary>
        public List<LingoParagraph> Paragraphs { get; private set; } = new();

        public LingoMemberText(int number, string name = "")
            : base(LingoMemberType.Text, number, name)
        {
        }

        private void ParseParagraphs(string text)
        {
            Paragraphs.Clear();
            foreach (var paragraphText in text.Split(new[] { '\n' }, StringSplitOptions.None))
            {
                var paragraph = new LingoParagraph(paragraphText);
                Paragraphs.Add(paragraph);
            }
        }
        public class LingoParagraph
        {
            public string Text { get; private set; }

            public List<LingoWord> Words { get; private set; }

            public LingoParagraph(string text)
            {
                Text = text;
                Words = text.Split([' '], StringSplitOptions.RemoveEmptyEntries)
                            .Select(word => new LingoWord(word))
                            .ToList();
            }

            public int WordCount => Words.Count;

            public LingoWord this[int index] => Words[index];
        }
        public class LingoWord
        {
            public string Value { get; private set; }

            public LingoWord(string word)
            {
                Value = word;
            }

            public override string ToString() => Value;
        }
    }

 
}
