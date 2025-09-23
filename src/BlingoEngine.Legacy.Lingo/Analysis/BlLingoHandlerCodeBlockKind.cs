namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Enumerates the logical kinds of statement or trivia blocks detected within a handler body.
/// </summary>
public enum BlLingoHandlerCodeBlockKind
{
    Blank,
    Comment,
    End,
    If,
    Else,
    ElseIf,
    RepeatWithRange,
    RepeatWithEach,
    RepeatWhile,
    RepeatUntil,
    RepeatForever,
    Case,
    CaseWhen,
    CaseOtherwise,
    Put,
    ActorListAppend,
    ActorListRemove,
    SendSprite,
    ExitRepeat,
    ExitRepeatIf,
    NextRepeat,
    NextRepeatIf,
    Return,
    MovieCall,
    Expression,
}
