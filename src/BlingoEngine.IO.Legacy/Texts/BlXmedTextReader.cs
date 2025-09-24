using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Tools;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BlingoEngine.IO.Legacy.Texts
{
    /// <summary>Paragraph/text alignment.</summary>
    public enum XmedAlignment
    {
        Center = 0,
        Right = 1,
        Left = 2,
        Justify = 3
    }
    public enum XmedEntryKind { Unknown, Text, TokenList, StyleRuns, Fonts, Sizes, Colors, Weights, Italics, Underlines, Spacing, Align, Justify,
        Index
    }
    /// <summary>Parsed XMED document.</summary>
    public sealed class XmedDocument
    {
        public string Text { get; set; } = string.Empty;
        public List<XmedTextRun> Runs { get; } = new();
        public List<XmedStyleDescriptor> Styles { get; } = new();
        public List<XmedRunMapEntry> RunMap { get; } = new();
        public uint Width { get; set; }
        public uint LineSpacing { get; set; }
        public uint TextLength { get; set; }
        public int DirectorVersion { get; set; }
        public XmedRichTextMetadata? RichText { get; set; }
    }

    /// <summary>Legacy rect for old rich text streams.</summary>
    public sealed class XmedRect
    {
        public short Top { get; set; }
        public short Left { get; set; }
        public short Bottom { get; set; }
        public short Right { get; set; }
    }

    /// <summary>Legacy rich text metadata (Director ≤10).</summary>
    public sealed class XmedRichTextMetadata
    {
        public XmedRect InitialRect { get; set; } = new();
        public XmedRect BoundingRect { get; set; } = new();
        public byte AntialiasFlag { get; set; }
        public byte CropFlags { get; set; }
        public ushort ScrollPosition { get; set; }
        public ushort AntialiasFontSize { get; set; }
        public ushort DisplayHeight { get; set; }
        public BlLegacyColor ForegroundColor { get; set; }
        public BlLegacyColor BackgroundColor { get; set; }
    }

    /// <summary>Single styled text run.</summary>
    public sealed class XmedTextRun
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public string FontName { get; set; } = string.Empty;
        public ushort FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public BlLegacyColor ForeColor { get; set; }
    }

    /// <summary>Style descriptor parsed from XMED.</summary>
    public sealed class XmedStyleDescriptor
    {
        public ushort StyleId { get; set; }
        public string FontName { get; set; } = string.Empty;
        public byte ColorIndex { get; set; }
        public ushort FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikeout { get; set; }
        public bool Subscript { get; set; }
        public bool Superscript { get; set; }
        public bool TabbedField { get; set; }
        public bool EditableField { get; set; }
        public XmedAlignment Alignment { get; set; } = XmedAlignment.Center;
        public XmedAlignment AlignmentFromFlags { get; set; } = XmedAlignment.Center;
        public bool WrapOff { get; set; }
        public bool HasTabs { get; set; }
        public byte AlignmentRaw { get; set; }
        public byte StyleFlags { get; set; }
        public byte ColorIndexRaw => ColorIndex;
    }

    /// <summary>20-char token entry.</summary>
    public sealed class XmedRunMapEntry
    {
        public XmedRunMapEntry(ushort type, ushort f2, ushort length, ushort f4, ushort styleId, long position)
        {
            Type = type; F2 = f2; Length = length; F4 = f4; StyleId = styleId; Position = position;
        }
        public ushort Type { get; }
        public ushort F2 { get; }
        public ushort Length { get; }
        public ushort F4 { get; }
        public ushort StyleId { get; }
        public long Position { get; }
    }

    /// <summary>Directory of header tokens.</summary>
    public sealed class XmedDir : IReadOnlyList<XmedDirEntry>
    {
        private readonly List<XmedDirEntry> _entries;
        public XmedDir(byte[] buffer, List<XmedDirEntry> entries, int headerLength)
        {
            Buffer = buffer; _entries = entries; HeaderLength = headerLength;
        }
        public byte[] Buffer { get; }
        public int HeaderLength { get; }
        public int Count => _entries.Count;
        public XmedDirEntry this[int index] => _entries[index];
        public IEnumerator<XmedDirEntry> GetEnumerator() => _entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
        public XmedDirEntry? FindEntry(string type)
        {
            for (int i = 0; i < _entries.Count; i++)
                if (string.Equals(_entries[i].Type, type, StringComparison.Ordinal))
                    return _entries[i];
            return null;
        }

        public XmedDirEntry? FindEntryByType(XmedEntryKind text)
            => _entries.FirstOrDefault(x => x.Kind == XmedEntryKind.Text);
        public XmedDirEntry? FindStyleRunsEntry() => _entries.FirstOrDefault(e => e.Kind == XmedEntryKind.StyleRuns);
        public XmedDirEntry? FindFontsEntry() => _entries.FirstOrDefault(e => e.Kind == XmedEntryKind.Fonts);
        public XmedDirEntry? FindSizesEntry() => _entries.FirstOrDefault(e => e.Kind == XmedEntryKind.Sizes);
    }

    /// <summary>Single directory token.</summary>
    [DebuggerDisplay("DirEntry:{Kind}:{Type}:offset={Offset}:count={Count}:position={Position}:dataOffset={dataOffset}:Terminator={Terminator}")]
    public sealed class XmedDirEntry
    {
        public XmedDirEntry(string type, long offset, int count, long position, long? dataOffset, byte terminator)
        {
            Type = type; Offset = offset; Count = count; Position = position; DataOffset = dataOffset; Terminator = terminator;
        }
        public string Type { get; }
        public long Offset { get; }
        /// <summary>
        /// FFFF isn’t a real token table like 0004/0005/0006. It’s a header marker that Director/XMED uses as a directory root. The Count field here often contains garbage or sentinel values (sometimes 65537, 131073, or 262145). It does not represent an actual run count.
        /// </summary>
        public int Count { get; }
        public long Position { get; }
        public long? DataOffset { get; }
        public byte Terminator { get; }
        public uint Signature { get; set; }
        public XmedEntryKind Kind { get; set; }
    }

    /// <summary>XMED reader.</summary>
    public sealed class BlXmedTextReader
    {
        private const int DefaultDirectorVersion = 13;
        private const int LegacyRichTextMaxVersion = 10;
        private static readonly byte[] FontMarker = { (byte)'4', (byte)'0', (byte)',' }; // "40,"

        /// <summary>Read from byte[] (modern v13 default).</summary>
        public XmedDocument Read(byte[] buffer) => Read(buffer, DefaultDirectorVersion);
        /// <summary>Read from byte[] with explicit Director version.</summary>
        public XmedDocument Read(byte[] buffer, int directorVersion)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            using var ms = new MemoryStream(buffer, writable: false);
            return Read(ms, directorVersion);
        }
        /// <summary>Read from stream (modern v13 default).</summary>
        public XmedDocument Read(Stream stream) => Read(stream, DefaultDirectorVersion);
        /// <summary>Read from stream with explicit Director version.</summary>
        public XmedDocument Read(Stream stream, int directorVersion)
        {
            ArgumentNullException.ThrowIfNull(stream);
            var buf = ReadAllBytes(stream);
            return ShouldUseLegacyRichText(directorVersion) ? ReadLegacyRichText(buf, directorVersion) : ReadModernXmed(buf, directorVersion);
        }

        private static bool ShouldUseLegacyRichText(int directorVersion) =>
            directorVersion > 0 && directorVersion <= LegacyRichTextMaxVersion;

        /// <summary>Modern XMED (v13+) parse.</summary>
        private XmedDocument ReadModernXmed(byte[] buffer, int directorVersion)
        {
            var directory = ReadHeaderDirectory(buffer);
            // var json = System.Text.Json.JsonSerializer.Serialize(directory, new JsonSerializerOptions { WriteIndented = true });
            //var (_, _, textData) = ReadTextBlock(directory);
            //var test = textData.ToArray().ToHexString();
            //var header = directory.Buffer;
            //var width = header.ReadUInt32LittleEndian(0x0018);
            //var styleFlags = header.ReadByteOrDefault(0x001C);
            //var alignFlags = header.ReadByteOrDefault(0x001D);
            //var lineSpacing = header.ReadUInt32LittleEndian(0x003C);
            //var fontSize = (ushort)Math.Clamp(header.ReadUInt32LittleEndian(0x0040), 0, 0xFFFF);
            //var headerTextLen = header.ReadUInt32LittleEndian(0x004C);

            //var doc = new XmedDocument
            //{
            //    Width = width,
            //    LineSpacing = lineSpacing,
            //    TextLength = headerTextLen,
            //    DirectorVersion = directorVersion
            //};

            //var baseStyle = new XmedStyleDescriptor
            //{
            //    FontSize = fontSize,
            //    AlignmentRaw = alignFlags,
            //    StyleFlags = styleFlags
            //};
            //ApplyStyleFlags(styleFlags, baseStyle);
            //ApplyAlignmentFlags(alignFlags, baseStyle);
            //doc.Styles.Add(baseStyle);

            //var text = Encoding.Latin1.GetString(textData.Span);
            //doc.Text = text;

            //var runMap = ReadRunMap(directory, text.Length);
            //foreach (var e in runMap) doc.RunMap.Add(e);

            //var descriptors = ReadStyleDescriptors(directory);
            //foreach (var d in descriptors) doc.Styles.Add(d);

            //int cursor = 0;
            //foreach (var m in runMap) // directory order
            //{
            //    if (cursor >= text.Length) break;
            //    int len = Math.Min(m.Length, text.Length - cursor);
            //    if (len <= 0) continue;

            //    var run = new XmedTextRun
            //    {
            //        Start = cursor,
            //        Length = len,
            //        Text = text.Substring(cursor, len),
            //        FontName = baseStyle.FontName,
            //        FontSize = baseStyle.FontSize,
            //        Bold = baseStyle.Bold,
            //        Italic = baseStyle.Italic,
            //        Underline = baseStyle.Underline,
            //        ForeColor = new BlLegacyColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex)
            //    };

            //    if (TryGetDescriptor(descriptors, m.StyleId, out var d))
            //    {
            //        if (!string.IsNullOrEmpty(d.FontName)) run.FontName = d.FontName;
            //        if (d.ColorIndex != 0) run.ForeColor = new BlLegacyColor(d.ColorIndex, d.ColorIndex, d.ColorIndex);
            //        if (d.FontSize != 0) run.FontSize = d.FontSize;
            //        run.Bold = d.Bold; run.Italic = d.Italic; run.Underline = d.Underline;
            //    }

            //    doc.Runs.Add(run);
            //    cursor += len;
            //}

            //if (text.Length > 0)
            //{
            //    var covered = new bool[text.Length];
            //    foreach (var r in doc.Runs)
            //    {
            //        int s = Math.Clamp(r.Start, 0, text.Length);
            //        int e = Math.Clamp(r.Start + r.Length, 0, text.Length);
            //        for (int k = s; k < e; k++) covered[k] = true;
            //    }

            //    int i = 0;
            //    while (i < text.Length)
            //    {
            //        if (covered[i]) { i++; continue; }
            //        int j = i + 1;
            //        while (j < text.Length && !covered[j]) j++;

            //        doc.Runs.Add(new XmedTextRun
            //        {
            //            Start = i,
            //            Length = j - i,
            //            Text = text.Substring(i, j - i),
            //            FontName = baseStyle.FontName,
            //            FontSize = baseStyle.FontSize,
            //            Bold = baseStyle.Bold,
            //            Italic = baseStyle.Italic,
            //            Underline = baseStyle.Underline,
            //            ForeColor = new BlLegacyColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex)
            //        });

            //        i = j;
            //    }

            //    MergeAdjacentEqualStyleRuns(doc.Runs);
            //}
//            return doc;
            throw new NotImplementedException();
        }

        /// <summary>Legacy (≤10) rich text parse.</summary>
        private XmedDocument ReadLegacyRichText(byte[] buffer, int directorVersion)
        {
            if (buffer.Length < 34) throw new InvalidDataException("Rich text header too small.");

            using var memory = new MemoryStream(buffer, writable: false);
            var reader = new BlStreamReader(memory) { Endianness = BlEndianness.BigEndian };

            var meta = new XmedRichTextMetadata
            {
                InitialRect = ReadLegacyRect(reader),
                BoundingRect = ReadLegacyRect(reader),
                AntialiasFlag = reader.ReadByte(),
                CropFlags = reader.ReadByte(),
                ScrollPosition = reader.ReadUInt16(),
                AntialiasFontSize = reader.ReadUInt16(),
                DisplayHeight = reader.ReadUInt16()
            };

            _ = reader.ReadByte(); // pad
            var foreR = reader.ReadByte();
            var foreG = reader.ReadByte();
            var foreB = reader.ReadByte();
            meta.ForegroundColor = new BlLegacyColor(foreR, foreG, foreB);

            var bgR = (byte)(reader.ReadUInt16() >> 8);
            var bgG = (byte)(reader.ReadUInt16() >> 8);
            var bgB = (byte)(reader.ReadUInt16() >> 8);
            meta.BackgroundColor = new BlLegacyColor(bgR, bgG, bgB);

            return new XmedDocument { DirectorVersion = directorVersion, RichText = meta };
        }

        private static XmedRect ReadLegacyRect(BlStreamReader reader)
        {
            return new XmedRect
            {
                Top = reader.ReadInt16(),
                Left = reader.ReadInt16(),
                Bottom = reader.ReadInt16(),
                Right = reader.ReadInt16()
            };
        }


        #region Directory reading
        public XmedDir ReadHeaderDirectory(byte[] buffer)
        {
            return new XmedDir(buffer, new List<XmedDirEntry>(), 0);
            var blocks = ReadBlocks(buffer);

            throw new NotImplementedException();
        }
        static List<byte[]> ReadBlocks(byte[] buffer)
        {
            // only for test : Text_Single_Line_Multi_Style_file_should_read_long_text_and_runs
            // Text_Single_Line_Multi_Style_13.xmed.bin
            // Todo : find the header legth and how to read it
            // TODO : find the 
            TryFindLength(buffer);
            var styles = new List<XmedStyleDescriptor>();

            var blocks = new List<byte[]>();
            int i = 0;// 233;
            var sb = new StringBuilder();
            string allText = "";
            while (i < buffer.Length)
            {
                int start = i + 1;
                (int len, int dataStart, int bytesRead) = buffer.ReadCommaLengthValue(start);
                i += len;
                var blockdata = buffer.AsSpan(dataStart, len).ToArray();
                var hexbytes = blockdata.ToHexString(16, true, dataStart, true);
                if (len > 4)
                {
                    blocks.Add(blockdata);
                    if (len == 68) // cue points block (hardcoded for now)
                    {
                        // reading somthing, is identical for all files.
                        // 045,046,182,181,149,181,165,165,046,039,034,145,146,147,148,133,131 
                        var parts = ReadRunTuples(blockdata, 0, blockdata.Length, allText.Length);
                    }
                    else if (len == 64) // font/style tail (hardcoded for now)
                    {
                        // absolute start of the style mini-block
                        var startStyleBlock = dataStart - bytesRead - 40;
                        var styleSpan = buffer.AsSpan(startStyleBlock, len + 40).ToArray();
                        var hexbytes1 = styleSpan.ToHexString(16, true, startStyleBlock, true);
                        //var hexbytes2 = styleSpan.ToHexString(256);//, true, startStyleBlock, true);
                        int marker = (dataStart - 3) - startStyleBlock;            // relative index inside styleSpan

                        var nextBlockOffsets = buffer.ReadCommaLengthValue(dataStart + len);
                        var startStyleBlock2 = dataStart + len + nextBlockOffsets.BytesRead;
                        var styleSpan2 = buffer.AsSpan(startStyleBlock2, nextBlockOffsets.DataLength).ToArray();
                        var hexbytes2 = styleSpan2.ToHexString(16, true, startStyleBlock2, true);

                        var nextData = startStyleBlock2 + nextBlockOffsets.DataLength;

                        var style = ReadStyle(styleSpan, marker, styleSpan2);
                        styles.Add(style!); 
                        //                        sb.AppendLine(hexbytes2);
                        sb.AppendLine();
                    }
                    else
                    {
                        //ReadRunTuples(blockdata, 0, 0, TextLen);
                        allText = Encoding.Latin1.GetString(blockdata);
                    }

                    i = dataStart + len;
                }
                i++;
                //while (i < buffer.Length && buffer[i] != 0x2c) i++;
            }
            var testtttt = sb.ToString();
            return blocks;
        }

        private static void TryFindLength(byte[] buffer)
        {
            var offst111 = 59;
            var testBlok = buffer.AsSpan(2327 - offst111, offst111 + 68).ToArray();
            var testBlokHex = testBlok.ToHexString(16, true, 2327 - offst111, true);
            var innerBlok = testBlok.AsSpan(20, testBlok.Length-20-68).ToArray();
        }

        private static List<byte> ReadRunTuples(byte[] buffer, int start, int endExclusive, int textLen)
        {

            // always returns 045,046,182,181,149,181,165,165,046,039,034,145,146,147,148,133,131 
            var numbers = new List<byte>();
            var numbers2 = new List<int>();
            for (int i = 0; i < 17; i++)
            {
                var number1 = buffer[(i * 4)];
                var number3 = buffer[(i * 4)+2];
                var number4 = buffer[(i * 4)+3];
                numbers.Add(buffer[(i*4) + 1]);
                if (number1 != 1 || number3 != 0 || number4 != 0)
                {

                }
                numbers2.Add(buffer[(i * 4) + 1] << 8 + buffer[(i * 4) + 2]);
            }
            
            return numbers;
        }






        private static XmedStyleDescriptor? ReadStyle(byte[] buffer, int marker, byte[] secondData)
        {
            int styleId = 0;
            byte styleByte = buffer[0];
            byte flagsByte = buffer[1];

            int nameLength = buffer[marker + 3];         
            int nameStart = marker + 4;

            var desc = new XmedStyleDescriptor
            {
                StyleId = (ushort)styleId,
                StyleFlags = styleByte,
                AlignmentRaw = flagsByte,
                FontName = Encoding.Latin1.GetString(buffer, nameStart, nameLength),
                //ColorIndex = (byte)colorIndex
            };
            ApplyStyleFlags(styleByte, desc);
            ApplyAlignmentFlags(flagsByte, desc);
            return desc;
        }

        /// <summary>Reads the XMED header directory and classifies entries by kind.</summary>
        private XmedDir ReadHeaderDirectory23(byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            var entries = new List<XmedDirEntry>();

            // 1) Classic: scan until first NUL
            int headerLength = Array.IndexOf(buffer, (byte)0x00);
            if (headerLength < 0) headerLength = buffer.Length;

            for (int i = 0; i + 20 <= headerLength; i++)
            {
                ReadOnlySpan<byte> span = buffer.AsSpan(i, 20);
                if (!span.IsAsciiHexOrDigits()) continue;

                string fullWord = Encoding.ASCII.GetString(span.Slice(0, 20));
                string type = fullWord.Substring(0,4);
                if (!type.IsAsciiHexOrDigitString()) continue;
                long something = long.Parse(fullWord.Substring(4, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                long off = long.Parse(fullWord.Substring(8, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                int count = int.Parse(fullWord.Substring(12, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                byte term = (i + 20 < buffer.Length) ? buffer[i + 20] : (byte)0;
                long? data = term == 0x00 ? i + 21 : null;

                entries.Add(new XmedDirEntry(type, off, count, i, data, term));
            }

            // 2) Fallback: scan full buffer and append uniques
            if (entries.Count < 5)
            {
                for (int i = 0; i + 20 <= buffer.Length; i++)
                {
                    ReadOnlySpan<byte> span = buffer.AsSpan(i, 20);
                    if (!span.IsAsciiHexOrDigits()) continue;

                    string type = Encoding.ASCII.GetString(span.Slice(0, 4));
                    if (!type.IsAsciiHexOrDigitString()) continue;

                    long off = span.Slice(4, 8).ParseHexInt64();
                    int count = (int)span.Slice(12, 8).ParseHexInt64();
                    byte term = (i + 20 < buffer.Length) ? buffer[i + 20] : (byte)0;
                    long? data = term == 0x00 ? i + 21 : null;

                    if (!entries.Exists(e => e.Position == i))
                        entries.Add(new XmedDirEntry(type, off, count, i, data, term));
                }
                headerLength = buffer.Length;
            }

            // 3) Classification (attach Signature + Kind)
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                uint sig = buffer.Peek3(e.DataOffset);
                var kind = ClassifyEntry(e, buffer);   // ← use buffer-aware classifier
                e.Signature = sig;
                e.Kind = kind;
                entries[i] = e;
            }



            var json = System.Text.Json.JsonSerializer.Serialize(entries); //, new JsonSerializerOptions { WriteIndented = true });
            return new XmedDir(buffer, entries, headerLength);
        }
        private static XmedEntryKind ClassifyEntry(XmedDirEntry e, byte[] buffer)
        {
            string type = e.Type;
           
            // 3) Type-based fallbacks
            return type switch
            {
                "0002" => XmedEntryKind.Text,        // common placement
                "0008" => XmedEntryKind.Text,        // common placement
                "0006" => XmedEntryKind.StyleRuns,
                "0004" => XmedEntryKind.Fonts,
                "0005" => XmedEntryKind.Sizes,
                "0007" => XmedEntryKind.Colors,
                "0009" => XmedEntryKind.Weights,
                "000A" => XmedEntryKind.Italics,
                "000B" => XmedEntryKind.Underlines,
                "000C" => XmedEntryKind.Spacing,
                "0128" => XmedEntryKind.Align,
                "0129" => XmedEntryKind.Justify,
                "FFFF" => XmedEntryKind.Index,
                _ => XmedEntryKind.Unknown
            };
        }



        #endregion

        /// <summary>Read text entry (type 0002).</summary>
        // ReadTextBlock: use "F1," then read until NUL
        

 



        /// <summary>Read run map entries; keep directory order.</summary>
        public IReadOnlyList<XmedRunMapEntry> ReadRunMap(XmedDir directory, int textLength)
        {
            var list = new List<XmedRunMapEntry>();

            // accept common run-bearing types (others are control/caps)
            static bool Accept(ushort t) =>
                t is 0x0004 or 0x0005 or 0x0007 or 0x0009 or 0x000A or 0x000B or 0x000C
                  or 0x000F or 0x0013 or 0x0128 or 0x0129;

            foreach (var entry in directory)
            {
                if (entry.Position < 0 || entry.Position + 20 > directory.Buffer.Length) continue;
                ReadOnlySpan<byte> span = directory.Buffer.AsSpan((int)entry.Position, 20);
                if (!span.IsAsciiHexOrDigits()) continue;

                ushort type = (ushort)span.Slice(0, 4).ParseHexInt64();
                ushort f2 = (ushort)span.Slice(4, 4).ParseHexInt64();
                int len = (int)span.Slice(8, 4).ParseHexInt64();
                ushort f4 = (ushort)span.Slice(12, 4).ParseHexInt64();
                ushort sid = (ushort)span.Slice(16, 4).ParseHexInt64();

                if (!Accept(type)) continue;
                if (len <= 0 || len > textLength * 2) continue;

                list.Add(new XmedRunMapEntry(type, f2, (ushort)len, f4, sid, entry.Position));
            }

            // Fallback: if nothing found, rescan the whole buffer to collect tokens
            if (list.Count == 0)
            {
                var buf = directory.Buffer;
                for (int i = 0; i + 20 <= buf.Length; i++)
                {
                    ReadOnlySpan<byte> span = buf.AsSpan(i, 20);
                    if (!span.IsAsciiHexOrDigits()) continue;

                    ushort type = (ushort)span.Slice(0, 4).ParseHexInt64();
                    ushort f2 = (ushort)span.Slice(4, 4).ParseHexInt64();
                    int len = (int)span.Slice(8, 4).ParseHexInt64();
                    ushort f4 = (ushort)span.Slice(12, 4).ParseHexInt64();
                    ushort sid = (ushort)span.Slice(16, 4).ParseHexInt64();

                    if (!Accept(type)) continue;
                    if (len <= 0 || len > textLength * 2) continue;

                    list.Add(new XmedRunMapEntry(type, f2, (ushort)len, f4, sid, i));
                }
            }

            list.Sort((a, b) => a.Position.CompareTo(b.Position)); // directory order → cursor assembly
            return list;
        }


        /// <summary>Read style descriptors by backtracking from "40," marker.</summary>
        public IReadOnlyList<XmedStyleDescriptor> ReadStyleDescriptors(XmedDir directory)
        {
            var buffer = directory.Buffer;
            var styles = new List<XmedStyleDescriptor>();
            int i = 0; // scan entire buffer; some files place descriptors after text

            while (true)
            {
                int marker = buffer.IndexOfSequence(i, FontMarker); // "40,"
                if (marker < 0 || marker < 6) break;
                (bool flowControl, (i, int nameEnd, XmedStyleDescriptor desc)) = NewMethod(buffer, i, marker);
                if (!flowControl)
                {
                    continue;
                }

                if (!styles.Any(s => s.StyleId == desc.StyleId))
                    styles.Add(desc);

                i = nameEnd + 1; // advance beyond this string
            }

            return styles;
        }

        private static (bool flowControl, (int i, int nameEnd, XmedStyleDescriptor desc) value) NewMethod(byte[] buffer, int i, int marker)
        {
            int idStart = marker - 4;      // 4 ASCII-hex chars
            int flagsAt = idStart - 1;     // flags
            int styleAt = idStart - 2;     // style byte
            if (styleAt < 0) { i = marker + FontMarker.Length; return (flowControl: false, value: default); }

            ReadOnlySpan<byte> idSpan = buffer.AsSpan(idStart, 4);
            if (!idSpan.IsAsciiHexOrDigits()) { i = marker + FontMarker.Length; return (flowControl: false, value: default); }

            int styleId = (int)idSpan.ParseHexInt64();
            byte styleByte = buffer[styleAt];
            byte flagsByte = buffer[flagsAt];

            int nameStart = marker + FontMarker.Length;
            if (nameStart >= buffer.Length) { i = marker + FontMarker.Length; return (flowControl: false, value: default); }

            int colorIndex = buffer[nameStart];
            nameStart++;

            int nameEnd = nameStart;
            while (nameEnd < buffer.Length && buffer[nameEnd] != 0x00 && buffer[nameEnd].IsPrintable()) nameEnd++;
            if (nameEnd <= nameStart) { i = marker + FontMarker.Length; return (flowControl: false, value: default); }

            var desc = new XmedStyleDescriptor
            {
                StyleId = (ushort)styleId,
                StyleFlags = styleByte,
                AlignmentRaw = flagsByte,
                FontName = Encoding.Latin1.GetString(buffer, nameStart, nameEnd - nameStart),
                ColorIndex = (byte)colorIndex
            };
            ApplyStyleFlags(styleByte, desc);
            ApplyAlignmentFlags(flagsByte, desc);
            return (flowControl: true, value: default);
        }

        private static byte[] ReadAllBytes(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanSeek)
            {
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                memory.Position = 0;
                var reader = new BlStreamReader(memory);
                if (reader.Length > int.MaxValue) throw new InvalidDataException("XMED > 2GB not supported.");
                var len = (int)reader.Length;
                var buffer = new byte[len];
                reader.ReadExactly(buffer);
                return buffer;
            }

            var sreader = new BlStreamReader(stream);
            long saved = sreader.Position;
            sreader.Position = 0;
            if (sreader.Length > int.MaxValue) throw new InvalidDataException("XMED > 2GB not supported.");
            var size = (int)sreader.Length;
            var result = new byte[size];
            sreader.ReadExactly(result);
            sreader.Position = saved;
            return result;
        }

        private static void ApplyStyleFlags(byte flags, XmedStyleDescriptor style)
        {
            style.Bold = (flags & 0x01) != 0;
            style.Italic = (flags & 0x02) != 0;
            style.Underline = (flags & 0x04) != 0;
            style.Strikeout = (flags & 0x08) != 0;
            style.Subscript = (flags & 0x10) != 0;
            style.Superscript = (flags & 0x20) != 0;
            style.TabbedField = (flags & 0x40) != 0;
            style.EditableField = (flags & 0x80) != 0;
        }

        private static void ApplyAlignmentFlags(byte flags, XmedStyleDescriptor style)
        {
            style.WrapOff = (flags & 0x08) != 0;
            style.HasTabs = (flags & 0x10) != 0;
            style.AlignmentFromFlags = (XmedAlignment)(flags & 0x03);
            style.Alignment = (flags & 0x03) switch
            {
                0x01 => XmedAlignment.Right,
                0x02 => XmedAlignment.Left,
                0x03 => XmedAlignment.Justify,
                _ => XmedAlignment.Center
            };
        }

        private static bool TryGetDescriptor(IReadOnlyList<XmedStyleDescriptor> descriptors, int id, out XmedStyleDescriptor descriptor)
        {
            for (int i = 0; i < descriptors.Count; i++)
                if (descriptors[i].StyleId == (ushort)id) { descriptor = descriptors[i]; return true; }
            descriptor = default!;
            return false;
        }

        private static void MergeAdjacentEqualStyleRuns(List<XmedTextRun> runs)
        {
            if (runs.Count < 2) return;
            runs.Sort((a, b) => a.Start.CompareTo(b.Start));
            var merged = new List<XmedTextRun>();
            var cur = runs[0];

            for (int i = 1; i < runs.Count; i++)
            {
                var nxt = runs[i];
                bool adjacent = cur.Start + cur.Length == nxt.Start;
                bool same =
                    cur.FontName == nxt.FontName &&
                    cur.FontSize == nxt.FontSize &&
                    cur.Bold == nxt.Bold &&
                    cur.Italic == nxt.Italic &&
                    cur.Underline == nxt.Underline &&
                    cur.ForeColor.Equals(nxt.ForeColor);

                if (adjacent && same)
                {
                    cur.Length += nxt.Length;
                    cur.Text += nxt.Text;
                }
                else
                {
                    merged.Add(cur);
                    cur = nxt;
                }
            }

            merged.Add(cur);
            runs.Clear();
            runs.AddRange(merged);
        }
    }
}
