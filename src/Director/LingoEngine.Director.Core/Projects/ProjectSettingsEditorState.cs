

namespace LingoEngine.Director.Core.Projects
{
    public class ProjectSettingsEditorState
    {
        public string ProjectName { get; set; } = "";
        public string ProjectFolder { get; set; } = "";
        public IdeType SelectedIde { get; set; }
        public string VisualStudioPath { get; set; } = "";
        public string VisualStudioCodePath { get; set; } = "";

        public void LoadFrom(ProjectSettings settings)
        {
            ProjectName = settings.ProjectName;
            ProjectFolder = settings.ProjectFolder;
            SelectedIde = settings.PreferredIde;
            VisualStudioPath = settings.VisualStudioPath ?? "";
            VisualStudioCodePath = settings.VisualStudioCodePath ?? "";
        }

        public void SaveTo(ProjectSettings settings)
        {
            settings.ProjectName = ProjectName.Trim();
            settings.ProjectFolder = ProjectFolder.Trim();
            settings.PreferredIde = SelectedIde;
            settings.VisualStudioPath = VisualStudioPath.Trim();
            settings.VisualStudioCodePath = VisualStudioCodePath.Trim();
        }
    }
}
