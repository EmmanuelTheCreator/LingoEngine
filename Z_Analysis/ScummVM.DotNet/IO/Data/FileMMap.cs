using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Resources;

namespace Director.IO.Data
{
    /// <summary>
    /// The Memory Map has an array of resources within the Director Movie File. Resources are Director's interpretation of chunks in the file. 
    /// The index of these resources correspond to what is called a ResourceID (however, not all resources are in the Memory Map.) 
    /// ResourceIDs are INT32s. For example, a ResourceID of 4 can refer to the fifth resource in the Memory Map (because they are zero indexed.) 
    /// Negative one is the null ResourceID, and is invalid. The Memory Map is read into memory, and is not read directly from the file. 
    /// It may be named after the Unix mmap function.
    /// 
    /// One may wonder why the Memory Map needs to exist in a RIFX Container where each chunk is one after the other.The reason is that not all
    /// of these chunks are used, because when saving over an old file, deleting one of these chunks would mean moving everything after it up in 
    /// the file, taking a significant amount of time. Instead, the Memory Map resources point to the chunks being used, and deleted chunks are 
    /// simply not pointed to anymore by their resources instead of being outright deleted.
    /// 
    /// The Memory Map has extra bytes on the end, past the number of used resources, in order to allow easy adding of resources to the Memory Map 
    /// and ensure a new Memory Map doesn’t have to be created often.
    /// </summary>
    public class FileMMapData
    {
        /// <summary>
        /// The first property in the Memory Map is the Properties Size, which seems to be ignored by recent Director versions. 
        /// </summary>
        public short PropertySize { get; internal set; }
        /// <summary>
        /// The second value is the Resource Size, which is not ignored, and is the size of an individual resource as the name would suggest. 
        /// </summary>
        public short ResourceSize { get; internal set; }
        /// <summary>
        /// These two values describe (1) the maximum amount of resources allowed in this Memory Map before a new one must be created and the Input Map be updated
        /// </summary>
        public uint MaxResourceCount { get; internal set; }
        /// <summary>
        /// Describes the number of resources that currently exist in this Memory Map.
        /// These two values describe (2) the maximum amount of resources allowed in this Memory Map before a new one must be created and the Input Map be updated
        /// </summary>
        public uint UsedResourceCount { get; internal set; }
        /// <summary>
        /// The first is the ResourceID of the last junk resource in the Memory Map. Resources are considered junk if they exist but are empty
        /// - for example, a Labels resource if there are no labels in the movie. 
        /// It is not currently entirely for certain what they do, but they do exhibit common patterns. 
        /// Remember that ResourceIDs will be -1 if null (to avoid using 0 because they are zero-indexed) which is often the case with these ResourceIDs.
        /// Furthermore, each junk chunk entry’s lastResourceID points to the junk chunk before it. This allows Director to quickly loop through all junk
        /// chunks to see how many chunks in the Memory Map are not in use.
        /// </summary>
        public int FirstJunkResourceId { get; internal set; }
        /// <summary>
        /// The second appears to be the ResourceID of a resource that corresponds to the previous Memory Map before this one was created.
        /// It is not currently entirely for certain what they do, but they do exhibit common patterns. 
        /// Remember that ResourceIDs will be -1 if null (to avoid using 0 because they are zero-indexed) which is often the case with these ResourceIDs.
        /// </summary>
        public int OldMapResourceId { get; internal set; }
        /// <summary>
        /// The third is the ResourceID of the last free resource in the Memory Map. This is a similar scenario to junk resources. Free resources
        /// correspond to deleted chunks or chunks that may eventually exist.
        /// It is not currently entirely for certain what they do, but they do exhibit common patterns. 
        /// Remember that ResourceIDs will be -1 if null (to avoid using 0 because they are zero-indexed) which is often the case with these ResourceIDs.
        /// </summary>
        public int FirstFreeResourceId { get; internal set; }

        /// <summary>Array of resources described by the memory map.</summary>
        public List<MMapEntry> Entries { get; internal set; } = new();

        public class MMapEntry
        {
            public string Tag { get; set; } = "";
            /// <summary>
            ///The number of cast member that has locked the chunk if any (or zero if none, since cast member numbers are not zero indexed and start at one.) 
            /// </summary>
            public int ResourceId { get; set; }
            /// <summary>
            /// The absolute position of the chunk in the file.
            /// The size and offset can be negative one if the resource has not been properly initialized.
            /// </summary>
            public uint Offset { get; set; }
            /// <summary>
            /// Size of memory to allocate.
            /// The size and offset can be negative one if the resource has not been properly initialized.
            /// </summary>
            public uint Size { get; set; }
            /// <summary>
            /// a seemingly unused value
            /// </summary>
            public short Unused { get; set; }

            public override string ToString() => $"[{Tag}] ID={ResourceId} Offset=0x{Offset:X} Size={Size}";
        }
    }
}
