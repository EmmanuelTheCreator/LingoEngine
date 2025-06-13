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
        /// <summary>Total number of memory maps available in the file.</summary>
        public int MemoryMapCount { get; internal set; }
        /// <summary>File position of the first memory map.</summary>
        public int MemoryMapOffset { get; internal set; }
        /// <summary>Reserved value, usually zero.</summary>
        public short Reserved { get; internal set; }
        /// <summary>Unknown purpose, kept for compatibility.</summary>
        public short Unknown { get; internal set; }
        /// <summary>Second reserved integer.</summary>
        public int Reserved2 { get; internal set; }
        /// <summary>Version of the memory map structure.</summary>
        public int MemoryMapFileVersion { get; internal set; }
    }
}
