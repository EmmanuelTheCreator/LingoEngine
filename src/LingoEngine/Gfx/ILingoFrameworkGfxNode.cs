namespace LingoEngine.Gfx
{
    /// <summary>
    /// Basic framework object that can be positioned and sized on screen.
    /// </summary>
    public interface ILingoFrameworkGfxNode : System.IDisposable
    {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        bool Visibility { get; set; }
    }
}
