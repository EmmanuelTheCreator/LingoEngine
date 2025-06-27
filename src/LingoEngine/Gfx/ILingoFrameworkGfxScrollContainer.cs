namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific scroll container implementation.
    /// </summary>
    public interface ILingoFrameworkGfxScrollContainer : ILingoFrameworkGfxNode
    {
        void AddChild(ILingoFrameworkGfxNode child);
        float ScrollHorizontal { get; set; }
        float ScrollVertical { get; set; }
        bool ClipContents { get; set; }
    }
}
