// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;
using BlingoEngine.Movies;
#pragma warning disable IDE1006 // Naming Styles
namespace BlingoEngine.Demo.TetriGrounds.Core.ParentScripts
{
    // Converted from 3_SpriteManager.ls
    /// <summary>
    /// Provides pooled sprite allocation similar to the original Director parent script.
    /// </summary>
    public class SpriteManager : BlingoParentScript
    {
        private int pNum;
        private readonly List<int> pDestroyList = new();
        private readonly List<int> pSpriteNums = new();
        private object? pGame;

        public SpriteManager(IBlingoMovieEnvironment env) : base(env) { }

        /// <summary>
        /// Sets the initial sprite index and clears any cached state.
        /// </summary>
        public void Init(int beginningSprite)
        {
            pNum = beginningSprite;
            pDestroyList.Clear();
            pSpriteNums.Clear();
            pGame = null;
        }
        private int MaxSpriteNum = 900;
        /// <summary>
        /// Allocates a sprite number, reusing destroyed entries when possible.
        /// </summary>
        public int Sadd()
        {
            if (pDestroyList.Count == 0) // are there destroyed sprites
            {
                pNum += 1; // create a new one
                // check if we are at the maximum
                if (pNum > MaxSpriteNum-1)
                {
                    var spr = Sprite(MaxSpriteNum);
                    _Movie.PuppetSprite(MaxSpriteNum, true);
                    spr.Loc = Point(100, 30);
                    spr.SetMember("TomuchSprites");
                    spr.Blend = 100;
                    spr.LocZ = MaxSpriteNum;
                    spr.Blend = 0;
                    spr.Loc = Point(1, -40);
                    pNum -= 1;
                    return 0;
                }
                _Movie.PuppetSprite(pNum, true);
                if (pSpriteNums.Contains(pNum))
                    pNum += 100000;
                pSpriteNums.Add(pNum);
                var sprite = Sprite(pNum);
                sprite.Ink = 36;
                return pNum;
            }
            else // create a new from the destroyed sprite list
            {
                int pNumDestroy = pDestroyList[0];
                _Movie.PuppetSprite(pNumDestroy, true);
                pDestroyList.RemoveAt(0);
                pSpriteNums.Add(pNumDestroy);
                Sprite(pNumDestroy).Ink = 36;
                return pNumDestroy;
            }
        }

        /// <summary>
        ///  destroy a sprite
        /// </summary>
        /// <param name="sprNum">The sprite num</param>
        public void SDestroy(int sprNum)
        {
            if (!pSpriteNums.Contains(sprNum))
                return;
            pSpriteNums.Remove(sprNum);
            pDestroyList.Add(sprNum);
            var spr = Sprite(sprNum);
            spr.SetMember("empty");
            spr.LocZ = sprNum;
            _Movie.PuppetSprite(sprNum, false);
        }

        /// <summary>
        /// Returns the list of active sprite numbers.
        /// </summary>
        public IReadOnlyList<int> GetSpriteNums() => pSpriteNums;

        /// <summary>
        /// Returns 1 if the sprite is currently managed by the pool; mimics the original Lingo behaviour.
        /// </summary>
        public int CheckSprite(int num) => pSpriteNums.Contains(num) ? 1 : 0;

        /// <summary>
        /// Frees all managed sprites and clears lookup lists.
        /// </summary>
        public void Destroy()
        {
            foreach (var i in pDestroyList.ToArray())
                SDestroy(i);
            pGame = null;
        }
    }
}

