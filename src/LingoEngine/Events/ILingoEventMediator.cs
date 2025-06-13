namespace LingoEngine.Events
{
    public interface ILingoEventMediator
    {
        void Subscribe(object ms);
        void Unsubscribe(object ms);
    }
}
