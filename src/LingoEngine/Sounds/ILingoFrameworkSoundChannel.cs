namespace LingoEngine.Sounds
{
    public interface ILingoFrameworkSoundChannel
    {
        int SampleRate { get; }
        int Volume { get; set; }
        int Pan { get; set; }
        float CurrentTime { get; set; }
        bool IsPlaying { get; }
        int ChannelCount { get; }
        int SampleCount { get; }
        float ElapsedTime { get; }
        float StartTime { get; set; }
        float EndTime { get; set; }

        void Pause();
        void PlayFile(string stringFilePath);
        void PlayNow(LingoMemberSound member);
        void Repeat();
        void Rewind();
        void Stop();
    }
}
