using System.Collections.Generic;
using System.Text;

namespace Director.Scripts;

public class CSharpWriter
{
    public static string WriteTokens(IEnumerable<LingoToken> tokens)
    {
        var sb = new StringBuilder();
        foreach (var tok in tokens)
        {
            switch (tok.Type)
            {
                case LingoTokenType.Put:
                    sb.Append("// put\n");
                    break;
                case LingoTokenType.Into:
                    sb.Append("// into\n");
                    break;
                default:
                    sb.Append(tok.Lexeme + " ");
                    break;
            }
        }
        return sb.ToString();
    }
}
