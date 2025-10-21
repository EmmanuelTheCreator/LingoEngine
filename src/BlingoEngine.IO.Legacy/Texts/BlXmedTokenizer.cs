using BlingoEngine.IO.Legacy.Texts.Data;
using System.Globalization;
using System.Text;
using static BlingoEngine.IO.Legacy.Texts.Data.BlXmedToken;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class BlXmedTokenizer
    {
        public (List<BlXmedToken> Tokens, List<int> LastNumbers) Tokenize(byte[] buf) => Tokenize(buf.AsSpan());

        // In BlXmedTokenizer: add helper
        static bool IsControl(byte b) => b is 0x00 or 0x01 or 0x02 or 0x03 or 0xC1 or 0xC2 or 0xC3 or 0x81 or 0x82;

        public static (List<BlXmedToken> Tokens, List<int> LastNumbers) Tokenize(ReadOnlySpan<byte> buffer)
        {
            var tokens = new List<BlXmedToken>();
            int i = 0;
            int n = buffer.Length;

            bool IsC(byte b) => b is 0xC1 or 0xC2 or 0xC3;
            bool IsB(byte b) => b is 0x81 or 0x82;
            bool IsCtrl(byte b) => b is 0x01 or 0x02 or 0x03;
            bool IsHexOrDash(byte b) => (b >= '0' && b <= '9') || (b >= 'A' && b <= 'F') || b == '-';
            var compositeStack = new Stack<(byte Token, int TypeValue)>();

            void PushComposite(byte token, int typeValue)
            {
                compositeStack.Push((token, typeValue));
            }

            void PopComposite()
            {
                if (compositeStack.Count == 0)
                    return;

                var (token, typeValue) = compositeStack.Pop();
            }

            var a00Tokens = new List<BlXmedToken>();
            while (i < n)
            {
                byte b = buffer[i];

                // Handle 0x00 block
                // BlXmedTokenizer.Tokenize — replace the 0x00 branch with this
                // In Tokenize(): replace the 0x00 branch
                if (TryReadBlock00(buffer, i, out var blk, out var next))
                {
                    a00Tokens.Add(blk);
                    tokens.Add(blk);
                    i = next;
                    continue;
                }


                // Handle C1/C2/C3
                if (IsC(b))
                {
                    var type = b == 0xC1 ? TokenType.C1 :
                               b == 0xC2 ? TokenType.C2 : TokenType.C3;

                    int typeVal = (i + 1 < n) ? buffer[i + 1] : -1;
                    int len = Math.Min(2, n - i);

                    tokens.Add(new BlXmedToken(type, i, len, typeValue: typeVal));
                    PushComposite(b, typeVal);
                    i += len;
                    continue;
                }

                // Handle B_81 / B_82
                if (IsB(b))
                {
                    var type = b == 0x81 ? TokenType.B_81 : TokenType.B_82;
                    tokens.Add(new BlXmedToken(type, i, 1, linkToPrevious: true));
                    if (type == TokenType.B_82)
                    {
                        PopComposite();
                    }
                    i++;
                    continue;
                }

                // Handle control-prefixed values (0x01/0x02/0x03)
                if (IsCtrl(b))
                {
                    int start = i++;

                    // Prefixed hex/string block
                    int j = i;
                    while (j < n && !IsCtrl(buffer[j]) && !IsC(buffer[j]) && !IsB(buffer[j]) && IsHexOrDash(buffer[j]))
                        j++;

                    int dataLen = j - (start + 1);
                    if (dataLen > 0)
                    {
                        var span = buffer.Slice(start + 1, dataLen);
                        string text = Encoding.ASCII.GetString(span);

                        int? value = int.TryParse(text, NumberStyles.HexNumber, null, out var v)
                            ? v : null;

                        tokens.Add(new BlXmedToken(
                            TokenType.PrefixedHex,
                            start,
                            1 + dataLen,
                            ascii: text,
                            value: value,
                            typeValue: b
                        ));

                        i = j;
                        continue;
                    }

                    var splitType = b == 0x01 ? TokenType.Split01 :
                                    b == 0x02 ? TokenType.Split02 : TokenType.Split03;
                    tokens.Add(new BlXmedToken(splitType, start, 1));
                    continue;
                }

                // ASCII chunk
                if (buffer[i] >= 0x20 && buffer[i] <= 0x7E)
                {
                    int start = i;
                    while (i < n &&
                           buffer[i] >= 0x20 && buffer[i] <= 0x7E &&
                           !IsC(buffer[i]) && !IsB(buffer[i]) && !IsCtrl(buffer[i]))
                        i++;

                    var str = Encoding.ASCII.GetString(buffer.Slice(start, i - start));
                    tokens.Add(new BlXmedToken(TokenType.Ascii, start, i - start, str));
                    continue;
                }

                // Fallback byte
                tokens.Add(new BlXmedToken(TokenType.Byte, i, 1, value: buffer[i]));
                i++;
            }
            var lastNumbers = a00Tokens.LastOrDefault()?.ReadBlock00Numbers()?.ToList() ?? new List<int>();
            foreach (var token in tokens)
            {
                ParseTokenValueSubType(token);
            }
            return (tokens, lastNumbers);
        }
        private static void ParseTokenValueSubType(BlXmedToken t)
        {
            switch (t.Type)
            {
                case TokenType.C2:
                    switch (t.TypeValue)
                    {
                        case 0x0A: break;
                        case 0x06: break;
                        case 0x07:  break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }


        private static bool TryReadBlock00(ReadOnlySpan<byte> buf, int start, out BlXmedToken tok, out int next)
        {
            tok = default!; next = start;
            if (buf[start] != 0x00) return false;
            if (start + 64 >= buf.Length) return false;
            var test = buf.Slice(start, 64).ToArray();

            int n = buf.Length;
            int i = start + 1;

            // read declared number until comma
            int j = i; while (j < n && buf[j] != (byte)',') j++;
            if (j >= n) return false;

            int declared = 0;
            if (!int.TryParse(Encoding.ASCII.GetString(buf.Slice(i, j - i)), out declared)) declared = -1;
            int dataStart = j + 1;
            int k = dataStart;
            // Special-case: Font family block (declared "40", fixed 58 bytes, 1-byte length + ASCII + zero pad)
            // declared == 40  → font family pair (name block + fixed tail)
            if (declared == 40 && dataStart + 64 <= n)
            {
                var data = buf.Slice(dataStart, 58).ToArray();     // 1 len + name + pad
                int nameLen = data[0];
                var name = (nameLen >= 0 && nameLen <= 57) ? Encoding.ASCII.GetString(data, 1, nameLen) : "";

                tok = new BlXmedToken(TokenType.Block00, start, (dataStart + 64) - start, value: declared, data: data, ascii: name);
                next = dataStart + 64;                              // consume the paired 64-byte style tail
                return true;
            }
            else
            {
                if (declared == 44)
                {
                    // last block
                    k = dataStart + 68;
                }
            }

            // Fallback: ASCII until next control

            while (k < n && !IsControl(buf[k])) k++;
            var fallback = buf.Slice(dataStart, k - dataStart).ToArray();
            tok = new BlXmedToken(TokenType.Block00, start, k - start, value: declared, data: fallback, ascii: Encoding.ASCII.GetString(fallback));
            next = k;
            return true;
        }


        /// <summary>
        /// After tokenizing, this extracts the content of all 0x00 blocks.
        /// All but the last are ASCII strings. The last one is a list of numbers.
        /// </summary>
        public static (List<string> TextBlocks, byte[] NumberList) DecodeBlock00Tokens(List<BlXmedToken> tokens)
        {
            var blocks = tokens.FindAll(t => t.Type == TokenType.Block00);
            var texts = new List<string>();

            for (int k = 0; k < blocks.Count - 1; k++)
                texts.Add(Encoding.ASCII.GetString(blocks[k].Data!));

            var lastBlock = blocks.Count > 0 ? blocks[^1].Data! : Array.Empty<byte>();
            return (texts, lastBlock);
        }


        public static List<XmedMainTokenGroup> CreateGroups(List<BlXmedToken> tokens)
        {
            return new XmedTokenGrouper().CreateGroups(tokens);
        }

           
     




        #region Log/Dump
        public static string DumpTokensCompact(List<BlXmedToken> tokens)
        {
            var sb = new StringBuilder();
            foreach (var t in tokens)
            {
                sb.Append($"{t.Start:X6} {t.Type,-9} L={t.Length}");
                if (t.Ascii is { Length: > 0 }) sb.Append($" a=\"{t.Ascii}\"");
                if (t.Value.HasValue) sb.Append($" v={t.Value}");
                if (t.TypeValue is > 0) sb.Append($" t={t.TypeValue:X2}");
                if (t.LinkToPrevious) sb.Append(" <-");
                if (t.Data is { Length: > 0 })
                {
                    int take = Math.Min(32, t.Data.Length);
                    sb.Append(" d=[");
                    for (int i = 0; i < take; i++) sb.Append($"{t.Data[i]:X2} ");
                    sb.Length--; sb.Append(t.Data.Length > take ? " …]" : " ]");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
      


      
        public static string DumpTokensUltraCompact(List<BlXmedToken> tokens, int startIndent = 0)
        {
            var sb = new StringBuilder();
            int last00 = tokens.FindLastIndex(t => t.Type == TokenType.Block00);
            int depth = startIndent;

            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];
                WriteToken(sb, last00== i, t, depth);
            }
            return sb.ToString().TrimEnd();
        }
        private static bool _lastWrittenWasNewLine;
        internal static void WriteTab(StringBuilder s, int num, int depth) => s.Append(new string (' ', (depth + num) * 2));
        public static void WriteToken(StringBuilder sb, bool isLast00,BlXmedToken t, int depth, bool byGroup = false, string? comment = null)
        {
            var lastWrittenWasNewLine = _lastWrittenWasNewLine;
            _lastWrittenWasNewLine = false;
            if (depth < 0) { sb.AppendLine($"! Depth<0 error ! {depth}"); depth = 0; }
            if (t is XmedTokenGroup group1)
            {
                sb.AppendLine();
              
                sb.Append($"*{t.TypeValue ?? 0:X2}");
            }
               
            int startLength = sb.Length;
            switch (t.Type)
            {
                case TokenType.Ascii:
                    if (t.Ascii != null && t.Ascii.Length >= 10)
                    {
                        if (!lastWrittenWasNewLine)
                            sb.AppendLine();
                        sb.Append($"{t.TypeValue ?? 0:X2}:{t.Ascii ?? "<empty>"} ");
                        sb.AppendLine();
                        WriteTab(sb,2, depth);
                        _lastWrittenWasNewLine = true;
                    }
                    else
                    {
                        WriteTab(sb, 2, depth);
                        sb.Append($"{t.TypeValue ?? 0:X2}:{t.Ascii ?? "<empty>"} ");
                    }
                    break;
                case TokenType.PrefixedHex:
                    if (t.Ascii == "FFFF")
                    {
                        if (!lastWrittenWasNewLine &&!byGroup)
                            sb.AppendLine();
                        WriteTab(sb, 1, depth);
                    }
                    if (t.Ascii != null && t.Ascii.Length >= 10)
                    {
                        if (!lastWrittenWasNewLine)
                            sb.AppendLine();
                        sb.Append($"{t.TypeValue ?? 0:X2}:{t.Ascii ?? "<empty>"} ");
                        sb.AppendLine();
                        WriteTab(sb, 2, depth);
                        _lastWrittenWasNewLine = true;
                    }
                    else
                        sb.Append($"{t.TypeValue ?? 0:X2}:{t.Ascii ?? "<empty>"} ");
                    break;
                
                case TokenType.C1:
                case TokenType.C2:
                case TokenType.C3:
                    if (!byGroup)
                        sb.AppendLine();
                    WriteTab(sb, 3, depth);
                    //if (t.SubType != TokenSubType.Unkown)
                    //    sb.Append($"{t.SubType}:{t.Type}({t.TypeValue ?? 0:X2}) ");
                    //else
                        sb.Append($"{t.Type}({t.TypeValue ?? 0:X2}) ");
                    break;

                case TokenType.B_82:
                    sb.Append("<82 ");
                    break;

                case TokenType.Block00:
                    if (!lastWrittenWasNewLine)
                        sb.AppendLine();
                    WriteTab(sb, 4, depth);
                    if (!isLast00)
                    {
                        sb.Append($"00({t.Value}):\"");
                        sb.Append(t.Ascii);
                        sb.Append('"' + Environment.NewLine);
                    } 
                    else
                    {
                        var lastNumbers = t.ReadBlock00Numbers();
                        sb.Append($"00({t.Value}):{string.Join(',', lastNumbers)}"); 
                        sb.AppendLine();
                    }
                    _lastWrittenWasNewLine = true;
                    break;

                case TokenType.B_81:
                    sb.Append("<81 ");
                    break;
                case TokenType.Split01:
                case TokenType.Split02:
                case TokenType.Split03:
                default:
                    if (!string.IsNullOrEmpty(t.Ascii))
                        sb.Append(t.Ascii! + " ");
                    break;
            }

            bool wroteNewline = sb.Length > startLength && sb[^1] == '\n';

            if (!string.IsNullOrEmpty(comment))
                wroteNewline = AppendComment(sb, startLength, comment!, wroteNewline);

            if (wroteNewline)
                _lastWrittenWasNewLine = true;
        }

        private static bool AppendComment(StringBuilder sb, int startLength, string comment, bool hadTrailingNewline)
        {
            if (hadTrailingNewline)
            {
                sb.Length--;
                if (sb.Length > startLength && sb[^1] == '\r')
                    sb.Length--;

                if (sb.Length > startLength && sb[^1] != ' ')
                    sb.Append(' ');

                sb.Append("// ");
                sb.Append(comment);
                sb.AppendLine();
                return true;
            }

            if (sb.Length > startLength && sb[^1] != ' ')
                sb.Append(' ');

            sb.Append("// ");
            sb.Append(comment);
            return false;
        }



        #endregion
    }


}




