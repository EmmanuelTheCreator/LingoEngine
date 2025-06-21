using System;

namespace LingoEngine.Director.Core.Stages
{
    public enum StageTool
    {
        Pointer,
        Move,
        Rotate
    }

    public class StageToolManager : IStageToolManager
    {
        private StageTool _current;
        public StageTool CurrentTool
        {
            get => _current;
            set
            {
                if (_current != value)
                {
                    _current = value;
                    ToolChanged?.Invoke(value);
                }
            }
        }

        public event Action<StageTool>? ToolChanged;
    }
}
