using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Sounds
{
    public interface ILingoFrameworkMemberSound : ILingoFrameworkMember
    {
        bool Stereo { get; }
        double Length { get; }
    }
}
