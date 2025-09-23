namespace BlingoEngine.Legacy.Lingo.Analysis;

public interface IBlBlockDataVisitor
{
    void Visit(IBlLegacyHandlerBlockDataVisitor visitor);
}

public interface IBlLegacyHandlerBlockDataVisitor
{
    void Visit(BlLingoIfBlockData data);

    void Visit(BlLingoElseIfBlockData data);

    void Visit(BlLingoRepeatWithRangeBlockData data);

    void Visit(BlLingoRepeatWithEachBlockData data);

    void Visit(BlLingoRepeatWhileBlockData data);

    void Visit(BlLingoRepeatUntilBlockData data);

    void Visit(BlLingoCaseBlockData data);

    void Visit(BlLingoCaseWhenBlockData data);

    void Visit(BlLingoPutBlockData data);

    void Visit(BlLingoActorListMutationBlockData data);

    void Visit(BlLingoSendSpriteBlockData data);

    void Visit(BlLingoExitRepeatIfBlockData data);

    void Visit(BlLingoMovieCallBlockData data);

    void Visit(BlLingoExpressionBlockData data);
}
