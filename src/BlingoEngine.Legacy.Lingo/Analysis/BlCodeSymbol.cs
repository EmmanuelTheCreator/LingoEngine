using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Represents a named element discovered while scanning a Lingo script, such as a variable,
/// handler parameter, or class declaration.
/// </summary>
public sealed class BlCodeSymbol
{
    private readonly List<BlSyntaxToken> _declarations = new();
    private readonly List<string> _resolvedTypeNames = new();
    private readonly HashSet<string> _resolvedTypeNameSet = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _typeCodes = new();
    private readonly HashSet<string> _typeCodeSet = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new <see cref="BlCodeSymbol"/> with the supplied kind and name.
    /// </summary>
    /// <param name="kind">The category of symbol that is being tracked.</param>
    /// <param name="name">The canonical name of the symbol.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is whitespace.</exception>
    public BlCodeSymbol(BlCodeSymbolKind kind, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Symbol name cannot be empty.", nameof(name));
        }

        Kind = kind;
        Name = name;
    }

    /// <summary>
    /// Gets the canonical name recorded for the symbol.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the semantic kind of the symbol.
    /// </summary>
    public BlCodeSymbolKind Kind { get; }

    /// <summary>
    /// Gets the list of tokens that declared this symbol.
    /// </summary>
    public IReadOnlyList<BlSyntaxToken> Declarations => _declarations;

    /// <summary>
    /// Gets the raw type code most recently assigned to the symbol, if any.
    /// </summary>
    public string? TypeCode => _typeCodes.Count > 0 ? _typeCodes[^1] : null;

    /// <summary>
    /// Gets the collection of type codes recorded for the symbol.
    /// </summary>
    public IReadOnlyList<string> TypeCodes => _typeCodes;

    /// <summary>
    /// Gets the most recent resolved type name recorded for the symbol, if known.
    /// </summary>
    public string? ResolvedTypeName => _resolvedTypeNames.Count > 0 ? _resolvedTypeNames[^1] : null;

    /// <summary>
    /// Gets the collection of resolved type names recorded for the symbol.
    /// </summary>
    public IReadOnlyList<string> ResolvedTypeNames => _resolvedTypeNames;

    /// <summary>
    /// Gets a value indicating whether any recorded type codes have not been resolved yet.
    /// </summary>
    public bool HasUnresolvedTypeCodes
    {
        get
        {
            if (_typeCodes.Count == 0)
            {
                return false;
            }

            if (_resolvedTypeNames.Count == 0)
            {
                return true;
            }

            foreach (var typeCode in _typeCodes)
            {
                if (!_resolvedTypeNameSet.Contains(typeCode))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Adds a token that declared the symbol, preserving source locations for diagnostics.
    /// </summary>
    /// <param name="declaration">The declaring token.</param>
    internal void AddDeclaration(BlSyntaxToken declaration)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        _declarations.Add(declaration);
    }

    /// <summary>
    /// Records the textual type code assigned to the symbol in the source, normalizing whitespace and
    /// retaining each distinct value so dynamic typing scenarios are preserved.
    /// </summary>
    /// <param name="typeCode">The type code extracted from the script.</param>
    public void SetTypeCode(string? typeCode)
    {
        var normalized = Normalize(typeCode);
        if (normalized is null)
        {
            return;
        }

        if (_typeCodeSet.Add(normalized))
        {
            _typeCodes.Add(normalized);
        }
    }

    /// <summary>
    /// Records a resolved type name for the symbol, typically filled in by later passes.
    /// </summary>
    /// <param name="typeName">The resolved type name.</param>
    public void SetResolvedTypeName(string? typeName)
    {
        var normalized = Normalize(typeName);
        if (normalized is null)
        {
            return;
        }

        if (_resolvedTypeNameSet.Add(normalized))
        {
            _resolvedTypeNames.Add(normalized);
        }
    }

    /// <summary>
    /// Determines whether a resolved type name has already been recorded for the supplied identifier.
    /// </summary>
    /// <param name="typeName">The resolved type to check.</param>
    /// <returns><see langword="true"/> when the type name has been recorded; otherwise, <see langword="false"/>.</returns>
    public bool HasResolvedTypeName(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);
        return _resolvedTypeNameSet.Contains(typeName);
    }

    /// <summary>
    /// Trims and normalizes the supplied value so empty strings are treated as <see langword="null"/>.
    /// </summary>
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
