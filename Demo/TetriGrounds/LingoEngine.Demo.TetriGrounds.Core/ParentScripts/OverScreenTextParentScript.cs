using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Texts;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 6_OverScreenText.ls
    public class OverScreenTextParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private readonly GlobalVars _global;
        private int myNum = 48;
        private int myNum2 = 49;
        private int myCounter;
        public int Duration { get; set; } = 130;
        public string Text { get; set; } = string.Empty;
        public int LocV { get; set; } = 100;
        public ScoreManagerParentScript? Parent { get; set; }
;
        public OverScreenTextParentScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)
        {
            _global = global;
            _Movie.ActorList.Add(this);
            Sprite(myNum).Puppet = true;
            Sprite(myNum2).Puppet = true;
            Sprite(myNum).SetMember("T_OverScreen");
            Sprite(myNum2).SetMember("T_OverScreen2");
            Sprite(myNum).LocZ = 1000;
            Sprite(myNum2).LocZ = 999;
            Sprite(myNum).Ink = 36;
            Sprite(myNum2).Ink = 36;
        }

        public void StepFrame()
        {
            myCounter += 1;
            if (myCounter > Duration)
            {
                _Movie.ActorList.Remove(this);
                Sprite(myNum).Puppet = false;
                Sprite(myNum2).Puppet = false;
                Parent?.TextFinished(this);
                return;
            }
            LocV += 2;
            Sprite(myNum).LocH = 180;
            Sprite(myNum2).LocH = 182;
            Sprite(myNum).LocV = LocV;
            Sprite(myNum2).LocV = LocV + 2;
            float blend = 70f - (float)myCounter / Duration * 100f;
            if (blend < 0) blend = 0;
            Sprite(myNum).Blend = (int)blend;
            Sprite(myNum2).Blend = (int)blend;
            Member<LingoMemberText>("T_OverScreen")?.SetText(Text);
            Member<LingoMemberText>("T_OverScreen2")?.SetText(Text);
        }

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
            Sprite(myNum).Blend = 100;
            Sprite(myNum2).Blend = 100;
            Sprite(myNum).Puppet = false;
            Sprite(myNum2).Puppet = false;
        }
    }
}
