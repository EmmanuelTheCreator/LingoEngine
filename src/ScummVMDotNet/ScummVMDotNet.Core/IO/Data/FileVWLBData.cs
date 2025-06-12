namespace Director.IO.Data
{
    /// <summary>
    /// Frame labels
    /// </summary>
    public class FileVWLBData
    {
        public List<LabelTimeLine> Labels { get; set; } = new();
        public class LabelTimeLine
        {
            public int Frame { get; set; }
            public string Label { get; set; } = "";
        }
    }
}
