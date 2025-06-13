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
        /// <summary>
        /// Key table establishing ownership between resources
        /// </summary>
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
        /// <summary>
        /// Raw cast library member data if available
        /// </summary>
        public FileCAStData? CASt { get; set; }
        /// <summary>
        /// Archive object giving access to raw chunks
        /// </summary>
        public Archive Archive { get; internal set; }
    }
}
