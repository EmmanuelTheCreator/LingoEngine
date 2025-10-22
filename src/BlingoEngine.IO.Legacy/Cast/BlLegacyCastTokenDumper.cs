using BlingoEngine.IO.Legacy.Afterburner;
using BlingoEngine.IO.Legacy.Classic;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Tools;

namespace BlingoEngine.IO.Legacy.Cast;

internal static class BlLegacyCastTokenDumper
{
    /// <summary>Dump CASt bytes per CAS* slot into a text file.</summary>
    public static void DumpCastTokens(ReaderContext ctx, string outputFilePath)
    {
        var classic = new BlClassicPayloadLoader(ctx);
        BlAfterburnerPayloadLoader? ab = ctx.AfterburnerState is null ? null : new(ctx, ctx.AfterburnerState);

        using var sw = new StreamWriter(outputFilePath, false, System.Text.Encoding.UTF8);
        foreach (var cas in ctx.Resources.Entries)
        {
            if (cas.Tag != BlTag.CasStar) continue;
            var casBytes = cas.StorageKind == BlResourceStorageKind.AfterburnerSegment
                ? (ab is null ? Array.Empty<byte>() : cas.LoadAfterburner(ab))
                : cas.ReadClassicPayload(classic);

            var slots = casBytes.Length / 4;
            sw.WriteLine($"== CAS* #{cas.Id} ({slots} slots) ==");

            var mr = ctx.CreateMemoryReader(casBytes, BlEndianness.BigEndian);
            for (int slot = 0; slot < slots; slot++)
            {
                int caStId = unchecked((int)mr.ReadUInt32());
                if (caStId == 0) continue;
                if (!ctx.Resources.TryGetEntry(caStId, out var st)) { sw.WriteLine($"[{slot:000}] CASt #{caStId} <missing>"); continue; }

                var stBytes = st.StorageKind == BlResourceStorageKind.AfterburnerSegment
                    ? (ab is null ? Array.Empty<byte>() : st.LoadAfterburner(ab))
                    : st.ReadClassicPayload(classic);

                sw.WriteLine($"\n[{slot:000}] CASt #{st.Id}  len={stBytes.Length}");
                DumpSingleCASt(sw, stBytes);
            }
        }
    }

    /// <summary>Parse CASt header and slice info block with key offsets.</summary>
    private static void DumpSingleCASt(StreamWriter sw, byte[] buf)
    {
        if (buf.Length < 12) { sw.WriteLine("  <too short>"); return; }
        using var ms = new MemoryStream(buf, false);
        var r = new BlStreamReader(ms) { Endianness = BlEndianness.BigEndian };

        uint type = r.ReadUInt32();
        uint infoLen = r.ReadUInt32(); // 139, 8B
        uint specLen = r.ReadUInt32();

        int infoAvail = Math.Max(0, buf.Length - (int)r.Position);
        int infoTake = (int)Math.Min(infoLen, (uint)infoAvail);
        var info = infoTake > 0 ? r.ReadBytes(infoTake) : Array.Empty<byte>();

        sw.WriteLine($"  Type=0x{type:X8}  InfoLen={infoLen}  SpecLen={specLen}");
        DumpSlices(sw, "INFO", info);
        DumpMarks(sw, info, [
                (0x44, "CastInfo Flags (DTS, etc.)"),
                (0x46, "Text Framing"),
            ]);
   

        // optional: specific block raw
        int specAvail = Math.Max(0, buf.Length - (int)r.Position);
        int specTake = (int)Math.Min(specLen, (uint)specAvail);
        if (specTake > 0)
        {
            var specific = r.ReadBytes(specTake);
            DumpHex(sw, "SPECIFIC", specific, 0);
        }
        DumpMarks(sw, info, [
                (0x03, "AntiAlias & Kerning mode nibbles"),
                (0x17, "Is Editable"),
                (0x2F, "AntiAlias threshold (pt)"),
                (0x43, "Kerning enabled flag"),
                (0x47, "Kerning threshold (pt)"),
                (0x4B, "Use Hyperlink Styles"),
                (0x5B, "Ink")
            ]);
    }

    /// <summary>Dump named slices and highlight key offsets.</summary>
    private static void DumpSlices(StreamWriter sw, string label, byte[] data)
    {
        if (data.Length == 0) { sw.WriteLine($"  {label}: <empty>"); return; }
        sw.WriteLine($"  {label}: len={data.Length}");
        DumpHex(sw, label, data, 0);

        
    }

    private static void DumpMarks(StreamWriter sw, byte[] data, (int Address, string Description)[] marks)
    {
        foreach (var off in marks.OrderBy(x => x.Address))
        {
            if (off.Address < 0 || off.Address >= data.Length) continue;
            byte v = data[off.Address];
            sw.WriteLine($"    @0x{off.Address:X2} = 0x{v:X2}  ({v}) \t{off.Description}");
        }
    }

    /// <summary>Hex + ASCII with absolute offsets.</summary>
    private static void DumpHex(StreamWriter sw, string label, byte[] data, int baseOffset)
    {
        const int W = 16;
        for (int i = 0; i < data.Length; i += W)
        {
            var slice = data.AsSpan(i, Math.Min(W, data.Length - i));
            var hex = string.Join(' ', slice.ToArray().Select(b => b.ToString("X2")));
            var asc = new string(slice.ToArray().Select(b => b is >= 32 and <= 126 ? (char)b : '.').ToArray());
            sw.WriteLine($"    {label} 0x{(baseOffset + i):X6}: {hex.PadRight(W * 3 - 1)}  |{asc}|");
        }
    }
}
