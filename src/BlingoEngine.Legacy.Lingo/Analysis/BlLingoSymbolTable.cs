using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Provides hierarchical symbol tracking for a Lingo script, capturing project, class, and handler scopes.
/// </summary>
public sealed class BlLingoSymbolTable
{
    private readonly Dictionary<string, BlCodeSymbol> _globalVariables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BlCodeSymbol> _classSymbols = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, BlLingoClassSymbolTable> _classes = new(StringComparer.OrdinalIgnoreCase);
    private readonly BlLingoClassSymbolTable _movieScript;
    private BlLingoClassSymbolTable _currentClass;
    private BlLingoHandlerSymbolTable? _currentHandler;

    /// <summary>
    /// Initializes a new <see cref="BlLingoSymbolTable"/> instance with a default movie script scope.
    /// </summary>
    public BlLingoSymbolTable()
    {
        _movieScript = new BlLingoClassSymbolTable(BlLingoClassSymbolTable.MovieScriptName, null, isMovieScript: true);
        _currentClass = _movieScript;
    }

    /// <summary>
    /// Gets the collection of global variables declared in the script.
    /// </summary>
    public IReadOnlyDictionary<string, BlCodeSymbol> Globals => _globalVariables;

    /// <summary>
    /// Gets the collection of explicit class symbols declared in the script.
    /// </summary>
    public IReadOnlyDictionary<string, BlCodeSymbol> Classes => _classSymbols;

    /// <summary>
    /// Gets the implicit movie script class scope used when no explicit class is active.
    /// </summary>
    public BlLingoClassSymbolTable MovieScript => _movieScript;

    /// <summary>
    /// Gets all explicit class scopes that were discovered.
    /// </summary>
    public IReadOnlyDictionary<string, BlLingoClassSymbolTable> ClassScopes => _classes;

    /// <summary>
    /// Gets the class scope currently receiving new declarations.
    /// </summary>
    public BlLingoClassSymbolTable CurrentClass => _currentClass;

    /// <summary>
    /// Gets the handler scope currently receiving new declarations, if any.
    /// </summary>
    public BlLingoHandlerSymbolTable? CurrentHandler => _currentHandler;

    /// <summary>
    /// Declares a global variable.
    /// </summary>
    /// <param name="identifier">The token that identified the global variable.</param>
    public BlCodeSymbol DeclareGlobal(BlSyntaxToken identifier) => DeclareSymbol(_globalVariables, identifier, BlCodeSymbolKind.GlobalVariable);

    /// <summary>
    /// Declares a property within the current class scope.
    /// </summary>
    /// <param name="identifier">The token that introduced the property.</param>
    public BlCodeSymbol DeclareProperty(BlSyntaxToken identifier) => _currentClass.DeclareProperty(identifier);

    /// <summary>
    /// Declares a parameter within the active handler scope, creating an implicit handler if required.
    /// </summary>
    /// <param name="identifier">The token that introduced the parameter.</param>
    public BlCodeSymbol DeclareParameter(BlSyntaxToken identifier) => EnsureCurrentHandler().DeclareParameter(identifier);

    /// <summary>
    /// Declares a local variable within the active handler scope, creating an implicit handler if required.
    /// </summary>
    /// <param name="identifier">The token that introduced the local variable.</param>
    public BlCodeSymbol DeclareLocal(BlSyntaxToken identifier) => EnsureCurrentHandler().DeclareLocal(identifier);

    /// <summary>
    /// Declares or updates a class symbol and switches the current scope to the class.
    /// </summary>
    /// <param name="name">The name of the class declared in the script.</param>
    /// <param name="token">The token responsible for the declaration, if available.</param>
    public BlCodeSymbol DeclareClass(string name, BlSyntaxToken? token = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!_classes.TryGetValue(name, out var classScope))
        {
            classScope = new BlLingoClassSymbolTable(name, token, isMovieScript: false);
            _classes.Add(name, classScope);
            _classSymbols[name] = classScope.Symbol;
        }
        else if (token is not null)
        {
            classScope.Symbol.AddDeclaration(token);
        }

        _currentClass = classScope;
        _currentHandler = null;
        return classScope.Symbol;
    }

    /// <summary>
    /// Begins a handler scope within the current class.
    /// </summary>
    /// <param name="handlerToken">The token that introduced the handler.</param>
    public BlLingoHandlerSymbolTable BeginHandler(BlSyntaxToken handlerToken)
    {
        ArgumentNullException.ThrowIfNull(handlerToken);
        var handlerName = handlerToken.ValueText;
        ArgumentException.ThrowIfNullOrWhiteSpace(handlerName);

        var handler = _currentClass.GetOrAddHandler(handlerName, handlerToken);
        _currentHandler = handler;
        return handler;
    }

    /// <summary>
    /// Closes the current handler scope so subsequent declarations do not leak into it.
    /// </summary>
    public void EndHandler()
    {
        _currentHandler = null;
    }

    /// <summary>
    /// Switches back to the movie script scope and clears the active handler.
    /// </summary>
    public void ResetToMovieScript()
    {
        _currentClass = _movieScript;
        _currentHandler = null;
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

    private BlLingoHandlerSymbolTable EnsureCurrentHandler()
    {
        if (_currentHandler is null)
        {
            _currentHandler = _currentClass.GetOrAddHandler(BlLingoHandlerSymbolTable.ImplicitHandlerName, declarationToken: null);
        }

        return _currentHandler;
    }
}
