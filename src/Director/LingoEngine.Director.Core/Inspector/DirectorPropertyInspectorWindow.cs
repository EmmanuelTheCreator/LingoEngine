using LingoEngine.Director.Core.Windows;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Director.Core.Gfx;

namespace LingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>
    {
        private LingoLabel? _sprite;
        private LingoLabel? _member;
        private LingoLabel? _cast;

        public string SpriteText { get => _sprite?.Text ?? string.Empty; set { if (_sprite != null) _sprite.Text = value; } }
        public string MemberText { get => _member?.Text ?? string.Empty; set { if (_member != null) _member.Text = value; } }
        public string CastText { get => _cast?.Text ?? string.Empty; set { if (_cast != null) _cast.Text = value; } }

        public record HeaderElements(LingoPanel Panel, LingoWrapPanel Header, LingoPanel ThumbPanel, DirMemberThumbnail Thumbnail, LingoWrapPanel TextContainer);

        public HeaderElements CreateHeaderElements(ILingoFrameworkFactory factory, IDirIconManager? iconManager)
        {
            var thumb = new DirMemberThumbnail(36, 36, factory, iconManager);

            var thumbPanel = factory.CreatePanel();
            thumbPanel.Margin = new LingoMargin(4, 2, 4, 2);
            thumbPanel.BackgroundColor = new LingoColor(255, 255, 255);
            thumbPanel.BorderColor = new LingoColor(64, 64, 64);
            thumbPanel.BorderWidth = 1;
            thumbPanel.AddChild(thumb.Canvas);

            var container = factory.CreateWrapPanel(LingoOrientation.Vertical);
            container.ItemMargin = new LingoMargin(0, 0, 1, 0);

            _sprite = factory.CreateLabel();
            _sprite.FontSize = 10;
            _sprite.FontColor = new LingoColor(0, 0, 0);

            _member = factory.CreateLabel();
            _member.FontSize = 10;
            _member.FontColor = new LingoColor(0, 0, 0);

            _cast = factory.CreateLabel();
            _cast.FontSize = 10;
            _cast.FontColor = new LingoColor(0, 0, 0);

            container.AddChild(_sprite);
            container.AddChild(_member);
            container.AddChild(_cast);

            var header = factory.CreateWrapPanel(LingoOrientation.Horizontal);
            header.AddChild(thumbPanel);
            header.AddChild(container);

            var panel = factory.CreatePanel();
            panel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            panel.AddChild(header);

            return new HeaderElements(panel, header, thumbPanel, thumb, container);
        }

        public override void OpenWindow()
        {
            base.OpenWindow();
        }
    }
}
