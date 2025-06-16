namespace LingoEngine.Animations
{
    /// <summary>
    /// Additional configuration for sprite tweening.
    /// </summary>
    public class LingoTweenOptions
    {
        public bool Enabled { get; set; } = true;
        public float Curvature { get; set; } = 0f;
        public bool ContinuousAtEndpoints { get; set; } = true;
        public LingoSpeedChangeType SpeedChange { get; set; } = LingoSpeedChangeType.Sharp;
        public float EaseIn { get; set; } = 0f;
        public float EaseOut { get; set; } = 0f;
    }
}
