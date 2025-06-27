using LingoEngine.Director.Core.Windows;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Icons;

namespace LingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>
    {
        private LingoGfxLabel? _sprite;
        private LingoGfxLabel? _member;
        private LingoGfxLabel? _cast;

        public string SpriteText { get => _sprite?.Text ?? string.Empty; set { if (_sprite != null) _sprite.Text = value; } }
        public string MemberText { get => _member?.Text ?? string.Empty; set { if (_member != null) _member.Text = value; } }
        public string CastText { get => _cast?.Text ?? string.Empty; set { if (_cast != null) _cast.Text = value; } }

        public record HeaderElements(LingoGfxPanel Panel, LingoGfxWrapPanel Header,DirectorMemberThumbnail Thumbnail);

        public HeaderElements CreateHeaderElements(ILingoFrameworkFactory factory, IDirectorIconManager? iconManager)
        {
            var thumb = new DirectorMemberThumbnail(36, 36, factory, iconManager);

            var thumbPanel = factory.CreatePanel("ThumbPanel");
            thumbPanel.Margin = new LingoMargin(4, 2, 4, 2);
            thumbPanel.BackgroundColor = new LingoColor(255, 255, 255);
            thumbPanel.BorderColor = new LingoColor(64, 64, 64);
            thumbPanel.BorderWidth = 1;
            thumbPanel.AddChild(thumb.Canvas);

            var container = factory.CreateWrapPanel(LingoOrientation.Vertical, "InfoContainer");
            container.ItemMargin = new LingoMargin(0, 0, 1, 0);
            // Center the labels within the header panel
            container.Margin = new LingoMargin(0, 7, 0, 0);

            _sprite = factory.CreateLabel("SpriteLabel");
            _sprite.FontSize = 10;
            _sprite.FontColor = new LingoColor(0, 0, 0);

            _member = factory.CreateLabel("MemberLabel");
            _member.FontSize = 10;
            _member.FontColor = new LingoColor(0, 0, 0);

            _cast = factory.CreateLabel("CastLabel");
            _cast.FontSize = 10;
            _cast.FontColor = new LingoColor(0, 0, 0);

            container.AddChild(_sprite);
            container.AddChild(_member);
            container.AddChild(_cast);

            var header = factory.CreateWrapPanel(LingoOrientation.Horizontal, "HeaderPanel");
            header.AddChild(thumbPanel);
            header.AddChild(container);

            var panel = factory.CreatePanel("RootPanel");
            panel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            panel.AddChild(header);

            return new HeaderElements(panel, header, thumb);
        }

        public override void OpenWindow()
        {
            base.OpenWindow();
        }
    }
}
