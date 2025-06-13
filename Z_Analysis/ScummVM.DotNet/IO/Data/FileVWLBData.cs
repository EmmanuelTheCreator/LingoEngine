namespace Director.IO.Data
{
    /// <summary>
    /// Frame labels
    /// </summary>
    public class FileVWLBData
    {
        /// <summary>List of frame labels defined in the movie.</summary>
        public List<LabelTimeLine> Labels { get; set; } = new();

        public class LabelTimeLine
        {
            /// <summary>Frame number at which the label appears.</summary>
            public int Frame { get; set; }
            /// <summary>Text of the label.</summary>
            public string Label { get; set; } = "";
        }
    }
}
