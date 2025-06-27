using Godot;
using LingoEngine.LGodot.Movies;
using LingoEngine.Stages;

namespace LingoEngine.LGodot.Stages
{
    public class LingoGodotStageContainer : ILingoFrameworkStageContainer
    {
        private bool _stageSet;
        private Node? _Root;
        private Node2D _stageContainer = new Node2D();
        private LingoGodotStage? _stage1;

        //private LingoGodotStage? _stage;
        public Node2D Container => _stageContainer;

        public LingoGodotStageContainer(LingoGodotRootNode lingoGodotRootNode)
        {
            _stageContainer.Name = "StageContainer";
            _Root = lingoGodotRootNode.RootNode;
            if (!lingoGodotRootNode.WithStageInWindow)
                _Root.AddChild(_stageContainer);
           
        }
        
        public void SetStage(ILingoFrameworkStage stage)
        {
            _stage1 = stage as LingoGodotStage;
            _stageContainer.AddChild(_stage1);
        }

        public void SetScale(float scale)
        {
            if (_stage1 == null) return;
            _stage1.SetScale(scale);
        }
    }
}
