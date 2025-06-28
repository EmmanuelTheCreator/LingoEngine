using LingoEngine.Members;

namespace LingoEngine.Sounds;

public class LingoMovieAudioClip
{
    public int Channel { get; set; }
    public int BeginFrame { get; set; }
    public int EndFrame { get; set; }
    public LingoMemberSound Sound { get; }

    public LingoMovieAudioClip(int channel, int beginFrame, int endFrame, LingoMemberSound sound)
    {
        Channel = channel;
        BeginFrame = beginFrame;
        EndFrame = endFrame;
        Sound = sound;
    }
}
