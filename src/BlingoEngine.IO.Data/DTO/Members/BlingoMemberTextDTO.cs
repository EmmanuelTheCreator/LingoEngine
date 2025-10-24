namespace BlingoEngine.IO.Data.DTO.Members;

public class BlingoMemberTextDTO : BlingoMemberDTO
{
    public string MarkDownText { get; set; } = string.Empty;
    public bool IsEditable { get; set; }
    public bool TabsEnabled { get; set; }
    public bool DtdEnabled { get; set; }
    public bool IsAntialiasEnabled { get; set; }
    public int AntialiasMode { get; set; }
    public int AntialiasLargerThanPointSize { get; set; }
    public int KerningMode { get; set; }
    public bool IsKerningEnabled { get; set; }
    public int KerningLargerThanPointSize { get; set; }
}

