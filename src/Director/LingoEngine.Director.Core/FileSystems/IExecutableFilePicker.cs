


namespace LingoEngine.Director.Core.FileSystems
{
    public interface IExecutableFilePicker
    {
        void PickExecutable(Action<string> onPicked);
    }
}
