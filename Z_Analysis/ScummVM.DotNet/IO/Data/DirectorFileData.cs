namespace Director.IO.Data
{
    public class DirectorFileData
    {
        /// <summary>
        /// Input map
        /// </summary>
        public FileIMapData? IMap { get; set; }
        /// <summary>
        /// Memory map
        /// </summary>
        public FileMMapData? MMap { get; set; }
        public FileKeyStarData? KeyStar { get; set; }
        /// <summary>
        /// Movie information
        /// </summary>
        public FileDRCFData? DRCF { get; set; }
        /// <summary>
        /// Score information
        /// </summary>
        public FileVWSCData? VWSC { get; set; }
        /// <summary>
        /// Frame labels
        /// </summary>
        public FileVWLBData? VWLB { get; set; }
        public Archive Archive { get; internal set; }
    }
}
