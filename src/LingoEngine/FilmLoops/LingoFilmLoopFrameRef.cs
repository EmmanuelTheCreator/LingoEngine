namespace LingoEngine.FilmLoops
{
    /// <summary>
    /// Represents a reference to a cast member used as a frame within a film loop.
    /// </summary>
    public readonly record struct LingoFilmLoopFrameRef(int CastLibNum, int MemberNum);
}
