using LingoEngine.Core;
using LingoEngine.Movies;
using LingoEngine.Primitives;

namespace LingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 7_Gfx.ls
    public class GfxParentScript : LingoParentScript
    {
        private int myStartX;
        private int myStartY;
        public GfxParentScript(ILingoMovieEnvironment env) : base(env)
        {
            myStartX = 250;
            myStartY = 45;
        }

        public void PositionBlock(int sprNum, int x, int y)
        {
            if (sprNum == 0) return;
            int xx = myStartX + x * 17;
            int yy = myStartY + y * 17;
            var spr = Sprite(sprNum);
            spr.LocH = xx;
            spr.LocV = yy;
        }

        public void Destroy() { }
    }
}
