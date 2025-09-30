using Blingo.PacMan.Core.Engine;
using BlingoEngine.Inputs;
using BlingoEngine.Movies;
using BlingoEngine.Texts;

namespace Blingo.PacMan.Core.Game
{
    internal class BlPacManScoreManager
    {

        private int _score;
        private int _highScore;
        private GlobalVars _globals;
        private bool _isInitialized;
        private IBlingoMovie _movie = null!;
        private IBlingoMemberTextBase _memberScore1 = null!;
        private IBlingoMemberTextBase _memberScore2 = null!;
        private IBlingoMemberTextBase _memberHighScore = null!;
        private readonly BlPacManEventMediator<int> _scoreChanged = new();
        private readonly BlPacManEventMediator<int> _highScoreChanged = new();


        public int Score => _score;
        public int HighScore => _highScore;
        public int ExtraLifeScore { get; private set; } = 10_000;

        public BlPacManEventSubscription SubscribeScoreChanged(Action<int> handler) => _scoreChanged.Subscribe(handler);
        public BlPacManEventSubscription SubscribeHighScoreChanged(Action<int> handler) => _highScoreChanged.Subscribe(handler);


        public BlPacManScoreManager(GlobalVars globals)
        {
            _globals = globals;
        }

        public void Init(IBlingoMovie movie)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            _movie = movie;
            _memberScore1 = _movie.GetMember<IBlingoMemberTextBase>("T_Player1_Score")!;
            _memberScore2 = _movie.GetMember<IBlingoMemberTextBase>("T_Player2_Score")!;
            _memberHighScore = _movie.GetMember<IBlingoMemberTextBase>("T_HighScore")!;
        }

        public void Reset()
        {
            SetScore(0);
            //SetHighScore(0);
        }

        public void AddScore(int score)
        {
            if (score == 0)
                return;

            SetScore(_score + score);
        }

        public void SetScore(int score)
        {
            _score = Math.Max(0, score);
            _memberScore1.Text = score.ToString("D5");
            OnScoreChanged();
        }
        public void SetHighScore(int score)
        {
            _highScore = Math.Max(0, score);
        }
        public void ResetScore()
        {
            if (_score == 0)
                return;

            SetScore(0);
        }
        private void OnScoreChanged()
        {
            _scoreChanged.Publish(_score);

            if (_score >= ExtraLifeScore)
                _globals.LivesManager.AddExtraLiveIfPossible();

            if (_score > _highScore)
            {
                SetHighScore();
            }
        }

        private void SetHighScore()
        {
            _highScore = _score;
            _memberHighScore.Text = _highScore.ToString("D5");
            _highScoreChanged.Publish(_highScore);
        }
    }
}
