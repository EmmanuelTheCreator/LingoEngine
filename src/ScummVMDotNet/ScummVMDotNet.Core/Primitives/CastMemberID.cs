namespace Director.Primitives
{
    public readonly struct CastMemberID
    {
        public int Id { get; }
        public int Lib { get; }

        public CastMemberID(int id, int lib)
        {
            Id = id;
            Lib = lib;
        }

        public bool IsValid => Id >= 0;

        public override string ToString() => $"ID: {Id}, Lib: {Lib}";

        public static bool operator ==(CastMemberID left, CastMemberID right) => left.Id == right.Id && left.Lib == right.Lib;
        public static bool operator !=(CastMemberID left, CastMemberID right) => !(left == right);

        public override bool Equals(object? obj) => obj is CastMemberID other && this == other;
        public override int GetHashCode() => HashCode.Combine(Id, Lib);
    }
}


