using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal sealed class BlLegacyTypeCollection
{
    private readonly Dictionary<BlLingoClassSymbolTable, Scope> _scopes = new();
    private readonly Dictionary<string, Scope> _scopesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<HandlerScope>> _handlersByScript = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<HandlerScope>> _handlersByName = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Dictionary<int, string>> s_sharedByScript = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Dictionary<int, string>> s_sharedByName = new(StringComparer.Ordinal);
    private readonly Dictionary<BlLingoHandlerSymbolTable, HandlerScope> _handlersBySymbol = new();

    public IEnumerable<Scope> Scopes => _scopes.Values;

    public Scope GetOrAddScope(BlLingoClassSymbolTable scope)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (!_scopes.TryGetValue(scope, out var result))
        {
            result = new Scope(scope);
            _scopes.Add(scope, result);
            _scopesByName[result.Name] = result;
        }

        return result;
    }

    public bool TryFindScopeByName(string? scriptName, out Scope? scope)
    {
        if (!string.IsNullOrWhiteSpace(scriptName) && _scopesByName.TryGetValue(scriptName, out var located))
        {
            scope = located;
            return true;
        }

        scope = null;
        return false;
    }

    public HandlerScope? FindHandler(string? scriptName, string handlerName, BlLingoScriptKind expectedKind)
    {
        if (string.IsNullOrWhiteSpace(handlerName))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(scriptName))
        {
            var scriptKey = ComposeScriptKey(scriptName, handlerName);
            if (_handlersByScript.TryGetValue(scriptKey, out var scriptHandlers) && scriptHandlers.Count > 0)
            {
                return scriptHandlers[0];
            }

            if (expectedKind == BlLingoScriptKind.Movie)
            {
                var movieKey = ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, handlerName);
                if (_handlersByScript.TryGetValue(movieKey, out var movieHandlers) && movieHandlers.Count > 0)
                {
                    return movieHandlers[0];
                }
            }

            if (TryFindScopeByName(scriptName, out var namedScope) && namedScope is not null &&
                namedScope.TryGetHandler(handlerName, out var directHandler))
            {
                return directHandler;
            }
        }

        if (expectedKind == BlLingoScriptKind.Movie)
        {
            var movieKey = ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, handlerName);
            if (_handlersByScript.TryGetValue(movieKey, out var movieHandlers) && movieHandlers.Count > 0)
            {
                return movieHandlers[0];
            }

            foreach (var scope in _scopes.Values)
            {
                if (!scope.IsMovieScript && scope.ScriptKind != BlLingoScriptKind.Movie)
                {
                    continue;
                }

                if (scope.TryGetHandler(handlerName, out var handler))
                {
                    return handler;
                }
            }
        }
        else if (expectedKind == BlLingoScriptKind.Behavior)
        {
            foreach (var scope in _scopes.Values)
            {
                if (scope.IsMovieScript)
                {
                    continue;
                }

                if (scope.TryGetHandler(handlerName, out var handler))
                {
                    return handler;
                }
            }
        }

        var handlerKey = ComposeHandlerKey(handlerName);
        if (_handlersByName.TryGetValue(handlerKey, out var globalHandlers) && globalHandlers.Count > 0)
        {
            return globalHandlers[0];
        }

        foreach (var scope in _scopes.Values)
        {
            if (scope.TryGetHandler(handlerName, out var handler))
            {
                return handler;
            }
        }

        return null;
    }

    public bool TryGetHandler(BlLingoHandlerSymbolTable handler, out HandlerScope? scope)
    {
        return _handlersBySymbol.TryGetValue(handler, out scope);
    }

    public void RegisterHandler(HandlerScope handlerScope)
    {
        ArgumentNullException.ThrowIfNull(handlerScope);

        if (handlerScope.Symbol is not null)
        {
            _handlersBySymbol[handlerScope.Symbol] = handlerScope;
        }
        var handlerKey = ComposeHandlerKey(handlerScope.LookupName);
        AddHandlerMapping(_handlersByName, handlerKey, handlerScope);

        var scriptName = handlerScope.Owner.Name;
        if (!string.IsNullOrEmpty(scriptName))
        {
            var scriptKey = ComposeScriptKey(scriptName, handlerScope.LookupName);
            AddHandlerMapping(_handlersByScript, scriptKey, handlerScope);
        }

        if (handlerScope.Owner.ScriptKind == BlLingoScriptKind.Movie)
        {
            var movieKey = ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, handlerScope.LookupName);
            AddHandlerMapping(_handlersByScript, movieKey, handlerScope);
        }

        ApplySharedHintsForHandler(handlerScope, handlerKey);
        if (!string.IsNullOrEmpty(scriptName))
        {
            var scriptKey = ComposeScriptKey(scriptName, handlerScope.LookupName);
            ApplySharedHintsForScript(handlerScope, scriptKey);
        }

        if (handlerScope.Owner.ScriptKind == BlLingoScriptKind.Movie)
        {
            var movieKey = ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, handlerScope.LookupName);
            ApplySharedHintsForScript(handlerScope, movieKey);
        }
    }

    public void AddSharedHint(BlLingoScriptKind scriptKind, string? scriptName, string handlerName, int parameterIndex, string typeName)
    {
        if (parameterIndex < 0 || string.IsNullOrWhiteSpace(handlerName))
        {
            return;
        }

        var normalized = BlLegacyTypeUtilities.NormalizeTypeName(typeName);
        if (string.IsNullOrEmpty(normalized))
        {
            return;
        }

        var handlerKey = ComposeHandlerKey(handlerName);
        RegisterSharedHint(s_sharedByName, handlerKey, parameterIndex, normalized);

        if (!string.IsNullOrWhiteSpace(scriptName))
        {
            var scriptKey = ComposeScriptKey(scriptName!, handlerName);
            RegisterSharedHint(s_sharedByScript, scriptKey, parameterIndex, normalized);
        }

        if (scriptKind == BlLingoScriptKind.Movie)
        {
            var movieKey = ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, handlerName);
            RegisterSharedHint(s_sharedByScript, movieKey, parameterIndex, normalized);
        }
    }

    public void ApplyResolvedTypes()
    {
        foreach (var scope in _scopes.Values)
        {
            scope.Apply();
        }
    }

    private static void AddHandlerMapping(Dictionary<string, List<HandlerScope>> table, string key, HandlerScope handler)
    {
        if (!table.TryGetValue(key, out var list))
        {
            list = new List<HandlerScope>();
            table[key] = list;
        }

        if (!list.Contains(handler))
        {
            list.Add(handler);
        }
    }

    private void ApplySharedHintsForScript(HandlerScope handlerScope, string key)
    {
        if (s_sharedByScript.TryGetValue(key, out var hints))
        {
            foreach (var pair in hints)
            {
                handlerScope.AddArgumentHint(pair.Key, pair.Value);
            }
        }
    }

    private void ApplySharedHintsForHandler(HandlerScope handlerScope, string handlerKey)
    {
        if (s_sharedByName.TryGetValue(handlerKey, out var hints))
        {
            foreach (var pair in hints)
            {
                handlerScope.AddArgumentHint(pair.Key, pair.Value);
            }
        }
    }

    private static void RegisterSharedHint(Dictionary<string, Dictionary<int, string>> table, string key, int index, string typeName)
    {
        if (!table.TryGetValue(key, out var map))
        {
            map = new Dictionary<int, string>();
            table[key] = map;
        }

        if (map.TryGetValue(index, out var existing))
        {
            map[index] = BlLegacyTypeUtilities.MergeTypeNames(existing, typeName);
        }
        else
        {
            map[index] = typeName;
        }
    }

    private static string ComposeScriptKey(string scriptName, string handlerName)
    {
        return $"{scriptName.ToUpperInvariant()}|{handlerName.ToUpperInvariant()}";
    }

    private static string ComposeHandlerKey(string handlerName)
    {
        return handlerName.ToUpperInvariant();
    }

    internal sealed class Scope
    {
        private readonly Dictionary<string, TypeTarget> _properties = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, HandlerScope> _handlers = new(StringComparer.OrdinalIgnoreCase);

        public Scope(BlLingoClassSymbolTable symbol)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Name = symbol.Symbol?.Name ?? string.Empty;
            ScriptKind = symbol.ScriptKind;
            IsMovieScript = symbol.IsMovieScript;
        }

        public string Name { get; }

        public BlLingoClassSymbolTable Symbol { get; }

        public BlLingoScriptKind ScriptKind { get; }

        public bool IsMovieScript { get; }

        public IEnumerable<HandlerScope> Handlers => _handlers.Values;

        public TypeTarget RegisterProperty(BlCodeSymbol symbol)
        {
            ArgumentNullException.ThrowIfNull(symbol);

            if (!_properties.TryGetValue(symbol.Name, out var target))
            {
                target = new TypeTarget(symbol.Name, BlLegacyTypeTargetKind.Property, symbol, isSelfParameter: false);
                _properties[symbol.Name] = target;
            }

            return target;
        }

        public bool TryGetProperty(string name, out TypeTarget target)
        {
            return _properties.TryGetValue(name, out target!);
        }

        public HandlerScope RegisterHandler(BlLingoHandlerSymbolTable handler)
        {
            ArgumentNullException.ThrowIfNull(handler);

            if (!_handlers.TryGetValue(handler.OriginalName, out var scope))
            {
                scope = new HandlerScope(this, handler);
                foreach (var parameter in BlLegacyTypeUtilities.GetOrderedParameters(handler))
                {
                    var isSelf = string.Equals(parameter.Name, "me", StringComparison.OrdinalIgnoreCase);
                    scope.AddParameter(parameter, isSelf);
                }

                _handlers[handler.OriginalName] = scope;
            }

            return scope;
        }

        public bool TryGetHandler(string handlerName, out HandlerScope handler)
        {
            return _handlers.TryGetValue(handlerName, out handler!);
        }

        public void Apply()
        {
            foreach (var property in _properties.Values)
            {
                property.Apply();
            }

            foreach (var handler in _handlers.Values)
            {
                handler.Apply();
            }
        }
    }

    internal sealed class HandlerScope
    {
        private readonly Dictionary<string, TypeTarget> _parametersByName = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<TypeTarget> _orderedParameters = new();

        public HandlerScope(Scope owner, BlLingoHandlerSymbolTable? symbol)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Symbol = symbol;
            LookupName = symbol?.OriginalName ?? symbol?.Symbol?.Name ?? string.Empty;
            CanonicalName = symbol?.Symbol?.Name ?? string.Empty;
        }

        public Scope Owner { get; }

        public BlLingoHandlerSymbolTable? Symbol { get; }

        public string LookupName { get; }

        public string CanonicalName { get; }

        public IReadOnlyList<TypeTarget> Parameters => _orderedParameters;

        public void AddParameter(BlCodeSymbol symbol, bool isSelfParameter)
        {
            ArgumentNullException.ThrowIfNull(symbol);

            if (_parametersByName.ContainsKey(symbol.Name))
            {
                return;
            }

            var target = new TypeTarget(symbol.Name, BlLegacyTypeTargetKind.Parameter, symbol, isSelfParameter);
            _parametersByName[symbol.Name] = target;
            _orderedParameters.Add(target);
        }

        public bool TryGetParameter(string name, out TypeTarget target)
        {
            return _parametersByName.TryGetValue(name, out target!);
        }

        public bool TryGetFirstNonSelfParameter(out TypeTarget target)
        {
            foreach (var parameter in _orderedParameters)
            {
                if (parameter.IsSelfParameter)
                {
                    continue;
                }

                target = parameter;
                return true;
            }

            target = null!;
            return false;
        }

        public void AddArgumentHint(int argumentIndex, string typeName)
        {
            var target = GetParameterByArgumentIndex(argumentIndex);
            target?.AddHint(typeName);
        }

        public int GetArgumentIndex(TypeTarget target)
        {
            if (target is null)
            {
                return -1;
            }

            var index = 0;
            foreach (var parameter in _orderedParameters)
            {
                if (parameter.IsSelfParameter)
                {
                    continue;
                }

                if (ReferenceEquals(parameter, target))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void Apply()
        {
            foreach (var parameter in _orderedParameters)
            {
                parameter.Apply();
            }
        }

        private TypeTarget? GetParameterByArgumentIndex(int argumentIndex)
        {
            if (argumentIndex < 0)
            {
                return null;
            }

            var index = 0;
            foreach (var parameter in _orderedParameters)
            {
                if (parameter.IsSelfParameter)
                {
                    continue;
                }

                if (index == argumentIndex)
                {
                    return parameter;
                }

                index++;
            }

            return null;
        }
    }

    internal sealed class TypeTarget
    {
        private string? _mergedType;
        private readonly List<TypeTarget> _linkedTargets = new();

        public TypeTarget(string name, BlLegacyTypeTargetKind kind, BlCodeSymbol? symbol, bool isSelfParameter)
        {
            Name = name ?? string.Empty;
            Kind = kind;
            Symbol = symbol;
            IsSelfParameter = isSelfParameter;
        }

        public string Name { get; }

        public BlLegacyTypeTargetKind Kind { get; }

        public BlCodeSymbol? Symbol { get; }

        public bool IsSelfParameter { get; }

        public void AddHint(string typeName)
        {
            var normalized = BlLegacyTypeUtilities.NormalizeTypeName(typeName);
            if (string.IsNullOrEmpty(normalized))
            {
                return;
            }

            _mergedType = string.IsNullOrEmpty(_mergedType)
                ? normalized
                : BlLegacyTypeUtilities.MergeTypeNames(_mergedType!, normalized);
        }

        public void LinkTo(TypeTarget? target)
        {
            if (target is null || ReferenceEquals(this, target))
            {
                return;
            }

            if (!_linkedTargets.Contains(target))
            {
                _linkedTargets.Add(target);
            }
        }

        public void Apply()
        {
            var normalized = BlLegacyTypeUtilities.NormalizeTypeName(_mergedType);
            if (!string.IsNullOrEmpty(normalized) && Symbol is not null)
            {
                Symbol.SetResolvedTypeName(normalized);
                foreach (var linked in _linkedTargets)
                {
                    linked.AddHint(normalized);
                }
            }
        }
    }
}

internal enum BlLegacyTypeTargetKind
{
    Property,
    Parameter,
}

