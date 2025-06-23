namespace LingoEngine.Primitives
{
    public static class LingoMath
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int RoundToInt(double value)
        {
            return (int)Math.Round(value);
        }

        public static int FloorToInt(double value)
        {
            return (int)Math.Floor(value);
        }

        public static int CeilToInt(double value)
        {
            return (int)Math.Ceiling(value);
        }
    }
}
