using LingoEngine.Primitives;

namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkMovie
    {
        void UpdateStage();
        void RemoveMe();
        LingoPoint GetGlobalMousePosition();
    }
}
