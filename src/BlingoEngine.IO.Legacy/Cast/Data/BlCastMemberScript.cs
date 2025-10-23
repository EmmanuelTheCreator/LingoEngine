namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastMemberScript : BlCastMemberItem
    {
        public enum BlScriptType
        {
            Behavior = 1,
            MovieScript = 3,
            ParentScript = 7
        }
        public BlScriptType ScriptType { get; set; }
        public string Script { get; set; } = "";
        public bool IsJavascript { get; set; }
        public string? LinkName { get; set; } 

        public BlCastMemberScript()
        {
            MemberType = BlLegacyCastMemberType.Script;
        }
    }
}
