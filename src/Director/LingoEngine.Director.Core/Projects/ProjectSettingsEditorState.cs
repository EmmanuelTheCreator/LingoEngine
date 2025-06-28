using LingoEngine.Projects;

namespace LingoEngine.Director.Core.Projects
{
    public class ProjectSettingsEditorState
    {
        public string ProjectName { get; set; } = "";
        public string ProjectFolder { get; set; } = "";
        public DirectorIdeType SelectedIde { get; set; }
        public string VisualStudioPath { get; set; } = "";
        public string VisualStudioCodePath { get; set; } = "";

        public void LoadFrom(LingoProjectSettings settings, DirectorProjectSettings directorProjectSettings)
        {
            ProjectName = settings.ProjectName;
            ProjectFolder = settings.ProjectFolder;
            SelectedIde = directorProjectSettings.PreferredIde;
            VisualStudioPath = directorProjectSettings.VisualStudioPath ?? "";
            VisualStudioCodePath = directorProjectSettings.VisualStudioCodePath ?? "";
        }

        public void SaveTo(LingoProjectSettings settings, DirectorProjectSettings directorProjectSettings)
        {
            settings.ProjectName = ProjectName.Trim();
            settings.ProjectFolder = ProjectFolder.Trim();
            directorProjectSettings.PreferredIde = SelectedIde;
            directorProjectSettings.VisualStudioPath = VisualStudioPath.Trim();
            directorProjectSettings.VisualStudioCodePath = VisualStudioCodePath.Trim();
        }
    }
}
