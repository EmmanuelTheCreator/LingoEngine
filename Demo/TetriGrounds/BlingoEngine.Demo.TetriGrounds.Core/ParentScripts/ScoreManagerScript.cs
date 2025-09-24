// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using AbstUI.Resources;
using BlingoEngine.Core;
using BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors;
using BlingoEngine.Movies;
using BlingoEngine.Texts;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 5_ScoreManager.ls
    /// <summary>
    /// Handles scoring, combo calculation, level progression and high score persistence.
    /// </summary>
    public class ScoreManagerScript : BlingoParentScript, IOverScreenTextParent
    {
        private readonly BlingoMemberText _memberScore;
        private readonly BlingoMemberText _memberTData;
        private readonly List<OverScreenTextScript> myOverScreenText = new();
        private readonly GlobalVars _global;
        private readonly ScoresRepository _scoresRepository;
        private int myPlayerScore;
        private int myLevel;
        private int myNumberLinesRemoved;
        private int myNumberLinesTot;
        private bool myLevelUp;
        private int myLevelUpNeededScore;
        private int myBlocksDroped;
        private DateTime myLastLineClear = DateTime.MinValue;
        private DateTime _started = DateTime.MinValue;
        private TimeSpan _elapsed;
        private readonly TimeSpan myComboDuration = TimeSpan.FromSeconds(2);
        private const int BlockFreezeBaseScore = 4;
        private const double MinimumDropSeconds = 0.1;
        private const double MaximumSpeedForBonus = 10.0;
        private const int DropSpeedLevelMultiplier = 2;

        public bool IsNewHighScore { get; private set; }

        /// <summary>
        /// Loads the initial state from cast members and prepares score UI for gameplay.
        /// </summary>
        public ScoreManagerScript(IBlingoMovieEnvironment env, GlobalVars global, ScoresRepository scoresRepository) : base(env)
        {
            _global = global;
            _scoresRepository = scoresRepository;
            myPlayerScore = 0;
            myNumberLinesTot = 0;
            myLevelUp = false;
            myBlocksDroped = 0;
            var txt = Member<BlingoMemberText>("T_StartLevel");
            _memberScore = Member<BlingoMemberText>("T_Score")!;
            _memberTData = Member<BlingoMemberText>("T_data")!;
            myLevel = txt != null && int.TryParse(txt.Text, out var lvl) ? lvl : 1;
            myLevelUpNeededScore = 100 * (myLevel + 1);

            UpdateGfxScore();
            NewText("Go!");
            _started = DateTime.UtcNow;
            Refresh();
        }

        /// <summary>
        /// Applies deferred scoring actions such as cleared lines and combo bonuses.
        /// </summary>
        public void Refresh()
        {
            var linesThisTurn = myNumberLinesRemoved;
            switch (linesThisTurn)
            {
                case 1: LineRemoved1(); break;
                case 2: LineRemoved2(); break;
                case 3: LineRemoved3(); break;
                case 4: LineRemoved4(); break;
            }
            if (linesThisTurn > 0)
            {
                var now = DateTime.UtcNow;
                if (now - myLastLineClear <= myComboDuration)
                {
                    myPlayerScore += 20 * myLevel * linesThisTurn;
                }
                myLastLineClear = now;
            }
            else
            {
                myLastLineClear = DateTime.MinValue;
            }
            myNumberLinesRemoved = 0;
            // check for level up (its the number of blocks droped)
            if (myBlocksDroped > myLevelUpNeededScore)
            {
                SendSprite<AnimationScriptBehavior>(22, x => x.StartAnim());
                myLevelUp = true;
                myLevel += 1;
                NewText($"Level {myLevel} !!");
                myLevelUpNeededScore += 1000;
                myPlayerScore += 200 * myLevel;
            }
            UpdateGfxScore();
            _memberTData.Text = $"Level {myLevel}";
        }

        /// <summary>
        /// Awards points for clearing a single line.
        /// </summary>
        public void LineRemoved1() => myPlayerScore += 80 * myLevel;
        /// <summary>
        /// Awards points and plays a sound when two lines are cleared.
        /// </summary>
        public void LineRemoved2()
        {
            _Player.SoundPlayRowsDeleted(2);
            NewText("2 Lines Removed!!"); myPlayerScore += 120 * myLevel;
        }
        /// <summary>
        /// Awards points and plays a sound when three lines are cleared.
        /// </summary>
        public void LineRemoved3()
        {
            _Player.SoundPlayRowsDeleted(3);
            NewText("3 Lines Removed!!"); myPlayerScore += 180 * myLevel;
        }
        /// <summary>
        /// Awards points and plays a sound when four lines are cleared.
        /// </summary>
        public void LineRemoved4()
        {
            _Player.SoundPlayRowsDeleted(4);
            NewText("Wooow, 4 Lines Removed!!"); myPlayerScore += 320 * myLevel;
        }

        /// <summary>
        /// Tracks the number of hard dropped blocks to adjust level progression speed.
        /// </summary>
        public void AddDropedBlock(bool hardDrop) => myBlocksDroped += hardDrop ? 4 : 0;
        /// <summary>
        /// Marks that a line was removed so <see cref="Refresh"/> can process the score increment.
        /// </summary>
        public void LineRemoved()
        {
            myNumberLinesRemoved += 1;
            myNumberLinesTot += 1;
        }
        /// <summary>
        /// Called when the current block locks in place, awarding score based on how quickly it fell.
        /// </summary>
        public void BlockFrozen(TimeSpan fallDuration, int rowsTravelled)
        {
            var clampedRows = Math.Max(rowsTravelled, 0);
            var seconds = Math.Max(fallDuration.TotalSeconds, MinimumDropSeconds);
            var speed = clampedRows / seconds;
            var clampedSpeed = Math.Min(Math.Max(speed, 0), MaximumSpeedForBonus);
            var levelMultiplier = Math.Max(myLevel, 1);
            var speedBonus = (int)Math.Round(clampedSpeed * levelMultiplier * DropSpeedLevelMultiplier);

            myPlayerScore += BlockFreezeBaseScore + speedBonus;
            Refresh();
        }
        /// <summary>
        /// Updates the on-screen score text and records whether a new high score was achieved.
        /// </summary>
        public void UpdateGfxScore()
        {
            _memberScore.Text = myPlayerScore.ToString();
            IsNewHighScore = myPlayerScore > _scoresRepository.LowestScore;
        }

        /// <summary>
        /// Returns true once when a level up occurs, allowing the caller to react.
        /// </summary>
        public bool GetLevelUp()
        {
            var t = myLevelUp;
            myLevelUp = false;
            return t;
        }
        /// <summary>
        /// Finalises the run, optionally prompting for the player's name if a new high score was achieved.
        /// </summary>
        public void GameFinished()
        {
            NewText("You're Terminated....");
            _elapsed = (DateTime.UtcNow - _started);
            if (IsNewHighScore)
            {
                SendSprite<EnterHighScoreBehavior>(38, x => x.Show(name =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                        name = "Anonymous";
                    _scoresRepository.StoreScore(name, myPlayerScore, myLevel, _started, _elapsed);
                }));
            }
        }

        /// <summary>
        /// Returns the current level reached by the player.
        /// </summary>
        public int GetLevel() => myLevel;
        /// <summary>
        /// Returns the accumulated score.
        /// </summary>
        public int GetScore() => myPlayerScore;
        // -----------------------------
        /// <summary>
        /// Spawns a temporary overlay text message to celebrate milestones.
        /// </summary>
        public void NewText(string text)
        {
            var o = new OverScreenTextScript(_env, _global, 130, text, this);
            myOverScreenText.Add(o);
        }

        /// <summary>
        /// Removes an overlay text once its animation has completed.
        /// </summary>
        public void TextFinished(OverScreenTextScript obj)
        {
            myOverScreenText.Remove(obj);
            obj.Destroy();
        }

        /// <summary>
        /// Destroys all overlay texts, usually when tearing down the gameplay scene.
        /// </summary>
        public void DestroyOverScreenTxt()
        {
            foreach (var o in myOverScreenText.ToArray())
            {
                o.Destroy();
            }
            myOverScreenText.Clear();
        }
        // -----------------------------
        /// <summary>
        /// Cleans up managed overlay texts.
        /// </summary>
        public void Destroy() => DestroyOverScreenTxt();
        // -----------------------------

        


       
    }
}

