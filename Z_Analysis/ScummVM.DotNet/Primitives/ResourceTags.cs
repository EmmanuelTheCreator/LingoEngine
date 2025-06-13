namespace Director.Primitives
{
    public static class ResourceTags
    {
        public static uint MKTAG(char a, char b, char c, char d)
        {
            return (uint)((byte)a << 24 | (byte)b << 16 | (byte)c << 8 | (byte)d);
        }
        public static uint ToTag(string tag)
        {
            if (tag.Length != 4)
                throw new ArgumentException("Tag must be exactly 4 characters.", nameof(tag));

            return (uint)(tag[0] << 24 | tag[1] << 16 | tag[2] << 8 | tag[3]);
        }
        public static string FromTag(uint tag)
        {
            return new string(new[] {
        (char)((tag >> 24) & 0xFF),
        (char)((tag >> 16) & 0xFF),
        (char)((tag >> 8) & 0xFF),
        (char)(tag & 0xFF)
    });
        }
        public static uint Imap = MKTAG('i', 'm', 'a', 'p'); // Director Configuration
        public static uint DRCF = MKTAG('D', 'R', 'C', 'F'); // Director Configuration
        public static uint KEYStar = MKTAG('K', 'E', 'Y', '*'); 
        public static uint VWLB = MKTAG('V', 'W', 'L', 'B'); 
        /// <summary>
        ///  / VWFI stands for View File Info. The VWFI resource contains metadata about the movie itself — typically high-level information such as:
        ///  - Movie name or file reference
        ///  - Authoring version
        ///  - Platform indicators(Mac/Windows)
        ///  - General configuration values
        /// </summary>
        public static uint VWFI = MKTAG('V', 'W', 'F', 'I');
        /// <summary>
        /// View Action List.Stores the action list associated with the Director movie — this includes script-driven actions that are linked to score frames, sprites, or cast members. These are the Lingo actions that Director executes at specific points, such as:
        /// - on enterFrame
        /// - on exitFrame
        /// - on mouseDown
        /// - Frame scripts or behaviors tied to timeline events 
        /// This chunk is typically loaded after the score(VWSC) and contains references to compiled or raw script instructions.
        /// </summary>
        public static uint VWAC = MKTAG('V', 'W', 'A', 'C'); 
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
        public static uint Mmap = MKTAG('m', 'm', 'a', 'p'); // Memory map
        public static uint MOA = MKTAG('M', 'O', 'A', ' ');  // MOA Xtra resource
        public static uint GFXM = MKTAG('G', 'F', 'X', 'M'); // Graphics matrix
        public static uint DISP = MKTAG('D', 'I', 'S', 'P'); // Display options
        public static uint PICT = MKTAG('P', 'I', 'C', 'T'); // Picture resource
        public static uint ICON = MKTAG('I', 'C', 'O', 'N'); // Icon image
        public static uint FONT = MKTAG('F', 'O', 'N', 'T'); // Font descriptor
        public static uint VWCI = MKTAG('V', 'W', 'C', 'I'); // The tag VWCI is used in Macromedia Director to denote View Information or View Configuration Information, typically related to Score window settings or Movie window settings.
        public static uint VWSC = MKTAG('V', 'W', 'S', 'C'); // View Score — it's the Score view settings chunk. Specifically, it stores how the Score window is configured in the Director authoring environment, including
        /// <summary>
        /// This is a chunked Director file using the XF (Extended Format) resource layout.
        /// </summary>
        public static uint XFIR = MKTAG('X', 'F', 'I', 'R');
        public static uint EERF = MKTAG('e', 'e', 'r', 'f');
        public static uint PAMM = MKTAG('p', 'a', 'm', 'm');
        public static uint Lnam = MKTAG('L', 'n', 'a', 'm');
        /// <summary>
        /// 'Lctx' compiled script context, // Native readable
        /// </summary>
        public static uint Lctx = MKTAG('L', 'c', 't', 'x');
        /// <summary>
        /// Frame/score data
        /// </summary>
        public static uint FCRD = MKTAG('L', 'c', 't', 'x');
        public static uint MV93 = MKTAG('M', 'V', '9', '3');
        /// <summary>
        ///  Legacy ScummVM byte order
        /// </summary>
        public static uint XtcL = MKTAG('X', 't', 'c', 'L');
        public static uint VWCR = MKTAG('V', 'W', 'C', 'R');
        public static uint CASt = MKTAG('C', 'A', 'S', 't');


    }
}
