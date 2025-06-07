namespace LingoEngine.Texts
{
    public interface ILingoWord
    {
        string this[int index] { get; }

        public int Count { get; }
    }

 
}
