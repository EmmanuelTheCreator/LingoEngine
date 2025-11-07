using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlingoEngine.IO.Legacy.Director
{
    public class BlLegacyMovieConvertContext
    {
        private HashSet<string> _usedNames = new HashSet<string>();
        private BlingoCastDTO _currentCast = new BlingoCastDTO();
        private readonly BlLegacyBitmapExporter _bitmapExporter;
        private readonly BlLegacySoundExporter _soundExporter;
        private readonly BlLegacyMovieArchive _archive;
        private readonly DirFilesContainerDTO _resources;
        private readonly ILogger _logger;

        public BlLegacyMovieArchive Archive => _archive;

        public BlingoCastDTO CurrentCast => _currentCast;

        public BlLegacyMovieConvertContext(BlLegacyMovieArchive archive, DirFilesContainerDTO resources, ILogger logger)
        {
            _bitmapExporter = new BlLegacyBitmapExporter();
            _soundExporter = new BlLegacySoundExporter();
            var usedNames = new HashSet<string>(
            resources.Files.Where(f => f.Kind != DirFileResourceKind.Unknown).Select(f => f.FileName),
            StringComparer.OrdinalIgnoreCase);
            _usedNames = usedNames;
            _archive = archive;
            _resources = resources;
            _logger = logger;
        }

        public void SetCurrentCast(BlingoCastDTO castDto) => _currentCast = castDto;

        public string BuildScriptFileName(BlingoMemberDTO baseDto, BlCastRawMemberScript memberScript)
        {
            if (_currentCast == null) throw new Exception($"{nameof(_currentCast)} is not set in import context");

            var extension = memberScript.IsJavascript ? ".js" : ".ls";
            var linkedName = memberScript.LinkedFileName;
            var baseName = string.IsNullOrWhiteSpace(linkedName)
                ? $"{_currentCast.Number}_{baseDto.NumberInCast}"
                : Path.GetFileNameWithoutExtension(linkedName);

            if (string.IsNullOrWhiteSpace(baseName))
                baseName = $"{_currentCast.Number}_{baseDto.NumberInCast}";

            var sanitized = SanitizeFileName(baseName);
            return sanitized + extension;
        }

        private static string SanitizeFileName(string candidate)
        {
            if (string.IsNullOrEmpty(candidate))
                return "script";

            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(candidate.Length);

            for (var i = 0; i < candidate.Length; i++)
            {
                var ch = candidate[i];
                if (Array.IndexOf(invalidChars, ch) >= 0)
                    continue;

                builder.Append(ch);
            }

            return builder.Length == 0 ? "script" : builder.ToString();
        }

        public string DecodeText(BlLegacyText text)
        {
            return text.Format switch
            {
                BlLegacyTextFormatKind.Stxt => XmedExtensions.DecodeSTXT(text.Bytes),
                BlLegacyTextFormatKind.Xmed => DecodeStyledText(text.Bytes),
                _ => string.Empty
            };
        }

        public string DecodeField(BlLegacyField field)
        {
            return field.Format switch
            {
                BlLegacyFieldFormatKind.Stxt => XmedExtensions.DecodeSTXT(field.Bytes),
                BlLegacyFieldFormatKind.Xmed => DecodeStyledText(field.Bytes),
                _ => string.Empty
            };
        }

        public string DecodeStyledText(byte[] data)
        {
            int directorVersion = _archive.DirectorVersion;
            var reader = new BlXmedTextReader(_logger);
            var document = directorVersion > 0 ? reader.Read(data, directorVersion) : reader.Read(data);
            return BlXmedMarkdownConverter.ToCustomMarkdown(document);
        }

        public string EnsureUniqueFileName(string fileName)
        {
            var castName = _currentCast.Name;
            if (_usedNames.Add(castName + "/" + fileName))
                return fileName;

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var index = 1;
            string candidate;
            do
            {
                candidate = $"{nameWithoutExtension}_{index}{extension}";
                index++;
            }
            while (!_usedNames.Add(castName + "/" + candidate));

            return candidate;
        }



        public string GetCastFileName(string? castPath)
        {
            if (string.IsNullOrWhiteSpace(castPath))
                return string.Empty;

            var fileName = Path.GetFileName(castPath);
            return string.IsNullOrEmpty(fileName) ? castPath : fileName;
        }

        public bool TryGetText(int castResourceId, [NotNullWhen(true)] out BlLegacyText? text) { if (_archive.TryGetText(castResourceId, out var element)) {text = element; return true;} text = null; return false;}
        public bool TryGetField(int castResourceId, [NotNullWhen(true)] out BlLegacyField? field) { if (_archive.TryGetField(castResourceId, out var element)) { field = element; return true;} field = null; return false; }
        public bool TryGetBitmap(int castResourceId, [NotNullWhen(true)] out BlLegacyBitmap? bitmap) { if (_archive.TryGetBitmap(castResourceId, out var element)) { bitmap = element; return true;} bitmap = null; return false; }
        public bool TryGetSound(int castResourceId, [NotNullWhen(true)] out BlLegacySound? sound) { if (_archive.TryGetSound(castResourceId, out var element)) { sound = element; return true; } sound = null; return false; }

        internal void AddResource(DirFileResourceDTO resource)
        {
           _resources.Files.Add(resource);
        }

        public T CreateMember<T>(BlLegacyCastMemberSlot slot)
            where T : BlingoMemberDTO, new()
        {
            var rawMember = slot.Member;
            var memberIndex = slot.SlotIndex + 1;
            var memberName = string.IsNullOrWhiteSpace(rawMember.Name) ? $"Member {memberIndex}" : rawMember.Name;
            var member = new T
            {
                Name = memberName,
                CastLibNum = CurrentCast.Number,
                NumberInCast = memberIndex,
                Type = rawMember.MemberType.ToDto(),
                RegPoint = new BlingoPointDTO(),
                Width = 0,
                Height = 0,
                Size = 0,
                Comments = string.Empty,
                FileName = string.Empty,
                PurgePriority = 0
            };
            // Common
            member.DateCreated = rawMember.Created.GetValueOrDefault();
            member.DateModified = rawMember.Modified.GetValueOrDefault();
            member.MediaContentType = rawMember.MediaContentType ?? "";
            return member;
        }

        internal DirFileResourceDTO CreateSoundResource(BlLegacySound sound, int numberInCast)
        {
            var resource = _soundExporter.CreateResource(sound, _currentCast.Name, $"{_currentCast.Number}_{numberInCast}", _currentCast.Number, numberInCast);
            var fileName = EnsureUniqueFileName(resource.FileName);
            resource.FileName = fileName;
            AddResource(resource);
            return resource;
        }

        internal DirFileResourceDTO CreateBitmapResource(BlLegacyBitmap bitmap, int numberInCast)
        {
            var resource = _bitmapExporter.CreateResource(bitmap, CurrentCast.Name, $"{CurrentCast.Number}_{numberInCast}");
            var fileName = EnsureUniqueFileName(resource.FileName);
            resource.FileName = fileName;
            AddResource(resource);
            return resource;
        }

        internal DirFileResourceDTO CreateScriptResource(BlCastRawMemberScript memberScript, BlingoMemberScriptDTO baseDto)
        {
            var baseFileName = BuildScriptFileName(baseDto, memberScript);
            var fileName = EnsureUniqueFileName(baseFileName);
            var scriptBytes = Encoding.UTF8.GetBytes(memberScript.Script ?? string.Empty);
            baseDto.LinkedFilePath = fileName;

            var resource = new DirFileResourceDTO
            {
                CastName = _currentCast.Name,
                FileName = fileName,
                Bytes = scriptBytes,
                CastLibNum = _currentCast.Number,
                NumberInCast = baseDto.NumberInCast,
                Kind = DirFileResourceKind.Script
            };
            return resource;
        }
    }
}
