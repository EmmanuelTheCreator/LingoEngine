using static BlingoEngine.IO.Data.DTO.Members.BlingoMemberScriptDTO;

namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastMemberScript : BlCastMemberItem
    {
        public BlScriptType ScriptType { get; set; }
        public string Script { get; set; } = "";
        public bool IsJavascript { get; set; }
        public string? LinkedFilePath { get; set; } 

        public BlCastMemberScript()
        {
            MemberType = BlLegacyCastMemberType.Script;
        }
    }
}
