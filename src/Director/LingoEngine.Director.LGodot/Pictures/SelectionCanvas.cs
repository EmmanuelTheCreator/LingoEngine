using Godot;
using System.Linq;

namespace LingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow
{
    private partial class SelectionCanvas : Control
    {
        private readonly DirGodotPictureMemberEditorWindow _owner;
        public SelectionCanvas(DirGodotPictureMemberEditorWindow owner)
        {
            _owner = owner;
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Draw()
        {
            float factor = _owner._scale;
            Vector2 canvasHalf = _owner._centerContainer.CustomMinimumSize / 2f;
            Vector2 imageHalf = _owner._imageRect.CustomMinimumSize / 2f;
            Vector2 offset = canvasHalf - imageHalf;

            var color = new Color(0, 0.6f, 1f, 0.3f);
            foreach (var px in _owner._selectedPixels)
            {
                Vector2 pos = (offset + px) * factor + canvasHalf * (1 - factor);
                DrawRect(new Rect2(pos, Vector2.One * factor), color);
            }

            if (_owner._dragSelecting && _owner._dragRect.HasValue)
            {
                var rect = _owner._dragRect.Value;
                Vector2 pos = (offset + rect.Position) * factor + canvasHalf * (1 - factor);
                Vector2 size = rect.Size * factor;
                DrawRect(new Rect2(pos, size), Colors.Cyan, false);
            }
        }
    }
}
