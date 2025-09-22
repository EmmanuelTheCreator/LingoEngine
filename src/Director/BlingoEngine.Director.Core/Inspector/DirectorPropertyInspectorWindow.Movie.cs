using System;
using System.Collections.Generic;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using BlingoEngine.Director.Core.Movies.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Movies;
using BlingoEngine.Stages;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private static readonly KeyValuePair<string, string>[] StageResolutionOptions =
    {
        new("640x480", "640x480"),
        new("800x600", "800x600"),
        new("1024x768", "1024x768"),
        new("1280x720", "1280x720"),
        new("1920x1080", "1920x1080"),
    };

    private void AddMovieTab(IBlingoMovie? movie)
    {
        var wrap = AddTab(PropetyTabNames.Movie);
        var container = _factory.CreatePanel("MovieDetailPanel");
        container.BackgroundColor = DirectorColors.BG_WhiteMenus;
        container.Margin = new AMargin(5, 5, 5, 0);
        wrap.AddItem(container);

        if (movie == null)
            return;

        var adapter = new MovieCommandAdapter(this, movie);

        container.Compose(_factory.ComponentFactory)
            .Columns(8)
            .AddLabel("MovieStageSizeLabel", "Stage size:", 2)
            .AddNumericInputFloat("MovieStageWidth", "W:", adapter, m => m.StageWidth, inputSpan: 1, labelSpan: 1)
            .AddLabel("MovieStageSizeXLabel", "x", 1)
            .AddNumericInputFloat("MovieStageHeight", "H:", adapter, m => m.StageHeight, inputSpan: 1, labelSpan: 1)
            .AddCombobox("MovieResolutions", StageResolutionOptions, 90, adapter.CurrentResolutionKey, adapter.ApplyResolution)
            .NextRow()
            .AddColorPicker("MovieStageBackground", "Color:", adapter, m => m.StageBackgroundColor, inputSpan: 2, labelSpan: 2)
            .AddNumericInputInt("MovieMaxChannels", "Channels:", adapter, m => m.MaxSpriteChannelCount, inputSpan: 2, labelSpan: 2)
            .NextRow()
            .AddTextInput("MovieAbout", "About:", adapter, m => m.About, inputSpan: 6, labelSpan: 2)
            .AddTextInput("MovieCopyright", "Copyright:", adapter, m => m.Copyright, inputSpan: 6, labelSpan: 2)
            .Finalize();
    }

    private void DispatchMovieCommand(IBlingoMovie movie, IReadOnlyList<APropertyValue> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        _commandManager.Handle(new BlingoUpdateMoviePropertiesCommand(BlingoMovieRef.FromMovie(movie), changes));
    }

    private sealed class MovieCommandAdapter : PropertyCommandAdapterBase<IBlingoMovie>
    {
        public MovieCommandAdapter(DirectorPropertyInspectorWindow window, IBlingoMovie movie)
            : base(window, movie)
        {
        }

        private DirectorPropertyInspectorWindow Inspector => Window;

        public float StageWidth
        {
            get => Target.Width;
            set => DispatchIfChanged(nameof(BlingoStage.Width), Target.Width, value);
        }

        public float StageHeight
        {
            get => Target.Height;
            set => DispatchIfChanged(nameof(BlingoStage.Height), Target.Height, value);
        }

        public AColor StageBackgroundColor
        {
            get => Inspector._player.Stage.BackgroundColor;
            set => DispatchIfChanged(nameof(BlingoStage.BackgroundColor), Inspector._player.Stage.BackgroundColor, value);
        }

        public int MaxSpriteChannelCount
        {
            get => Target.MaxSpriteChannelCount;
            set => DispatchIfChanged(nameof(BlingoMovie.MaxSpriteChannelCount), Target.MaxSpriteChannelCount, value);
        }

        public string About
        {
            get => Target.About ?? string.Empty;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMovie.About), Target.About ?? string.Empty, sanitized);
            }
        }

        public string Copyright
        {
            get => Target.Copyright ?? string.Empty;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMovie.Copyright), Target.Copyright ?? string.Empty, sanitized);
            }
        }

        public string CurrentResolutionKey
        {
            get
            {
                var width = (int)Math.Round(Target.Width);
                var height = (int)Math.Round(Target.Height);
                return $"{width}x{height}";
            }
        }

        public void ApplyResolution(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var parts = value.Split('x');
            if (parts.Length != 2)
                return;

            if (!int.TryParse(parts[0], out var width) || !int.TryParse(parts[1], out var height))
                return;

            var changes = new List<APropertyValue>(2);
            if (Math.Abs(Target.Width - width) > float.Epsilon)
                changes.Add(new APropertyValue(nameof(BlingoStage.Width), (float)width));
            if (Math.Abs(Target.Height - height) > float.Epsilon)
                changes.Add(new APropertyValue(nameof(BlingoStage.Height), (float)height));

            Dispatch(changes);
        }

        protected override void DispatchChanges(IBlingoMovie target, IReadOnlyList<APropertyValue> changes)
            => Inspector.DispatchMovieCommand(target, changes);
    }
}
