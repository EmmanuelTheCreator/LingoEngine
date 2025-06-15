using System.Collections.Generic;

namespace LingoEngine.IO.Data.DTO;

public class LingoCastDTO
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Number { get; set; }
    public PreLoadModeTypeDTO PreLoadMode { get; set; }
    public List<LingoMemberDTO> Members { get; set; } = new();
}
