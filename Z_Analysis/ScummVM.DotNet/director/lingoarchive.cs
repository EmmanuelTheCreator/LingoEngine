using Director.IO;
using Director.Members;
using Director.Primitives;
using Director.Scripts;
using Director.ScummVM;
using System.Text;

namespace Director
{
    public class LingoArchive
    {
        private readonly Archive _archive;
        private readonly List<LingoScript> _scripts = new();
        private readonly Dictionary<ScriptType, Dictionary<int, LingoScriptContext>> _scriptContexts = new();

        public Dictionary<int, LingoScriptContext> LctxContexts { get; } = new();

        public uint Version { get; set; }
        public LingoScriptNames ScriptNames { get; private set; } 


        public LingoArchive(Archive archive)
        {
            _archive = archive ?? throw new ArgumentNullException(nameof(archive));
            ScriptNames = new LingoScriptNames(0);
        }



        #region Script contexts
        public Dictionary<ScriptType, Dictionary<int, LingoScriptContext>> ScriptContexts => _scriptContexts;
        public LingoScriptContext? GetScriptContext(ScriptType type, CastMemberID id)
        {
            if (_scriptContexts.TryGetValue(type, out var byId) && byId.TryGetValue(id.Id, out var script))
                return script;
            return null;
        }
        public LingoScriptContext? GetScriptContext(ScriptType type, int id)
        {
            if (_scriptContexts.TryGetValue(type, out var byId) && byId.TryGetValue(id, out var script))
                return script;
            return null;
        }
        public void AddScriptContext(ScriptType type, int id, LingoScriptContext context)
        {
            if (!_scriptContexts.TryGetValue(type, out var byId))
            {
                byId = new Dictionary<int, LingoScriptContext>();
                _scriptContexts[type] = byId;
            }
            LctxContexts[id] = context;
            byId[id] = context;
        }

        #endregion
        public void PatchScriptHandler(ScriptType type, CastMemberID id)
        {
            // Placeholder implementation. In the real engine this would update
            // script dispatch tables so that new Lingo handlers become
            // immediately callable.
            if (_scriptContexts.TryGetValue(type, out var ctxs) && ctxs.TryGetValue(id.Id, out var ctx))
            {
                // For now simply ensure the context is tracked in LctxContexts
                LctxContexts[id.Id] = ctx;
            }
        }


     
        public bool HasResource(uint tag, int id) => _archive.HasResource(tag, id);

        public SeekableReadStreamEndian? OpenResource(uint tag, int id)
        {
            if (!_archive.HasResource(tag, id))
                return null;
            return _archive.GetResource(tag, id);
        }

        public byte[]? LoadResource(uint tag, int id)
        {
            using var stream = OpenResource(tag, id);
            if (stream == null)
                return null;

            using var ms = new MemoryStream();
            stream.BaseStream.CopyTo(ms);
            return ms.ToArray();
        }

        public IEnumerable<(int id, string name)> ListResources(uint tag)
        {
            foreach (var res in _archive.ListResources(tag))
            {
                yield return (res.Id, res.Name ?? string.Empty);
            }
        }

        public bool TryGetString(uint tag, int id, out string result)
        {
            result = string.Empty;
            var data = LoadResource(tag, id);
            if (data == null) return false;
            result = System.Text.Encoding.ASCII.GetString(data);
            return true;
        }

        public PaletteV4? GetPalette(Cast cast, int castId)
        {
            var paletteId = castId + cast.CastIDOffset;
            if (!HasResource(ResourceTags.CLUT, paletteId))
                return null;

            using var stream = OpenResource(ResourceTags.CLUT, paletteId);
            if (stream == null)
                return null;

            var palData = cast.LoadPalette(stream, paletteId);
            palData.Id = new CastMemberID(castId, cast.CastLibID);
            return new PaletteV4(palData);
        }

        public void AddCode(string source, ScriptType type, int id, string? name = null, ScriptFlags flags = ScriptFlags.None)
        {
            if ((flags & ScriptFlags.TrimGarbage) != 0)
            {
                source = source.TrimEnd('\0', ' ', '\t', '\r', '\n');
            }

            var script = new LingoScript
            {
                Id = id,
                Type = type,
                Name = name ?? $"Script_{id}",
                Source = source,
                Flags = flags
            };

            _scripts[id] = script;
        }

        public void AddNamesV4(SeekableReadStreamEndian stream)
        {
            var names = new LingoScriptNames(Version);
            names.Read(stream);
            ScriptNames = names;
        }

        public void AddCodeV4(SeekableReadStreamEndian stream, int contextId, string macName, ushort version, LingoScriptContext scriptContext, Dictionary<int, CastMember> loadedCast)
        {
            var script = new LingoScript(version)
            {
                Id = contextId
            };

            script.Read(stream);
            script.Name = $"Script_{contextId}";
            script.Type = ScriptType.Movie; // Default

            script.SetContext(scriptContext);
            script.ContextId = contextId;

            scriptContext.AddScript(script); // 👈 use a method to inject into context

            if (loadedCast?.TryGetValue(script.CastId, out var member) == true && member is ScriptCastMember scriptMember)
            {
                script.Type = scriptMember.ScriptType;
            }

            if (ConfMan.GetBool("dump_scripts"))
            {
                var path = DumpScriptName(macName, script.Type, script.Id, "lingo");
                File.WriteAllText(path, script.ScriptText("\n", true));
            }
        }

        public static string DumpScriptName(string macName, ScriptType type, int id, string extension)
        {
            var name = EncodePathForDump(macName);
            var typeName = type.ToString().ToLowerInvariant();
            return Path.Combine("lingo", $"{name}_{typeName}_{id}.{extension}");
        }

        private static string EncodePathForDump(string path)
        {
            // Replace unsafe characters or directory separators
            var sb = new StringBuilder(path.Length);
            foreach (var c in path)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else
                    sb.Append('_');
            }

            return sb.ToString();
        }
    }
}

