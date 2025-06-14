namespace LingoEngine.Animations
{
    public class LingoKeyFrame<T>
    {
        public int Frame { get; set; }
        public T Value { get; set; }
        public LingoEaseType Ease { get; set; } = LingoEaseType.Linear;

        public LingoKeyFrame(int frame, T value)
        {
            Frame = frame;
            Value = value;
        }
    }
}
