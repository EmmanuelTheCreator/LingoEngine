using Godot;

namespace LingoEngine.Godot
{
    public class LingoGodotRootNode
    {
        public Node RootNode { get; }
        public LingoGodotRootNode(Node rootNode)
        {
            RootNode = rootNode;
        }

    }
}
