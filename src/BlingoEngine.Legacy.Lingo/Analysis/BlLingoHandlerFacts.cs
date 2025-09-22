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
        ["prepareframe"] = new("PrepareFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["enterframe"] = new("EnterFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["exitframe"] = new("ExitFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["stepframe"] = new("StepFrame", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["mousedown"] = new("MouseDown", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["mouseup"] = new("MouseUp", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["mousewithin"] = new("MouseWithin", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["mouseenter"] = new("MouseEnter", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["mouseleave"] = new("MouseLeave", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["mousemove"] = new("MouseMove", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["keydown"] = new("KeyDown", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["keyup"] = new("KeyUp", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["focus"] = new("Focus", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
        ["blur"] = new("Blur", BlLingoHandlerKind.Behavior, BlLingoScriptKind.Behavior),
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
