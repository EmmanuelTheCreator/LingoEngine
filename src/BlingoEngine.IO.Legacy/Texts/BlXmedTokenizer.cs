using System;
using System.Globalization;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal class BlXmedTokenizer
    {

        public enum TokenType { Split01, Split02, Split03, C1, C2, C3, B_81, B_82, PrefixedHex, Ascii, Boolean, Block00, Byte }
        public sealed record Token(TokenType Type, int Start, int Length, string? Ascii = null, int? Value = null, bool? BoolValue = null, int? TypeValue = null, bool LinkToPrevious = false, byte[]? Data = null)
        {
            public bool IsTextBlock() => Type == TokenType.Block00 && Value != 40 && Value != 44;

            public bool IsBlockBoundary()
            {
                return Type == TokenType.Block00 ||
                       (Type == TokenType.PrefixedHex && TypeValue == 0x03) ||
                       Type == TokenType.C1 ||
                       Type == TokenType.C2;
            }

            public bool TryGetNumericValue(out int value)
            {
                if (Value.HasValue)
                {
                    value = Value.Value;
                    return true;
                }

                value = 0;
                if (Ascii is not { } ascii || ascii.Length == 0)
                {
                    return false;
                }

                var text = ascii.Trim();
                bool negative = text.StartsWith("-", StringComparison.Ordinal);
                if (negative)
                {
                    text = text[1..];
                }

                if (text.Length == 0)
                {
                    return false;
                }

                if (!int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
                {
                    return false;
                }

                value = negative ? -parsed : parsed;
                return true;
            }

            public bool TryGetColorComponent(out byte component)
            {
                component = 0;
                if (string.IsNullOrWhiteSpace(Ascii))
                {
                    return false;
                }

                string text = Ascii.Trim();
                if (text.Length > 2)
                {
                    text = text[..2];
                }

                return byte.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out component);
            }
        }

        public (List<Token> Tokens, List<int> LastNumbers) Tokenize(byte[] buf) => Tokenize(buf.AsSpan());

        // In BlXmedTokenizer: add helper
        static bool IsControl(byte b) => b is 0x00 or 0x01 or 0x02 or 0x03 or 0xC1 or 0xC2 or 0xC3 or 0x81 or 0x82;

        public static (List<Token> Tokens, List<int> LastNumbers) Tokenize(ReadOnlySpan<byte> buffer)
        {
            var tokens = new List<Token>();
            int i = 0;
            int n = buffer.Length;

            bool IsC(byte b) => b is 0xC1 or 0xC2 or 0xC3;
            bool IsB(byte b) => b is 0x81 or 0x82;
            bool IsCtrl(byte b) => b is 0x01 or 0x02 or 0x03;
            bool IsHexOrDash(byte b) => (b >= '0' && b <= '9') || (b >= 'A' && b <= 'F') || b == '-';
            var a00Tokens = new List<Token>();
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

                    tokens.Add(new Token(type, i, len, TypeValue: typeVal));
                    i += len;
                    continue;
                }

                // Handle B_81 / B_82
                if (IsB(b))
                {
                    var type = b == 0x81 ? TokenType.B_81 : TokenType.B_82;
                    tokens.Add(new Token(type, i, 1, LinkToPrevious: true));
                    i++;
                    continue;
                }

                // Handle control-prefixed values (0x01/0x02/0x03)
                if (IsCtrl(b))
                {
                    int start = i++;

                    // Boolean: 01 30 or 01 31
                    if (b == 0x01 && i < n && (buffer[i] == '0' || buffer[i] == '1'))
                    {
                        bool bv = buffer[i] == '1';
                        tokens.Add(new Token(
                            TokenType.Boolean,
                            start,
                            2,
                            Ascii: bv ? "1" : "0",
                            Value: bv ? 1 : 0,
                            BoolValue: bv
                        ));
                        i++;
                        continue;
                    }

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

                        tokens.Add(new Token(
                            TokenType.PrefixedHex,
                            start,
                            1 + dataLen,
                            Ascii: text,
                            Value: value,
                            TypeValue: b
                        ));

                        i = j;
                        continue;
                    }

                    var splitType = b == 0x01 ? TokenType.Split01 :
                                    b == 0x02 ? TokenType.Split02 : TokenType.Split03;
                    tokens.Add(new Token(splitType, start, 1));
                    continue;
                }

                // ASCII chunk
                if (buffer[i] >= 0x20 && buffer[i] <= 0x7E)
                {
                    int start = i;
                    while (i < n &&
                           buffer[i] >= 0x20 && buffer[i] <= 0x7E &&
                           !IsC(buffer[i]) && !IsB(buffer[i]) && !IsCtrl(buffer[i]))
                    {
                        i++;
                    }

                    var str = Encoding.ASCII.GetString(buffer.Slice(start, i - start));
                    tokens.Add(new Token(TokenType.Ascii, start, i - start, str));
                    continue;
                }

                // Fallback byte
                tokens.Add(new Token(TokenType.Byte, i, 1));
                i++;
            }
            var lastNumbers = ParseLastBlock00Numbers(a00Tokens.Last());
            return (tokens, lastNumbers);
        }


        
        private static bool TryReadBlock00(ReadOnlySpan<byte> buf, int start, out Token tok, out int next)
        {
            tok = default!; next = start;
            if (buf[start] != 0x00) return false;

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

                tok = new Token(TokenType.Block00, start, (dataStart + 64) - start, Value: declared, Data: data, Ascii: name);
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
            tok = new Token(TokenType.Block00, start, k - start, Value: declared, Data: fallback, Ascii: Encoding.ASCII.GetString(fallback));
            next = k;
            return true;
        }





        /// <summary>
        /// After tokenizing, this extracts the content of all 0x00 blocks.
        /// All but the last are ASCII strings. The last one is a list of numbers.
        /// </summary>
        public static (List<string> TextBlocks, byte[] NumberList) DecodeBlock00Tokens(List<Token> tokens)
        {
            var blocks = tokens.FindAll(t => t.Type == TokenType.Block00);
            var texts = new List<string>();

            for (int k = 0; k < blocks.Count - 1; k++)
            {
                texts.Add(Encoding.ASCII.GetString(blocks[k].Data!));
            }

            var lastBlock = blocks.Count > 0 ? blocks[^1].Data! : Array.Empty<byte>();
            return (texts, lastBlock);
        }

        public static List<int> ParseLastBlock00Numbers(Token token)
        {
            var last = token.Data ?? Array.Empty<byte>();
            var list = new List<int>();
            int i = 0;
            while (i + 3 < last.Length)
            {
                if (last[i] != 0x01) { i++; continue; }   // skip noise
                list.Add(last[i + 1]);                    // code byte
                i += 4;                                   // 01 XX 00 00
            }
            return list;
        }

        public static string DumpTokensCompact(List<BlXmedTokenizer.Token> tokens)
        {
            var sb = new StringBuilder();
            foreach (var t in tokens)
            {
                sb.Append($"{t.Start:X6} {t.Type,-9} L={t.Length}");
                if (t.Ascii is { Length: > 0 }) sb.Append($" a=\"{t.Ascii}\"");
                if (t.Value.HasValue) sb.Append($" v={t.Value}");
                if (t.BoolValue.HasValue) sb.Append($" b={(t.BoolValue.Value ? 1 : 0)}");
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
        public static string DumpTokensUltraCompact(List<BlXmedTokenizer.Token> tokens)
        {
            var sb = new StringBuilder();
            int last00 = tokens.FindLastIndex(t => t.Type == BlXmedTokenizer.TokenType.Block00);
            int onLine = 0;

            void NL() { if (onLine > 0) { sb.Append('\n'); onLine = 0; } }
            void Add(string s) { if (onLine == 16) NL(); if (onLine > 0) sb.Append(' '); sb.Append(s); onLine++; }

            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];
                switch (t.Type)
                {
                    case BlXmedTokenizer.TokenType.Ascii:
                        NL(); sb.Append($"{(t.TypeValue ?? 0):X2}:{(t.Ascii ?? "<empty>")} "); break;



                    case BlXmedTokenizer.TokenType.C1:
                    case BlXmedTokenizer.TokenType.C2:
                    case BlXmedTokenizer.TokenType.C3:
                        NL(); sb.Append($"{t.Type}({(t.TypeValue ?? 0):X2}) ");  break;

                    case BlXmedTokenizer.TokenType.Block00:
                        NL();
                        if (i != last00)
                        {
                            sb.Append($"00({t.Value}):\"");
                            sb.Append(t.Ascii);
                            sb.Append('"'+Environment.NewLine);
                        }
                        else
                        {
                            var lastNumbers = ParseLastBlock00Numbers(t);
                            sb.Append($"00({t.Value}):{string.Join(',', lastNumbers)}");
                            sb.AppendLine();
                        }
                        NL();
                        break;

                    case BlXmedTokenizer.TokenType.Boolean:
                        Add(t.BoolValue == true ? "true" : "false");
                        break;

                    case BlXmedTokenizer.TokenType.B_81:
                        Add("<81 "); break;
                    case BlXmedTokenizer.TokenType.B_82:
                        Add("<82 "); break;

                    case BlXmedTokenizer.TokenType.PrefixedHex:
                        Add($"{(t.TypeValue ?? 0):X2}:{(t.Ascii ?? "<empty>")} "); break;


                    default:
                        if (!string.IsNullOrEmpty(t.Ascii)) Add(t.Ascii!);
                        break;
                }
            }
            return sb.ToString().TrimEnd();
        }


    }


}




