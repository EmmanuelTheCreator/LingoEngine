using System.Numerics;
using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Cast.Data;

public enum BlRawTextFraming : byte
{
    Fixed = 0x00,
    Scrolling = 0x01,
    AdjustToFit = 0x02
}

public enum BlRawTextAntiAlias : byte { None = 0, AllText = 1, LargerThan = 2 }
public enum BlRawTextKerningMode : byte { None = 0x44, AllText = 0x30, LargerThan = 0x75 }
public class BlCastRawMemberText : BlCastRawMemberItem
{
    public BlCastRawMemberText(
        string type,
        int specificDataLength,
        bool isEditable,
        BlRawTextFraming framing,
        bool tabsEnabled,
        bool dtdEnabled,
        bool isAntialiasEnabled,
        int antialiasMode,
        int antialiasLargerThanPointSize,
        int kerningLargerThanPointSize,
        bool isKerningEnabled,
        int kerningMode,
        bool useHyperlinkStyles,
        BlRawTextPreRenderInk preRenderInk,
        bool savePreRenderBitmap,
        string shaderTag,
        int shaderDataLength,
        int faceFlags,
        double tunnelDepth,
        bool isBevelEnabled,
        double bevelAmount,
        BlRawTextBevelEdge bevelEdge,
        int smoothness,
        BlRawTextDirectionalLight lightSetting,
        BlRawTextShaderTexture shaderTexture,
        int diffuseColorIndex,
        int specularColorIndex,
        int reflectivity,
        BlLegacyColor directionalColor,
        BlLegacyColor ambientColor,
        BlLegacyColor backgroundColor,
        Vector3 cameraPosition,
        double cameraDistance,
        Vector3 cameraRotation,
        double cameraFocalLength,
        string textureName)
    {
        MemberType = BlCastRawMemberType.Text;
        TextType = type;
        SpecificDataLength = specificDataLength;
        IsEditable = isEditable;
        Framing = framing;
        TabsEnabled = tabsEnabled;
        DtdEnabled = dtdEnabled;
        IsAntialiasEnabled = isAntialiasEnabled;
        AntialiasMode = antialiasMode;
        AntialiasLargerThanPointSize = antialiasLargerThanPointSize;
        KerningLargerThanPointSize = kerningLargerThanPointSize;
        IsKerningEnabled = isKerningEnabled;
        KerningMode = kerningMode;
        UseHyperlinkStyles = useHyperlinkStyles;
        PreRenderInk = preRenderInk;
        SavePreRenderBitmap = savePreRenderBitmap;
        ShaderTag = shaderTag;
        ShaderDataLength = shaderDataLength;
        FaceFlags = faceFlags;
        TunnelDepth = tunnelDepth;
        IsBevelEnabled = isBevelEnabled;
        BevelAmount = bevelAmount;
        BevelEdge = bevelEdge;
        Smoothness = smoothness;
        LightSetting = lightSetting;
        ShaderTexture = shaderTexture;
        DiffuseColorIndex = diffuseColorIndex;
        SpecularColorIndex = specularColorIndex;
        Reflectivity = reflectivity;
        DirectionalColor = directionalColor;
        AmbientColor = ambientColor;
        BackgroundColor = backgroundColor;
        CameraPosition = cameraPosition;
        CameraDistance = cameraDistance;
        CameraRotation = cameraRotation;
        CameraFocalLength = cameraFocalLength;
        TextureName = textureName;
    }

    public string TextType { get; }
    public int SpecificDataLength { get; }
    public bool IsEditable { get; }
    public BlRawTextFraming Framing { get; }
    public bool TabsEnabled { get; }
    public bool DtdEnabled { get; }
    public bool IsAntialiasEnabled { get; }
    public int AntialiasMode { get; }
    public int AntialiasLargerThanPointSize { get; }
    public int KerningLargerThanPointSize { get; }
    public bool IsKerningEnabled { get; }
    public int KerningMode { get; }
    public bool UseHyperlinkStyles { get; }
    public BlRawTextPreRenderInk PreRenderInk { get; }
    public bool SavePreRenderBitmap { get; }
    public string ShaderTag { get; }
    public int ShaderDataLength { get; }
    public int FaceFlags { get; }
    public double TunnelDepth { get; }
    public bool IsBevelEnabled { get; }
    public double BevelAmount { get; }
    public BlRawTextBevelEdge BevelEdge { get; }
    public int Smoothness { get; }
    public BlRawTextDirectionalLight LightSetting { get; }
    public BlRawTextShaderTexture ShaderTexture { get; }
    public int DiffuseColorIndex { get; }
    public int SpecularColorIndex { get; }
    public int Reflectivity { get; }
    public BlLegacyColor DirectionalColor { get; }
    public BlLegacyColor AmbientColor { get; }
    public BlLegacyColor BackgroundColor { get; }
    public Vector3 CameraPosition { get; }
    public double CameraDistance { get; }
    public Vector3 CameraRotation { get; }
    public double CameraFocalLength { get; }
    public string TextureName { get; }
}

public enum BlRawTextPreRenderInk
{
    None = 0,
    InkCopy = 1,
    InkOther = 2
}

public enum BlRawTextBevelEdge
{
    None = 0,
    Miter = 1,
    Round = 2
}

public enum BlRawTextDirectionalLight
{
    None = 0,
    TopLeft = 1,
    TopCenter = 2,
    TopRight = 3,
    MiddleLeft = 4,
    MiddleCenter = 5,
    MiddleRight = 6,
    BottomLeft = 7,
    BottomCenter = 8,
    BottomRight = 9
}

public enum BlRawTextShaderTexture
{
    None = 0,
    Default = 1,
    Member = 2
}
