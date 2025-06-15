

namespace LingoEngine.IO.Data.DTO;

public class LingoMemberSoundDTO : LingoMemberDTO
{
    public bool Stereo { get; set; }
    public double Length { get; set; }
    public bool Loop { get; set; }
    public bool IsLinked { get; set; }
    public string LinkedFilePath { get; set; } = string.Empty;
    public string? SoundFile { get; set; }
}
