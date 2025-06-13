
using Director.Graphics;

namespace Director.IO.Data
{
    /// <summary>
    /// In older versions of Director, the ChunkID for the Config was VWCF, but sometime after Director 6 it was replaced with DRCF. 
    /// They are both exactly identical, but DRCF cannot be loaded in older Director versions. The VW stands for VideoWorks and the DR stands for Director.
    /// </summary>
    public class FileDRCFData
    {
        public List<byte> BgColor { get; internal set; }
        public sbyte Tempo { get; internal set; }
        public Rect SourceRect { get; internal set; }
        public ushort FileVersion { get; internal set; }
        public short Size { get; internal set; }
        public short MovieFileVersion { get; internal set; }
        public short OldDefaultPalette { get; internal set; }
        public int DefaultPalette { get; internal set; }
        public int DownloadFramesBeforePlaying { get; internal set; }
        public bool ProtectedMovie { get; internal set; }
    }
}
