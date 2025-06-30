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
                Vector2 size = ((Vector2)rect.Size) * factor;
                DrawRect(new Rect2(pos, size), Colors.Cyan, false);
            }
            else if (_owner._lassoSelecting && _owner._lassoPoints.Count > 1)
            {
                for (int i = 0; i < _owner._lassoPoints.Count - 1; i++)
                {
                    Vector2 p1 = (offset + _owner._lassoPoints[i]) * factor + canvasHalf * (1 - factor);
                    Vector2 p2 = (offset + _owner._lassoPoints[i + 1]) * factor + canvasHalf * (1 - factor);
                    DrawLine(p1, p2, Colors.Cyan);
                }
            }
        }
    }
}
