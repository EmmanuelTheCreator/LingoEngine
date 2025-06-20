namespace LingoEngine.IO.Data.DTO;

public class LingoTweenOptionsDTO
{
    public bool Enabled { get; set; }
    public float Curvature { get; set; }
    public bool ContinuousAtEndpoints { get; set; }
    public LingoSpeedChangeTypeDTO SpeedChange { get; set; }
    public float EaseIn { get; set; }
    public float EaseOut { get; set; }
}
