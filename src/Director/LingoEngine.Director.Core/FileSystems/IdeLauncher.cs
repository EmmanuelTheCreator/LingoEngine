using System.Diagnostics;

#if USE_WINDOWS_FEATURES
using System.Runtime.InteropServices;
#endif



namespace LingoEngine.Director.Core.FileSystems
{
    public interface IIdeLauncher
    {
        void OpenFile(ProjectSettings settings, string filePath, int line = 0);
        void ResetSolutionCache();
    }

    public class IdeLauncher : IIdeLauncher
    {
        private readonly HashSet<string> _openSolutionPaths = new();
        private bool _hasScanned = false;

        public void OpenFile(ProjectSettings settings, string filePath, int line = 0)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File to open does not exist", filePath);

            switch (settings.PreferredIde)
            {
                case IdeType.VisualStudio:
                    OpenInVisualStudio(settings, filePath, line);
                    break;

                case IdeType.VisualStudioCode:
                    OpenInVisualStudioCode(settings.VisualStudioCodePath, filePath, line);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported IDE type: {settings.PreferredIde}");
            }
        }
        public void ResetSolutionCache()
        {
#if USE_WINDOWS_FEATURES
            _hasScanned = false;
            _openSolutionPaths.Clear();
#endif
        }

        private void OpenInVisualStudioCode(string? codePath, string filePath, int line)
        {
            if (string.IsNullOrEmpty(codePath))
                throw new InvalidOperationException("VisualStudioCodePath must be set.");

            string args = $"--reuse-window --goto \"{filePath}\":{line}";
            StartProcess(codePath, args);
        }

        private void StartProcess(string exePath, string arguments)
        {
            var info = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(info);
        }

#if USE_WINDOWS_FEATURES

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private void OpenInVisualStudio(ProjectSettings settings, string filePath, int line)
        {
            string? vsPath = settings.VisualStudioPath;
            if (string.IsNullOrEmpty(vsPath))
                throw new InvalidOperationException("VisualStudioPath must be set.");

            string? slnName = FindSolutionName(settings.ProjectFolder);
            bool solutionIsOpen = slnName != null && IsVisualStudioSolutionOpen(slnName);
            string args = solutionIsOpen
            ? $"/Edit \"{filePath}\""
            : $"\"{filePath}\"";

            if (line > 0 && solutionIsOpen)
                args += $" /Command \"Edit.GoTo {line}\"";

            StartProcess(vsPath, args);
        }
        private string? FindSolutionName(string projectFolder)
        {
            if (!Directory.Exists(projectFolder))
                return null;

            var slnFiles = Directory.GetFiles(projectFolder, "*.sln", SearchOption.TopDirectoryOnly);
            return slnFiles.Length > 0
                ? Path.GetFileNameWithoutExtension(slnFiles[0]).ToLowerInvariant()
                : null;
        }
        /// <summary>
        /// Checks if a Visual Studio window is open with the given .sln name in its title.
        /// </summary>
        private bool IsVisualStudioSolutionOpen(string slnFullPath)
        {
            if (!_hasScanned)
            {
                RefreshOpenSolutions();
            }

            var target = Path.GetFullPath(slnFullPath).ToLowerInvariant();
            return _openSolutionPaths.Contains(target);
        }
        private void RefreshOpenSolutions()
        {
            _openSolutionPaths.Clear();
            _hasScanned = true;

            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT CommandLine FROM Win32_Process WHERE Name = 'devenv.exe'");

                foreach (var obj in searcher.Get())
                {
                    string? commandLine = obj["CommandLine"]?.ToString()?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(commandLine))
                        continue;

                    if (commandLine.Contains(".sln"))
                    {
                        // Attempt to extract full .sln path
                        int slnIndex = commandLine.IndexOf(".sln", StringComparison.OrdinalIgnoreCase);
                        int start = commandLine.LastIndexOf('"', slnIndex);
                        int end = slnIndex + 4;

                        if (start >= 0 && end > start)
                        {
                            string path = commandLine.Substring(start + 1, end - start).Trim('"');
                            _openSolutionPaths.Add(Path.GetFullPath(path));
                        }
                    }
                }
            }
            catch
            {
                // Ignore WMI failures; cache remains empty
            }
        }



#else

        private void OpenInVisualStudio(string? vsPath, string filePath, int line)
        {
            if (string.IsNullOrEmpty(vsPath))
                throw new InvalidOperationException("VisualStudioPath must be set.");

            var isRunning = IsVisualStudioRunning();
            var command = isRunning ? "/Edit" : ""; // If already running, just edit the file

            string args = $"{command} \"{filePath}\"";

            if (line > 0 && isRunning)
                args += $" /Command \"Edit.GoTo {line}\"";

            StartProcess(vsPath, args);
        }
        private bool IsVisualStudioRunning()
        {
            // Check if any devenv processes are running
            return Process.GetProcessesByName("devenv").Any();
        }

#endif
    }


}
