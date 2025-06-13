using Godot;

namespace LingoEngine.LGodot
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
