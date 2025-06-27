namespace LingoEngine.Projects;

using System.IO;


public class LingoProjectSettings
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectFolder { get; set; } = string.Empty;

    public bool HasValidSettings =>
        !string.IsNullOrWhiteSpace(ProjectName) &&
        !string.IsNullOrWhiteSpace(ProjectFolder);

    public int MaxSpriteChannelCount { get; set; } = 1000;

   

    public LingoProjectSettings()
    {

    }

    public string GetMoviePath(string movieName)
    {
        var file = movieName + ".json";
        return Path.Combine(ProjectFolder, file);
    }
}
