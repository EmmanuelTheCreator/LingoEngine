using System;
using System.Collections.Generic;
using System.IO;
using AbstUI.Tasks;
using BlingoEngine.Casts;
using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Casts
{
    public record DirCastImportMessage(TaskMessageType Type, string Text);

    public record DirCastImportResult(int ImportedCount, IBlingoMember? LastImportedMember, IReadOnlyList<DirCastImportMessage> Messages);

    public interface IDirCastImportService
    {
        DirCastImportResult ImportMembers(IBlingoCast cast, string? projectRoot, string targetFolder, int startSlot, IReadOnlyList<string> sourceFiles);
    }

    public class DirCastImportService : IDirCastImportService
    {
        private static readonly Dictionary<string, BlingoMemberType> FileTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = BlingoMemberType.Bitmap,
            [".jpg"] = BlingoMemberType.Bitmap,
            [".jpeg"] = BlingoMemberType.Bitmap,
            [".gif"] = BlingoMemberType.Bitmap,
            [".bmp"] = BlingoMemberType.Bitmap,
            [".tga"] = BlingoMemberType.Bitmap,
            [".wav"] = BlingoMemberType.Sound,
            [".mp3"] = BlingoMemberType.Sound,
            [".ogg"] = BlingoMemberType.Sound,
            [".aiff"] = BlingoMemberType.Sound,
            [".aif"] = BlingoMemberType.Sound,
            [".txt"] = BlingoMemberType.Text,
            [".rtf"] = BlingoMemberType.Field,
            [".cs"] = BlingoMemberType.Script,
            [".ls"] = BlingoMemberType.Script,
            [".lingo"] = BlingoMemberType.Script
        };

        public DirCastImportResult ImportMembers(IBlingoCast cast, string? projectRoot, string targetFolder, int startSlot, IReadOnlyList<string> sourceFiles)
        {
            if (cast == null)
                throw new ArgumentNullException(nameof(cast));

            var messages = new List<DirCastImportMessage>();

            if (sourceFiles == null || sourceFiles.Count == 0)
                return new DirCastImportResult(0, null, messages);

            var normalizedFiles = NormalizeFiles(sourceFiles, messages);
            if (normalizedFiles.Count == 0)
                return new DirCastImportResult(0, null, messages);

            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Error, "Project folder is not configured."));
                return new DirCastImportResult(0, null, messages);
            }

            string normalizedProjectRoot;
            try
            {
                normalizedProjectRoot = Path.GetFullPath(projectRoot);
            }
            catch (Exception ex)
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Error, $"Project folder is invalid: {ex.Message}"));
                return new DirCastImportResult(0, null, messages);
            }

            if (!Directory.Exists(normalizedProjectRoot))
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Error, "Project folder does not exist."));
                return new DirCastImportResult(0, null, messages);
            }

            string normalizedTargetFolder;
            try
            {
                normalizedTargetFolder = Path.GetFullPath(targetFolder);
            }
            catch (Exception ex)
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Error, $"Destination folder is invalid: {ex.Message}"));
                return new DirCastImportResult(0, null, messages);
            }

            if (!normalizedTargetFolder.StartsWith(normalizedProjectRoot, StringComparison.OrdinalIgnoreCase))
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Error, "Please select a folder inside the project directory."));
                return new DirCastImportResult(0, null, messages);
            }

            try
            {
                Directory.CreateDirectory(normalizedTargetFolder);
            }
            catch (Exception ex)
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Error, $"Failed to create destination folder: {ex.Message}"));
                return new DirCastImportResult(0, null, messages);
            }

            var slots = cast.ResolveFreeSlotNumbers(startSlot, normalizedFiles.Count);
            if (slots.Count == 0)
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Warning, "No empty cast slots are available."));
                return new DirCastImportResult(0, null, messages);
            }

            if (slots.Count < normalizedFiles.Count)
            {
                messages.Add(new DirCastImportMessage(TaskMessageType.Warning, $"Only {slots.Count} of {normalizedFiles.Count} file(s) can be imported because the cast has no additional empty slots."));
            }

            int importLimit = Math.Min(slots.Count, normalizedFiles.Count);
            int imported = 0;
            IBlingoMember? lastImported = null;

            for (int i = 0; i < importLimit; i++)
            {
                var sourcePath = normalizedFiles[i];

                if (!File.Exists(sourcePath))
                {
                    messages.Add(new DirCastImportMessage(TaskMessageType.Warning, $"File not found: {sourcePath}"));
                    continue;
                }

                if (!TryResolveMemberType(sourcePath, out var memberType))
                {
                    messages.Add(new DirCastImportMessage(TaskMessageType.Warning, $"Unsupported member type for \"{Path.GetFileName(sourcePath)}\"."));
                    continue;
                }

                var destinationPath = EnsureUniqueFileName(Path.Combine(normalizedTargetFolder, Path.GetFileName(sourcePath)));
                try
                {
                    File.Copy(sourcePath, destinationPath, overwrite: false);
                }
                catch (Exception ex)
                {
                    messages.Add(new DirCastImportMessage(TaskMessageType.Error, $"Failed to copy \"{Path.GetFileName(sourcePath)}\": {ex.Message}"));
                    continue;
                }

                var relativePath = Path.GetRelativePath(normalizedProjectRoot, destinationPath)
                    .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                var memberName = Path.GetFileNameWithoutExtension(destinationPath);
                int slot = slots[i];

                try
                {
                    lastImported = cast.Add(memberType, slot, memberName, relativePath);
                    imported++;
                }
                catch (Exception ex)
                {
                    messages.Add(new DirCastImportMessage(TaskMessageType.Error, $"Failed to add cast member for \"{Path.GetFileName(sourcePath)}\": {ex.Message}"));
                }
            }

            return new DirCastImportResult(imported, lastImported, messages);
        }

        private static List<string> NormalizeFiles(IReadOnlyList<string> files, List<DirCastImportMessage> messages)
        {
            var normalized = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                    continue;

                try
                {
                    var fullPath = Path.GetFullPath(file);
                    if (seen.Add(fullPath))
                        normalized.Add(fullPath);
                }
                catch (Exception ex)
                {
                    messages.Add(new DirCastImportMessage(TaskMessageType.Warning, $"Skipping \"{file}\": {ex.Message}"));
                }
            }

            return normalized;
        }

        private static bool TryResolveMemberType(string filePath, out BlingoMemberType memberType)
        {
            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                memberType = BlingoMemberType.Unknown;
                return false;
            }

            if (FileTypeMap.TryGetValue(extension, out memberType))
                return true;

            memberType = BlingoMemberType.Unknown;
            return false;
        }

        private static string EnsureUniqueFileName(string destinationPath)
        {
            if (!File.Exists(destinationPath))
                return destinationPath;

            var directory = Path.GetDirectoryName(destinationPath) ?? string.Empty;
            var name = Path.GetFileNameWithoutExtension(destinationPath);
            var extension = Path.GetExtension(destinationPath);

            int counter = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(directory, $"{name}_{counter}{extension}");
                counter++;
            }
            while (File.Exists(candidate));

            return candidate;
        }
    }
}

