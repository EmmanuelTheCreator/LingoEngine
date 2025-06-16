using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;

namespace LingoEngine.Pictures
{
    public class LingoMemberFilmLoop : LingoMember
    {
        public LingoMemberFilmLoop(ILingoFrameworkMember frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default) : base(frameworkMember, LingoMemberType.FilmLoop, cast, numberInCast, name, fileName, regPoint)
        {
        }
    }
}
