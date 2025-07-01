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
using System.Numerics;
using LingoEngine.Director.Core.UI;
using LingoEngine.Tools;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations;

namespace LingoEngine.Director.Core.Inspector
{
    public class DirectorPropertyInspectorWindow : DirectorWindow<IDirFrameworkPropertyInspectorWindow>, IHasSpriteSelectedEvent, IHasMemberSelectedEvent
    {
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
        private readonly IDirectorBehaviorDescriptionManager _descriptionManager;
        private LingoGfxWrapPanel _behaviorPanel;
        private LingoGfxWrapPanel _behaviorDetail;
        private float _lastWidh;
        private float _lastHeight;
        private Dictionary<string, LingoSpriteBehavior> _behaviors = new();
        private LingoGfxItemList _behaviorList;

        public LingoGfxPanel HeaderPanel => _headerPanel;
        public LingoGfxTabContainer Tabs => _tabs;
        public string SpriteText { get => _sprite?.Text ?? string.Empty; set { if (_sprite != null) _sprite.Text = value; } }
        public string MemberText { get => _member?.Text ?? string.Empty; set { if (_member != null) _member.Text = value; } }
        public string CastText { get => _cast?.Text ?? string.Empty; set { if (_cast != null) _cast.Text = value; } }

        public record HeaderElements(LingoGfxPanel Panel, LingoGfxWrapPanel Header, DirectorMemberThumbnail Thumbnail);

        public DirectorPropertyInspectorWindow(LingoPlayer player, ILingoCommandManager commandManager, ILingoFrameworkFactory factory, IDirectorIconManager iconManager, IDirectorEventMediator mediator, IDirectorBehaviorDescriptionManager descriptionManager)
        {
            _player = player;
            _commandManager = commandManager;
            _factory = factory;
            _iconManager = iconManager;
            _mediator = mediator;
            _descriptionManager = descriptionManager;
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
            _tabs.Width = width - 10;
            _tabs.Height = height - 30 - HeaderHeight;
            _behaviorList.Width = _lastWidh - 15;
            _behaviorDetail.Width = _lastWidh - 15;
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
                    SpriteText = $"Sprite {sp.SpriteNum}: {member.Type}";
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
                MemberText = $"{member.NumberInCast}. {member.Name}";
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
            AddMemberTab(member);
            switch (member)
            {
                case LingoMemberText text:
                    AddTab("Text", text);
                    break;
                case LingoMemberBitmap pic:
                    AddBitmapTab(pic);
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
            CreateBehaviorPanel();
            var wrapContainer = AddTab("Sprite");
            var containerIcons = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "SpriteDetailIcons");
            var container = _factory.CreatePanel("SpriteDetailPanel");
            

            containerIcons.Margin = new LingoMargin(5, 5, 5, 5);
            containerIcons.Compose(_factory)
                .AddStateButton("SpriteLock", sprite, _iconManager.Get(DirectorIcon.Lock),c => c.Lock)
                .AddStateButton("SpriteFlipH", sprite, _iconManager.Get(DirectorIcon.FlipHorizontal), c => c.FlipH,"")
                .AddStateButton("SpriteFlipV", sprite, _iconManager.Get(DirectorIcon.FlipVertical), c => c.FlipV)
                ;

            container.Compose(_factory)
                   .Columns(4)
                   .AddTextInput("SpriteName", "Name:", sprite, s => s.Name, inputSpan: 3)
                   .Columns(8)
                   .AddNumericInput("SpriteLocH", "X:", sprite, s => s.LocH)
                   .AddNumericInput("SpriteLocV", "Y:", sprite, s => s.LocV, inputSpan: 5)
                   .AddNumericInput("SpriteLeft", "L:", sprite, s => s.Left)
                   .AddNumericInput("SpriteTop", "T:", sprite, s => s.Top)
                   .AddNumericInput("SpriteRight", "R:", sprite, s => s.Right)
                   .AddNumericInput("SpriteBottom", "B:", sprite, s => s.Bottom)
                   .AddNumericInput("SpriteWidth", "W:", sprite, s => s.Width)
                   .AddNumericInput("SpriteHeight", "H:", sprite, s => s.Height, inputSpan: 5)
                   .AddEnumInput<LingoInkType>("SpriteInk", "Ink:", sprite, s => s.Ink, inputSpan: 6)
                   .AddNumericInput("SpriteBlend", "%", sprite, s => s.Blend, showLabel: false)
                   .AddNumericInput("SpriteBeginFrame", "StartFrame:", sprite, s => s.BeginFrame, labelSpan: 3)
                   .AddNumericInput("SpriteEndFrame", "End:", sprite, s => s.EndFrame, inputSpan: 1, labelSpan: 3)
                   .AddNumericInput("SpriteRotation", "Rotation:", sprite, s => s.Rotation, labelSpan: 3)
                   .AddNumericInput("SpriteSkew", "Skew:", sprite, s => s.Skew, inputSpan: 1, labelSpan: 3)
                   .AddColorInput("SpriteForeColor", "Foreground:", sprite, s => s.ForeColor, inputSpan: 1, labelSpan: 3)
                   .AddColorInput("SpriteBackColor", "Background:", sprite, s => s.BackColor, inputSpan: 1, labelSpan: 3)
                   .Finalize();
                   ;
            var index = 0;
            _behaviors = sprite.Behaviors.ToDictionary(b =>
            {
                index++;
                return index + "." + b.Name;
            });
            _behaviorList.ClearItems();
            foreach (var item in _behaviors)
                _behaviorList.AddItem(item.Key, item.Value.Name);
            

            wrapContainer
                .AddItem(containerIcons)
                .AddHLine(_factory, "SpriteSplitterIconHLine", _lastWidh - 10, 5)
                .AddItem(container)
                .AddHLine(_factory, "SpriteSplitterIconHLine", _lastWidh - 10, 5)
                .AddItem(_behaviorPanel)
                ;
        }

        private void CreateBehaviorPanel()
        {
            _behaviorPanel = _factory.CreateWrapPanel(LingoOrientation.Vertical, "InspectorBehaviors");
            _behaviorDetail = _factory.CreateWrapPanel(LingoOrientation.Vertical, "InspectorBehaviorsWrapPanel");

            _behaviorList = _factory.CreateItemList("BehaviorList", x =>
            {
                if (x != null && _behaviors.TryGetValue(x, out var behavior))
                {
                    _behaviorDetail.Visibility = true;
                    ShowBehavior(behavior);
                }
                else
                    _behaviorDetail.Visibility = false;
            });
            _behaviorList.Height = 45;
            _behaviorList.Width = _lastWidh - 15;
            _behaviorDetail.Width = _lastWidh - 15;
            _behaviorList.Margin = new LingoMargin(5,0,0,0);
            _behaviorPanel.AddItem(_behaviorList);
            _behaviorPanel.AddItem(_behaviorDetail);
            _behaviorDetail.Visibility = false;
      
        }
        private void ShowBehavior(LingoSpriteBehavior behavior)
        {
            _behaviorDetail.RemoveAll();
            
            var panel = _descriptionManager.BuildBehaviorPanel(behavior);
            _behaviorDetail.AddItem(panel);
            OnResizing(_lastWidh, _lastHeight);
        }

        private void AddMemberTab(ILingoMember member)
        {
            var wrapContainer = AddTab("Member");
            var container = _factory.CreatePanel("MemberDetailPanel");
            wrapContainer
                .AddItem(container)
                ;

            container.Compose(_factory)
                   .Columns(4)
                   .AddTextInput("MemberName", "Name:", member, s => s.Name, inputSpan: 3)
                   .Columns(4)
                   .AddLabel("MemberSize","Size: ",2)
                   .AddLabel("MemberSizeV", CommonExtensions.BytesToShortString(member.Size),2)
                   .AddLabel("MemberCreationDate","Created: ",2)
                   .AddLabel("MemberCreationDateV",member.CreationDate.ToString("dd/MM/yyyy hh:mm"),2)
                   .AddLabel("MemberModifyDate","Modified: ",2)
                   .AddLabel("MemberModifyDateV",member.ModifiedDate.ToString("dd/MM/yyyy hh:mm"),2)
                   .Columns(4)
                   .AddTextInput("MemberFileName", "FileName:", member, s => s.FileName, inputSpan: 3)
                   .Columns(4)
                   .AddTextInput("MemberComments", "Comments:", member, s => s.Comments, inputSpan: 3)
                   .Finalize()
                   ;
        } 
        private void AddBitmapTab(LingoMemberBitmap member)
        {
            var wrapContainer = AddTab("Bitmap");
            var container = _factory.CreatePanel("MemberDetailPanel");
            wrapContainer
                .AddItem(container)
                ;

            container.Compose(_factory)
                   .Columns(4)
                   .AddLabel("BitmapSize","Dimensions: ",2)
                   .AddLabel("BitmapSizeV", member.Width + " x " + member.Height,2) 
                   .AddCheckBox("BitmapHighLight", "Hightlight: ",member,x => x.Hilite,2,true,2)
                   //.AddLabel("BitmapBitDepth", "BitDepth: ", 2)
                   //.AddLabel("BitmapBitDepthV", member.ColorDepth,2)
                   .Columns(8)
                   .AddNumericInput("BitmapRegPointX", "RegPoint X:", member, s => s.RegPoint.X, inputSpan: 1, labelSpan: 3)
                   .AddNumericInput("BitmapRegPointY", "Y:", member, s => s.RegPoint.Y, inputSpan: 4, labelSpan: 1)
                   .Finalize()
                   ;
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

            scroller.AddItem(container);
            var tabItem = _factory.CreateTabItem(name, name);
            tabItem.Content = scroller;
            _tabs.AddTab(tabItem);
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

  


       
    }
}
