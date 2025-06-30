using Godot;
using LingoEngine.Gfx;
using LingoEngine.LGodot.Styles;
using LingoEngine.Primitives;

namespace LingoEngine.LGodot.Gfx
{
    /// <summary>
    /// Godot implementation of <see cref="ILingoFrameworkGfxTabContainer"/>.
    /// </summary>
    public partial class LingoGodotTabContainer : TabContainer, ILingoFrameworkGfxTabContainer, IDisposable
    {
        private LingoMargin _margin = LingoMargin.Zero;
        private Theme _theme;
        private readonly List<ILingoFrameworkGfxTabItem> _nodes = new List<ILingoFrameworkGfxTabItem>();
        private readonly ILingoGodotStyleManager _lingoGodotStyleManager;

        public float X { get => Position.X; set => Position = new Vector2(value, Position.Y); }
        public float Y { get => Position.Y; set => Position = new Vector2(Position.X, value); }
        public float Width { get => Size.X; set => Size = new Vector2(value, Size.Y); }
        public float Height { get => Size.Y; set => Size = new Vector2(Size.X, value); }
        public bool Visibility { get => Visible; set => Visible = value; }
        string ILingoFrameworkGfxNode.Name { get => Name; set => Name = value; }

        public object FrameworkNode => this;


        public LingoGodotTabContainer(LingoGfxTabContainer tab, ILingoGodotStyleManager lingoGodotStyleManager)
        {
            tab.Init(this);
            SizeFlagsVertical = SizeFlags.ExpandFill;
            SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _lingoGodotStyleManager = lingoGodotStyleManager;

            // Apply the Director tab styling
            _theme = _lingoGodotStyleManager.GetTheme(LingoGodotThemeElementType.Tabs) ?? new Theme();
            
            Theme = Theme;
            ClipTabs = false;

        }
 



        public LingoMargin Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                AddThemeConstantOverride("margin_left", (int)_margin.Left);
                AddThemeConstantOverride("margin_right", (int)_margin.Right);
                AddThemeConstantOverride("margin_top", (int)_margin.Top);
                AddThemeConstantOverride("margin_bottom", (int)_margin.Bottom);
            }
        }

        
       

        public void AddTab(ILingoFrameworkGfxTabItem tabItem)
        {
            var content = ((LingoGodotTabItem)tabItem).ContentFrameWork.FrameworkNode;
            if (content is Control node)
                AddTab(tabItem.Title, node);

            _nodes.Add(tabItem);
        }

        public void RemoveTab(ILingoFrameworkGfxTabItem tabItem)
        {
            var content = ((LingoGodotTabItem)tabItem).ContentFrameWork.FrameworkNode;
            if (content is Node node)
                RemoveChild(node);
            _nodes.Remove(tabItem);
        }

        public IEnumerable<ILingoFrameworkGfxTabItem> GetTabs() => _nodes.ToArray();
        public void AddTab(string title, Node node)
        {
            AddChild(node);
            SetTabTitle(GetChildCount() - 1, title);
        }

        public void ClearTabs()
        {
            foreach (Node child in GetChildren())
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }

        public new void Dispose()
        {
            QueueFree();
            base.Dispose();
        }

        
    }
    public partial class LingoGodotTabItem : ILingoFrameworkGfxTabItem
    {
        private string _name;
        private LingoGfxTabItem _tabItem;
        public LingoGfxTabItem TabItem => _tabItem;
        public ILingoFrameworkGfxLayoutNode ContentFrameWork => (Content?.FrameworkObj as ILingoFrameworkGfxLayoutNode)!;
        public ILingoGfxNode? Content { get; set; }

        public float X { get => ContentFrameWork.X; set => ContentFrameWork.X = value; }
        public float Y { get => ContentFrameWork.Y; set => ContentFrameWork.Y = value; }
        public float Width { get => ContentFrameWork.Width; set => ContentFrameWork.Width = value; }
        public float Height { get => ContentFrameWork.Height; set => ContentFrameWork.Height = value; }
        public bool Visibility { get => ContentFrameWork.Visibility; set => ContentFrameWork.Visibility = value; }
        public string Title { get; set; }
        public LingoMargin Margin { get => ContentFrameWork.Margin; set => ContentFrameWork.Margin = value; }
        string ILingoFrameworkGfxNode.Name
        {
            get => ContentFrameWork.Name; set
            {
                if (ContentFrameWork != null) 
                    ContentFrameWork.Name = value;
            }
        }

        public object FrameworkNode => Content?.FrameworkObj.FrameworkNode!;

        public LingoGodotTabItem(LingoGfxTabItem tab)
        {
            tab.Init(this);
            _tabItem = tab;
        }

        public void Dispose()
        {
        }
    }
}
