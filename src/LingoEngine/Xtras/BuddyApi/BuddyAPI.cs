using System.ComponentModel;
using System.Diagnostics;

namespace LingoEngine.Xtras.BuddyApi
{
    public class BuddyAPI : IBuddyAPI
    {
        public string baSysFolder(string folder)
        {
            return folder.ToLower() switch
            {
                "system" => Environment.SystemDirectory,
                "windows" => Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _ => string.Empty
            };
        }

        public virtual string baFindApp(string extension)
        {
            //try
            //{
            //    var progId = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension)?.GetValue(null)?.ToString();
            //    if (progId == null) return string.Empty;
            //    var command = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($"{progId}\\shell\\open\\command")?.GetValue(null)?.ToString();
            //    if (command == null) return string.Empty;
            //    return command.Split('"')[1];
            //}
            //catch { return string.Empty; }
            return string.Empty;
        }

        public bool baFileExists(string fileName) => File.Exists(fileName);

        public bool baFolderExists(string folderName) => Directory.Exists(folderName);

        public bool baCreateFolder(string folderName)
        {
            try { Directory.CreateDirectory(folderName); return true; }
            catch { return false; }
        }

        public bool baDeleteFolder(string folderName)
        {
            try
            {
                if (Directory.Exists(folderName))
                {
                    Directory.Delete(folderName);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public bool baDeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName)) File.Delete(fileName);
                return true;
            }
            catch { return false; }
        }

        public bool baDeleteXFiles(string dirName, string fileSpec)
        {
            try
            {
                if (!Directory.Exists(dirName)) return true;
                foreach (var file in Directory.GetFiles(dirName, fileSpec))
                    File.Delete(file);
                return true;
            }
            catch { return false; }
        }

        public long baFileSize(string fileName)
        {
            try { return new FileInfo(fileName).Length; }
            catch { return -1; }
        }

        public string baFileAttributes(string fileName)
        {
            try { return File.GetAttributes(fileName).ToString(); }
            catch { return string.Empty; }
        }

        public bool baSetFileAttributes(string fileName, string attributes)
        {
            try
            {
                var attr = FileAttributes.Normal;
                foreach (var token in attributes.Split(','))
                {
                    var trimmed = token.Trim();
                    if (Enum.TryParse<FileAttributes>(trimmed, true, out var result))
                        attr |= result;
                }
                File.SetAttributes(fileName, attr);
                return true;
            }
            catch { return false; }
        }

        public int baCopyFile(string sourceFile, string destFile, string overwrite)
        {
            try
            {
                if (!File.Exists(sourceFile)) return 1;
                if (string.IsNullOrWhiteSpace(destFile)) return 2;
                var destExists = File.Exists(destFile);
                if (overwrite == "IfNotExist" && destExists) return 6;
                if (overwrite == "IfNewer" && destExists && File.GetLastWriteTime(destFile) >= File.GetLastWriteTime(sourceFile)) return 7;
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(sourceFile, destFile, true);
                return 0;
            }
            catch (IOException) { return 4; }
            catch { return 3; }
        }

        public int baCopyXFiles(string sourceDir, string destDir, string fileSpec, string overwrite)
        {
            try
            {
                if (!Directory.Exists(sourceDir)) return 1;
                if (string.IsNullOrWhiteSpace(destDir)) return 2;

                var files = Directory.GetFiles(sourceDir, fileSpec);
                if (files.Length == 0) return 8;

                foreach (var sourceFile in files)
                {
                    var destFile = Path.Combine(destDir, Path.GetFileName(sourceFile));
                    var result = baCopyFile(sourceFile, destFile, overwrite);
                    if (result != 0) return result;
                }
                return 0;
            }
            catch { return 3; }
        }

        public bool baOpenURL(string url, string state)
        {
            try
            {
                var psi = new ProcessStartInfo(url) { UseShellExecute = true };
                Process.Start(psi);
                return true;
            }
            catch { return false; }
        }

        public int baPrintFile(string fileName)
        {
            try
            {
                var psi = new ProcessStartInfo(fileName) { Verb = "print", UseShellExecute = true };
                Process.Start(psi);
                return 0;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode < 32)
            {
                return ex.NativeErrorCode;
            }
            catch { return -1; }
        }

        public IntPtr baWinHandle() => Process.GetCurrentProcess().MainWindowHandle;

        public void baAbout()
        {
            Console.WriteLine("BuddyAPI C# Implementation v1.0. Adapted from original Lingo Xtra by Emmanuel The Creator.");
        }

        public int baRegister(string userName, int regNumber)
        {
            // Simulate always successful registration
            return 30;
        }

        public bool baSaveRegistration(string userName, int regNumber)
        {
            // Simulate always successful save
            return true;
        }
    }

}