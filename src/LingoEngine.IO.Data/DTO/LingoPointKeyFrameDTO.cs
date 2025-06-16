namespace LingoEngine.IO.Data.DTO;

public class LingoPointKeyFrameDTO
{
    public int Frame { get; set; }
    public LingoPointDTO Value { get; set; }
    public LingoEaseTypeDTO Ease { get; set; }
}
