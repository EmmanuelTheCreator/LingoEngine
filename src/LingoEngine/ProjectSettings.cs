namespace LingoEngine;

using System.IO;

public class ProjectSettings
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectFolder { get; set; } = string.Empty;

    public bool HasValidSettings =>
        !string.IsNullOrWhiteSpace(ProjectName) &&
        !string.IsNullOrWhiteSpace(ProjectFolder);

    public int MaxSpriteChannelCount { get; set; } = 1000;

    public ProjectSettings()
    {
        
    }

    public string GetMoviePath(string movieName)
    {
        var file = movieName + ".json";
        return Path.Combine(ProjectFolder, file);
    }
}
