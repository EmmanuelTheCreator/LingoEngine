namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastRawMemberItem
    {
        public BlLegacyCastMemberType MemberType{ get; protected set; }
        public string Name { get; set; } = "";
        public string? MediaContentType {get;set;}
        public List<byte[]> Blobs {get;set;} = new List<byte[]>();
        public DateTime? Created {get;set;}
        public DateTime? Modified { get; set; }
        public string MemberTypeString { get; set; } = "";
        public string? MemberFormat { get; set; }
    }
}
