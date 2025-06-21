


namespace LingoEngine.Director.Core.FileSystems
{


    public interface IIdePathResolver
    {
        string? AutoDetectVisualStudioPath();
        string? AutoDetectVSCodePath();
    }


    public class IdePathResolver : IIdePathResolver
    {
#if WINDOWS && !USE_WINDOWS_FEATURES
#warning You are building for Windows but USE_WINDOWS_FEATURES is not defined. Visual Studio detection is disabled.
#endif

#if USE_WINDOWS_FEATURES
        public string? AutoDetectVisualStudioPath()
        {
            var knownPaths = new[]
            {
            @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe",
            @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe"
        };
            return knownPaths.FirstOrDefault(File.Exists);
        }

        public string? AutoDetectVSCodePath()
        {
            var knownPaths = new[]
            {
            @"C:\Program Files\Microsoft VS Code\Code.exe",
            $@"C:\Users\{Environment.UserName}\AppData\Local\Programs\Microsoft VS Code\Code.exe"
        };
            return knownPaths.FirstOrDefault(File.Exists);
        }
#else
    public string? AutoDetectVisualStudioPath() => null;
    public string? AutoDetectVSCodePath() => null;
#endif
    }
}
