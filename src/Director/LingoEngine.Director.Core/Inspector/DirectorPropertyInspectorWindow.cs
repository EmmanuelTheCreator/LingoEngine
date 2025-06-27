using LingoEngine.Director.Core.Windows;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Director.Core.Styles;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Director.Core.Icons;
using LingoEngine.Core;
using LingoEngine.Commands;
using LingoEngine.Members;
using LingoEngine.Sprites;
using LingoEngine.Texts;
using LingoEngine.Pictures;
using LingoEngine.Sounds;
using LingoEngine.Movies;
using LingoEngine.Director.Core.Gfx;
using LingoEngine.Director.Core.Windowing.Commands;
using System;

namespace LingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>
    {
        private LingoGfxLabel? _sprite;
        private LingoGfxLabel? _member;
        private LingoGfxLabel? _cast;
        private LingoPlayer? _player;
        private ILingoCommandManager? _commandManager;
        private LingoGfxTabContainer? _tabs;
        private DirectorMemberThumbnail? _thumb;

        public void Setup(LingoPlayer player, ILingoCommandManager commandManager, LingoGfxTabContainer tabs, DirectorMemberThumbnail thumb)
        {
            _player = player;
            _commandManager = commandManager;
            _tabs = tabs;
            _thumb = thumb;
        }

        public LingoGfxWrapPanel BuildProperties(ILingoFrameworkFactory factory, object obj)
        {
            var root = factory.CreateWrapPanel(LingoOrientation.Vertical, $"{obj.GetType().Name}Props");
            foreach (var prop in obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!prop.CanRead)
                    continue;
                if (!IsSimpleType(prop.PropertyType) && prop.PropertyType != typeof(LingoPoint))
                    continue;

                var row = factory.CreateWrapPanel(LingoOrientation.Horizontal, prop.Name + "Row");
                var label = factory.CreateLabel(prop.Name + "Label", prop.Name);
                label.Width = 80;
                row.AddChild(label);

                object? val = prop.GetValue(obj);

                if (prop.PropertyType == typeof(bool))
                {
                    var cb = factory.CreateInputCheckbox(prop.Name + "Check");
                    cb.Checked = val is bool b && b;
                    if (prop.CanWrite)
                        cb.ValueChanged += () => prop.SetValue(obj, cb.Checked);
                    else
                        cb.Enabled = false;
                    row.AddChild(cb);
                }
                else if (prop.PropertyType == typeof(LingoPoint))
                {
                    var point = val is LingoPoint p ? p : new LingoPoint();
                    var xSpin = factory.CreateSpinBox(prop.Name + "X");
                    var ySpin = factory.CreateSpinBox(prop.Name + "Y");
                    xSpin.Value = point.X;
                    ySpin.Value = point.Y;
                    if (prop.CanWrite)
                    {
                        xSpin.ValueChanged += () =>
                        {
                            var pVal = (LingoPoint)prop.GetValue(obj)!;
                            pVal.X = xSpin.Value;
                            prop.SetValue(obj, pVal);
                        };
                        ySpin.ValueChanged += () =>
                        {
                            var pVal = (LingoPoint)prop.GetValue(obj)!;
                            pVal.Y = ySpin.Value;
                            prop.SetValue(obj, pVal);
                        };
                    }
                    else
                    {
                        xSpin.Enabled = false;
                        ySpin.Enabled = false;
                    }
                    row.AddChild(xSpin);
                    row.AddChild(ySpin);
                    root.AddChild(row);
                    continue;
                }
                else
                {
                    var text = factory.CreateInputText(prop.Name + "Text");
                    text.Text = val?.ToString() ?? string.Empty;
                    if (prop.CanWrite)
                        text.ValueChanged += () =>
                        {
                            try
                            {
                                prop.SetValue(obj, ConvertTo(text.Text, prop.PropertyType));
                            }
                            catch { }
                        };
                    else
                        text.Enabled = false;
                    row.AddChild(text);
                }

                root.AddChild(row);
            }
            return root;
        }

        public void ShowObject(object obj)
        {
            if (_player == null || _tabs == null || _thumb == null)
                return;
            _tabs.ClearTabs();
            ILingoMember? member = null;
            if (obj is LingoSprite sp)
            {
                member = sp.Member;
                if (member != null)
                {
                    _thumb.SetMember(member);
                    SpriteText = $"Sprite : {sp.SpriteNum}: {member.Type}";
                }
            }
            else if (obj is ILingoMember m)
            {
                member = m;
                _thumb.SetMember(member);
                SpriteText = member.Type.ToString();
            }
            if (member != null)
            {
                MemberText = member.Name;
                CastText = GetCastName(member);
            }
            switch (obj)
            {
                case LingoSprite sp2:
                    AddTab("Sprite", sp2);
                    if (sp2.Member != null)
                        AddMemberTabs(sp2.Member);
                    break;
                case ILingoMember member2:
                    AddMemberTabs(member2);
                    break;
                default:
                    AddTab(obj.GetType().Name, obj);
                    break;
            }
        }

        private void AddMemberTabs(ILingoMember member)
        {
            AddTab("Member", member);
            switch (member)
            {
                case LingoMemberText text:
                    AddTab("Text", text);
                    break;
                case LingoMemberBitmap pic:
                    AddTab("Picture", pic);
                    break;
                case LingoMemberSound sound:
                    AddTab("Sound", sound);
                    break;
                case LingoMemberFilmLoop film:
                    AddTab("FilmLoop", film);
                    break;
            }
        }

        private void AddTab(string name, object obj)
        {
            if (_player == null || _tabs == null)
                return;

            var scroller = _player.Factory.CreateScrollContainer(name + "Scroll");
            var container = _player.Factory.CreateWrapPanel(LingoOrientation.Vertical, name + "Container");

            // TODO: edit button and behavior list

            var props = BuildProperties(_player.Factory, obj);
            container.AddChild(props);

            scroller.AddChild(container);
            _tabs.AddTab(new LingoGfxTabItem(name, scroller));
        }

        public LingoGfxWrapPanel BuildBehaviorPanel(ILingoFrameworkFactory factory, LingoSpriteBehavior behavior)
        {
            var container = factory.CreateWrapPanel(LingoOrientation.Vertical, "BehaviorPanel");
            var propsPanel = BuildProperties(factory, behavior);
            container.AddChild(propsPanel);
            if (behavior is ILingoPropertyDescriptionList descProvider)
            {
                string? desc = descProvider.GetBehaviorDescription();
                if (!string.IsNullOrEmpty(desc))
                    container.AddChild(factory.CreateLabel("DescLabel", desc));

                var props = behavior.UserProperties;
                if (props.Count > 0)
                {
                    container.AddChild(factory.CreateLabel("PropsLabel", "Properties"));
                    foreach (var item in props)
                    {
                        string labelText = item.Key.ToString();
                        if (props.DescriptionList != null && props.DescriptionList.TryGetValue(item.Key, out var desc2) && !string.IsNullOrEmpty(desc2.Comment))
                            labelText = desc2.Comment!;
                        var row = factory.CreateWrapPanel(LingoOrientation.Horizontal, "BehPropRow");
                        row.AddChild(factory.CreateLabel("PropName", labelText));
                        row.AddChild(factory.CreateLabel("PropVal", item.Value?.ToString() ?? string.Empty));
                        container.AddChild(row);
                    }
                }
            }
            return container;
        }

        private string GetCastName(ILingoMember m)
        {
            if (_player?.ActiveMovie is ILingoMovie movie)
                return movie.CastLib.GetCast(m.CastLibNum).Name;
            return string.Empty;
        }

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

        private static bool IsSimpleType(Type t)
        {
            return t.IsPrimitive || t == typeof(string) || t.IsEnum || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }

        private static object ConvertTo(string text, Type t)
        {
            if (t == typeof(string)) return text;
            if (t.IsEnum) return Enum.Parse(t, text);
            return Convert.ChangeType(text, t);
        }
    }
}
