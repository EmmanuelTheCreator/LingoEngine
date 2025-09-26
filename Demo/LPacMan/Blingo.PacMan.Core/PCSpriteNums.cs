namespace Blingo.PacMan.Core
{
    internal class PCSpriteNums
    {
        // sprite ranges
        //  1 -  19 : Generic
        // 20 - 29 : Lives
        // 30 - 39 : Bonuses taken
        // 50      : Pacman
        // 51 - 55 : Ghosts
        // 70 - 80 : Power pills
        // 80 - .. : Pellets


        // Start menu
        public static int BtnStart = 3;


        // Game
        public static int GameBG = 2;

        public static int T_Label_HighScore = 3;
        public static int T_HighScore = 4;
        public static int T_Player1_Label = 5;
        public static int T_Player2_Label = 6;
        public static int T_Player1_Score = 7;
        public static int T_Player2_Score = 8;
        public static int T_Player1_Text = 9;
        public static int T_Player2_Text = 10;
        public static int T_Ready = 11;

        public static int LivesStart = 20; 
        public static int BonusAvailable = 30; 
        public static int BonusesRoaming = 39; 

        public static int PacMan = 50;
        public static int GhostStart = 51;
        public static int PowerPillStart = 70;
        public static int PelletsStart = 80;
    }
}
