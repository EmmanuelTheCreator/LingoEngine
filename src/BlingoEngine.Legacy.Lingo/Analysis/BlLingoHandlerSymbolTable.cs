using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Maintains the symbols that belong to a single handler (method or event) within a class or script.
/// </summary>
public sealed class BlLingoHandlerSymbolTable
{
    /// <summary>
    /// Name assigned to the implicit handler scope used when locals are declared outside a named handler.
    /// </summary>
    public const string ImplicitHandlerName = "(implicit handler)";

    private readonly Dictionary<string, BlCodeSymbol> _locals = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BlCodeSymbol> _parameters = new(StringComparer.OrdinalIgnoreCase);

    internal BlLingoHandlerSymbolTable(string name, BlSyntaxToken? declarationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Symbol = new BlCodeSymbol(BlCodeSymbolKind.Handler, name);
        if (declarationToken is not null)
        {
            Symbol.AddDeclaration(declarationToken);
        }
    }

    /// <summary>
    /// Gets the symbol representing the handler itself.
    /// </summary>
    public BlCodeSymbol Symbol { get; }

    /// <summary>
    /// Gets the table of parameters that belong to the handler.
    /// </summary>
    public IReadOnlyDictionary<string, BlCodeSymbol> Parameters => _parameters;

    /// <summary>
    /// Gets the table of local variables declared within the handler.
    /// </summary>
    public IReadOnlyDictionary<string, BlCodeSymbol> Locals => _locals;

    /// <summary>
    /// Declares a parameter inside the handler scope.
    /// </summary>
    /// <param name="identifier">The token that introduced the parameter.</param>
    internal BlCodeSymbol DeclareParameter(BlSyntaxToken identifier) => DeclareSymbol(_parameters, identifier, BlCodeSymbolKind.Parameter);

    /// <summary>
    /// Declares a local variable within the handler scope.
    /// </summary>
    /// <param name="identifier">The token that introduced the local variable.</param>
    internal BlCodeSymbol DeclareLocal(BlSyntaxToken identifier) => DeclareSymbol(_locals, identifier, BlCodeSymbolKind.LocalVariable);

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
