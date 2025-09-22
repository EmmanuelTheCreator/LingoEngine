using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Captures the symbols declared within a single class or script, including handlers and properties.
/// </summary>
public sealed class BlLingoClassSymbolTable
{
    /// <summary>
    /// The name assigned to the implicit movie script scope when no explicit class is active.
    /// </summary>
    public const string MovieScriptName = "(movie script)";

    private readonly Dictionary<string, BlCodeSymbol> _properties = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BlLingoHandlerSymbolTable> _handlers = new(StringComparer.OrdinalIgnoreCase);

    internal BlLingoClassSymbolTable(string name, BlSyntaxToken? declarationToken, bool isMovieScript)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Symbol = new BlCodeSymbol(BlCodeSymbolKind.Class, name);
        if (declarationToken is not null)
        {
            Symbol.AddDeclaration(declarationToken);
        }

        IsMovieScript = isMovieScript;
    }

    /// <summary>
    /// Gets the symbol describing the class itself.
    /// </summary>
    public BlCodeSymbol Symbol { get; }

    /// <summary>
    /// Gets a value indicating whether this scope represents the implicit movie script rather than an explicit class.
    /// </summary>
    public bool IsMovieScript { get; }

    /// <summary>
    /// Gets the properties declared within this class or script.
    /// </summary>
    public IReadOnlyDictionary<string, BlCodeSymbol> Properties => _properties;

    /// <summary>
    /// Gets the handler tables declared for this class.
    /// </summary>
    public IReadOnlyDictionary<string, BlLingoHandlerSymbolTable> Handlers => _handlers;

    /// <summary>
    /// Declares a property within this class scope.
    /// </summary>
    /// <param name="identifier">The token that declared the property.</param>
    internal BlCodeSymbol DeclareProperty(BlSyntaxToken identifier) => DeclareSymbol(_properties, identifier, BlCodeSymbolKind.Property);

    /// <summary>
    /// Gets an existing handler scope or creates a new one if required.
    /// </summary>
    /// <param name="name">The handler name extracted from the source.</param>
    /// <param name="declarationToken">The token that introduced the handler.</param>
    internal BlLingoHandlerSymbolTable GetOrAddHandler(string name, BlSyntaxToken? declarationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_handlers.TryGetValue(name, out var handler))
        {
            handler = new BlLingoHandlerSymbolTable(name, declarationToken);
            _handlers.Add(name, handler);
        }
        else if (declarationToken is not null)
        {
            handler.Symbol.AddDeclaration(declarationToken);
        }

        return handler;
    }

    private static BlCodeSymbol DeclareSymbol(Dictionary<string, BlCodeSymbol> table, BlSyntaxToken token, BlCodeSymbolKind kind)
    {
        ArgumentNullException.ThrowIfNull(token);
        var name = token.ValueText;
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!table.TryGetValue(name, out var symbol))
        {
            symbol = new BlCodeSymbol(kind, name);
            table.Add(name, symbol);
        }

        symbol.AddDeclaration(token);
        return symbol;
    }
}
