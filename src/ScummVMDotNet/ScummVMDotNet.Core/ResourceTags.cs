using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Director
{
    public static class ResourceTags
    {
        public static uint MKTAG(char a, char b, char c, char d)
        {
            return (uint)((byte)a << 24 | (byte)b << 16 | (byte)c << 8 | (byte)d);
        }

        public static uint BITD = MKTAG('B', 'I', 'T', 'D'); // Bitmap data
        public static uint CLUT = MKTAG('C', 'L', 'U', 'T'); // Color lookup table (palette)
        public static uint STXT = MKTAG('S', 'T', 'X', 'T'); // Styled text
        public static uint IMAG = MKTAG('I', 'M', 'A', 'G'); // General image
        public static uint VWCF = MKTAG('V', 'W', 'C', 'F'); // View configuration
        public static uint FXmp = MKTAG('F', 'X', 'm', 'p'); // Font map
        public static uint KEYD = MKTAG('K', 'E', 'Y', 'D'); // Key frame or keyboard data
        public static uint SND = MKTAG('S', 'N', 'D', ' ');  // Sound data
        public static uint cAST = MKTAG('c', 'A', 'S', 'T'); // Cast metadata
        public static uint tEXt = MKTAG('t', 'E', 'X', 't'); // Plain text
        public static uint Lscr = MKTAG('L', 's', 'c', 'r'); // Lingo script
        public static uint XTRa = MKTAG('X', 'T', 'R', 'a'); // Xtra reference
        public static uint FADE = MKTAG('F', 'A', 'D', 'E'); // Fade definition
        public static uint BMAP = MKTAG('B', 'M', 'A', 'P'); // Alternative bitmap tag
        public static uint Mmap = MKTAG('M', 'm', 'a', 'p'); // Memory map
        public static uint MOA = MKTAG('M', 'O', 'A', ' ');  // MOA Xtra resource
        public static uint GFXM = MKTAG('G', 'F', 'X', 'M'); // Graphics matrix
        public static uint DISP = MKTAG('D', 'I', 'S', 'P'); // Display options
        public static uint PICT = MKTAG('P', 'I', 'C', 'T'); // Picture resource
        public static uint ICON = MKTAG('I', 'C', 'O', 'N'); // Icon image
        public static uint FONT = MKTAG('F', 'O', 'N', 'T'); // Font descriptor
        // Add more as needed...
    }
}
