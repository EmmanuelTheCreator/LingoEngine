namespace LingoEngine.Events
{
    public interface IHasLingoMessage
    {
        void HandleMessage(string message, params object[] args);
    }
}
