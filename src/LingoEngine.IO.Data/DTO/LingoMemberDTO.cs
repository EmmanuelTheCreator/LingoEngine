

namespace LingoEngine.IO.Data.DTO;

public class LingoMemberDTO
{
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public int CastLibNum { get; set; }
    public int NumberInCast { get; set; }
    public LingoMemberTypeDTO Type { get; set; }
    public LingoPointDTO RegPoint { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int PurgePriority { get; set; }
}
