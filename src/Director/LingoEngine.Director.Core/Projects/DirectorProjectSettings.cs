namespace LingoEngine.Director.Core.Projects;

public enum DirectorIdeType
{
    VisualStudio,
    VisualStudioCode
}
public class DirectorProjectSettings
{
    public string? VisualStudioPath { get; set; } = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe";
    public string? VisualStudioCodePath { get; set; } = @"C:\Program Files\Microsoft VS Code\Code.exe";
    public DirectorIdeType PreferredIde { get; set; } = DirectorIdeType.VisualStudio;
}
