using static BlingoEngine.IO.Data.DTO.Members.BlingoMemberScriptDTO;

namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastMemberScript : BlCastMemberItem
    {
        public BlScriptType ScriptType { get; set; }
        public string Script { get; set; } = "";
        public bool IsJavascript { get; set; }
        public string? LinkedFileName { get; set; }
        public string LinkedFolder { get; internal set; }

        public BlCastMemberScript()
        {
            MemberType = BlLegacyCastMemberType.Script;
        }
    }
}
