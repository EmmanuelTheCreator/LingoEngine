namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Describes the different categories of symbols that can be discovered inside a Lingo script.
/// </summary>
public enum BlCodeSymbolKind
{
    GlobalVariable,
    LocalVariable,
    Property,
    Parameter,
    Class,
    Handler,
}
