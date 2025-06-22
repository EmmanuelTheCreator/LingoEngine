using LingoEngine.Members;

namespace LingoEngine.Movies;

public class LingoAudioClip
{
    public int Channel { get; set; }
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }
    public LingoMemberSound Sound { get; }

    public LingoAudioClip(int channel, int beginFrame, int endFrame, LingoMemberSound sound)
    {
        Channel = channel;
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        Sound = sound;
    }
}
