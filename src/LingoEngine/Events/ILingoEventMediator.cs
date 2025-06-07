namespace LingoEngine.Movies
{
    public interface ILingoEventMediator
    {
        void Subscribe(object ms);
        void Unsubscribe(object ms);
    }
}
