
using Director.Graphics;

namespace Director.IO.Data
{
    /// <summary>
    /// In older versions of Director, the ChunkID for the Config was VWCF, but sometime after Director 6 it was replaced with DRCF. 
    /// They are both exactly identical, but DRCF cannot be loaded in older Director versions. The VW stands for VideoWorks and the DR stands for Director.
    /// </summary>
    public class FileDRCFData
    {
        /// <summary>RGB background color used by the movie.</summary>
        public List<byte> BgColor { get; internal set; }
        /// <summary>Playback tempo in frames per second.</summary>
        public sbyte Tempo { get; internal set; }
        /// <summary>Stage rectangle describing the visible area.</summary>
        public Rect SourceRect { get; internal set; }
        /// <summary>Version of Director that saved the file.</summary>
        public ushort FileVersion { get; internal set; }
        /// <summary>Size of this configuration chunk.</summary>
        public short Size { get; internal set; }
        /// <summary>Version of the movie format.</summary>
        public short MovieFileVersion { get; internal set; }
        /// <summary>Palette index used by legacy Director versions.</summary>
        public short OldDefaultPalette { get; internal set; }
        /// <summary>Active default palette index.</summary>
        public int DefaultPalette { get; internal set; }
        /// <summary>Frames to preload before starting playback.</summary>
        public int DownloadFramesBeforePlaying { get; internal set; }
        /// <summary>Flag indicating whether the movie is copy protected.</summary>
        public bool ProtectedMovie { get; internal set; }
    }
}
