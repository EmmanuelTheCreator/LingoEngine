namespace Director.IO.Data
{
    /// <summary>
    /// The Input Map must be the first chunk in the Director Movie File. It has the absolute address of the Memory Map within the current file. 
    /// The Input Map is read directly from the file rather than read into memory. It is changed when a new Memory Map is created,
    /// in order to avoid overwriting the old one, which would take more time. The Save and Compact feature removes old Memory Maps, 
    /// updating this chunk. The Input Map also has the version of Memory Map to use.
    /// </summary>
    public class FileIMapData
    {
        public int MemoryMapCount { get; internal set; }
        public int MemoryMapOffset { get; internal set; }
        public short Reserved { get; internal set; }
        public short Unknown { get; internal set; }
        public int Reserved2 { get; internal set; }
        public int MemoryMapFileVersion { get; internal set; }
    }
}
