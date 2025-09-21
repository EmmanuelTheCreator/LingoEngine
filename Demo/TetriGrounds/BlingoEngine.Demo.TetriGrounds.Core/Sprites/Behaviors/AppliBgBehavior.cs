// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Demo.TetriGrounds.Core.ParentScripts;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Texts;
using BlingoEngine.VerboseLanguage;
using System.ComponentModel;
#pragma warning disable IDE1006 // Naming Styles

namespace BlingoEngine.Demo.TetriGrounds.Core.Sprites.Behaviors
{
    // Converted from 16_AppliBg.ls
    /// <summary>
    /// Legacy behaviour responsible for coordinating menu data and on-screen text messages.
    /// </summary>
    public class AppliBgBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent, IHasExitFrameEvent, IHasEndSpriteEvent, IOverScreenTextParent, IHasCounterStartData
    {
        private int _pos;
        private int lowest;
        private bool myCheckStartData;
        private bool myStop;
        private int myStartLines;
        private int myStartLevel;
        private int WebName;
        private readonly GlobalVars _globalVars;
        private List<OverScreenTextScript>? myOverScreenText;

        /// <summary>
        /// Stores the shared global state reference.
        /// </summary>
        public AppliBgBehavior(IBlingoMovieEnvironment env, GlobalVars globalVars) : base(env)
        {
            _globalVars = globalVars;
        }

        /// <summary>
        /// Called when the behaviour starts; would normally trigger remote data requests.
        /// </summary>
        public void BeginSprite()
        {
            Member<IBlingoMemberTextBase>("PlayerName")!.Text = WebName.ToString();
            //myHsDown = new script("score_get").New();
            //myHsDown.SetShowType(1);
            //myHsUp = script("score_save").New();
            //myGetStartData = script("StartData_get").New(this);
            //// myStop = true
            //myCheckStartData = true;
            //mySendStartData = script("StartData_save").New();
            //Cursor = 200;
            // myHsUp.postScore(member("PlayerName").text, 10000)
        }


        /// <summary>
        /// Handles the completion of the start-data download.
        /// </summary>
        public void DataLoaded(string data, object obj)
        {
            this.Put(data).ToLog();
            if (data == "")
            {
                _Movie.GoTo(2);
            }
            else
            {
                myStartLevel = Convert.ToInt32(data.Line(1));
                myStartLines = Convert.ToInt32(data.Line(2));
                myStop = false;
            }
        }


        /// <summary>
        /// Updates cached start-level information before posting it back to the server.
        /// </summary>
        public void SendData(string _type, int? data)
        {
            if (data == null)
            {
                return;
            }
            if (_type == "StartLevel")
            {
                myStartLevel = data.Value;
            }
            if (_type == "StartLines")
            {
                myStartLines = data.Value;
            }
            //mySendStartData.Post(myStartLevel, myStartLines);
        }



        /// <summary>
        /// Keeps the movie on the current frame until remote data has been retrieved.
        /// </summary>
        public void ExitFrame()
        {
            if (myCheckStartData)
            {
                if (myStop)
                    _Movie.GoTo(_Movie.CurrentFrame);
                else
                    _Movie.GoTo("Game");
            }
        }

        /// <summary>
        /// Displays a message once the game is finished and would normally refresh highscores.
        /// </summary>
        public void GameFinished(int _score)
        {
            RefeshHighScores();
            //myHsDown.SetShowType(2);
            // check if the score is higher
            //lowest = myHsDown.GetLowestPersonalScore();
            //if (_score > lowest)
            {
           //     myHsUp.PostScore(GetMember<IBlingoMemberTextBase>("PlayerName").Text, _score);
            }
        }


        /// <summary>
        /// Handles the simulated response from the high score upload.
        /// </summary>
        public void ReturnFromSaveScore(string data)
        {
            if (data.Contains("Highscore"))  // new highscore
            {
                NewText(data);
                RefeshHighScores();
            }
        }
        /// <summary>
        /// Placeholder for showing personal high scores.
        /// </summary>
        public void PersonalHighscores()
        {
            //myHsDown.SetShowType(2);
            //myHsDown.OutputScores();
        }
        /// <summary>
        /// Placeholder for showing global high scores.
        /// </summary>
        public void ShowGeneralScores()
        {
            //myHsDown.SetShowType(1);
            //myHsDown.OutputScores();
        }
        /// <summary>
        /// Placeholder for refreshing the high score table from the server.
        /// </summary>
        public void RefeshHighScores()
        { 
            //myHsDown.downloadScores();
        }
        /// <summary>
        /// Displays a temporary overlay text message.
        /// </summary>
        public void NewText(string _text)
        {
            if (myOverScreenText == null)
                myOverScreenText = [];
            
            myOverScreenText.Add(new OverScreenTextScript(_env, _globalVars,130, _text, this));
        }



        /// <inheritdoc />
        public void TextFinished(OverScreenTextScript obj)
        {
            if (myOverScreenText == null) return;
            _pos = myOverScreenText.IndexOf(obj);
            myOverScreenText[_pos].Destroy();
            myOverScreenText.Remove(obj);
        }


        /// <summary>
        /// Destroys all overlay text instances managed by this behaviour.
        /// </summary>
        public void DestroyoverscreenTxt()
        {
            if (myOverScreenText == null) return;
            for (var i = 1; i <= myOverScreenText.Count; i++)
                myOverScreenText[i].Destroy();
            
            myOverScreenText = [];
        }

        /// <inheritdoc />
        public int GetCounterStartData(string _type)
        {
            if (_type == "StartLevel")
            {
                return myStartLevel;
            }
            if (_type == "StartLines")
            {
                return myStartLines;
            }
            return 0;
        }

        /// <summary>
        /// Cleans up when the behaviour is removed.
        /// </summary>
        public void EndSprite()
        {
           // myHsDown.Destroy();
            DestroyoverscreenTxt();
        }



    }
}

