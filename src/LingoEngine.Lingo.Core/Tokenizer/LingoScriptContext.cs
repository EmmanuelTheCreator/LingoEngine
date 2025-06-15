using Director.IO;
using Director.Primitives;
using Director.Tools;

namespace Director.Scripts
{
    public class LingoScriptContext
    {
        private readonly ushort _version;
        private readonly ChunkResolver _chunkResolver;
        private readonly Dictionary<int, LingoScript> _scripts = new();
        public LingoScriptNames ScriptNames { get; }

        public IReadOnlyDictionary<int, LingoScript> Scripts => _scripts;

        public LingoScriptContext(ushort version, ChunkResolver chunkResolver)
        {
            _version = version;
            _chunkResolver = chunkResolver;
            ScriptNames = new LingoScriptNames(version);
        }

        public void Read(SeekableReadStreamEndian stream)
        {
            var nameStream = _chunkResolver.Cast.CastArchive.GetResourceOrNull(ResourceTags.Lnam, 0);
            if (nameStream != null)
            {
                ScriptNames.Read(nameStream);
                nameStream.Dispose();
            }

            var lscrList = _chunkResolver.Cast.CastArchive.GetChunkList(ResourceTags.Lscr);
            foreach (var entry in lscrList)
            {
                using var scrStream = entry.CreateReadStream();
                var script = new LingoScript(_version)
                {
                    Id = entry.Id,
                    Name = ScriptNames.Get(entry.Id)
                };
                script.Read(scrStream);
                _scripts[entry.Id] = script;
            }
        }


        public void ParseScripts()
        {
            foreach (var script in _scripts.Values)
                script.Parse();
        }

        public bool ValidName(int id)
        {
            return id >= 0 && id < ScriptNames.Names.Count;
        }

        public string GetName(int id)
        {
            return ScriptNames.Names[id];
        }

        public void AddScript(LingoScript script)
        {
            _scripts[script.Id] = script;
        }
    }
}
