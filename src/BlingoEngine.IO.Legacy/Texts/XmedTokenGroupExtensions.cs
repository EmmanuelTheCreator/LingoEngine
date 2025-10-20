using BlingoEngine.IO.Legacy.Texts.Data;


namespace BlingoEngine.IO.Legacy.Texts
{
    internal static class XmedTokenGroupExtensions
    {
      

      

        public static int ReadNumericAt(this XmedTokenGroup? c2Group, int index)
        {
            if (c2Group == null || c2Group.Type != BlXmedToken.TokenType.C2)
                return 0;

            int cursor = 0;
            foreach (var item in c2Group.Items)
            {
                if (item is not BlXmedToken token)
                    continue;

                if (!token.TryGetNumericValue(out var numeric))
                    continue;

                if (cursor == index)
                    return numeric;

                cursor++;
            }

            return 0;
        }

        public static bool ReadBooleanAt(this XmedTokenGroup? c2Group, int index)
        {
            return c2Group.ReadNumericAt(index) != 0;
        }
    }
}
