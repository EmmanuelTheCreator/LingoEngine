using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Shapes;
using LingoEngine.Texts;

namespace LingoEngine.Members
{
    public interface ILingoMemberFactory
    {
        T Member<T>(int numberInCast = 0, string name = "") where T : LingoMember;
        LingoMemberBitmap Picture(int numberInCast = 0, string name = "");
        LingoMemberSound Sound(int numberInCast = 0, string name = "");
        LingoMemberFilmLoop FilmLoop(int numberInCast = 0, string name = "");
        LingoMemberShape Shape(int numberInCast = 0, string name = "");
        LingoMemberText Text(int numberInCast = 0, string name = "");
    }

    internal class LingoMemberFactory : ILingoMemberFactory
    {
        private readonly ILingoFrameworkFactory _frameworkFactory;
        private readonly ILingoMovieEnvironment _environment;

        public LingoMemberFactory(ILingoFrameworkFactory frameworkFactory, ILingoMovieEnvironment environment)
        {
            _frameworkFactory = frameworkFactory;
            _environment = environment;
        }
        public T Member<T>(int numberInCast = 0, string name = "") where T : LingoMember => _frameworkFactory.CreateMember<T>(_environment.CastLibsContainer.ActiveCast, numberInCast, name);

        public LingoMemberBitmap Picture(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberPicture(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberSound Sound(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberSound(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberFilmLoop FilmLoop(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberFilmLoop(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberShape Shape(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberShape(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberText Text(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberText(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
    }
}
