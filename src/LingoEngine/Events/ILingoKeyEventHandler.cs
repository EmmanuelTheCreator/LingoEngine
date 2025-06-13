using LingoEngine.Inputs;

namespace LingoEngine.Events
{
    public interface ILingoKeyEventHandler
    {
        void RaiseKeyDown(LingoKey lingoKey);
        void RaiseKeyUp(LingoKey lingoKey);
    }
}
