using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngine.Core
{
    public interface ILingoMemberFactory
    {
        T Member<T>(int numberInCast = 0, string name = "") where T : LingoMember;
        LingoMemberPicture Picture(int numberInCast = 0, string name = "");
        LingoMemberSound Sound(int numberInCast = 0, string name = "");
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

        public LingoMemberPicture Picture(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberPicture(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberSound Sound(int numberInCast = 0, string name = "") => _frameworkFactory.CreateMemberSound(_environment.CastLibsContainer.ActiveCast, numberInCast, name);
        public LingoMemberText Text(int numberInCast= 0, string name = "") => _frameworkFactory.CreateMemberText(_environment.CastLibsContainer.ActiveCast, numberInCast,name);
    }
}
