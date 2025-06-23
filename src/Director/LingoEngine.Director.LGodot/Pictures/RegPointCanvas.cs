using Godot;

namespace LingoEngine.Director.LGodot.Pictures;

internal partial class DirGodotPictureMemberEditorWindow
{
    private partial class RegPointCanvas : Control
    {
        private readonly DirGodotPictureMemberEditorWindow _owner;
        public RegPointCanvas(DirGodotPictureMemberEditorWindow owner)
        {
            _owner = owner;
            MouseFilter = MouseFilterEnum.Ignore;
        }

        public override void _Draw()
        {
            if (!_owner._showRegPoint) return;
            var member = _owner._member;
            if (member == null || _owner._imageRect.Texture == null) return;

            Vector2 areaSize = Size;
            float factor = _owner._scale;
            Vector2 canvasHalf = _owner._centerContainer.CustomMinimumSize / 2f;
            Vector2 imageHalf = _owner._imageRect.CustomMinimumSize / 2f;
            Vector2 offset = canvasHalf - imageHalf;
            // RegPoint origin is the texture's top-left corner
            Vector2 pos = (offset + new Vector2(member.RegPoint.X, member.RegPoint.Y)) * factor + canvasHalf * (1 - factor);

            DrawLine(new Vector2(pos.X, 0), new Vector2(pos.X, areaSize.Y), Colors.Red);
            DrawLine(new Vector2(0, pos.Y), new Vector2(areaSize.X, pos.Y), Colors.Red);
        }

        

    }
}
