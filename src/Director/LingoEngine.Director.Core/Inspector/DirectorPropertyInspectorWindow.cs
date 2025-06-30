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
using LingoEngine.Director.Core.Events;
using LingoEngine.Director.Core.Sprites;
using LingoEngine.Director.Core.Tools;
using System.Drawing;
using System.Numerics;
using System.Xml.Linq;
using LingoEngine.Director.Core.UI;
using System.ComponentModel;

namespace LingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>, IHasSpriteSelectedEvent, IHasMemberSelectedEvent
    {
        private bool _behaviorVisible;
        public const int HeaderHeight = 44;
        private LingoGfxLabel? _sprite;
        private LingoGfxLabel? _member;
        private LingoGfxLabel? _cast;
        private LingoPlayer _player;
        private ILingoCommandManager? _commandManager;
        private LingoGfxTabContainer _tabs;
        private DirectorMemberThumbnail? _thumb;
        private LingoGfxPanel? _header;
        private ILingoFrameworkFactory _factory;
        private IDirectorIconManager _iconManager;
        private LingoGfxPanel _headerPanel;
        private IDirectorEventMediator _mediator;

        private LingoGfxPanel _behaviorPanel;
        private LingoGfxWrapPanel _behaviorBox;
        private LingoGfxButton _behaviorClose;
        private int _titleBarHeight;
        private float _lastWidh;
        private float _lastHeight;

        public LingoGfxPanel HeaderPanel => _headerPanel;
        public LingoGfxTabContainer Tabs => _tabs;
        public LingoGfxPanel BehaviorPanel => _behaviorPanel;
        public string SpriteText { get => _sprite?.Text ?? string.Empty; set { if (_sprite != null) _sprite.Text = value; } }
        public string MemberText { get => _member?.Text ?? string.Empty; set { if (_member != null) _member.Text = value; } }
        public string CastText { get => _cast?.Text ?? string.Empty; set { if (_cast != null) _cast.Text = value; } }

        public record HeaderElements(LingoGfxPanel Panel, LingoGfxWrapPanel Header, DirectorMemberThumbnail Thumbnail);

        public DirectorPropertyInspectorWindow(LingoPlayer player, ILingoCommandManager commandManager, ILingoFrameworkFactory factory, IDirectorIconManager iconManager, IDirectorEventMediator mediator)
        {
            _player = player;
            _commandManager = commandManager;
            _factory = factory;
            _iconManager = iconManager;
            _mediator = mediator;
            _mediator.Subscribe(this);
        }
        public override void Dispose()
        {
            base.Dispose();
            _mediator.Unsubscribe(this);
        }
        public void Init(IDirFrameworkWindow frameworkWindow, float width, float height, int titleBarHeight)
        {
            base.Init(frameworkWindow);
            _lastHeight = height;
            _lastWidh = width;
            _titleBarHeight = titleBarHeight;
            CreateHeaderElements();
            _tabs = _factory.CreateTabContainer("InspectorTabs");
            CreateBehaviorPanel();
        }

      
        private LingoGfxPanel CreateHeaderElements()
        {
            var thumb = new DirectorMemberThumbnail(36, 36, _factory, _iconManager);

            var thumbPanel = _factory.CreatePanel("ThumbPanel");
            thumbPanel.X = 4;
            thumbPanel.Y = 2;
            thumbPanel.BackgroundColor = DirectorColors.Bg_Thumb;
            thumbPanel.BorderColor = DirectorColors.Border_Thumb;
            thumbPanel.BorderWidth = 1;
            thumbPanel.AddItem(thumb.Canvas);
            _thumb = thumb;
            var lineHeight = 11;

            var container = _factory.CreatePanel("InfoContainer");
            container.X = 50;

            _sprite = container.SetLabelAt(_factory, "SpriteLabel", 0, 0);
            _member = container.SetLabelAt(_factory, "MemberLabel", 0, 13);
            _cast = container.SetLabelAt(_factory, "MemberLabel", 0, 26);


            var header = _factory.CreatePanel("HeaderPanel");
            header.AddItem(thumbPanel);
            header.AddItem(container);


            _headerPanel = _factory.CreatePanel("RootHeaderPanel");
            _headerPanel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _headerPanel.AddItem(header);
            _headerPanel.Height = HeaderHeight;
            _header = header;
            return _headerPanel;
        }

        

        private void CreateBehaviorPanel()
        {
            _behaviorPanel = _factory.CreatePanel("InspectorTabs");
            _behaviorBox = _factory.CreateWrapPanel(LingoOrientation.Vertical , "InspectorTabs");
            _behaviorClose = _factory.CreateButton("InspectorTabs");

            
            _behaviorPanel.AddItem(_behaviorBox);
            _behaviorPanel.Visibility = false;
            //var closeRow = new HBoxContainer();
            //closeRow.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
            //_behaviorClose.Text = "X";
            //_behaviorClose.Modulate = Colors.Red;
            //_behaviorClose.CustomMinimumSize = new Vector2(12, 12);
            //_behaviorClose.Pressed += () => { _behaviorPanel.Visible = false; OnResizing(Size); };
            //closeRow.AddChild(_behaviorClose);
            //_behaviorBox.AddChild(closeRow);
        }
        private void ShowBehavior(LingoSpriteBehavior behavior)
        {
            foreach (var child in _behaviorBox.GetItems())
            {
                if (child != _behaviorBox.GetItem(0))
                    _behaviorBox.RemoveItem(child);
            }
            var panel = BuildBehaviorPanel(behavior);
            _behaviorBox.AddItem(panel);
            _behaviorPanel.Visibility = true;
            OnResizing(_lastWidh, _lastHeight);
        }


        public void SpriteSelected(ILingoSprite sprite) => ShowObject(sprite);
        public void MemberSelected(ILingoMember member) => ShowObject(member);

        public void OnResizing(float width, float height)
        {
            _lastWidh = width;
            _lastHeight = height;
            if (_tabs == null || _header == null)
                return;

            _header.Width = width - 10;
            _header.Height = HeaderHeight;

            _tabs.X = 0;
            _tabs.Y = _titleBarHeight + HeaderHeight;

            if (_behaviorVisible)
            {
                var half = (height - 30 - HeaderHeight) / 2f;
                _tabs.Width = width - 10;
                _tabs.Height = half;
            }
            else
            {
                _tabs.Width = width - 10;
                _tabs.Height = height - 30 - HeaderHeight;
                
            }

            _behaviorPanel.X = 0; 
            _behaviorPanel.Y = _tabs.Height;
            _behaviorPanel.Width = width;
            _behaviorPanel.Height = _tabs.Height;
        }

       

        public void ShowObject(object obj)
        {
            if (_tabs == null || _thumb == null)
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
                    AddSpriteTab(sp2);
                    //AddTab("Sprite", sp2);
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

        private void AddSpriteTab(LingoSprite sprite)
        {
            var wrapContainer = AddTab(sprite.Name);

            var containerIcons = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "SpriteDetailIcons");
            wrapContainer.AddItem(containerIcons);


            var container = _factory.CreatePanel("SpriteDetailPanel");
            wrapContainer.AddItem(container);


            var lineHeight  = 22;
            var marginLeft = 5;
            var defaultSmallWidth = 30;
            var quadX1 = 33;
            var quadX2 = 88;
            var doubleX2 = 70;
            var doubleX3 = 130;
            var doubleX4 = 180;


            
            container.SetLabelAt(_factory, "SpriteLabel", marginLeft, 2,"Name:");
            container.SetInputTextAt(_factory, sprite, "SpriteNameInput", 60, 0,140, x => x.Name);

            // Row 2: X / Y
            container.SetLabelAt(_factory, "SpriteX", 5, lineHeight * 1, "X:");
            container.SetInputNumberAt(_factory, sprite, "XInput", quadX1, lineHeight * 1, defaultSmallWidth, x => x.LocH);
            container.SetLabelAt(_factory, "SpriteY", 70, lineHeight * 1, "Y:");
            container.SetInputNumberAt(_factory, sprite, "YInput", quadX2, lineHeight * 1, defaultSmallWidth, x => x.LocV);

            // Row 3: L / T / R / B
            container.SetLabelAt(_factory, "SpriteL", 5, lineHeight * 2, "L:");
            container.SetInputNumberAt(_factory, sprite, "LInput", quadX1, lineHeight * 2, defaultSmallWidth, x => x.Left);
            container.SetLabelAt(_factory, "SpriteT", 70, lineHeight * 2, "T:");
            container.SetInputNumberAt(_factory, sprite, "TInput", quadX2, lineHeight * 2, defaultSmallWidth, x => x.Top);
            container.SetLabelAt(_factory, "SpriteR", 125, lineHeight * 2, "R:");
            container.SetInputNumberAt(_factory, sprite, "RInput", 141, lineHeight * 2, defaultSmallWidth, x => x.Right);
            container.SetLabelAt(_factory, "SpriteB", 178, lineHeight * 2, "B:");
            container.SetInputNumberAt(_factory, sprite, "BInput", 194, lineHeight * 2, defaultSmallWidth, x => x.Bottom);

            // Row 4: W / H
            container.SetLabelAt(_factory, "SpriteW", 5, lineHeight * 3, "W:");
            container.SetInputNumberAt(_factory, sprite, "WInput", quadX1, lineHeight * 3, defaultSmallWidth, x => x.Width);
            container.SetLabelAt(_factory, "SpriteH", 70, lineHeight * 3, "H:");
            container.SetInputNumberAt(_factory, sprite, "HInput", quadX2, lineHeight * 3, defaultSmallWidth, x => x.Height);

            // Row 5: Ink / %
            container.SetLabelAt(_factory, "SpriteInk", 5, lineHeight * 4, "Ink:");
            container.SetInputNumberAt(_factory, sprite, "InkCombo", quadX1, lineHeight * 4, 120, x => x.Ink);
            container.SetInputNumberAt(_factory, sprite, "OpacityCombo", 150, lineHeight * 4, 40, x => x.Blend);
            //container.SetComboBoxAt(_factory, sprite, "InkCombo", quadX1, lineHeight * 4, x => x.Ink.ToString());
            //container.SetComboBoxAt(_factory, sprite, "OpacityCombo", 110, lineHeight * 4, x => x.Blend.ToString());

            // Row 6: Start Frame / End Frame
            container.SetLabelAt(_factory, "SpriteStartFrame", 5, lineHeight * 5 + 4, "Start Frame:");
            container.SetInputNumberAt(_factory, sprite, "StartFrameInput", doubleX2, lineHeight * 5 + 4, defaultSmallWidth + 5, x => x.BeginFrame);
            container.SetLabelAt(_factory, "SpriteEndFrame", doubleX3, lineHeight * 5 + 4, "End:");
            container.SetInputNumberAt(_factory, sprite, "EndFrameInput", doubleX4, lineHeight * 5 + 4, defaultSmallWidth, x => x.EndFrame);

            // Row 7: Rotation / Skew
            container.SetLabelAt(_factory, "SpriteRotation", 5, lineHeight * 6 + 4, "Rotation:");
            container.SetInputNumberAt(_factory, sprite, "RotationInput", doubleX2, lineHeight * 6 + 4, defaultSmallWidth + 10, x => x.Rotation);
            container.SetLabelAt(_factory, "SpriteSkew", doubleX3, lineHeight * 6 + 4, "Skew:");
            container.SetInputNumberAt(_factory, sprite, "SkewInput", doubleX4, lineHeight * 6 + 4, defaultSmallWidth, x => x.Skew);



        }
        private LingoGfxWrapPanel AddTab(string name)
        { 
            var scroller = _factory.CreateScrollContainer(name + "Scroll");
            LingoGfxWrapPanel container = _factory.CreateWrapPanel(LingoOrientation.Vertical, name + "Container");
            scroller.AddItem(container);
            var tabItem = _factory.CreateTabItem(name, name);
            tabItem.Content = scroller;
            _tabs.AddTab(tabItem);
            return container;
        }


        private void AddTab(string name, object obj)
        {
            if (_tabs == null)
                return;

            var scroller = _factory.CreateScrollContainer(name + "Scroll");
            LingoGfxWrapPanel container = _factory.CreateWrapPanel(LingoOrientation.Vertical, name + "Container");

            if (_commandManager != null && (obj is LingoMemberBitmap || obj is ILingoMemberTextBase))
            {
                var editBtn = _factory.CreateButton("EditButton", "Edit");
                editBtn.Pressed += () =>
                {
                    string code = obj switch
                    {
                        LingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                        ILingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                        _ => string.Empty
                    };
                    if (!string.IsNullOrEmpty(code))
                        _commandManager.Handle(new OpenWindowCommand(code));
                };
                container.AddItem(editBtn);
            }

            // TODO: behavior list
            //if (obj as LingoSprite sprite)
            //    ShowBehavior(sprite)

            var props = BuildProperties(obj);
            container.AddItem(props);

            scroller.AddItem(container);
            var tabItem = _factory.CreateTabItem(name, name);
            tabItem.Content = scroller;
            _tabs.AddTab(tabItem);
        }

        public LingoGfxWrapPanel BuildBehaviorPanel(LingoSpriteBehavior behavior)
        {
            var container = _factory.CreateWrapPanel(LingoOrientation.Vertical, "BehaviorPanel");
            var propsPanel = BuildProperties(behavior);
            container.AddItem(propsPanel);
            if (behavior is ILingoPropertyDescriptionList descProvider)
            {
                string? desc = descProvider.GetBehaviorDescription();
                if (!string.IsNullOrEmpty(desc))
                    container.AddItem(_factory.CreateLabel("DescLabel", desc));

                var props = behavior.UserProperties;
                if (props.Count > 0)
                {
                    container.AddItem(_factory.CreateLabel("PropsLabel", "Properties"));
                    foreach (var item in props)
                    {
                        string labelText = item.Key.ToString();
                        if (props.DescriptionList != null && props.DescriptionList.TryGetValue(item.Key, out var desc2) && !string.IsNullOrEmpty(desc2.Comment))
                            labelText = desc2.Comment!;
                        var row = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "BehPropRow");
                        row.AddItem(_factory.CreateLabel("PropName", labelText));
                        row.AddItem(_factory.CreateLabel("PropVal", item.Value?.ToString() ?? string.Empty));
                        container.AddItem(row);
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


        public LingoGfxWrapPanel BuildProperties(object obj)
        {
            ILingoFrameworkFactory factory = _factory;
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
                row.AddItem(label);

                object? val = prop.GetValue(obj);

                if (prop.PropertyType == typeof(bool))
                {
                    var cb = factory.CreateInputCheckbox(prop.Name + "Check");
                    cb.Checked = val is bool b && b;
                    if (prop.CanWrite)
                        cb.ValueChanged += () => prop.SetValue(obj, cb.Checked);
                    else
                        cb.Enabled = false;
                    row.AddItem(cb);
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
                    row.AddItem(xSpin);
                    row.AddItem(ySpin);
                    root.AddItem(row);
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
                    row.AddItem(text);
                }

                root.AddItem(row);
            }
            return root;
        }
    }
}
