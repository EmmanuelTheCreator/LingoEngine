using System.Collections.Generic;

namespace LingoEngine.IO.Data.DTO;

public class DirFilesContainerDTO
{
    public List<DirFileResourceDTO> Files { get; set; } = new();
}
