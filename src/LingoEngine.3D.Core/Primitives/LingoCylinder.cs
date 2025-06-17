namespace LingoEngine.Primitives3D;

/// <summary>
/// Model resource primitive of type #cylinder.
/// </summary>
public class LingoCylinder
{
    public float TopRadius { get; set; } = 25.0f;
    public float BottomRadius { get; set; } = 25.0f;
    public float Height { get; set; } = 50.0f;
    public bool TopCap { get; set; } = false;
    public bool BottomCap { get; set; } = false;
}
