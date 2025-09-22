using System;

namespace BlingoEngine.Movies;

public readonly struct BlingoMovieRef : IEquatable<BlingoMovieRef>
{
    public BlingoMovieRef(int movieNumber)
    {
        MovieNumber = movieNumber;
    }

    public int MovieNumber { get; }

    public static BlingoMovieRef FromMovie(IBlingoMovie movie)
    {
        if (movie == null)
            throw new ArgumentNullException(nameof(movie));

        return new BlingoMovieRef(movie.Number);
    }

    public bool Equals(BlingoMovieRef other) => MovieNumber == other.MovieNumber;

    public override bool Equals(object? obj) => obj is BlingoMovieRef other && Equals(other);

    public override int GetHashCode() => MovieNumber;

    public override string ToString() => MovieNumber.ToString();
}
