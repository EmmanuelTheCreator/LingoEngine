using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyMemberPropertyFacts
{
    internal sealed record MemberPropertyInfo(string MemberTypeName, string PropertyName, string? ValueTypeName);

    private static readonly Dictionary<string, MemberPropertyInfo> s_memberProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] = new("IBlingoMemberTextBase", "Text", "string"),
        ["line"] = new("IBlingoMemberTextBase", "Line", "string"),
        ["word"] = new("IBlingoMemberTextBase", "Word", "string"),
        ["char"] = new("IBlingoMemberTextBase", "Char", "string"),
        ["editable"] = new("IBlingoMemberTextBase", "Editable", "bool"),
        ["wordwrap"] = new("IBlingoMemberTextBase", "WordWrap", "bool"),
        ["scrolltop"] = new("IBlingoMemberTextBase", "ScrollTop", "int"),
        ["textfont"] = new("IBlingoMemberTextBase", "Font", "string"),
        ["font"] = new("IBlingoMemberTextBase", "Font", "string"),
        ["textsize"] = new("IBlingoMemberTextBase", "FontSize", "int"),
        ["fontsize"] = new("IBlingoMemberTextBase", "FontSize", "int"),
        ["textstyle"] = new("IBlingoMemberTextBase", "FontStyle", null),
        ["fontstyle"] = new("IBlingoMemberTextBase", "FontStyle", null),
        ["textcolor"] = new("IBlingoMemberTextBase", "Color", "global::BlingoEngine.Primitives.AColor"),
        ["color"] = new("IBlingoMemberTextBase", "Color", "global::BlingoEngine.Primitives.AColor"),
        ["bold"] = new("IBlingoMemberTextBase", "Bold", "bool"),
        ["italic"] = new("IBlingoMemberTextBase", "Italic", "bool"),
        ["underline"] = new("IBlingoMemberTextBase", "Underline", "bool"),
        ["alignment"] = new("IBlingoMemberTextBase", "Alignment", null),
        ["margin"] = new("IBlingoMemberTextBase", "Margin", "int"),

        ["loop"] = new("BlingoMemberSound", "Loop", "bool"),
        ["linked"] = new("BlingoMemberSound", "IsLinked", "bool"),
        ["islinked"] = new("BlingoMemberSound", "IsLinked", "bool"),
        ["linkedfilepath"] = new("BlingoMemberSound", "LinkedFilePath", "string"),
        ["isexternal"] = new("BlingoMemberSound", "IsExternal", "bool"),
        ["length"] = new("BlingoMemberSound", "Length", "double"),
        ["stereo"] = new("BlingoMemberSound", "Stereo", "bool"),

        ["format"] = new("BlingoMemberBitmap", "Format", "string"),
        ["imagedata"] = new("BlingoMemberBitmap", "ImageData", "byte[]?"),
        ["isloaded"] = new("BlingoMemberBitmap", "IsLoaded", "bool"),

        ["vertexlist"] = new("BlingoMemberShape", "VertexList", null),
        ["shapetype"] = new("BlingoMemberShape", "ShapeType", "global::BlingoEngine.Shapes.BlingoShapeType"),
        ["shapetypeint"] = new("BlingoMemberShape", "ShapeTypeInt", "int"),
        ["fillcolor"] = new("BlingoMemberShape", "FillColor", "global::BlingoEngine.Primitives.AColor"),
        ["endcolor"] = new("BlingoMemberShape", "EndColor", "global::BlingoEngine.Primitives.AColor"),
        ["strokecolor"] = new("BlingoMemberShape", "StrokeColor", "global::BlingoEngine.Primitives.AColor"),
        ["strokewidth"] = new("BlingoMemberShape", "StrokeWidth", "int"),
        ["closed"] = new("BlingoMemberShape", "Closed", "bool"),
        ["antialias"] = new("BlingoMemberShape", "AntiAlias", "bool"),
        ["filled"] = new("BlingoMemberShape", "Filled", "bool"),

        ["duration"] = new("BlingoMemberMedia", "Duration", "int"),
        ["currenttime"] = new("BlingoMemberMedia", "CurrentTime", "int"),
        ["mediastatus"] = new("BlingoMemberMedia", "MediaStatus", "global::BlingoEngine.Medias.BlingoMediaStatus"),

        ["scripttype"] = new("BlingoMemberScript", "ScriptType", "global::BlingoEngine.Scripts.BlingoScriptType"),
        ["behaviortypename"] = new("BlingoMemberScript", "BehaviorTypeName", "string"),
    };

    public static bool TryGet(string? propertyName, out MemberPropertyInfo info)
    {
        if (!string.IsNullOrWhiteSpace(propertyName) && s_memberProperties.TryGetValue(propertyName, out var value))
        {
            info = value;
            return true;
        }

        info = default!;
        return false;
    }

    public static bool TryGetValueType(string? propertyName, out string typeName)
    {
        if (TryGet(propertyName, out var info) && !string.IsNullOrWhiteSpace(info.ValueTypeName))
        {
            typeName = info.ValueTypeName!;
            return true;
        }

        typeName = string.Empty;
        return false;
    }
}
