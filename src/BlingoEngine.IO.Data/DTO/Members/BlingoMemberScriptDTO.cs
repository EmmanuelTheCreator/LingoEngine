namespace BlingoEngine.IO.Data.DTO.Members;

public class BlingoMemberScriptDTO : BlingoMemberDTO
{
    public string? LinkedFilePath { get; set; } 

    public enum BlScriptType
    {
        Behavior = 1,
        MovieScript = 3,
        ParentScript = 7
    }
    public BlScriptType ScriptType { get; set; }
    public string Script { get; set; } = "";
    public bool IsJavascript { get; set; }
}

