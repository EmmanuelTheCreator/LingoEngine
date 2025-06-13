namespace LingoEngine.FrameworkCommunication
{
    public interface ILingoFrameworkKey
    {
        bool CommandDown { get; }
        bool ControlDown { get; }
        bool OptionDown { get; }
        bool ShiftDown { get; }
        bool KeyPressed(LingoEngine.Inputs.LingoKeyType key);
        bool KeyPressed(char key);
        bool KeyPressed(int keyCode);
        string Key { get; }
        int KeyCode { get; }
    }
}
