using LingoEngine.Core;
using LingoEngine.Events;
using LingoEngine.Movies;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 10_Block.ls
    public class BlockParentScript : LingoParentScript, IHasStepFrameEvent
    {
        private readonly GlobalVars _global;
        private string myMember = "Block1";
        private int myNum;
        private bool myDestroyAnim;
        private int myMemberNumAnim;

        public BlockParentScript(ILingoMovieEnvironment env, GlobalVars global, int chosenType = 1) : base(env)
        {
            _global = global;
            string[] members = { "Block1", "Block2", "Block3", "Block4", "Block5", "Block6", "Block7" };
            if (chosenType >= 1 && chosenType <= members.Length) myMember = members[chosenType - 1];
        }

        public void StepFrame()
        {
            if (myDestroyAnim)
            {
                if (myMemberNumAnim > 7)
                {
                    myMemberNumAnim = 0;
                    _Movie.ActorList.Remove(this);
                    Destroy();
                    return;
                }
                myMemberNumAnim += 1;
                Sprite(myNum).SetMember("Destroy" + myMemberNumAnim);
            }
        }

        public void DestroyAnim()
        {
            myDestroyAnim = true;
            myMemberNumAnim = 0;
            _Movie.ActorList.Add(this);
        }

        public void FinishBlock()
        {
            Sprite(myNum).SetMember("Destroy1");
        }

        public void CreateBlock()
        {
            myNum = _global.SpriteManager?.Sadd() ?? 0;
            var spr = Sprite(myNum);
            spr.SetMember(myMember);
            spr.Ink = 36;
        }

        public int GetSpriteNum() => myNum;

        public void Destroy()
        {
            _Movie.ActorList.Remove(this);
            _global.SpriteManager?.SDestroy(myNum);
        }
    }
}
