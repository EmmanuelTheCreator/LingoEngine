using System;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Provides metadata about a known handler name, including how it should be categorized.
/// </summary>
public readonly struct BlLingoHandlerClassification
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlLingoHandlerClassification"/> struct.
    /// </summary>
    /// <param name="canonicalName">The canonical representation of the handler name.</param>
    /// <param name="kind">The category assigned to the handler.</param>
    /// <param name="impliedScriptKind">The script kind implied by the handler, if any.</param>
    /// <param name="requiresLeadingMeParameter">True when the implied script kind only applies when the first parameter is <c>me</c>.</param>
    public BlLingoHandlerClassification(
        string canonicalName,
        BlLingoHandlerKind kind,
        BlLingoScriptKind impliedScriptKind,
        bool requiresLeadingMeParameter = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalName);

        CanonicalName = canonicalName;
        Kind = kind;
        ImpliedScriptKind = impliedScriptKind;
        RequiresLeadingMeParameter = requiresLeadingMeParameter;
    }

    /// <summary>
    /// Gets the canonical representation of the handler name.
    /// </summary>
    public string CanonicalName { get; }

    /// <summary>
    /// Gets the handler category.
    /// </summary>
    public BlLingoHandlerKind Kind { get; }

    /// <summary>
    /// Gets the script kind implied by the handler.
    /// </summary>
    public BlLingoScriptKind ImpliedScriptKind { get; }

    /// <summary>
    /// Gets a value indicating whether the implied script kind only applies when the first parameter is <c>me</c>.
    /// </summary>
    public bool RequiresLeadingMeParameter { get; }
}
