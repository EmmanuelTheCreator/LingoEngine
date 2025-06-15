using LingoEngine.Lingo.Core.Tokenizer;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Very small helper that converts simple Lingo statements to their C#
/// equivalents. This initial implementation only supports the "put <value> into <var>"
/// pattern to demonstrate tokenizer based translation.
/// </summary>
public static class LingoToCSharpConverter
{
    public static string Convert(string lingoSource)
    {
        var parser = new LingoAstParser();
        var ast = parser.Parse(lingoSource);
        return CSharpWriter.Write(ast);
    }
}
