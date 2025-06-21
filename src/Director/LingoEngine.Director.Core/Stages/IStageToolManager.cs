using System;
namespace LingoEngine.Director.Core.Stages
{
    public interface IStageToolManager
    {
        StageTool CurrentTool { get; set; }
        event Action<StageTool>? ToolChanged;
    }
}
