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

public sealed record BlLingoIfBlockData(string Condition);

public sealed record BlLingoElseIfBlockData(string Condition);

public sealed record BlLingoRepeatWithRangeBlockData(string VariableName, string StartExpression, string EndExpression);

public sealed record BlLingoRepeatWithEachBlockData(string VariableName, string SourceExpression);

public sealed record BlLingoRepeatWhileBlockData(string Condition);

public sealed record BlLingoRepeatUntilBlockData(string Condition);

public sealed record BlLingoCaseBlockData(string Expression);

public sealed record BlLingoCaseWhenBlockData(string Expression);

public sealed record BlLingoPutBlockData(
    BlLingoPutAssignmentKind Kind,
    string ValueExpression,
    string? TargetExpression = null,
    string? FieldName = null,
    string? SpriteIndexExpression = null,
    string? SpritePropertyName = null,
    string? SpriteMemberArguments = null,
    string? ListExpression = null,
    string? ListIndexExpression = null);

public sealed record BlLingoActorListMutationBlockData(string ArgumentExpression);

public sealed record BlLingoSendSpriteBlockData(
    string ChannelExpression,
    string HandlerName,
    string ParameterName,
    string? TargetScriptName,
    IReadOnlyList<string> Arguments,
    bool UsesResult,
    string? ResultTargetExpression = null,
    string? ResultTypeName = null);

public sealed record BlLingoExitRepeatIfBlockData(string Condition);

public sealed record BlLingoMovieCallBlockData(
    string HandlerName,
    string? TargetScriptName,
    bool UsesResult,
    IReadOnlyList<string> Arguments,
    string? ParameterName = null,
    string? ResultTargetExpression = null,
    string? ResultTypeName = null);

public sealed record BlLingoExpressionBlockData(string Expression);
