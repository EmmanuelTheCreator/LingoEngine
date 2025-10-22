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
        public int BaselineOffset { get; set; }
        public int ParagraphWidth { get; set; }
        public int SpacingTopOffset { get; set; }
        public int SpacingBottomOffset { get; set; }
        public XmedAlignment Alignment { get; set; } = XmedAlignment.Left;
        public List<(int Position, BlXmedTabAlignment TabAlign)> TabStops { get; set; } = new();
        public string Text { get; set; } = "";
        public XmedParagraphFormatRecord? FormatRecord { get; set; }

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
                BaselineOffset = BaselineOffset,
                ParagraphWidth = ParagraphWidth,
                SpacingTopOffset = SpacingTopOffset,
                SpacingBottomOffset = SpacingBottomOffset,
                TabStops = [.. TabStops],
                Text = Text,
                FormatRecord = FormatRecord?.Clone()
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
            BaselineOffset = source.BaselineOffset;
            ParagraphWidth = source.ParagraphWidth;
            SpacingTopOffset = source.SpacingTopOffset;
            SpacingBottomOffset = source.SpacingBottomOffset;

            if (source.TabStops.Count > 0)
            {
                TabStops.Clear();
                TabStops.AddRange(source.TabStops);
            }

            if (source.FormatRecord != null)
                FormatRecord = source.FormatRecord.Clone();
        }
    }

    public sealed class XmedParagraphFormatRecord
    {
        public int EndOffset { get; set; }
        public int LeadingMargin { get; set; }
        public int Span { get; set; }
        public int Flags { get; set; }
        public int TrailingValue { get; set; }
        public int AlignmentCode { get; set; }

        public int FirstLineIndent
        {
            get
            {
                if (TrailingValue > LeadingMargin)
                    return TrailingValue - LeadingMargin;

                return 0;
            }
        }

        public XmedParagraphFormatRecord Clone()
        {
            return new XmedParagraphFormatRecord
            {
                EndOffset = EndOffset,
                LeadingMargin = LeadingMargin,
                Span = Span,
                Flags = Flags,
                TrailingValue = TrailingValue,
                AlignmentCode = AlignmentCode
            };
        }
    }
}
