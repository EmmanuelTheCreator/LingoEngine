using System.Text;

namespace Director.Tools
{
    public enum DebugChannel
    {
        Loading,
        NoBytecode,
        Compile,
        ImGui,
        
    }
    public static class LogHelper
    {
        private static int _level = 1;
        private static readonly HashSet<DebugChannel> _enabledChannels = new();

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
            if (DebugChannelSet(level, channel))
                Console.WriteLine($"[Debug][Level {level}][{channel}] {message}");
        }

        public static bool DebugChannelSet(int level, DebugChannel channel)
        {
            if (_level < 0) return true; // Always log
            return level >= _level && _enabledChannels.Contains(channel);
        }

        public static void Enable(DebugChannel channel)
        {
            _enabledChannels.Add(channel);
        }

        public static void Disable(DebugChannel channel)
        {
            _enabledChannels.Remove(channel);
        }

        public static void SetLevel(int level)
        {
            _level = level;
        }
    }

}
