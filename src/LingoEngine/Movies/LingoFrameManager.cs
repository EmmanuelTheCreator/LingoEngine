using LingoEngine.Sprites;

namespace LingoEngine.Movies
{
    /// <summary>
    /// Handles frame related data such as score labels and frame specific behaviours.
    /// </summary>
    internal class LingoFrameManager
    {
        private readonly LingoMovieEnvironment _environment;
        private readonly LingoMovie _movie;
        private readonly Action _raiseSpriteListChanged;
        private readonly List<LingoSprite> _allTimeSprites;
        private readonly Dictionary<string, int> _scoreLabels = new();

        internal LingoFrameManager(LingoMovie movie, LingoMovieEnvironment environment, List<LingoSprite> allTimeSprites, Action raiseSpriteListChanged)
        {
            _movie = movie;
            _environment = environment;
            _allTimeSprites = allTimeSprites;
            _raiseSpriteListChanged = raiseSpriteListChanged;
        }

        internal IReadOnlyDictionary<int, string> MarkerList =>
            _scoreLabels.ToDictionary(kv => kv.Value, kv => kv.Key);

        internal IReadOnlyDictionary<string, int> ScoreLabels => _scoreLabels;


        internal void SetScoreLabel(int frameNumber, string? name)
        {
            string? existingLabel = null;
            foreach (var item in _scoreLabels)
            {
                if (item.Value == frameNumber)
                {
                    existingLabel = item.Key;
                    break;
                }
            }
            if (existingLabel != null)
                _scoreLabels.Remove(existingLabel);
            if (!string.IsNullOrEmpty(name))
                _scoreLabels[name] = frameNumber;
        }

        internal int GetNextLabelFrame(int frame)
        {
            var next = _scoreLabels.Values
                .Where(v => v > frame)
                .DefaultIfEmpty(int.MaxValue)
                .Min();
            if (next == int.MaxValue)
                return frame + 10;
            return next;
        }

        internal int GetNextSpriteStart(int channel, int frame)
        {
            int next = int.MaxValue;
            foreach (var sp in _allTimeSprites)
            {
                if (sp.SpriteNum - 1 == channel && sp.BeginFrame > frame)
                    next = Math.Min(next, sp.BeginFrame);
            }
            return next == int.MaxValue ? -1 : next;
        }

        internal int GetPrevSpriteEnd(int channel, int frame)
        {
            int prev = -1;
            foreach (var sp in _allTimeSprites)
            {
                if (sp.SpriteNum - 1 == channel && sp.EndFrame < frame)
                    prev = Math.Max(prev, sp.EndFrame);
            }
            return prev;
        }

        internal int GetNextMarker(int frame)
        {
            if (_scoreLabels.Count == 0)
                return 1;
            var next = _scoreLabels.Values.Where(v => v > frame).DefaultIfEmpty(_scoreLabels.Values.Max()).Min();
            return next;
        }

        internal int GetPreviousMarker(int frame)
        {
            if (_scoreLabels.Count == 0)
                return 1;
            var markers = _scoreLabels.Values.OrderBy(v => v).ToList();
            bool currentIsMarker = markers.Contains(frame);
            int target;
            if (currentIsMarker)
            {
                int idx = markers.IndexOf(frame);
                target = idx > 0 ? markers[idx - 1] : frame;
            }
            else
            {
                int prev = markers.Where(v => v < frame).DefaultIfEmpty(0).Max();
                if (prev == 0)
                {
                    int right = markers.Where(v => v > frame).DefaultIfEmpty(1).Min();
                    target = right;
                }
                else
                {
                    int idx = markers.IndexOf(prev);
                    target = idx > 0 ? markers[idx - 1] : prev;
                }
            }
            return target;
        }

        internal int GetLoopMarker(int frame)
        {
            if (_scoreLabels.Count == 0)
                return 1;
            var markers = _scoreLabels.Values.OrderBy(v => v).ToList();
            int prev = markers.Where(v => v < frame).DefaultIfEmpty(0).Max();
            if (prev > 0)
            {
                return prev;
            }
            else
            {
                if (markers.Contains(frame))
                    return frame;
                else
                {
                    int right = markers.Where(v => v > frame).DefaultIfEmpty(1).Min();
                    return right;
                }
            }
        }
    }
}
