namespace BlingoEngine.IO.Legacy.Texts.Data
{
    /// <summary>Paragraph-level formatting descriptor.</summary>
    public sealed class XmedParagraphDescriptor
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public int End => Start + Length;
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }
        public int FirstLineIndent { get; set; }
        public int? AdditionalIndent { get; set; }
        public int SpacingBefore { get; set; }
        public int SpacingAfter { get; set; }
        public XmedAlignment Alignment { get; set; } = XmedAlignment.Left;
        public List<int> TabStops { get; } = new();
        public string Text { get; set; } = "";

        public XmedParagraphDescriptor Clone()
        {
            var copy = new XmedParagraphDescriptor
            {
                Start = Start,
                Length = Length,
                LeftMargin = LeftMargin,
                RightMargin = RightMargin,
                FirstLineIndent = FirstLineIndent,
                AdditionalIndent = AdditionalIndent,
                SpacingBefore = SpacingBefore,
                SpacingAfter = SpacingAfter,
                Alignment = Alignment
            };

            if (TabStops.Count > 0)
            {
                copy.TabStops.AddRange(TabStops);
            }

            return copy;
        }
        public void ParseValuesFrom(XmedParagraphDescriptor source)
        {
            LeftMargin = source.LeftMargin;
            RightMargin = source.RightMargin;
            FirstLineIndent = source.FirstLineIndent;
            AdditionalIndent = source.AdditionalIndent;
            Alignment = source.Alignment;

            if (source.TabStops.Count > 0)
            {
                TabStops.Clear();
                TabStops.AddRange(source.TabStops);
            }
        }
    }
}
