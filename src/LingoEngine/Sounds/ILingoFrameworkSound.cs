namespace LingoEngine.Sounds
{
    public interface ILingoFrameworkSound
    {
        List<LingoSoundDevice> SoundDeviceList { get; }
        int SoundLevel { get; set; }
        bool SoundEnable { get; set; }
        void Beep();
    }
}
