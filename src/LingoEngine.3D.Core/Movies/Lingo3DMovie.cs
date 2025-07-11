using LingoEngine.Casts;
using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Projects;
using LingoEngine.Stages;

namespace LingoEngine.L3D.Core.Movies;

/// <summary>
/// Movie subclass exposing basic 3D related properties.
/// </summary>
public class Lingo3DMovie : LingoMovie
{
    public string Active3dRenderer { get; set; } = string.Empty;
    public string Preferred3dRenderer { get; set; } = string.Empty;

    public Lingo3DMovie(LingoMovieEnvironment environment, LingoStage movieStage,
        LingoCastLibsContainer castLibContainer, ILingoMemberFactory memberFactory,
        string name, int number, LingoEventMediator mediator,
        Action<LingoMovie> onRemoveMe, LingoProjectSettings projectSettings)
        : base(environment, movieStage, castLibContainer, memberFactory, name,
            number, mediator, onRemoveMe, projectSettings)
    {
    }
}
