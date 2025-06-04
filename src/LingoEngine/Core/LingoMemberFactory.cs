using LingoEngine.Core;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Movies;
using LingoEngine.Pictures.LingoEngine;
using LingoEngine.Sounds;
using LingoEngine.Texts;

namespace LingoEngineGodot
{
    public interface ILingoMemberFactory
    {
        T Member<T>(string name = "") where T : LingoMember;
        LingoMemberPicture Picture(string name = "");
        LingoMemberSound Sound(string name = "");
        LingoMemberText Text(string name = "");
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
        public T Member<T>(string name = "") where T : LingoMember => _frameworkFactory.CreateMember<T>(_environment.CastLibsContainer.ActiveCast, name);

        public LingoMemberPicture Picture(string name = "") => _frameworkFactory.CreateMemberPicture(_environment.CastLibsContainer.ActiveCast, name);
        public LingoMemberSound Sound(string name = "") => _frameworkFactory.CreateMemberSound(_environment.CastLibsContainer.ActiveCast, name);
        public LingoMemberText Text(string name = "") => _frameworkFactory.CreateMemberText(_environment.CastLibsContainer.ActiveCast, name);
    }
}
