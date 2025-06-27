using Godot;

namespace LingoEngine.Director.LGodot.Pictures;

/// <summary>
/// Utility to apply a pixel-perfect (nearest-filtered) material to any Control or CanvasItem.
/// </summary>
public static class PixelPerfectMaterial
{
    private static ShaderMaterial? _sharedMaterial;

    private static ShaderMaterial GetMaterial()
    {
        if (_sharedMaterial == null)
        {
            var shader = new Shader();
            shader.Code = """
                shader_type canvas_item;
                render_mode unshaded, texture_filter_nearest;
            """;

            _sharedMaterial = new ShaderMaterial();
            _sharedMaterial.Shader = shader;
        }

        return _sharedMaterial;
    }

    /// <summary>
    /// Applies pixel-perfect shader material to the given control.
    /// </summary>
    public static void ApplyTo(CanvasItem target)
    {
        target.Material = GetMaterial();
    }
}
