using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Aggregates analysis data about class members that code generation consumes.
/// </summary>
public sealed class BlLegacyClassMemberInfo
{
    public static BlLegacyClassMemberInfo Empty { get; } = new(
        Array.Empty<BlLegacyPropertyInfo>(),
        Array.Empty<BlLingoHandlerSymbolTable>(),
        false);

    public BlLegacyClassMemberInfo(
        IReadOnlyList<BlLegacyPropertyInfo> properties,
        IReadOnlyList<BlLingoHandlerSymbolTable> handlerOrder,
        bool hasGlobalDeclarations)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        HandlerOrder = handlerOrder ?? throw new ArgumentNullException(nameof(handlerOrder));
        HasGlobalDeclarations = hasGlobalDeclarations;
    }

    /// <summary>
    /// Gets the ordered properties discovered for the class.
    /// </summary>
    public IReadOnlyList<BlLegacyPropertyInfo> Properties { get; }

    /// <summary>
    /// Gets the handlers in the order they appear in source.
    /// </summary>
    public IReadOnlyList<BlLingoHandlerSymbolTable> HandlerOrder { get; }

    /// <summary>
    /// Gets a value indicating whether the class declares global variables.
    /// </summary>
    public bool HasGlobalDeclarations { get; }
}
