using AbstUI.Commands;
using BlingoEngine.Movies;

namespace BlingoEngine.Director.Core.Movies.Commands;

public abstract record BlingoMovieBaseCommand(BlingoMovieRef MovieReference) : IAbstCommand;
