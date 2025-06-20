namespace LingoEngine.Animations
{
    /// <summary>
    /// Represents a position of a sprite for a specific frame.
    /// </summary>
    public record LingoSpriteMotionFrame(int Frame, Primitives.LingoPoint Position, bool IsKeyFrame);

    /// <summary>
    /// Contains a collection of sprite motion frames describing its motion path.
    /// </summary>
    public class LingoSpriteMotionPath
    {
        public List<LingoSpriteMotionFrame> Frames { get; } = new();
    }
}
