namespace BlingoEngine.IO.Legacy.Texts.Data
{
    public enum BlXmedTabAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Decimal = 4
    }
    /// <summary>Paragraph-level formatting descriptor.</summary>
    public sealed class XmedParagraphDescriptor
    {
        public int Start { get; set; }
        public int Length => Text.Length;
        public int End => Start + Length;
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }
        public int FirstLineIndent { get; set; }
        public int? AdditionalIndent { get; set; }
        public int SpacingBefore { get; set; }
        public int SpacingAfter { get; set; }
        public int LineSpacing { get; set; }
        public XmedAlignment Alignment { get; set; } = XmedAlignment.Left;
        public List<(int Position, BlXmedTabAlignment TabAlign)> TabStops { get; set; } = new();
        public string Text { get; set; } = "";

        public XmedParagraphDescriptor Clone()
        {
            var copy = new XmedParagraphDescriptor
            {
                Start = Start,
                LeftMargin = LeftMargin,
                RightMargin = RightMargin,
                FirstLineIndent = FirstLineIndent,
                AdditionalIndent = AdditionalIndent,
                SpacingBefore = SpacingBefore,
                SpacingAfter = SpacingAfter,
                Alignment = Alignment,
                LineSpacing = LineSpacing,
                TabStops = [.. TabStops],
                Text = Text
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
