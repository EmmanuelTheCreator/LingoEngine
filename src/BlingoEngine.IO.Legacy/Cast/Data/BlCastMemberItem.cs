namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastMemberItem
    {
        public BlLegacyCastMemberType MemberType{ get; protected set; }
        public string Name { get; set; } = "";
        public string? MediaContentType {get;set;}
        public byte[]? Blob {get;set;}
        public DateTime? Created {get;set;}
        public DateTime? Modified { get; set; }
        public string MemberTypeString { get; set; } = "";
    
    }
}
