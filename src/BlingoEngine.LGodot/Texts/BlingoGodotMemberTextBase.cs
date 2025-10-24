using AbstUI.Texts;
using Godot;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.LGodot.Sprites;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;
using AbstUI.Styles;
using BlingoEngine.Texts;
using BlingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;
using AbstUI.LGodot.Helpers;
using AbstUI.LGodot.Texts;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using System.Threading.Tasks;

namespace BlingoEngine.LGodot.Texts
{
    public abstract class BlingoGodotMemberTextBase<TBlingoText> : IBlingoFrameworkMemberTextBase, IDisposable
         where TBlingoText : IBlingoMemberTextBase
    {
        protected TBlingoText _blingoMemberText = default!;
        protected string _text = "";
        protected IAbstFontManager _fontManager;
        protected readonly ILogger _logger;

        private GodotMemberTextNode _defaultTextNode;
        private List<GodotMemberTextNode> _usedNodes = new List<GodotMemberTextNode>();
        private AbstGodotTexture2D? _texture;
        public IAbstTexture2D? TextureBlingo => _texture;
        public AbstGodotTexture2D? TextureGodot => _texture;
        public IAbstFontManager FontManager => _fontManager;
        internal class GodotMemberTextNode
        {
            protected readonly TextureRect _parentNode;
            //protected readonly Label _labelNode;
            private readonly int _index;

            public TextureRect LabelNode => _parentNode;
            public TextureRect Node2D => _parentNode;

            public GodotMemberTextNode(int index)
            {
                _parentNode = new TextureRect();
                _index = index;
            }
            public void Dispose()
            {
                _parentNode.Dispose();
            }

            internal void SetName(string name)
            {
                _parentNode.Name = name + _index;
            }
        }

        private void AlignNode(GodotMemberTextNode node)
        {
            if (node == null || _blingoMemberText ==null)
                return;

            var size = node.Node2D.Size;
            if (size == Vector2.Zero)
            {
                size = node.Node2D.CustomMinimumSize;
                if (size == Vector2.Zero)
                    size = new Vector2(_blingoMemberText.Width, _blingoMemberText.Height);
            }

            var regPoint = _blingoMemberText!.RegPoint.ToVector2();
            regPoint -= size / 2f;

            node.Node2D.Position = regPoint;
            //node.Node2D.SetPivotOffset(regPoint);
        }



        private void RequireRedraw()
        {
            _blingoMemberText.RequireRedraw();
        }

        #region Properties

        public string Text { get => _text; set { UpdateText(value); } }

        public bool _wordWrap;
        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                if (_wordWrap == value) return;
                _wordWrap = value;
                RequireRedraw();
            }
        }

        public int ScrollTop { get; set; }
        //{
        //    get => _defaultTextNode.LabelNode.LinesSkipped;
        //    set { Apply(x => x.LabelNode.LinesSkipped = value); }
        //}

        private BlingoTextStyle _textStyle = BlingoTextStyle.None;

        public BlingoTextStyle FontStyle
        {
            get => _textStyle;
            set
            {
                if (_textStyle == value) return;
                _textStyle = value;
                //if (_textStyle != BlingoTextStyle.None)
                //{
                //    // todo : implement a way for rtf

                //    var rtl = new RichTextLabel();
                //    // Bold/Italic handled via FontStyle
                //    TextServer.FontStyle style = 0; // _LabelSettings.Font.GetFontStyle();

                //    if (value.HasFlag(BlingoTextStyle.Bold))
                //        style |= TextServer.FontStyle.Bold;

                //    if (value.HasFlag(BlingoTextStyle.Italic))
                //        style |= TextServer.FontStyle.Italic;
                //    //_LabelSettings.Font.Styl();


                //    //rtl.FontStyle = style;

                //    //// Underline handled separately
                //    //_LabelSettings.UnderlineMode = value.HasFlag(BlingoTextStyle.Underline)
                //    //    ? UnderlineMode.Always
                //    //    : UnderlineMode.Disabled;

                //}

                UpdateSize();
            }
        }

        private int _margin = 0;
        public int Margin
        {
            get => _margin;
            set
            {
                if (_margin == value) return;
                _margin = value;
                Apply(x =>
                {
                    x.LabelNode.AddThemeConstantOverride("margin_left", value);
                    x.LabelNode.AddThemeConstantOverride("margin_right", value);
                    x.LabelNode.AddThemeConstantOverride("margin_top", value);
                    x.LabelNode.AddThemeConstantOverride("margin_bottom", value);
                });

            }
        }
        public AbstTextAlignment _alignment;
        public AbstTextAlignment Alignment
        {
            get => _alignment;
            set
            {
                if (_alignment == value) return;
                _alignment = value;
                RequireRedraw();
            }
        }

        private AColor _blingoColor = AColor.FromRGB(0, 0, 0);
        public AColor TextColor
        {
            get => _blingoColor;
            set
            {
                if (_blingoColor.Equals(value)) return;
                _blingoColor = value;
                RequireRedraw();

            }
        }
        public int _fontSize;
        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize == value) return;
                _fontSize = value;
                UpdateSize();
            }
        }

        private string _fontName = "";
        public string FontName
        {
            get => _fontName;
            set
            {
                if (_fontName == value) return;
                _fontName = value;
                UpdateSize();
            }
        }


        public bool IsLoaded { get; private set; }
        public APoint Size { get; private set; }
        private bool _widthSet;
        public int Width
        {
            get => _widthSet ? (int)_defaultTextNode.LabelNode.CustomMinimumSize.X : (int)Size.X;
            set
            {
                Apply(x =>
                {
                    x.LabelNode.CustomMinimumSize = new Vector2(value, x.LabelNode.CustomMinimumSize.Y);
                    AlignNode(x);
                });
                _widthSet = true;
            }
        }
        private bool _heightSet;

        public int Height
        {
            get => _heightSet ? (int)_defaultTextNode.LabelNode.CustomMinimumSize.Y : (int)Size.Y;
            set
            {
                Apply(x =>
                {
                    x.LabelNode.CustomMinimumSize = new Vector2(x.LabelNode.CustomMinimumSize.X, value);
                    AlignNode(x);
                });
                _heightSet = true;
            }
        }


        #endregion

        private bool _hasLoadedTexTure;

        public BlingoGodotMemberTextBase(IAbstFontManager blingoFontManager, ILogger logger)
        {
            _fontManager = blingoFontManager;
            _logger = logger;
            _defaultTextNode = new GodotMemberTextNode(1);
            _usedNodes.Add(_defaultTextNode);
        }
        public void Dispose()
        {
            foreach (var usedNode in _usedNodes)
                usedNode.Dispose();
            _usedNodes.Clear();
        }

        internal void Init(TBlingoText blingoInstance)
        {
            _blingoMemberText = blingoInstance;
            if (!string.IsNullOrWhiteSpace(blingoInstance.Name))
            {
                _defaultTextNode.SetName(blingoInstance.Name);
                foreach (var usedNode in _usedNodes)
                    usedNode.SetName(blingoInstance.Name);
            }
            blingoInstance.InitDefaults();
        }


        protected Node CreateForSpriteDraw(BlingoGodotMemberTextBase<TBlingoText> copiedNode)
        {
            var newNode = new GodotMemberTextNode(_usedNodes.Count + 1);
            //var newNode = new Control(); // _usedNodes.Count + 1, _LabelSettings);


            if (!string.IsNullOrWhiteSpace(_blingoMemberText?.Name))
                newNode.SetName(_blingoMemberText.Name);

            //newNode.LabelNode.Text = _text;
            //newNode.LabelNode.AutowrapMode = _defaultTextNode.LabelNode.AutowrapMode;
            //newNode.LabelNode.LinesSkipped = _defaultTextNode.LabelNode.LinesSkipped;
            //newNode.LabelNode.HorizontalAlignment = _defaultTextNode.LabelNode.HorizontalAlignment;
            //newNode.LabelNode.CustomMinimumSize = _defaultTextNode.LabelNode.CustomMinimumSize;
            //newNode.LabelNode.AddThemeConstantOverride("margin_left", _margin);
            //newNode.LabelNode.AddThemeConstantOverride("margin_right", _margin);
            //newNode.LabelNode.AddThemeConstantOverride("margin_top", _margin);
            //newNode.LabelNode.AddThemeConstantOverride("margin_bottom", _margin);


            newNode.LabelNode.AddThemeConstantOverride("margin_left", _margin);
            newNode.LabelNode.AddThemeConstantOverride("margin_right", _margin);
            newNode.LabelNode.AddThemeConstantOverride("margin_top", _margin);
            newNode.LabelNode.AddThemeConstantOverride("margin_bottom", _margin);

            _usedNodes.Add(newNode);
            IAbstTexture2D? texture = _blingoMemberText!.GetTexture();
            if (texture is AbstGodotTexture2D godotTexture)
            {
                _texture = godotTexture;
                godotTexture.DebugWriteToDisk();
                newNode.Node2D.Size = new Vector2(godotTexture.Width, godotTexture.Height);
                newNode.Node2D.CustomMinimumSize = new Vector2(godotTexture.Width, godotTexture.Height);
                newNode.Node2D.Texture = _texture.Texture;
            }
            Apply(AlignNode);
            return newNode.Node2D;
        }
        public void ReleaseFromSprite(BlingoSprite2D blingoSprite)
        {
            if (blingoSprite.Member == null) return;
            var godotNode = blingoSprite.Framework<BlingoGodotSprite2D>().ChildMemberNode;
            if (godotNode == null) return;
            var nodeLocal = _usedNodes.FirstOrDefault(x => x.Node2D == godotNode);
            if (nodeLocal != null)
                _usedNodes.Remove(nodeLocal);
        }



        public IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor)
        {
            //_hasLoadedTexTure = true;
            //int w = Width > 0 ? Width : (int)Size.X;
            //int h = Height > 0 ? Height : (int)Size.Y;
            //if (w <= 0 || h <= 0)
            //    w = h = 1;
            //var viewport = new SubViewport
            //{
            //    Size = new Vector2I(w, h),
            //    TransparentBg = true,
            //    RenderTargetUpdateMode = SubViewport.UpdateMode.Once
            //};
            //var label = new Label
            //{
            //    LabelSettings = _LabelSettings,
            //    Text = Text,
            //    Size = new Vector2(w, h)
            //};
            //label.AutowrapMode = WordWrap ? TextServer.AutowrapMode.Word : TextServer.AutowrapMode.Off;
            //label.HorizontalAlignment = Alignment switch
            //{
            //    AbstTextAlignment.Center => HorizontalAlignment.Center,
            //    AbstTextAlignment.Right => HorizontalAlignment.Right,
            //    _ => HorizontalAlignment.Left
            //};
            //viewport.AddChild(label);
            //var img = viewport.GetTexture().GetImage();
            //viewport.Dispose();
            //var tex = ImageTexture.CreateFromImage(img);
            //_texture = new AbstGodotTexture2D(tex);
            //return _texture;
            return _texture;
        }


        private void UpdateText(string value)
        {
            if (_text == value) return;
            _text = value;
            //foreach (var item in _usedNodes)
            //    item.LabelNode.Text = value;
            UpdateSize();

        }
        private void Apply(Action<GodotMemberTextNode> action)
        {
            foreach (var item in _usedNodes)
                action(item);
        }

        private void UpdateSize()
        {
            //var font = _LabelSettings.Font ?? _fontManager.GetDefaultFont<Font>();
            //Size = _defaultTextNode != null ? _defaultTextNode.LabelNode.GetCombinedMinimumSize().ToAbstPoint() : font.GetMultilineStringSize(Text).ToAbstPoint();
            //_blingoMemberText.Width = (int)Size.X;
            //_blingoMemberText.Height = (int)Size.Y;
            //var asc = font.GetAscent();
            //var height = font.GetHeight();
            //var dif = height - asc;
            //_blingoMemberText.RegPoint = new APoint(0, dif);
            RequireRedraw();
        }

        public void Erase()
        {
            Unload();
            IsLoaded = false;
        }


        public void Preload()
        {
            if (IsLoaded)
                return;
            //if (!_hasLoadedTexTure)
            //    RenderToTexture(BlingoInkType.Copy, AColors.White);
            IsLoaded = true;
        }

        public Task PreloadAsync()
        {
            Preload();
            return Task.CompletedTask;
        }

        public void Unload()
        {
            _hasLoadedTexTure = false;
            IsLoaded = false;
        }
        public bool IsPixelTransparent(int x, int y) => false;


        #region Clipboard
        public void ImportFileInto()
        {
        }

        public void CopyToClipboard()
        {
            DisplayServer.ClipboardSet(Text);
        }
        public void PasteClipboardInto()
        {
            var _RAWTextData = DisplayServer.ClipboardGet();
            _blingoMemberText.Text = _RAWTextData;
        }
        public void Copy(string text) => DisplayServer.ClipboardSet(text);
        public string PasteClipboard() => DisplayServer.ClipboardGet();




        #endregion
    }
}

