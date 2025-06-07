namespace LingoEngine.FrameworkCommunication.Events
{
    public interface ILingoFontManager
    {
        ILingoFontManager AddFont(string name, string pathAndName);
        void LoadAll();
        T Get<T>(string name) where T : class;
    }
}
