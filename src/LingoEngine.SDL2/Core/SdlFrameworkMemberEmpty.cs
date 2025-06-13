using LingoEngine.FrameworkCommunication;

namespace LingoEngineSDL2.Core;

public class SdlFrameworkMemberEmpty : ILingoFrameworkMemberEmpty
{
    public bool IsLoaded => true;
    public void CopyToClipboard() { }
    public void Erase() { }
    public void ImportFileInto() { }
    public void PasteClipboardInto() { }
    public void Preload() { }
    public void Unload() { }
}
