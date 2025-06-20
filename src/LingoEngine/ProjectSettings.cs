namespace LingoEngine;

using System.IO;

public enum IdeType
{
    VisualStudio,
    VisualStudioCode
}

public class ProjectSettings
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectFolder { get; set; } = string.Empty;

    public bool HasValidSettings =>
        !string.IsNullOrWhiteSpace(ProjectName) &&
        !string.IsNullOrWhiteSpace(ProjectFolder);

    public int MaxSpriteChannelCount { get; set; } = 1000;

    public IdeType PreferredIde { get; set; } = IdeType.VisualStudio;

    public ProjectSettings()
    {
        
    }

    public string GetMoviePath(string movieName)
    {
        var file = movieName + ".json";
        return Path.Combine(ProjectFolder, file);
    }
}
