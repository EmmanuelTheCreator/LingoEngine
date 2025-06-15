using Director.Graphics;
using Director.Primitives;

namespace Director
{
    public class StageWindow
    {
        private Surface _surface = new Surface();

        /// <summary>
        /// Gets the rendering surface.
        /// </summary>
        public Surface GetSurface() => _surface;

        /// <summary>
        /// Resizes the stage's internal resolution.
        /// </summary>
        public void ResizeInner(int width, int height)
        {
            var currentFormat = _surface.Format;
            _surface.Create(width, height, currentFormat);
        }

        /// <summary>
        /// Sets the background stage color.
        /// </summary>
        public void SetStageColor(LingoColor color, bool immediate)
        {
            // Optional: clear Pixels[] with the color
            // Here we assume 8-bit indexed color for simplicity
            if (_surface.Format.BytesPerPixel == 1)
            {
                byte index = 0; // You can map LingoColor to a palette index if needed
                Array.Fill(_surface.Pixels, index);
            }
            else if (_surface.Format.BytesPerPixel == 4)
            {
                uint colorValue = (uint)((color.R << 16) | (color.G << 8) | color.B);
                for (int y = 0; y < _surface.Height; y++)
                    for (int x = 0; x < _surface.Width; x++)
                        _surface.SetUInt32(x, y, colorValue);
            }
        }

        /// <summary>
        /// Repositions the window on screen.
        /// </summary>
        public void Center(object centerStage)
        {
            // This stub simply resets the surface's origin. Real window
            // managers would move the native window.
            // centerStage is ignored for now.
        }
    }

}
