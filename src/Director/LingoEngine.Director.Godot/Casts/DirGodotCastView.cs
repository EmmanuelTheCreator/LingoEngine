using Godot;
using LingoEngine.Core;
using LingoEngine.Director.Core.Casts;
using LingoEngine.Movies;

namespace LingoEngine.Director.Godot.Casts
{
    internal class DirGodotCastViewer : IDisposable
    {
        private readonly TabContainer _tabs;
        private readonly DirGodotCastMemberPropViewer _selectedItem;
        private readonly Node2D _castsViewerNode2D;
        
        private readonly ILingoMovie _lingoMovie;

        public ILingoCast? ActiveCastLib { get; private set; }
        public DirGodotCastView CastLibViewer { get; }

        public DirGodotCastViewer(Node2D parent,ILingoMovie lingoMovie)
        {
            _castsViewerNode2D = new Node2D();
            _castsViewerNode2D.Position = new Vector2(650, 20);

            

            _tabs = new TabContainer();
            _tabs.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _tabs.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _tabs.Size = new Vector2(350, 600);
            _castsViewerNode2D.AddChild(_tabs);

            parent.AddChild(_castsViewerNode2D);
            _lingoMovie = lingoMovie;
            foreach (var cast in lingoMovie.CastLib.GetAll())
            {
                var castLibViewer = new DirGodotCastView(OnSelectElement);
                castLibViewer.Show(cast);
                var tabContent = new VBoxContainer
                {
                    Name = cast.Name,
                };
                
                tabContent.AddChild(castLibViewer.Node);
                _tabs.AddChild(tabContent);
            }
            _selectedItem = new DirGodotCastMemberPropViewer();
            _selectedItem.Position = new Vector2(0, 620);
            _castsViewerNode2D.AddChild(_selectedItem);
        }
        private void OnSelectElement(DirGodotCastItem castItem)
        {
            _selectedItem.Load(castItem.LingoMember);
        }
        public void Activate(int castlibNum)
        {
            //ActiveCastLib = _lingoMovie.CastLib.GetCast(castlibNum);
            //if (ActiveCastLib != null)
            //    CastLibViewer.Show(ActiveCastLib);
        }

        public void Dispose()
        {
            CastLibViewer.Hide();
            _castsViewerNode2D.Dispose();
        }
    }
    internal class DirGodotCastView : IDirFrameworkCast
    {
        private readonly HFlowContainer _elementsContainer;
        private readonly ScrollContainer _ScrollContainer;
        private readonly List<DirGodotCastItem> _elements = new List<DirGodotCastItem>();
        private readonly Action<DirGodotCastItem> _onSelectItem;

        public Node Node => _ScrollContainer;

        public DirGodotCastView(Action<DirGodotCastItem> onSelect)
        {
            _ScrollContainer = new ScrollContainer();
            _ScrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _ScrollContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            _elementsContainer = new HFlowContainer();
            _elementsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _elementsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _ScrollContainer.AddChild(_elementsContainer);
            this._onSelectItem = onSelect;
        }

        public void Show(ILingoCast cast)
        {
            Hide();
            var i = 0;
            foreach (var castItem in cast.GetAll())
            {
                var dirCastItem = new DirGodotCastItem(castItem, i+1, _onSelectItem);
                dirCastItem.Init();
                _elements.Add(dirCastItem);
                _elementsContainer.AddChild(dirCastItem);
                i++;
            }
        }
        public void Hide()
        {
            foreach (var element in _elements)
            {
                element.Dispose();
            }
            _elements.Clear();
        }
       

    }
    public partial class DirGodotCastMemberPropViewer : VBoxContainer
    {
        private Label _labelName;
        private readonly Label _labelType;
        private readonly Label _labelSize;
        private readonly Label _labelCastNumber;
        private readonly Label _labelFileSize;

        public DirGodotCastMemberPropViewer()
        {
            _labelName = new Label{Text = "" };
            _labelType = new Label{Text = "" };
            _labelSize = new Label{Text = "" };
            _labelCastNumber = new Label{Text = "" };
            _labelFileSize = new Label{Text = "" };
            AddChild(_labelName);
            AddChild(_labelType);
            AddChild(_labelSize);
            AddChild(_labelCastNumber);
            AddChild(_labelFileSize);
            
        }
        public void Load(ILingoMember lingoMember)
        {
            _labelName.Text = lingoMember.NumberInCast + ". " + lingoMember.Name;
            _labelType.Text = "Type:" + lingoMember.Type;
            _labelSize.Text = "CastNumber:" + lingoMember.CastLibNum;
            _labelCastNumber.Text = "Size:" + lingoMember.Width + "x" + lingoMember.Height;
            _labelFileSize.Text = "File Size:" + lingoMember.Size + " bytes";
           
        }
    }
}
