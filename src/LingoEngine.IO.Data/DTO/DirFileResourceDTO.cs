using System;

namespace LingoEngine.IO.Data.DTO;

public class DirFileResourceDTO
{
    public string CastName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
}
