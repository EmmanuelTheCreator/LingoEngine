using Director.IO;
using Director.Members;
using Director.Primitives;
using Director.Scripts;

namespace Director.Tools
{
    public interface ILingoChunkResolver
    {
        /// <summary>
        /// Gets a compiled Lingo script by resource ID.
        /// </summary>
        LingoScript GetScript(int id);

        /// <summary>
        /// Gets the list of script names by resource ID.
        /// </summary>
        LingoScriptNames GetScriptNames(int id);
    }

    public class ChunkResolver : ILingoChunkResolver
    {
        private readonly Cast _cast;
        private readonly Dictionary<int, LingoScript> _scripts = new();
        private readonly Dictionary<int, LingoScriptNames> _scriptNames = new();
        public Cast Cast => _cast;
        public ChunkResolver(Cast cast)
        {
            _cast = cast;
        }

        public LingoScript GetScript(int id)
        {
            if (_scripts.TryGetValue(id, out var script))
                return script;

            using var stream = _cast.CastArchive.GetResource(ResourceTags.Lscr, id);
            script = new LingoScript(_cast.Version);
            script.Read(stream);
            _scripts[id] = script;

            return script;
        }

        public LingoScriptNames GetScriptNames(int id)
        {
            if (_scriptNames.TryGetValue(id, out var names))
                return names;

            using var stream = _cast.CastArchive.GetResource(ResourceTags.Lnam, id);
            names = new LingoScriptNames(_cast.Version);
            names.Read(stream);
            _scriptNames[id] = names;

            return names;
        }
        public void Resolve(SeekableReadStreamEndian stream, ScriptCastMember member, int version)
        {
            uint chunkType = stream.ReadUInt32BE();
            uint chunkSize = stream.ReadUInt32BE();

            switch (chunkType)
            {
                case 0x4C534352: // 'LSCR'
                    member.Read(stream, chunkSize, version);
                    break;
                default:
                    Console.WriteLine($"ChunkResolver::Resolve(): unknown chunk type 0x{chunkType:X8}");
                    break;
            }
        }
    }

}
