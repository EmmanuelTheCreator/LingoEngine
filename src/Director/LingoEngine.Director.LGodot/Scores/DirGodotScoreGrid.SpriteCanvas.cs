using Godot;
using System;

namespace LingoEngine.Director.LGodot.Scores;

internal partial class DirGodotScoreGrid
{
    private partial class SpriteCanvas : Control
    {
        private readonly DirGodotScoreGrid _owner;
        public SpriteCanvas(DirGodotScoreGrid owner) => _owner = owner;
        public override void _Draw()
        {
            if (_owner.SpritePreviewRect.HasValue)
            {
                var rect = _owner.SpritePreviewRect.Value;
                DrawRect(rect, new Color(1, 1, 1, 0.25f), filled: true);
                DrawRect(rect, new Color(1, 1, 1, 1), filled: false, width: 1);
            }

            var movie = _owner._movie;
            if (movie == null) return;

            int channelCount = movie.MaxSpriteChannelCount;
            var font = ThemeDB.FallbackFont;

            foreach (var sp in _owner._sprites)
            {
                int ch = sp.Sprite.SpriteNum - 1;
                if (ch < 0 || ch >= channelCount) continue;
                float x = _owner._gfxValues.LeftMargin + (sp.Sprite.BeginFrame - 1) * _owner._gfxValues.FrameWidth;
                float width = (sp.Sprite.EndFrame - sp.Sprite.BeginFrame + 1) * _owner._gfxValues.FrameWidth;
                float y = ch * _owner._gfxValues.ChannelHeight;
                sp.Selected = _owner.IsSpriteSelected(sp);
                sp.Draw(this, new Vector2(x, y), width, _owner._gfxValues.ChannelHeight, font);
            }

            int cur = movie.CurrentFrame - 1;
            if (cur < 0) cur = 0;
            float barX = _owner._gfxValues.LeftMargin + cur * _owner._gfxValues.FrameWidth + _owner._gfxValues.FrameWidth / 2f;
            DrawLine(new Vector2(barX, 0), new Vector2(barX, channelCount * _owner._gfxValues.ChannelHeight), Colors.Red, 2);

            if (_owner.ShowPreview)
            {
                float px = _owner._gfxValues.LeftMargin + (_owner.PreviewBegin - 1) * _owner._gfxValues.FrameWidth;
                float pw = (_owner.PreviewEnd - _owner.PreviewBegin + 1) * _owner._gfxValues.FrameWidth;
                float py = _owner.PreviewChannel * _owner._gfxValues.ChannelHeight;
                DrawRect(new Rect2(px, py, pw, _owner._gfxValues.ChannelHeight), new Color(0, 0, 1, 0.3f));
            }

        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                var clickPos = mouseEvent.Position;
                var cell = _owner.GetCellFromPosition(clickPos);

                if (cell != null)
                {
                    var ctrl = Input.IsKeyPressed(Key.Ctrl);
                    var shift = Input.IsKeyPressed(Key.Shift);

                    _owner.HandleSelection(cell.Value, ctrl, shift);
                }
            }
        }

    }

}

