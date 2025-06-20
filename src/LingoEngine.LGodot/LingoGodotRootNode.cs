using Godot;

namespace LingoEngine.LGodot
{
    public class LingoGodotRootNode
    {
        public Node RootNode { get; }
        public bool WithStageInWindow { get; }

        public LingoGodotRootNode(Node rootNode, bool withStageInWindow) 
        {
            RootNode = rootNode;
            WithStageInWindow = withStageInWindow;
        }
    }
}
