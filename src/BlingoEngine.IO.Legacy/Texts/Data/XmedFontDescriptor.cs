using System;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts.Data
{
    [Flags]
    public enum XmedFontPitchFlags : byte
    {
        Default = 0x00,
        Fixed = 0x01,
        Variable = 0x02,
        Vector = 0x04
    }

    public enum XmedFontFamilyClass : byte
    {
        DontCare = 0x00,
        Roman = 0x10,
        Swiss = 0x20,
        Modern = 0x30,
        Script = 0x40,
        Decorative = 0x50
    }

    /// <summary>Represents a font descriptor decoded from the XMED font table.</summary>
    public sealed class XmedFontDescriptor
    {
        private static readonly Lazy<bool> EncodingProviderRegistered = new(() =>
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return true;
        });

        /// <summary>Table index declared for this font entry.</summary>
        public int TableIndex { get; set; }

        /// <summary>Font family name (e.g. Arial, Terminal).</summary>
        public string FamilyName { get; set; } = string.Empty;

        /// <summary>Font style name stored alongside the family (empty means regular).</summary>
        public string StyleName { get; set; } = string.Empty;

        /// <summary>OEM or raster font identifier (0 for vector fonts, 0xFF60+ for raster Terminal variants).</summary>
        public int FontId { get; set; }

        /// <summary>Windows code page / character set identifier (e.g. 1252 for Western Latin).</summary>
        public int CodePage { get; set; }

        /// <summary>LOGFONT weight value (400 = normal, 700 = bold, etc.).</summary>
        public int Weight { get; set; }

        /// <summary>Unmapped 16-bit flag that precedes the segmented records (observed 0 in samples).</summary>
        public int Flags { get; set; }

        /// <summary>Font kind marker – observed 1 for vector entries.</summary>
        public int FontKind { get; set; }

        /// <summary>Raster character cell height or zero for scalable fonts.</summary>
        public int CellHeight { get; set; }

        /// <summary>LOGFONT pitch and family flags packed in the observed 0x40008 pattern.</summary>
        public int PitchAndFamily { get; set; }

        /// <summary>Low byte of <see cref="PitchAndFamily"/>.</summary>
        public byte PitchAndFamilyByte => (byte)(PitchAndFamily & 0xFF);

        /// <summary>Win32 pitch bits extracted from the descriptor.</summary>
        public XmedFontPitchFlags PitchFlags => (XmedFontPitchFlags)(PitchAndFamilyByte & 0x07);

        /// <summary>Win32 font family classification extracted from the descriptor.</summary>
        public XmedFontFamilyClass FamilyClass => (XmedFontFamilyClass)(PitchAndFamilyByte & 0xF0);

        /// <summary>High-order decoration bits observed in some descriptors (underline/italic hints).</summary>
        public int PitchDecorations => PitchAndFamily & ~0xFF;

        /// <summary>Trailing slot used by Director – currently always 0 in samples.</summary>
        public int Reserved { get; set; }

        /// <summary>Script identifier (257 = Western Latin).</summary>
        public int ScriptId { get; set; }

        /// <summary>Name index referencing the inline string table (usually 0).</summary>
        public int NameIndex { get; set; }

        /// <summary>Preferred <see cref="Encoding"/> resolved from <see cref="CodePage"/>, if available.</summary>
        public Encoding? Encoding => ResolveEncoding(CodePage);

        private static Encoding? ResolveEncoding(int codePage)
        {
            if (codePage <= 0)
                return null;

            _ = EncodingProviderRegistered.Value;

            foreach (var info in Encoding.GetEncodings())
            {
                if (info.CodePage == codePage)
                    return info.GetEncoding();
            }

            return null;
        }
    }
}
