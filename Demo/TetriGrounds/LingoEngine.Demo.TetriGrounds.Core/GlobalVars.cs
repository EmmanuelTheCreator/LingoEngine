namespace LingoEngine.Demo.TetriGrounds.Core
{
    public class GlobalVars
    {
        public string LastInfo { get; internal set; } = "";

        // Globals created in StarMovieScript
        public ParentScripts.SpriteManagerParentScript? SpriteManager { get; set; }
        public ParentScripts.MousePointer? MousePointer { get; set; }
    }
}
