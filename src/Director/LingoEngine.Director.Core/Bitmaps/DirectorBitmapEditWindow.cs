using LingoEngine.Commands;
using LingoEngine.Director.Core.Bitmaps;
using LingoEngine.Director.Core.Pictures.Commands;
using LingoEngine.Director.Core.Windows;
using System.Reflection;

namespace LingoEngine.Director.Core.Pictures
{
    public class DirectorBitmapEditWindow : DirectorWindow<IDirFrameworkBitmapEditWindow>,
            ICommandHandler<PainterToolSelectCommand>,
            ICommandHandler<PainterDrawPixelCommand>,
            ICommandHandler<PainterFillCommand>
    {
        public bool CanExecute(PainterToolSelectCommand command) => true;

        public bool Handle(PainterToolSelectCommand command) => Framework.SelectTheTool(command.Tool);

        public bool CanExecute(PainterDrawPixelCommand command) => true;

        public bool Handle(PainterDrawPixelCommand command) => Framework.DrawThePixel(command.X,command.Y);

        public bool CanExecute(PainterFillCommand command) => false;

        public bool Handle(PainterFillCommand command) => true;
    }
}
