using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 27_interesting encrypto.ls
    public class InterestingEncryptoParentScript : LingoParentScript
    {
        public InterestingEncryptoParentScript(ILingoMovieEnvironment env) : base(env){}

        public uint DoHexToInt(string hex)
        {
            uint v = 0;
            for (int i = 0; i < 8 && i < hex.Length; i++)
            {
                char c = hex[i];
                if (c >= 'a' && c <= 'f') v = v * 16 + (uint)(c - 'a' + 10);
                else if (c >= 'A' && c <= 'F') v = v * 16 + (uint)(c - 'A' + 10);
                else v = v * 16 + (uint)(c - '0');
            }
            return v;
        }

        public string DoIntToHex(uint value)
        {
            const string digits = "0123456789abcdef";
            char[] res = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                res[i] = digits[(int)(value & 0xF)];
                value >>= 4;
            }
            return new string(res);
        }

        public uint[] DoIntToBytes(int value)
        {
            uint[] bytes = new uint[4];
            bytes[0] = (uint)((value >> 24) & 0xFF);
            bytes[1] = (uint)((value >> 16) & 0xFF);
            bytes[2] = (uint)((value >> 8) & 0xFF);
            bytes[3] = (uint)(value & 0xFF);
            return bytes;
        }

        public int UnsignedAdd(int x, int y)
        {
            uint ux = (uint)x & 0x7FFFFFFF;
            uint uy = (uint)y & 0x7FFFFFFF;
            int res = (int)(ux + uy);
            if ((x < 0 && y > 0) || (x > 0 && y < 0))
                res = (int)((uint)res ^ 0x80000000);
            return res;
        }

        public uint ShiftRight(uint n)
        {
            return n >> 1;
        }

        public string TripletToQuad(byte a, byte b, byte c)
        {
            const string digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            int q1 = (a & 0xFC) >> 2;
            int q2 = ((a & 0x03) << 4) + ((b & 0xF0) >> 4);
            int q3 = ((b & 0x0F) << 2) + ((c & 0xC0) >> 6);
            int q4 = c & 0x3F;
            return string.Create(4, 0, (span, _) => { span[0] = digits[q1]; span[1] = digits[q2]; span[2] = digits[q3]; span[3] = digits[q4]; });
        }

        public byte[] QuadToTriplet(string quad)
        {
            const string digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            int plus = '+';
            int a = digits.IndexOf(quad[0]) - plus + plus;
            int bIdx = digits.IndexOf(quad[1]) - plus;
            int b = bIdx;
            int temp = digits.IndexOf(quad[2]) - plus;
            b = b + (temp >> 4);
            int c = ((temp & 0x0F) << 6) + (digits.IndexOf(quad[3]) - plus);
            a = (a << 2) + (bIdx >> 4);
            b = ((bIdx & 0x0F) << 4) + (digits.IndexOf(quad[2]) - plus >> 2);
            return new byte[] { (byte)a, (byte)b, (byte)c };
        }

        public byte[] HexToBinString(string str)
        {
            int len = str.Length;
            byte[] res = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                res[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }
            return res;
        }

        public string BinToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty).ToUpper();
        }

        public byte[] IntToBinString(int i)
        {
            return BitConverter.GetBytes(i);
        }

        public int BinStringToInt(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }
    }
}
