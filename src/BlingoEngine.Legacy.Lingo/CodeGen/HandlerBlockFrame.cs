namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class HandlerBlockFrame
{
    public HandlerBlockFrame(BlockKind kind, string? condition)
    {
        Kind = kind;
        Condition = condition;
    }

    public BlockKind Kind { get; }

    public string? Condition { get; set; }

    public bool CaseOpen { get; set; }
}
