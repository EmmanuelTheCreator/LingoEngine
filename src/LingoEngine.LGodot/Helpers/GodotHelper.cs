using Godot;

namespace LingoEngine.LGodot.Helpers
{
    public static class GodotHelper
    {
        public static string EnsureGodotUrl(string fileName) => (!fileName.StartsWith("res://") ? $"res://{fileName}" : fileName).Replace("\\", "/");
        public static string? ReadFile(string fileName)
        {
            var filePath = EnsureGodotUrl(fileName);
            var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
                return null;
            
            var rawTextData = file.GetAsText();
            return rawTextData;
        }
    }
}
