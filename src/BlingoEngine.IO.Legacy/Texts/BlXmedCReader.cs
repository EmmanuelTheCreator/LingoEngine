
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using static BlingoEngine.IO.Legacy.Texts.BlXmedCReader;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedHeaderAssuptionsPreamble
    {
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int? BaselineOffset { get; internal set; }
        public int? SpacingPad { get; internal set; }
        public int? DefaultFontSize { get; internal set; }
        public int? DefaultExtra { get; internal set; }
        public List<(byte,string)> Unknown { get; internal set; } = new();
        public string HeaderFlagRaw { get; internal set; } = "";
    }
    internal class BlXmedCReader
    {
        private readonly ILogger _logger;

        public sealed record XmedToken(byte Kind, ReadOnlyMemory<byte> Payload, int Offset);

        public sealed record XmedStyleRun(
            byte Open, byte Close,
            string? RHex, string? GHex, string? BHex,
            int? Size,
            List<int> SmallMetrics,
            List<XmedToken> Raw // keep everything for later re-interpretation
        ) : XmedBlock(Open, Close);

        /// <summary>Base for parsed C1…C2 blocks.</summary>
        public abstract record XmedBlock(byte Open, byte Close);



        /// <summary>Parsed alignment block (C1 04 → C2 20).</summary>
        public sealed record XmedAlignmentBlock(
            byte Open, byte Close,
            int? Alignment,
            int? Extra
        ) : XmedBlock(Open, Close);

        /// <summary>Parsed tabs/spacing block (C1 05 → C2 04/05).</summary>
        public sealed record XmedTabsBlock(
            byte Open, byte Close,
            List<int> Values
        ) : XmedBlock(Open, Close);

        /// <summary>Unknown/opaque C1…C2 block.</summary>
        public sealed record XmedUnknownBlock(
            byte Open, byte Close
        ) : XmedBlock(Open, Close);

        public BlXmedCReader(ILogger logger)
        {
            _logger = logger;
        }

        public XmedHeaderAssuptionsPreamble ReadHeaderPreamble(XMEDByteReader reader)
        {
            var pre = new XmedHeaderAssuptionsPreamble();

            // Expect we are right after a header boundary: C2 03
            // (If not already consumed, allow optional C2 03 here.)
            if (reader.Peek() == 0xC2 && reader.Peek(1) == 0x03) { reader.ReadByte(); reader.ReadByte(); }

            // 1) Dimensions Perhaps: 02 WWWWHHHH
            if (!reader.TryReadAsciiHexPair(out var dims)) return pre; // returns (width,height)
            pre.Width = dims.High;
            pre.Height = dims.Low;

            // 2) Then a short sequence of fields until a new section/opener.
            //    We read field-by-field; stop when hitting a hard boundary for header (C1 xx)
            //    or a section closer (C2 07/0A/20). C2 03 may repeat; ignore as boundary.
            for (; ; )
            {
                byte b = reader.Peek();
                if (b == 0xFF) break;                        // safety
                if (b == 0x00) { reader.ReadByte(); continue; }     // padding
                if (b == 0xC1) break;                        // next section starts → we’re done
                if (b == 0xC2)
                {
                    byte yy = reader.Peek(1);
                    // C2 03: header boundary → consume and continue
                    if (yy == 0x03) { reader.ReadByte(); reader.ReadByte(); continue; }
                    // Hard closers in header context: treat as done
                    if (yy == 0x07 || yy == 0x0A || yy == 0x20) break;
                    // Other C2 yy here are field delimiters; consume and continue
                    reader.ReadByte(); reader.ReadByte();
                    continue;
                }

                if (b == 0x02) // NUM
                {
                    if (!reader.TryReadNumericToken(out var val)) break;

                    // Assign first four numerics by position (ASSUMPTION labels):
                    if (!pre.BaselineOffset.HasValue) { pre.BaselineOffset = val; continue; }   // often -1
                    if (!pre.SpacingPad.HasValue) { pre.SpacingPad = val; continue; }   // often 0
                    if (!pre.DefaultFontSize.HasValue) { pre.DefaultFontSize = val; continue; }   // e.g., 0x18
                    if (!pre.DefaultExtra.HasValue) { pre.DefaultExtra = val; continue; }   // often 0

                    pre.Unknown.Add((0x02, val.ToString()));
                    continue;
                }

                if (b == 0x01) // VAL
                {
                    if (!reader.TryReadAsciiInt(out var lit)) // or a TryReadAsciiLiteral(out string s)
                    {
                        // Fallback literal read: consume until sep
                        var s = reader.ReadAsciiUntilControl();
                        pre.HeaderFlagRaw = s;
                    }
                    else
                    {
                        pre.HeaderFlagRaw = lit.ToString();
                    }
                    continue;
                }

                // Anything else: consume one and continue to avoid lock-ups
                reader.ReadByte();
            }
            return pre;
        }
        [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
        public sealed class XmedRun
        {
            public byte Op { get; set; }              // C1 opcode
            public int Start { get; set; }            // byte offset where run begins
            public int End { get; set; }              // byte offset after run ends
            public byte CloseTail { get; set; }       // C2 tail byte
            public byte RawFlags { get; set; }        // raw numeric/flag byte

            // --- Decoded values ---
            public XmedAlignment? Alignment { get; set; } // used when Op == 0x04

            // (later)
            // public (byte R, byte G, byte B)? Color { get; set; }
            // public int? FontSize { get; set; }
            // public bool Bold { get; set; }
            // etc.
            public string GetDebuggerDisplay()
            {
                string op = $"C1 {Op:X2}";
                string range = $"[{Start:X4}-{End:X4}]";
                string align = Alignment.HasValue ? Alignment.Value.ToString() : "-";
                string tail = $"C2 {CloseTail:X2}";
                return $"{op} {range} Flags={RawFlags:X2} Align={align} {tail}";
            }
        }

        
        /// <summary>Find the top-most *open* run with the given opcode in the stack.</summary>
        private static XmedRun? PeekOpenRun(Stack<XmedRun> stack, byte opcode)
        {
            foreach (var r in stack) if (r.Op == opcode) return r;
            return null;
        }
        // Inside BlXmedCReader.cs

        /// <summary>Final, stable result type for applying alignment to text.</summary>
        [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
        public sealed class XmedTextSpan
        {
            public int StartChar { get; set; }          // character offset
            public int Length { get; set; }             // character length
            public XmedAlignment Alignment { get; set; }// applied alignment
            public int RunIndex { get; set; }           // index of matched C1 03 (for debugging)
            public string GetDebuggerDisplay()
            {
                return $"StartChar={StartChar}/Length={Length}:RunIndex={RunIndex}:Align={Alignment}";
            }
        }

       

        /// <summary>
        /// Extracts ordered text slices (char Start/Length) from XMED 20-byte header cards.
        /// Keeps only entries that look like visible text spans.
        /// </summary>
        private static List<(int Start, int Length)> ReadTextSlices(byte[] buffer)
        {
            var r = new XMEDByteReader(buffer);
            var slices = new List<(int, int)>();

            while (!r.EOF)
            {
                if (!r.TryReadHeaderRecord(out var rec))
                {
                    // fall back to inline sentinel/cards
                    if (!r.TryReadHeaderOrInlineRecord(out rec)) { r.Skip(1); continue; }
                }

                // Heuristic: entries with Count>0 are span-like; Offset=char start, Count=length (observed).
                // Keep the widest coverage — we’ll rely on order to match C1 03.
                if (rec.Count > 0)
                    slices.Add((rec.Offset, rec.Count));
            }

            // Order by Start to ensure text-order mapping
            slices.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return slices;
        }


        /// <summary>Reads C1…C2 runs with paragraph-scoped alignment. Only Alignment is applied to C1 03 runs for now.</summary>
        public List<XmedRun> ReadRuns(byte[] buffer)
        {
            var r = new XMEDByteReader(buffer);
            var runs = new List<XmedRun>();

            // Stacks
            var opStack = new Stack<byte>();
            var runStack = new Stack<XmedRun>();

            // Paragraph alignment context (last C1 04 seen within current C1 20)
            XmedAlignment? currentAlign = null;
            var alignScopeStack = new Stack<XmedAlignment?>();

            while (!r.EOF)
            {
                // Skip cards/sentinels
                if (r.TryReadHeaderOrInlineRecord(out _))
                    continue;

                byte b = r.Peek();

                // --- OPEN: C1 xx ---
                if (b == XMEDByteReader.RUN && r.TryReadC1Opcode(out var op))
                {
                    var run = new XmedRun { Op = op, Start = r.Position - 2 };

                    // Paragraph start → push current scope
                    if (op == 0x20)
                        alignScopeStack.Push(currentAlign);

                    // Style/text block gets current paragraph alignment applied
                    if (op == 0x03)
                        run.Alignment = currentAlign;

                    opStack.Push(op);
                    runStack.Push(run);
                    continue;
                }

                // --- CLOSE: C2 yy ---
                if (b == XMEDByteReader.REND && r.TryReadC2Tail(out var tail))
                {
                    if (opStack.Count > 0 && runStack.Count > 0)
                    {
                        var closedOp = opStack.Pop();
                        var run = runStack.Pop();
                        run.CloseTail = tail;

                        // if this is a C1 03 and Alignment wasn't stamped yet, inherit currentAlign now
                        if (closedOp == 0x03 && run.Alignment == null)
                            run.Alignment = currentAlign;

                        // --- decode alignment on close for 0x04 and 0x1E (content between Start..End) ---
                        if (closedOp == 0x04 || closedOp == 0x1E)
                        {
                            // slice content: [after op][...][before C2]
                            int contentStart = run.Start + 2;         // skip C1 + opcode
                            int contentLen = (r.Position - 2) - contentStart; // exclude C2 + tail
                            if (contentLen > 0)
                            {
                                var inner = new XMEDByteReader(buffer.AsSpan(contentStart, contentLen).ToArray());

                                byte flags = 0;
                                while (!inner.EOF)
                                {
                                    if (inner.Peek() == XMEDByteReader.NUM && inner.TryReadNumericToken(out var n))
                                    { flags = (byte)(n & 0xFF); break; }

                                    if (inner.TryReadSmallParam(out var p2))
                                    { flags = (byte)(p2 & 0xFF); break; }

                                    if (inner.Peek() == XMEDByteReader.VAL && inner.TryReadAsciiInt(out var ai))
                                    { flags = (byte)(ai & 0xFF); break; }

                                    if (inner.Peek() is XMEDByteReader.SEP or XMEDByteReader.BND) { inner.SkipSeparators(); continue; }
                                    inner.Skip(1);
                                }

                                run.RawFlags = flags;
                                currentAlign = XMEDByteReader.DecodeAlignment(flags);

                                // if a C1 03 is currently open, stamp it now
                                var open03 = PeekOpenRun(runStack, 0x03);
                                if (open03 != null) open03.Alignment = currentAlign;
                            }
                        }


                        run.End = r.Position;
                        runs.Add(run);

                        if (closedOp == 0x20) // paragraph end
                            currentAlign = alignScopeStack.Count > 0 ? alignScopeStack.Pop() : (XmedAlignment?)null;
                    }
                    continue;
                }


                // --- TOKENS inside blocks ---

                // Alignment numeric inside C1 04
                if (b == XMEDByteReader.NUM && opStack.Count > 0 && opStack.Peek() == 0x04)
                {
                    if (r.TryReadNumericToken(out var n))
                    {
                        byte flags = (byte)(n & 0xFF);
                        currentAlign = XMEDByteReader.DecodeAlignment(flags);

                        // record flags on the open 0x04 run
                        var top = runStack.Pop();
                        top.RawFlags = flags;
                        runStack.Push(top);

                        // also stamp alignment onto the most-recent open C1 03 (if any)
                        var open03 = PeekOpenRun(runStack, 0x03);
                        if (open03 != null) open03.Alignment = currentAlign;
                    }
                    continue;
                }


                // Alignment small param (VAL…VAL) inside C1 04
                if (opStack.Count > 0 && opStack.Peek() == 0x04 && r.TryReadSmallParam(out var p))
                {
                    byte flags = (byte)(p & 0xFF);
                    currentAlign = XMEDByteReader.DecodeAlignment(flags);

                    var top = runStack.Pop();
                    top.RawFlags = flags;
                    runStack.Push(top);

                    var open03 = PeekOpenRun(runStack, 0x03);
                    if (open03 != null) open03.Alignment = currentAlign;
                    continue;
                }

                // Alignment ASCII-int inside C1 04 (e.g., VAL + "B2")
                if (opStack.Count > 0 && opStack.Peek() == 0x04 && b == XMEDByteReader.VAL)
                {
                    if (r.TryReadAsciiInt(out var ai))
                    {
                        byte flags = (byte)(ai & 0xFF);
                        currentAlign = XMEDByteReader.DecodeAlignment(flags);

                        // record flags on the open 0x04 run
                        var top = runStack.Pop();
                        top.RawFlags = flags;
                        runStack.Push(top);

                        // also stamp alignment onto the most-recent open C1 03 (if any)
                        var open03 = PeekOpenRun(runStack, 0x03);
                        if (open03 != null) open03.Alignment = currentAlign;
                        continue;
                    }
                }

                // Generic progress over separators/values/numbers
                if (b is XMEDByteReader.SEP or XMEDByteReader.BND) { r.SkipSeparators(); continue; }
                if (b == XMEDByteReader.VAL) { r.TryReadValueAscii(out _); continue; }
                if (b == XMEDByteReader.NUM) { r.TryReadNumericToken(out _); continue; }

                // Fallback progress
                r.Skip(1);
            }

            return runs;
        }

        /// <summary>
        /// Reads text spans from the header/run-map and assigns alignment from the C1 03 runs
        /// (order-matched within paragraphs). Only alignment is handled for now.
        /// </summary>
        public List<XmedTextSpan> ReadTextSpansWithAlignment(byte[] buffer)
        { 
            // 1) Parse C1…C2 blocks and compute paragraph-scoped alignment (your existing method).
            var runs = ReadRuns(buffer);
            var rCount = runs.Count(x => x.Op == 0x03);
            DumpNumericCandidates(_logger,buffer, minCount: 2 * rCount);

            // 2) Collect the plaintext run-map slices (char Start/Length) from 20-char header cards.
            var slices = ReadTextSlices(buffer); // returns ordered (Start, Length)

            // 3) Take C1 03 runs in the order they appear, carrying their resolved Alignment.
            var styleRuns = runs.Where(r => r.Op == 0x03).ToList();

            // 4) Map: k-th slice gets the k-th C1 03 alignment (within same paragraph order).
            // (If a style run has no alignment, default to Center.)
            var result = new List<XmedTextSpan>(slices.Count);
            for (int i = 0; i < slices.Count; i++)
            {
                var (start, len) = slices[i];

                XmedAlignment align = XmedAlignment.Center; // default
                if (i < styleRuns.Count && styleRuns[i].Alignment.HasValue)
                    align = styleRuns[i].Alignment.Value;

                result.Add(new XmedTextSpan
                {
                    StartChar = start,
                    Length = len,
                    Alignment = align,
                    RunIndex = i
                });
            }

            return result;
        }







        // TRASH CODE for finding numeric candidates in XMED blobs.


        // BlXmedCReader — diagnostics only (prints to Console)
        public static void DumpNumericCandidates(ILogger logger, byte[] buffer, int minCount)
        {
            var r = new XMEDByteReader(buffer);

            while (!r.EOF)
            {
                // Skip cards/inlines fast
                if (r.TryReadHeaderOrInlineRecord(out _)) continue;

                byte b = r.Peek();

                // 03-list (ASCII numbers list)
                if (b == 0x03)
                {
                    int start = r.Position;
                    if (r.TryReadAsciiNumbers(out var nums) && nums.Count >= minCount)
                    {
                        int end = r.Position;
                        logger.LogInformation($"Candidate: Origin=03-list  ByteSpan=[0x{start:X}-{end:X})  Count={nums.Count}");
                        logger.LogInformation("Values: " + string.Join(" ", nums.Take(40)));
                    }
                    continue;
                }

                // 02 repeated numerics
                if (b == 0x02)
                {
                    int start = r.Position;
                    var vals = new List<int>();
                    while (!r.EOF && r.Peek() == 0x02 && r.TryReadNumericToken(out var n)) vals.Add(n);

                    if (vals.Count >= minCount)
                    {
                        int end = r.Position;
                        logger.LogInformation($"Candidate: Origin=02-num   ByteSpan=[0x{start:X}-{end:X})  Count={vals.Count}");
                        logger.LogInformation("Values: " + string.Join(" ", vals.Take(40)));
                    }
                    continue;
                }

                // 01 repeated ascii-int
                if (b == 0x01)
                {
                    int start = r.Position;
                    var vals = new List<int>();
                    while (!r.EOF && r.Peek() == 0x01 && r.TryReadAsciiInt(out var ai)) vals.Add(ai);

                    if (vals.Count >= minCount)
                    {
                        int end = r.Position;
                        logger.LogInformation($"Candidate: Origin=01-int   ByteSpan=[0x{start:X}-{end:X})  Count={vals.Count}");
                        logger.LogInformation("Values: " + string.Join(" ", vals.Take(40)));
                    }
                    continue;
                }

                // progress
                if (b is XMEDByteReader.SEP or XMEDByteReader.BND) { r.SkipSeparators(); continue; }
                if (b == XMEDByteReader.REND) { r.TryReadC2Tail(out _); continue; }
                r.Skip(1);
            }
        }
    }



}
