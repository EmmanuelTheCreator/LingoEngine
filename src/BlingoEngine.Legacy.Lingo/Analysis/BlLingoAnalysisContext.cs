using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Shared context that analysis passes use to exchange tokens, symbols, and derived data.
/// </summary>
public sealed class BlLingoAnalysisContext
{
    private readonly Dictionary<Type, Dictionary<string, object>> _sharedData = new();

    /// <summary>
    /// Initializes a new <see cref="BlLingoAnalysisContext"/> instance.
    /// </summary>
    /// <param name="tokens">The token stream produced by the tokenizer.</param>
    /// <param name="symbols">The symbol table shared across passes.</param>
    public BlLingoAnalysisContext(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols)
    {
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
    }

    /// <summary>
    /// Gets the tokens being analyzed.
    /// </summary>
    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    /// <summary>
    /// Gets the symbol table being populated by passes.
    /// </summary>
    public BlLingoSymbolTable Symbols { get; }

    /// <summary>
    /// Stores a typed value that other passes can consume.
    /// </summary>
    /// <typeparam name="T">The value type being stored.</typeparam>
    /// <param name="key">The logical name of the value.</param>
    /// <param name="value">The value to store.</param>
    public void SetData<T>(string key, T value)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var bucket = GetOrCreateBucket(typeof(T));
        bucket[key] = value;
    }

    /// <summary>
    /// Attempts to retrieve a previously stored value.
    /// </summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="key">The logical name of the value.</param>
    /// <param name="value">The retrieved value, if found.</param>
    /// <returns><see langword="true"/> when a matching entry was retrieved; otherwise <see langword="false"/>.</returns>
    public bool TryGetData<T>(string key, out T? value)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (_sharedData.TryGetValue(typeof(T), out var bucket) && bucket.TryGetValue(key, out var stored))
        {
            value = (T)stored;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Produces a flattened snapshot of all stored data for reporting purposes.
    /// </summary>
    internal IReadOnlyDictionary<string, object?> GetDataSnapshot()
    {
        var snapshot = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var bucket in _sharedData.Values)
        {
            foreach (var pair in bucket)
            {
                snapshot[pair.Key] = pair.Value;
            }
        }

        return snapshot;
    }

    private Dictionary<string, object> GetOrCreateBucket(Type type)
    {
        if (!_sharedData.TryGetValue(type, out var bucket))
        {
            bucket = new Dictionary<string, object>(StringComparer.Ordinal);
            _sharedData[type] = bucket;
        }

        return bucket;
    }
}
