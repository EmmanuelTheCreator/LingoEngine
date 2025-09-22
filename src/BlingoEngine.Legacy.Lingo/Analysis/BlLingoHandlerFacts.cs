using System;
using System.Collections.Generic;
using System.Text;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Supplies handler classification information based on well-known Lingo messages.
/// </summary>
internal static class BlLingoHandlerFacts
{
    private static readonly Dictionary<string, BlLingoHandlerClassification> s_classifications = new(
        StringComparer.OrdinalIgnoreCase)
    {
        ["beginsprite"] = new("BeginSprite", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["endsprite"] = new("EndSprite", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["prepareframe"] = new("PrepareFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["enterframe"] = new("EnterFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["exitframe"] = new("ExitFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["stepframe"] = new("StepFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["mousedown"] = new("MouseDown", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["mouseup"] = new("MouseUp", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["mousewithin"] = new("MouseWithin", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["mouseenter"] = new("MouseEnter", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["mouseleave"] = new("MouseLeave", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["mousemove"] = new("MouseMove", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["keydown"] = new("KeyDown", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["keyup"] = new("KeyUp", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["focus"] = new("Focus", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["blur"] = new("Blur", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Unknown),
        ["preparemovie"] = new("PrepareMovie", BlLingoHandlerKind.Movie, BlLingoScriptKind.Movie),
        ["startmovie"] = new("StartMovie", BlLingoHandlerKind.Movie, BlLingoScriptKind.Movie),
        ["stopmovie"] = new("StopMovie", BlLingoHandlerKind.Movie, BlLingoScriptKind.Movie),
        ["getpropertydescriptionlist"] = new("GetPropertyDescriptionList", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["new"] = new("New", BlLingoHandlerKind.Custom, BlLingoScriptKind.Parent, requiresLeadingMeParameter: true),
    };

    /// <summary>
    /// Produces classification information for the supplied handler name.
    /// </summary>
    /// <param name="name">The raw handler name extracted from source.</param>
    public static BlLingoHandlerClassification GetClassification(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new BlLingoHandlerClassification(string.Empty, BlLingoHandlerKind.Custom, BlLingoScriptKind.Unknown);
        }

        if (s_classifications.TryGetValue(name, out var classification))
        {
            return classification;
        }

        var canonical = ToCanonicalName(name);
        return new BlLingoHandlerClassification(canonical, BlLingoHandlerKind.Custom, BlLingoScriptKind.Unknown);
    }

    private static string ToCanonicalName(string name)
    {
        var builder = new StringBuilder(name.Length);
        var makeUpper = true;

        foreach (var ch in name)
        {
            if (ch == '_' || char.IsWhiteSpace(ch))
            {
                makeUpper = true;
                continue;
            }

            if (makeUpper)
            {
                builder.Append(char.ToUpperInvariant(ch));
                makeUpper = false;
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }
}
