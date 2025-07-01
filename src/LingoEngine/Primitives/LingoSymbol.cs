using System;
using System.Collections.Concurrent;

namespace LingoEngine.Primitives
{


    public readonly struct LingoSymbol : IEquatable<LingoSymbol>
    {
        // Base types
        public static LingoSymbol String = new LingoSymbol("string");
        public static LingoSymbol Int = new LingoSymbol("int");
        public static LingoSymbol Float = new LingoSymbol("float");
        public static LingoSymbol Boolean = new LingoSymbol("Boolean");
        // member types
        public static LingoSymbol Text = new LingoSymbol("text");
        public static LingoSymbol Video = new LingoSymbol("video");
        public static LingoSymbol Audio = new LingoSymbol("audio");
        public static LingoSymbol Bitmap = new LingoSymbol("bitmap");


        private static readonly ConcurrentDictionary<string, LingoSymbol> _symbolTable = new();

        public string Name { get; }
        private static LingoSymbol _empty = new LingoSymbol("");
        public static LingoSymbol Empty => _empty;
        public bool IsEmpty => Name =="";

        static LingoSymbol()
        {
            _symbolTable.AddOrUpdate("", _empty, (key, oldValue) => _empty);
        }

        private LingoSymbol(string name)
        {
            Name = name;
        }

        public static LingoSymbol New(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Symbol name cannot be null or whitespace.", nameof(name));

            return _symbolTable.GetOrAdd(name, n => new LingoSymbol(n));
        }

        public static implicit operator LingoSymbol(string name) => New(name);
        public static implicit operator string(LingoSymbol symbol) => symbol.Name;

        public override string ToString() => $"#{Name}";
        public override bool Equals(object? obj) => obj is LingoSymbol s && Equals(s);
        public bool Equals(LingoSymbol other) => string.Equals(Name, other.Name, StringComparison.Ordinal);
        public override int GetHashCode() => Name.GetHashCode();

        public static bool operator ==(LingoSymbol left, LingoSymbol right) => left.Equals(right);
        public static bool operator !=(LingoSymbol left, LingoSymbol right) => !left.Equals(right);
    }

}
