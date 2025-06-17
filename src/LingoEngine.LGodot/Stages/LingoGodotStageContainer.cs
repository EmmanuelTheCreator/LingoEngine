using Godot;
using LingoEngine.FrameworkCommunication;
using LingoEngine.LGodot.Movies;

namespace LingoEngine.LGodot.Stages
{
    public class LingoGodotStageContainer : ILingoFrameworkStageContainer
    {
        private bool _stageSet;
        private Node? _Root;
        private Node2D _stageContainer = new Node2D();
        //private LingoGodotStage? _stage;
        public Node2D Container => _stageContainer;

        public LingoGodotStageContainer(LingoGodotRootNode lingoGodotRootNode)
        {
            _Root = lingoGodotRootNode.RootNode;
            if (!lingoGodotRootNode.WithStageInWindow)
                _Root.AddChild(_stageContainer);
           
        }
        
        public void SetStage(ILingoFrameworkStage stage)
        {
            var stage1 = stage as LingoGodotStage;
            _stageContainer.AddChild(stage1);
        }
        
    }
}
