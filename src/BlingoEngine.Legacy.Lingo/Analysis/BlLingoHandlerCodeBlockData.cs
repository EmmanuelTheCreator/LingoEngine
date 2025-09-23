using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis;

public enum BlLingoPutAssignmentKind
{
    Direct,
    Field,
    SpriteProperty,
    SpriteMember,
    ListElement,
}

public sealed record BlLingoIfBlockData(string Condition) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoElseIfBlockData(string Condition) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoRepeatWithRangeBlockData(string VariableName, string StartExpression, string EndExpression)
    : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoRepeatWithEachBlockData(string VariableName, string SourceExpression) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoRepeatWhileBlockData(string Condition) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoRepeatUntilBlockData(string Condition) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoCaseBlockData(string Expression) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoCaseWhenBlockData(string Expression) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoPutBlockData(
    BlLingoPutAssignmentKind Kind,
    string ValueExpression,
    string? TargetExpression = null,
    string? FieldName = null,
    string? SpriteIndexExpression = null,
    string? SpritePropertyName = null,
    string? SpriteMemberArguments = null,
    string? ListExpression = null,
    string? ListIndexExpression = null) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoActorListMutationBlockData(string ArgumentExpression, bool IsRemoval = false) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoSendSpriteBlockData(
    string ChannelExpression,
    string HandlerName,
    string ParameterName,
    string? TargetScriptName,
    IReadOnlyList<string> Arguments,
    bool UsesResult,
    string? ResultTargetExpression = null,
    string? ResultTypeName = null) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoExitRepeatIfBlockData(string Condition, bool UseContinue = false) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoMovieCallBlockData(
    string HandlerName,
    string? TargetScriptName,
    bool UsesResult,
    IReadOnlyList<string> Arguments,
    string? ParameterName = null,
    string? ResultTargetExpression = null,
    string? ResultTypeName = null) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}

public sealed record BlLingoExpressionBlockData(string Expression) : IBlBlockDataVisitor
{
    public void Visit(IBlLegacyHandlerBlockDataVisitor visitor) => visitor.Visit(this);
}
