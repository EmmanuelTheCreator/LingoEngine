using System.Collections.Generic;

namespace LingoEngine.IO.Data.DTO;

public class LingoMovieDTO
{
    public string Name { get; set; } = string.Empty;
    public int Number { get; set; }
    public int Tempo { get; set; }
    public int FrameCount { get; set; }
    public List<LingoCastDTO> Casts { get; set; } = new();
    public List<LingoSpriteDTO> Sprites { get; set; } = new();
}
