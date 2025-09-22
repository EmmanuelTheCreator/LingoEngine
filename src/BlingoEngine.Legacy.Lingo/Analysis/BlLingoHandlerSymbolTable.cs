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

    private readonly BlLingoHandlerClassification _classification;
    private readonly Dictionary<string, BlCodeSymbol> _locals = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BlCodeSymbol> _parameters = new(StringComparer.OrdinalIgnoreCase);

    internal BlLingoHandlerSymbolTable(string originalName, BlLingoHandlerClassification classification, BlSyntaxToken? declarationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalName);

        _classification = classification;
        OriginalName = originalName;
        Symbol = new BlCodeSymbol(BlCodeSymbolKind.Handler, classification.CanonicalName);
        if (declarationToken is not null)
        {
            Symbol.AddDeclaration(declarationToken);
        }

        HandlerKind = classification.Kind;
        ImpliedScriptKind = classification.RequiresLeadingMeParameter ? BlLingoScriptKind.Unknown : classification.ImpliedScriptKind;
    }

    /// <summary>
    /// Gets the symbol representing the handler itself.
    /// </summary>
    public BlCodeSymbol Symbol { get; }

    /// <summary>
    /// Gets the raw name used to declare the handler in the script.
    /// </summary>
    public string OriginalName { get; }

    /// <summary>
    /// Gets the high-level category assigned to the handler.
    /// </summary>
    public BlLingoHandlerKind HandlerKind { get; }

    /// <summary>
    /// Gets the script kind implied by this handler, if any.
    /// </summary>
    public BlLingoScriptKind ImpliedScriptKind { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the handler declares <c>me</c> as its first parameter.
    /// </summary>
    public bool HasLeadingMeParameter { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the handler requires <c>me</c> as the first parameter to imply its script kind.
    /// </summary>
    public bool RequiresLeadingMeParameter => _classification.RequiresLeadingMeParameter;

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

    /// <summary>
    /// Records whether the handler declared <c>me</c> as its leading parameter and updates the implied script kind accordingly.
    /// </summary>
    /// <param name="hasMe">Indicates whether the first parameter token was <c>me</c>.</param>
    internal void SetLeadingParameterInfo(bool hasMe)
    {
        HasLeadingMeParameter = hasMe;
        if (_classification.RequiresLeadingMeParameter)
        {
            ImpliedScriptKind = hasMe ? _classification.ImpliedScriptKind : BlLingoScriptKind.Unknown;
        }
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
