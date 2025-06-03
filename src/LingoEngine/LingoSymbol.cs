using System;
using System.Collections.Concurrent;

namespace LingoEngine
{


    public readonly struct LingoSymbol : IEquatable<LingoSymbol>
    {
        private static readonly ConcurrentDictionary<string, LingoSymbol> _symbolTable = new();

        public string Name { get; }

        private LingoSymbol(string name)
        {
            Name = name;
        }

        public static LingoSymbol Intern(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Symbol name cannot be null or whitespace.", nameof(name));

            return _symbolTable.GetOrAdd(name, n => new LingoSymbol(n));
        }

        public static implicit operator LingoSymbol(string name) => Intern(name);
        public static implicit operator string(LingoSymbol symbol) => symbol.Name;

        public override string ToString() => $"#{Name}";
        public override bool Equals(object? obj) => obj is LingoSymbol s && Equals(s);
        public bool Equals(LingoSymbol other) => string.Equals(Name, other.Name, StringComparison.Ordinal);
        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(LingoSymbol left, LingoSymbol right) => left.Equals(right);
        public static bool operator !=(LingoSymbol left, LingoSymbol right) => !left.Equals(right);
    }

}
