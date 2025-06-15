using Director.IO;
using System.Text;

namespace Director.Scripts
{

    /// <summary>
    /// Represents a structure holding Lingo script names from a resource.
    /// </summary>
    public class LingoScriptNames
    {
        public int Unknown0 { get; private set; }
        public int Unknown1 { get; private set; }
        public uint Length1 { get; private set; }
        public uint Length2 { get; private set; }
        public ushort NamesOffset { get; private set; }
        public ushort NamesCount { get; private set; }
        public List<string> Names { get; } = new();

        private readonly uint _version;

        public LingoScriptNames(uint version)
        {
            _version = version;
        }

        /// <summary>
        /// Reads the script name data from the stream.
        /// </summary>
        public void Read(SeekableReadStreamEndian stream)
        {
            Unknown0 = stream.ReadInt32();
            Unknown1 = stream.ReadInt32();
            Length1 = stream.ReadUInt32();
            Length2 = stream.ReadUInt32();
            NamesOffset = stream.ReadUInt16();
            NamesCount = stream.ReadUInt16();

            long basePos = stream.Position;

            for (int i = 0; i < NamesCount; i++)
            {
                stream.Seek(basePos + NamesOffset + i * 2);
                ushort offset = stream.ReadUInt16();
                stream.Seek(basePos + offset);
                string name = stream.ReadPascalString();
                Names.Add(name);
            }
        }
        /// <summary>
        /// Clears the current script name table.
        /// </summary>
        public void Clear()
        {
            Names.Clear();
        }

        /// <summary>
        /// Adds a new name to the script name table.
        /// </summary>
        /// <param name="name">The name to add.</param>
        public void Add(string name)
        {
            Names.Add(name);
        }
        public bool ValidName(int id)
        {
            return id >= 0 && id < Names.Count;
        }

        public string GetName(int id)
        {
            return ValidName(id) ? Names[id] : string.Empty;
        }
        /// <summary>
        /// Retrieves a name by ID safely.
        /// </summary>
        /// <param name="id">The name ID index.</param>
        /// <returns>The corresponding name, or empty string if not found.</returns>
        public string Get(int id)
        {
            return id >= 0 && id < Names.Count ? Names[id] : string.Empty;
        }
    }
}
