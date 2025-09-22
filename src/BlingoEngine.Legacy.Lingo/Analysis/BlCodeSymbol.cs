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
    private string? _resolvedTypeName;
    private string? _typeCode;

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
    /// Gets the raw type code assigned to the symbol, if any.
    /// </summary>
    public string? TypeCode => _typeCode;

    /// <summary>
    /// Gets the resolved type name recorded for the symbol, if known.
    /// </summary>
    public string? ResolvedTypeName => _resolvedTypeName;

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
    /// Records the textual type code assigned to the symbol in the source, normalizing whitespace.
    /// </summary>
    /// <param name="typeCode">The type code extracted from the script.</param>
    public void SetTypeCode(string? typeCode)
    {
        _typeCode = Normalize(typeCode);
    }

    /// <summary>
    /// Records a resolved type name for the symbol, typically filled in by later passes.
    /// </summary>
    /// <param name="typeName">The resolved type name.</param>
    public void SetResolvedTypeName(string? typeName)
    {
        _resolvedTypeName = Normalize(typeName);
    }

    /// <summary>
    /// Trims and normalizes the supplied value so empty strings are treated as <see langword="null"/>.
    /// </summary>
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
