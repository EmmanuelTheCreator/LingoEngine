// Copyright to EmmanuelTheCreator.com
// This file was written in 2005, yeah a lot has evolved since then :-)
// Converted from original Lingo code, tried to keep it as identical as possible.

using BlingoEngine.Core;

namespace Blingo.PacMan.Core
{
    /// <summary>
    /// Convenience extension methods that wrap the various sound cues used throughout the Pac-Man game.
    /// </summary>
    internal static class PacManSounds
    {
        public static void SoundPlayBack(this IBlingoPlayer player)
        {
            var channel = player.Sound.Channel(1);
            if (channel != null)
            {
                channel.Volume = 160;
            }

            player.Sound.PuppetSound(1, "S_back");
        }

        public static void SoundStopBack(this IBlingoPlayer player)
        {
            player.Sound.Channel(1)?.Stop();
        }

        public static void SoundPlayBonus(this IBlingoPlayer player) => player.Sound.PuppetSound(2, "S_bonus");

        public static void SoundPlayDead(this IBlingoPlayer player) => player.Sound.PuppetSound(3, "S_dead");

        public static void SoundPlayDot(this IBlingoPlayer player) => player.Sound.PuppetSound(4, "S_dot");

        public static void SoundPlayEat(this IBlingoPlayer player) => player.Sound.PuppetSound(5, "S_eat");

        public static void SoundPlayEaten(this IBlingoPlayer player) => player.Sound.PuppetSound(6, "S_eaten");

        public static void SoundPlayFrightened(this IBlingoPlayer player)
        {
            var channel = player.Sound.Channel(1);
            if (channel != null)
            {
                channel.Volume = 160;
            }

            player.Sound.PuppetSound(1, "S_frightened");
        }

        public static void SoundPlayIntro(this IBlingoPlayer player)
        {
            var channel = player.Sound.Channel(1);
            if (channel != null)
            {
                channel.Volume = 160;
            }

            player.Sound.PuppetSound(1, "S_intro");
        }

        public static void SoundPlayLife(this IBlingoPlayer player) => player.Sound.PuppetSound(7, "S_life");
    }
}
