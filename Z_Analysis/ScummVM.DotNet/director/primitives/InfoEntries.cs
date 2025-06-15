namespace Director.Primitives
{
    public class InfoEntries
    {
        public List<InfoStringEntry> Strings { get; set; } = new();
        public uint Flags { get; set; }
        public uint ScriptId { get; set; }
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }
    }
}


