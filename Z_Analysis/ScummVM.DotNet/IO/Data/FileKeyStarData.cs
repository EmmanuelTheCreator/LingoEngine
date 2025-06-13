namespace Director.IO.Data
{
    /// <summary>
    /// The Key Table must be the third chunk in the file. It allows for an “owned by” relationship to exist between resources. For example, 
    /// Sound Headers and Sound Samples can be “owned by” a Cast Member. A Cast Member’s Thumbnail can also be “owned by” a Cast Member. Crucially,
    /// the Key Table is context specific. This is because ResourceIDs do not uniquely identify resources. You may only reliably use it if you 
    /// already know which resource is the owner and the ChunkID of the chunk you want to find.
    /// 
    /// This is best illustrated by the fact that all of the resources owned by the movie itself are “owned by” a resource with ResourceID 1024.
    /// This is always true - it is a constant within Director itself, and it is hardwired to be this number.However, it is entirely possible for
    /// the Memory Map to also have a 1024th resource - say, a bitmap Cast Member. In that scenario, the movie and the bitmap Cast Member resource
    /// will share ResourceID 1024, and all of the resources they both own - such as the movie-owned Score, and the bitmap Cast Member-owned Bitmap 
    /// Data - will be owned by ResourceID 1024. However, since Director only cares about the Bitmap Data for bitmap Cast Members, it ignores the
    /// Score which is owned by the movie, and since Director only cares about the Score for the movie, it ignores the Bitmap Data.
    /// 
    /// The same is true for the ResourceIDs of the Cast resources. Cast Info, Cast Tables, Lingo Contexts and Score References may all belong to a Cast. 
    /// The ResourceIDs of the Casts are defined in the Cast Properties and may be any arbitrary number.
    /// 
    /// Much like the Memory Map, the Key Table has a maximum number of allowed entries. However, it is for a 
    /// different reason: Director doesn’t use the Key Table right away. It only uses the Key Table when it needs to find a resource owned by a different
    /// resource, and when it does, it loops through every entry in the Key Table. It applies an algorithm which skip counts over entries until it has
    /// passed the one it is looking for, and then it heads back. The reason for the extra unused memory on the end of the Key Table is to allow for 
    /// easy addition of resources to the table. This is also why the entries must be in numerical order by Owner ID.
    /// </summary>
    public class FileKeyStarData
    {
        /// <summary>Size in bytes of the properties section.</summary>
        public short PropertiesSize { get; internal set; }
        /// <summary>Size of each key entry.</summary>
        public short KeySize { get; internal set; }
        /// <summary>Maximum number of key entries allowed.</summary>
        public int MaxKeyCount { get; internal set; }
        /// <summary>Number of key entries currently used.</summary>
        public int UsedKeyCount { get; internal set; }

        /// <summary>All key table entries.</summary>
        public List<KeyEntryData> Keys { get; internal set; } = new();
        public class KeyEntryData
        {
            /// <summary>ID of the resource being referenced.</summary>
            public int OwnedResourceID { get; internal set; }
            /// <summary>Resource ID of the owner.</summary>
            public int OwnerResourceID { get; internal set; }
            /// <summary>Chunk ID tag of the owned resource.</summary>
            public string OwnedChunkID { get; internal set; }
        }
    }
}
