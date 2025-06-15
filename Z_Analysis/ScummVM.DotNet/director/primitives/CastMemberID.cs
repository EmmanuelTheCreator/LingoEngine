using Director;
﻿namespace Director.Primitives
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

        /// <summary>
        /// Returns a string representation similar to the original Director
        /// CastMemberID::asString implementation.
        /// </summary>
        public string AsString(DirectorEngine engine, Movie movie)
        {
            string res = $"member {Id}";
            if (engine.Version < 400 || movie.AllowOutdatedLingo)
                res += "(" + Util.NumToCastNum(Id) + ")";
            else if (engine.Version >= 500)
                res += $" of castLib {Lib}";
            return res;
        }

        public static bool operator ==(CastMemberID left, CastMemberID right) => left.Id == right.Id && left.Lib == right.Lib;
        public static bool operator !=(CastMemberID left, CastMemberID right) => !(left == right);

        public override bool Equals(object? obj) => obj is CastMemberID other && this == other;
        public override int GetHashCode() => HashCode.Combine(Id, Lib);
    }
}


