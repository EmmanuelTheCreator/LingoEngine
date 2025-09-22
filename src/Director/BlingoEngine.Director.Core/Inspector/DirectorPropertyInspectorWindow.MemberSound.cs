using System;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using BlingoEngine.Members;
using BlingoEngine.Sounds;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private void AddSoundTab(BlingoMemberSound member)
    {
        var soundChannel = _player.Sound.Channel(1);
        if (soundChannel == null)
            return;

        var wrap = AddTab(PropetyTabNames.Sound);
        var soundAdapter = new SoundMemberCommandAdapter(this, member);
        var btnPanel = _factory.CreateWrapPanel(AOrientation.Horizontal, "SoundButtons");
        var playBtn = _factory.CreateButton("SoundPlay", "Play");
        var stopBtn = _factory.CreateButton("SoundStop", "Stop");
        playBtn.Pressed += () => soundChannel.Play(member);
        stopBtn.Pressed += () => soundChannel.Stop();
        btnPanel.AddItem(playBtn);
        btnPanel.AddItem(stopBtn);

        var panel = _factory.CreatePanel("SoundPanel");
        wrap.AddItem(btnPanel);
        wrap.AddItem(panel);

        string duration = TimeSpan.FromSeconds(member.Length).ToString(@"hh\:mm\:ss\.fff");
        panel.Compose(_factory.ComponentFactory)
            .Columns(4)
            .AddCheckBox("SoundLoop", "Loop:", soundAdapter, m => m.Loop, 1, true, 3)
            .AddLabel("SoundDuration", "Duration: ", 2)
            .AddLabel("SoundDurationV", duration, 2)
            .AddLabel("SoundSampleRate", "Sample rate: ", 2)
            .AddLabel("SoundSampleRateV", soundChannel.SampleRate + " Hz", 2)
            .AddLabel("SoundBitDepth", "Bit Depth: ", 2)
            .AddLabel("SoundBitDepthV", "16", 2)
            .AddLabel("SoundChannels", "Channels: ", 2)
            .AddLabel("SoundChannelsV", member.Stereo ? "Stereo" : "Mono", 2)
            .Finalize();
    }

    private sealed class SoundMemberCommandAdapter : MemberCommandAdapterBase<BlingoMemberSound>
    {
        public SoundMemberCommandAdapter(DirectorPropertyInspectorWindow window, BlingoMemberSound member)
            : base(window, member)
        {
        }

        public bool Loop
        {
            get => Member.Loop;
            set => DispatchIfChanged(nameof(BlingoMemberSound.Loop), Member.Loop, value);
        }
    }
}
