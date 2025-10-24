using System;
using System.Collections.Generic;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using BlingoEngine.Medias;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private static readonly KeyValuePair<string, string>[] MediaPlaybackRateOptions =
    {
        new("Sync to Sound", "Sync"),
        new("Every Frame", "EveryFrame"),
        new("Maximum Speed", "Maximum"),
        new("Fixed FPS", "Fixed"),
    };

    private void AddMediaTab(BlingoMemberMedia member)
    {
        var wrap = AddTab(PropetyTabNames.AVI);
        var adapter = new MediaMemberCommandAdapter(this, member);

        var togglePanel = _factory.CreatePanel("MediaTogglePanel");
        togglePanel.Margin = new AMargin(5, 5, 0, 0);
        togglePanel.Compose(_factory.ComponentFactory)
            .Columns(8)
            .AddCheckBox("MediaPlayVideo", "Video:", adapter, m => m.PlayVideo, 2, true, 2)
            .AddCheckBox("MediaStartPaused", "Paused:", adapter, m => m.StartPaused, 2, true, 2)
            .NextRow()
            .AddCheckBox("MediaLoop", "Loop:", adapter, m => m.EnableLoop, 2, true, 2)
            .AddCheckBox("MediaAudio", "Audio:", adapter, m => m.PlayAudio, 2, true, 2)
            .Finalize();
        wrap.AddItem(togglePanel);

        var playbackPanel = _factory.CreatePanel("MediaPlaybackPanel");
        playbackPanel.Margin = new AMargin(5, 5, 0, 0);
        var duration = TimeSpan.FromSeconds(member.DurationSeconds);
        var durationText = duration == TimeSpan.Zero
            ? "Unknown"
            : duration.ToString(duration.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");

        playbackPanel.Compose(_factory.ComponentFactory)
            .Columns(8)
            .AddLabel("MediaDurationLabel", "Duration:", 2)
            .AddLabel("MediaDurationValue", durationText, 2)
            .AddCombobox("MediaPlaybackMode", MediaPlaybackRateOptions, 110, adapter.CurrentPlaybackRateKey, adapter.ApplyPlaybackRate)
            .NextRow()
            .AddNumericInputInt("MediaStartMs", "Start (ms):", adapter, m => m.StartValueMs, inputSpan: 2, labelSpan: 2)
            .AddNumericInputInt("MediaVideoFps", "FPS:", adapter, m => m.VideoFps, inputSpan: 2, labelSpan: 2)
            .Finalize();
        wrap.AddItem(playbackPanel);

        var linkPanel = _factory.CreatePanel("MediaLinkPanel");
        linkPanel.Margin = new AMargin(5, 5, 5, 0);
        linkPanel.Compose(_factory.ComponentFactory)
            .Columns(8)
            .AddTextInput("MediaLinkedFile", "File:", adapter, m => m.LinkedFileName, inputSpan: 6, labelSpan: 2)
            .AddTextInput("MediaLinkedFolder", "Folder:", adapter, m => m.LinkedFolder, inputSpan: 6, labelSpan: 2)
            .Finalize();
        wrap.AddItem(linkPanel);
    }

    private sealed class MediaMemberCommandAdapter : MemberCommandAdapterBase<BlingoMemberMedia>
    {
        public MediaMemberCommandAdapter(DirectorPropertyInspectorWindow window, BlingoMemberMedia member)
            : base(window, member)
        {
        }

        public bool PlayVideo
        {
            get => Member.PlayVideo;
            set => DispatchIfChanged(nameof(BlingoMemberMedia.PlayVideo), Member.PlayVideo, value);
        }

        public bool StartPaused
        {
            get => Member.StartPause;
            set => DispatchIfChanged(nameof(BlingoMemberMedia.StartPause), Member.StartPause, value);
        }

        public bool EnableLoop
        {
            get => Member.EnableLoop;
            set => DispatchIfChanged(nameof(BlingoMemberMedia.EnableLoop), Member.EnableLoop, value);
        }

        public bool PlayAudio
        {
            get => Member.PlayAudio;
            set => DispatchIfChanged(nameof(BlingoMemberMedia.PlayAudio), Member.PlayAudio, value);
        }

        public int StartValueMs
        {
            get => Member.StartValueMs;
            set => DispatchIfChanged(nameof(BlingoMemberMedia.StartValueMs), Member.StartValueMs, value);
        }

        public int VideoFps
        {
            get => Member.VideoFps;
            set => DispatchIfChanged(nameof(BlingoMemberMedia.VideoFps), Member.VideoFps, value);
        }

        public string LinkedFileName
        {
            get => Member.LinkedFileName;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMemberMedia.LinkedFileName), Member.LinkedFileName, sanitized);
            }
        }

        public string LinkedFolder
        {
            get => Member.LinkedFolder;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMemberMedia.LinkedFolder), Member.LinkedFolder, sanitized);
            }
        }

        public string CurrentPlaybackRateKey
        {
            get
            {
                var fps = Member.VideoFps;
                return fps switch
                {
                    1 => MediaPlaybackRateOptions[1].Value,
                    2 => MediaPlaybackRateOptions[2].Value,
                    0 => MediaPlaybackRateOptions[0].Value,
                    _ => MediaPlaybackRateOptions[3].Value,
                };
            }
        }

        public void ApplyPlaybackRate(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            foreach (var option in MediaPlaybackRateOptions)
            {
                if (!string.Equals(option.Value, key, StringComparison.Ordinal))
                    continue;

                var newValue = option.Value switch
                {
                    "Sync" => 0,
                    "EveryFrame" => 1,
                    "Maximum" => 2,
                    _ => 3,
                };
                if (newValue == 3 && Member.VideoFps > 3)
                    newValue = Member.VideoFps;

                DispatchIfChanged(nameof(BlingoMemberMedia.VideoFps), Member.VideoFps, newValue);
                break;
            }
        }
    }
}

