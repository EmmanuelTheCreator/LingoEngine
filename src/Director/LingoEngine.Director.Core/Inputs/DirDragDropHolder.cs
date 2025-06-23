using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Inputs
{
    public class DirDragDropHolder
    {
        public static ILingoMember? Member { get; private set; }
        public static ILingoSprite? Sprite { get; private set; }
        public static bool IsDragging { get; private set; }

        public static void StartDrag(ILingoMember payload, string type)
        {
            Member = payload;
            IsDragging = true;
        }
        public static void StartDrag(ILingoSprite payload, string type)
        {
            Sprite = payload;
            IsDragging = true;
        }

        public static void CancelDrag()
        {
            Clear();
        }

        public static void EndDrag()
        {
            Clear();
        }

        private static void Clear()
        {
            Member = null;
            Sprite = null;
            IsDragging = false;
        }
    }
}
