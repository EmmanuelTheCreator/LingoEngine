namespace LingoEngine._3D.Members;

/// <summary>
/// Visible object within a 3D world.
/// </summary>
public class LingoModel
{
    public LingoModelResource? ModelResource { get; set; }
    public LingoShader? Shader { get; set; }
    public LingoTexture? Texture { get; set; }
    public LingoVector3 Position { get; set; } = new(0,0,0);
    public LingoVector3 Rotation { get; set; } = new(0,0,0);
    public LingoVector3 Scale { get; set; } = new(1,1,1);
}
