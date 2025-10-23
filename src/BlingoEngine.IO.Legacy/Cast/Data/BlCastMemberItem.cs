namespace BlingoEngine.IO.Legacy.Cast.Data
{
    internal class BlCastMemberItem
    {
        public string Name { get; set; } = "";
        public string? MediaContentType {get;set;}
        public byte[]? Blob {get;set;}
        public DateTime? Created {get;set;}
        public DateTime? Modified { get; set; }
        public string MemberType { get; set; } = "";
    }
}
