using System;
using System.Collections.Generic;

namespace Director.IO.Data
{
    /// <summary>
    /// Represents a cast library's raw member data.
    /// </summary>
    public class FileCAStData
    {
        /// <summary>Raw bytes for each cast member.</summary>
        public List<MemberData> MembersData { get; } = new();

        public class MemberData
        {
            /// <summary>Cast member ID.</summary>
            public ushort Id { get; internal set; }
            /// <summary>Type identifier found in the CASt table.</summary>
            public ushort Type { get; internal set; }
            /// <summary>Raw data slice belonging to the member.</summary>
            public byte[] Data { get; internal set; } = Array.Empty<byte>();
        }
    }
}
