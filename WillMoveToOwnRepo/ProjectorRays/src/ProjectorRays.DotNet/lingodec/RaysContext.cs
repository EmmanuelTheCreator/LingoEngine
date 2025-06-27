using ProjectorRays.Common;
using System.Collections.Generic;

namespace ProjectorRays.LingoDec;

public interface ChunkResolver
{
    RaysScript? GetScript(int id);
    ScriptNames? GetScriptNames(int id);
}

public class ScriptContext
{
    public int Unknown0;
    public int Unknown1;
    public uint EntryCount;
    public uint EntryCount2;
    public ushort EntriesOffset;
    public short Unknown2;
    public int Unknown3;
    public int Unknown4;
    public int Unknown5;
    public int LnamSectionID;
    public ushort ValidCount;
    public ushort Flags;
    public short FreePointer;

    public uint Version;
    public ChunkResolver Resolver;
    public ScriptNames? Lnam;
    public List<ScriptContextMapEntry> SectionMap = new();
    public Dictionary<uint, RaysScript> Scripts = new();

    public ScriptContext(uint version, ChunkResolver resolver)
    {
        Version = version;
        Resolver = resolver;
    }

    public void Read(ReadStream stream)
    {
        stream.Endianness = Endianness.BigEndian;
        Unknown0 = stream.ReadInt32();
        Unknown1 = stream.ReadInt32();
        EntryCount = stream.ReadUint32();
        EntryCount2 = stream.ReadUint32();
        EntriesOffset = stream.ReadUint16();
        Unknown2 = stream.ReadInt16();
        Unknown3 = stream.ReadInt32();
        Unknown4 = stream.ReadInt32();
        Unknown5 = stream.ReadInt32();
        LnamSectionID = stream.ReadInt32();
        ValidCount = stream.ReadUint16();
        Flags = stream.ReadUint16();
        FreePointer = stream.ReadInt16();

        stream.Seek(EntriesOffset);
        SectionMap.Clear();
        for (int i = 0; i < EntryCount; i++)
        {
            var entry = new ScriptContextMapEntry();
            entry.Read(stream);
            SectionMap.Add(entry);
        }

        Lnam = Resolver.GetScriptNames(LnamSectionID);
        for (uint i = 1; i <= EntryCount; i++)
        {
            var section = SectionMap[(int)i - 1];
            if (section.SectionID > -1)
            {
                var script = Resolver.GetScript(section.SectionID);
                if (script != null)
                {
                    script.SetContext(this);
                    Scripts[i] = script;
                }
            }
        }
        foreach (var script in Scripts.Values)
        {
            if (script.IsFactory())
            {
                var parent = Scripts[(uint)script.ParentNumber + 1];
                parent.Factories.Add(script);
            }
        }
    }

    public void ParseScripts()
    {
        foreach (var script in Scripts.Values)
            script.Parse();
    }

    public bool ValidName(int id) => Lnam != null && Lnam.ValidName(id);
    public string GetName(int id) => Lnam != null ? Lnam.GetName(id) : $"NAME_{id}";
}

public class ScriptContextMapEntry
{
    public int Unknown0;
    public int SectionID;
    public ushort Unknown1;
    public ushort Unknown2;

    public void Read(ReadStream stream)
    {
        Unknown0 = stream.ReadInt32();
        SectionID = stream.ReadInt32();
        Unknown1 = stream.ReadUint16();
        Unknown2 = stream.ReadUint16();
    }
}
