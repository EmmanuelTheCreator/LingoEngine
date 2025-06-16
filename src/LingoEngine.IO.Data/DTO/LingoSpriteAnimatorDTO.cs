using System.Collections.Generic;

namespace LingoEngine.IO.Data.DTO;

public class LingoSpriteAnimatorDTO
{
    public List<LingoPointKeyFrameDTO> Position { get; set; } = new();
    public LingoTweenOptionsDTO PositionOptions { get; set; } = new();

    public List<LingoFloatKeyFrameDTO> Rotation { get; set; } = new();
    public LingoTweenOptionsDTO RotationOptions { get; set; } = new();

    public List<LingoFloatKeyFrameDTO> Skew { get; set; } = new();
    public LingoTweenOptionsDTO SkewOptions { get; set; } = new();

    public List<LingoColorKeyFrameDTO> ForegroundColor { get; set; } = new();
    public LingoTweenOptionsDTO ForegroundColorOptions { get; set; } = new();

    public List<LingoColorKeyFrameDTO> BackgroundColor { get; set; } = new();
    public LingoTweenOptionsDTO BackgroundColorOptions { get; set; } = new();

    public List<LingoFloatKeyFrameDTO> Blend { get; set; } = new();
    public LingoTweenOptionsDTO BlendOptions { get; set; } = new();
}
