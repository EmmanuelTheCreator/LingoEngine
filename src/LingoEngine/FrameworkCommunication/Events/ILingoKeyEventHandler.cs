namespace LingoEngine.Core
{
    public interface ILingoKeyEventHandler
    {
        void RaiseKeyDown(LingoKey lingoKey);
        void RaiseKeyUp(LingoKey lingoKey);
    }
}
