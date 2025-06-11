using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Director.Tools
{
    public enum DebugChannel
    {
        Loading,
        NoBytecode,
        Compile,
        // Add other channels as needed
    }
    public static class LogHelper
    {
        public static void DebugWarning(string message)
           => Console.WriteLine("Warning: " + message);
        public static void DebugHexdump(byte[] data, int length)
        {
            const int bytesPerLine = 16;
            for (int i = 0; i < length; i += bytesPerLine)
            {
                var hex = new StringBuilder();
                var ascii = new StringBuilder();

                for (int j = 0; j < bytesPerLine; j++)
                {
                    int index = i + j;
                    if (index < length)
                    {
                        byte b = data[index];
                        hex.Append($"{b:X2} ");
                        ascii.Append(b >= 32 && b <= 126 ? (char)b : '.');
                    }
                    else
                    {
                        hex.Append("   ");
                        ascii.Append(' ');
                    }
                }

                Console.WriteLine($"{i:X4}: {hex}- {ascii}");
            }


        }
        public static void DebugLog(int level, DebugChannel channel, string message)
        {
            Console.WriteLine($"[Debug][Level {level}][{channel}] {message}");
        }

        /// <summary>
        /// Determines whether the specified debug channel is enabled at the given level.
        /// This is a stub — extend with real filtering logic if needed.
        /// </summary>
        public static bool DebugChannelSet(int level, DebugChannel channel)
        {
            // For now, enable all channels and levels >= 1
            return level >= 1;
        }
    }
}
