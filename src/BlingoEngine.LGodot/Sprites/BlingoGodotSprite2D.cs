using Godot;
using BlingoEngine.Casts;
using BlingoEngine.LGodot.Texts;
using BlingoEngine.Primitives;
using BlingoEngine.Texts;
using BlingoEngine.Members;
using BlingoEngine.Sprites;
using BlingoEngine.Medias;
using BlingoEngine.Bitmaps;
using BlingoEngine.LGodot.Bitmaps;
using BlingoEngine.Tools;
using BlingoEngine.Shapes;
using BlingoEngine.LGodot.Shapes;
using BlingoEngine.FilmLoops;
using BlingoEngine.LGodot.FilmLoops;
using AbstUI.Primitives;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using BlingoEngine.LGodot.Medias;

namespace BlingoEngine.LGodot.Sprites
{

    public partial class BlingoGodotSprite2D : IBlingoFrameworkSprite, IBlingoFrameworkSpriteVideo, IDisposable
    {
        private readonly CenterContainer _container2D;
        private readonly Node2D _parentNode2D;
        private readonly Sprite2D _sprite2D;
        private readonly Action<BlingoGodotSprite2D> _showMethod;
        private readonly Action<BlingoGodotSprite2D> _removeMethod;
        private readonly Action<BlingoGodotSprite2D> _hideMethod;
        private readonly BlingoSprite2D _blingoSprite2D;
        private bool _wasShown;
        private BlingoFilmLoopPlayer? _filmloopPlayer;
        private VideoStreamPlayer? _videoPlayer;
        private CanvasItemMaterial _material = new();
        private int _ink;
        private AbstGodotTexture2D? _texture;
        internal BlingoSprite2D BlingoSprite => _blingoSprite2D;
        internal Node? ChildMemberNode => _previousChildElementNode;
        internal bool IsDirty { get; set; } = true;
        internal bool IsDirtyMember { get; set; } = true;
        private float _x;
        private float _y;
        private int _zIndex;
        private float _blend = 1f;
        private string _name = string.Empty;


        #region Properties

        public AMargin Margin { get; set; } = AMargin.Zero;

        public object FrameworkNode => _container2D;
        public float X
        {
            get => _x;
            set
            {
                if (_x == value) return;
                _x = value;
                if (_blingoSprite2D.SpriteNum == 101)
                {

                }
                SetDirty();
            }
        }
        public float Y
        {
            get => _y;
            set
            {
                if (_y == value) return;
                _y = value;
                SetDirty();
            }
        }

        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex == value) return;
                if (value > 4096) _zIndex = 4096;
                else if (value < -4096) _zIndex = -4096;
                else
                    _zIndex = value;
                ApplyZIndex();
            }
        }
        public APoint RegPoint
        {
            get => (_container2D.Position.X, _container2D.Position.Y);
            set
            {
                if (_x == value.X && _y == value.Y) return;
                _container2D.Position = new Vector2(value.X, value.Y);
                SetDirty();
            }
        }

        public bool Visibility
        {
            get => _container2D.Visible;
            set
            {
                if (_container2D.Visible == value) return;
                _container2D.Visible = value;
            }
        }
        public IBlingoCast? Cast { get; private set; }


        public float Blend
        {
            get => _blend;
            set
            {
                if (_blend == value) return;
                _blend = value;
                ApplyBlend();
            }
        }

        public string Name
        {
            get => _name.ToString();
            set
            {
                _name = value;
                UpdateSprite2DName();
            }
        }

        private void UpdateSprite2DName()
        {
            var fullName = _blingoSprite2D.GetFullName();
            _sprite2D.Name = fullName + ".sprite";
            _container2D.Name = fullName;
        }

        public float Width { get; set; }
        public float Height { get; set; }
        private float _desiredWidth;
        private float _desiredHeight;
        public float DesiredWidth
        {
            get => _desiredWidth;
            set
            {
                if (_desiredWidth == value) return;
                _desiredWidth = value; SetDirty();
            }
        }
        public float DesiredHeight
        {
            get => _desiredHeight;
            set
            {
                if (_desiredHeight == value) return;
                _desiredHeight = value; SetDirty();
            }
        }

        public float Rotation
        {
            get => Mathf.RadToDeg(_sprite2D.Rotation);
            set
            {
                var val = Mathf.DegToRad(value);
                if (_sprite2D.Rotation == val) return;
                _sprite2D.Rotation = Mathf.DegToRad(value);
            }
        }
        public float Skew { get; set; }
        public bool FlipH { get => _sprite2D.FlipH; set => _sprite2D.FlipH = value; }
        public bool FlipV { get => _sprite2D.FlipV; set => _sprite2D.FlipV = value; }
        private bool _directToStage;
        public bool DirectToStage
        {
            get => _directToStage;
            set
            {
                _directToStage = value;
                ApplyZIndex();
                ApplyBlend();
            }
        }

        public int Ink
        {
            get => _ink;
            set
            {
                if (_ink == value) return;
                _ink = value;
                ApplyInk();
            }
        }

        #endregion


        public BlingoGodotSprite2D(BlingoSprite2D blingoSprite, Node2D parentNode, Action<BlingoGodotSprite2D> showMethod, Action<BlingoGodotSprite2D> hideMethod, Action<BlingoGodotSprite2D> removeMethod)
        {
            _parentNode2D = parentNode;
            _blingoSprite2D = blingoSprite;
            _showMethod = showMethod;
            _hideMethod = hideMethod;
            _removeMethod = removeMethod;
            _sprite2D = new Sprite2D();
            _container2D = new CenterContainer();
            _container2D.AddChild(_sprite2D);
            blingoSprite.Init(this);
            _zIndex = blingoSprite.SpriteNum;
            _directToStage = blingoSprite.DirectToStage;
            ApplyZIndex();
            ApplyBlend();
            _ink = blingoSprite.Ink;
            ApplyInk();
        }



        private void ApplyZIndex()
        {
            _container2D.ZIndex = _directToStage ? 100000 + _zIndex : _zIndex;
        }

        private void ApplyBlend()
        {
            var col = _sprite2D.SelfModulate;
            float alpha = Mathf.Clamp(_blend / 100f, 0f, 1f);
            _sprite2D.SelfModulate = new Color(col.R, col.G, col.B, _directToStage ? 1f : alpha);
            SetDirty();
        }

        private void ApplyInk()
        {
            if (_ink == 0) return;

            if (InkPreRenderer.CanHandle((BlingoInkType)_ink))
            {
                _material.BlendMode = CanvasItemMaterial.BlendModeEnum.Mix;
                _sprite2D.Material = _material;
                return;
            }

            CanvasItemMaterial.BlendModeEnum mode = _ink switch
            {
                (int)BlingoInkType.AddPin => CanvasItemMaterial.BlendModeEnum.Add,
                (int)BlingoInkType.Add => CanvasItemMaterial.BlendModeEnum.Add,
                (int)BlingoInkType.SubstractPin => CanvasItemMaterial.BlendModeEnum.Sub,
                (int)BlingoInkType.Substract => CanvasItemMaterial.BlendModeEnum.Sub,
                (int)BlingoInkType.Darken => CanvasItemMaterial.BlendModeEnum.Mul,
                (int)BlingoInkType.Lighten => CanvasItemMaterial.BlendModeEnum.Add,
                _ => CanvasItemMaterial.BlendModeEnum.Mix,
            };

            if (mode == CanvasItemMaterial.BlendModeEnum.Mix)
            {
                _sprite2D.Material = InkShaderMaterial.Create(
                    _blingoSprite2D.InkType,
                    _blingoSprite2D.BackColor.ToGodotColor());
            }
            else
            {
                _material.BlendMode = mode;
                _sprite2D.Material = _material;
            }
        }



        public void RemoveMe()
        {
            _removeMethod(this);
            Dispose();
        }
        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed) return;
            var parent = _container2D.GetParent();
            if (parent != null)
                parent.RemoveChild(_container2D);
            _sprite2D.Dispose();
            _container2D.Dispose();
            _isDisposed = true;
        }

        public void Show()
        {
            if (_isDisposed) 
                return;
            if (!_wasShown)
            {
                _wasShown = true;
                _parentNode2D.AddChild(_container2D);
                _showMethod(this);
            }
            Update();
        }
        public void Hide()
        {
            if (_isDisposed) return;
            if (!_wasShown)
                return;
            _wasShown = false;
            _hideMethod(this);
            var parent = _container2D.GetParent();
            parent.RemoveChild(_container2D);
            if (parent is Node2D node2D)
                node2D.QueueRedraw();
        }

        public void SetPosition(APoint blingoPoint)
        {
            _x = blingoPoint.X;
            _y = blingoPoint.Y;
            SetDirty();
        }

        private void SetDirty()
        {
            IsDirty = true;
            _container2D.QueueRedraw();
        }

        public void MemberChanged()
        {
            _filmloopPlayer = null;
            if (BlingoSprite.Member != null)
            {
                var (_, sourceWidth, sourceHeight) = BlingoSprite.GetMemberSourceMetrics();
                if (sourceWidth <= 0)
                    sourceWidth = BlingoSprite.Member.Width;
                if (sourceHeight <= 0)
                    sourceHeight = BlingoSprite.Member.Height;

                if (Width == 0)
                    Width = sourceWidth;
                if (Height == 0)
                    Height = sourceHeight;
            }
            UpdateTextureRegion();
            IsDirtyMember = true;
        }
        private void UpdateSizeFromTexture()
        {
            if (_texture == null)
            {
                // this breaks the filmloop
                //Width = 0;
                //Height = 0;
                return;
            }

            //if (Width == 0 || Height == 0)
            {
                float baseWidth = _texture.Width;
                float baseHeight = _texture.Height;
                if (BlingoSprite.Member is BlingoMemberBitmap && BlingoSprite.MemberSourceRect is { } rect)
                {
                    baseWidth = rect.Width;
                    baseHeight = rect.Height;
                }
                Width = baseWidth;
                Height = baseHeight;
            }
        }
        internal void Update()
        {
            if (IsDirtyMember)
                UpdateMember();

            if (IsDirty)
            {
                // update complex properties
                if (_texture != null)
                {
                    if (_desiredWidth != Width || _desiredHeight != Height)
                    {
                        UpdateSizeFromTexture();
                        Width = _desiredWidth;
                        Height = _desiredHeight;
                        Resize(_desiredWidth, _desiredHeight);
                    }
                }
                IsDirty = false;
                //Console.WriteLine($"{_blingoSprite2D.SpriteNum}: {_blingoSprite2D.Member?.Name}: {_x}: {_y}");
            }
            // todo: move this 2 lines in IsDirty if test
            var offset = GetRegPointOffset();
            _sprite2D.Position = new Vector2(_x - offset.X, _y - offset.Y);
            
        }


        private void UpdateMember()
        {
            if (!IsDirtyMember) return;
            IsDirtyMember = false;


            switch (_blingoSprite2D.Member)
            {
                case BlingoMemberBitmap pictureMember:
                    RemoveLastChildElement();
                    UpdateMemberPicture(pictureMember.Framework<BlingoGodotMemberBitmap>());
                    //UpdateSizeFromTexture();
                    if (_desiredWidth == 0) _desiredWidth = Width;
                    if (_desiredHeight == 0) _desiredHeight = Height;
                    SetDirty();
                    return;
                case BlingoFilmLoopMember flm:
                    RemoveLastChildElement();
                    UpdateMemberFilmLoop(flm.Framework<BlingoGodotFilmLoopMember>());
                    //UpdateSizeFromTexture();
                    if (_desiredWidth == 0) _desiredWidth = Width;
                    if (_desiredHeight == 0) _desiredHeight = Height;
                    SetDirty();
                    return;
                case BlingoMemberMedia mediaMember:
                    RemoveLastChildElement();
                    UpdateMemberVideo(mediaMember.Framework<BlingoGodotMemberMedia>());
                    if (_desiredWidth == 0) _desiredWidth = Width;
                    if (_desiredHeight == 0) _desiredHeight = Height;
                    SetDirty();
                    return;
                case BlingoMemberText textMember:
                    var godotElement = textMember.Framework<BlingoGodotMemberText>();
                    UpdateNodeMember(textMember, godotElement.CreateForSpriteDraw());
                    //_blingoSprite2D.UpdateTexture(godotElement.RenderToTexture());
                    break;
                case BlingoMemberField fieldMember:
                    var godotElementF = fieldMember.Framework<BlingoGodotMemberField>();
                    UpdateNodeMember(fieldMember, godotElementF.CreateForSpriteDraw());
                    //_blingoSprite2D.UpdateTexture(godotElementF.RenderToTexture());
                    break;
                // all generice godot node base class members
                case BlingoMemberShape shape:
                    UpdateNodeMember(_blingoSprite2D.Member, (Node)shape.Framework<BlingoGodotMemberShape>().CloneForSpriteDraw());
                    break;
            }

            UpdateSprite2DName();
        }
        public void ApplyMemberChangesOnStepFrame()
        {
            //switch (_blingoSprite2D.Member)
            //{
            //    case BlingoMemberBitmap pictureMember:
            //        return;
            //    case BlingoMemberText textMember:
            //        break;
            //    case BlingoMemberField fieldMember:
            //        //fieldMember.ApplyMemberChanges();
            //        break;
            //    // all generice godot node base class members
            //    case BlingoMemberShape shape:
            //        break;
            //    case BlingoFilmLoopMember filmloop:

            //        break;
            //}
        }
        private void RemoveLastChildElement()
        {
            if (_previousChildElementNode != null)
                _sprite2D.RemoveChild(_previousChildElementNode);
            _previousChildElementNode = null;
            _previousChildElement = null;
            _videoPlayer = null;
        }

        private void UpdateMemberPicture(BlingoGodotMemberBitmap godotPicture)
        {
            godotPicture.Preload();
            if (godotPicture.TextureBlingo == null)
                return;

            if (InkPreRenderer.CanHandle(_blingoSprite2D.InkType))
            {
                var texture1 = godotPicture.GetTextureForInk(_blingoSprite2D.InkType, _blingoSprite2D.BackColor) as AbstGodotTexture2D;
                if (texture1 != null)
                    TextureHasChanged(texture1);
            }
            else
                TextureHasChanged((AbstGodotTexture2D)godotPicture.TextureBlingo);
        }
        private void TextureHasChanged(AbstGodotTexture2D tex)
        {
            if (tex.Texture == _sprite2D.Texture) return;
            if (tex.IsDisposed)
            {

            }
            _sprite2D.Texture = tex.Texture;
            _texture = tex;
            // because we dont need to clone the textures, we dont need to subscribe unsubscribe to  texture.
            _blingoSprite2D.FWTextureHasChanged(tex, false);
            UpdateTextureRegion();

            float sourceWidth = tex.Width;
            float sourceHeight = tex.Height;
            if (_blingoSprite2D.Member is BlingoMemberBitmap && _blingoSprite2D.MemberSourceRect is { } rect)
            {
                sourceWidth = rect.Width;
                sourceHeight = rect.Height;
            }

            if (Width == 0)
                Width = sourceWidth;

            if (Height == 0)
                Height = sourceHeight;
        }
        public void SetTexture(IAbstTexture2D texture)
        {
            TextureHasChanged((AbstGodotTexture2D)texture);
        }

        private void UpdateTextureRegion()
        {
            if (_sprite2D.Texture == null)
                return;

            if (_blingoSprite2D.Member is BlingoMemberBitmap && _blingoSprite2D.MemberSourceRect is { } rect)
            {
                _sprite2D.RegionEnabled = true;
                _sprite2D.RegionRect = new Rect2(rect.Left, rect.Top, rect.Width, rect.Height);
            }
            else
            {
                _sprite2D.RegionEnabled = false;
            }
        }

        private void UpdateMemberFilmLoop(BlingoGodotFilmLoopMember filmLoop)
        {
            var size = filmLoop.GetBoundingBox();
            _desiredHeight = size.Height;
            _desiredWidth = size.Width;
            Width = size.Width;
            Height = size.Height;
            var offset = filmLoop.Offset;
            _sprite2D.Offset = new Vector2(Width / 2f - offset.X, Height / 2f - offset.Y);
            _filmloopPlayer = _blingoSprite2D.GetFilmLoopPlayer();
            if (_filmloopPlayer == null) return;
            if (_filmloopPlayer.Texture is not AbstGodotTexture2D tex)
                return;
            TextureHasChanged(tex);
        }
        private void UpdateMemberVideo(BlingoGodotMemberMedia media)
        {
            media.Preload();
            var player = new VideoStreamPlayer
            {
                Stream = media.Stream
            };
            _videoPlayer = player;
            UpdateNodeMember(_blingoSprite2D.Member!, player);
        }
        private IBlingoMember? _previousChildElement;
        private Node? _previousChildElementNode;


        private void UpdateNodeMember(IBlingoMember member, Node godotElement) // Clone required to be able to draw multiple times the same member
        {
            if (_previousChildElement == member) return;
            RemoveLastChildElement();

            member.Preload();

            _sprite2D.AddChild(godotElement);
            _previousChildElementNode = godotElement;
            _previousChildElement = member;
            _desiredWidth = member.Width;
            _desiredHeight = member.Height;
        }

        public void Resize(float targetWidth, float targetHeight)
        {
            float sourceWidth = _texture?.Width ?? _sprite2D.Texture?.GetWidth() ?? 0f;
            float sourceHeight = _texture?.Height ?? _sprite2D.Texture?.GetHeight() ?? 0f;
            if (BlingoSprite.Member is BlingoMemberBitmap && BlingoSprite.MemberSourceRect is { } rect)
            {
                sourceWidth = rect.Width;
                sourceHeight = rect.Height;
            }

            if (sourceWidth == 0 || sourceHeight == 0)
                return;

            float scaleFactorW = targetWidth / sourceWidth;
            float scaleFactorH = targetHeight / sourceHeight;
            _sprite2D.Scale = new Vector2(scaleFactorW, scaleFactorH);
        }

        private APoint GetRegPointOffset()
        {
            if (_blingoSprite2D.Member == null) return new APoint();

            var (baseOffset, sourceWidth, sourceHeight) = _blingoSprite2D.GetMemberSourceMetrics();
            if (sourceWidth != 0 && sourceHeight != 0)
            {
                float scaleX = Width / sourceWidth;
                float scaleY = Height / sourceHeight;
                return new APoint(baseOffset.X * scaleX, baseOffset.Y * scaleY);
            }
            return baseOffset;
        }


        #region Video/Media

        /// <inheritdoc/>
        public void Play()
        {
            if (_videoPlayer == null) return;
            if (_videoPlayer.Paused)
                _videoPlayer.Paused = false;
            else
                _videoPlayer?.Play();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (_videoPlayer != null)
                _videoPlayer.Paused = true;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (_videoPlayer == null) return;
            _videoPlayer.Stop();
            _videoPlayer.StreamPosition = 0;
        }

        /// <inheritdoc/>
        public void Seek(int milliseconds)
        {
            if (_videoPlayer == null) return;
            _videoPlayer.StreamPosition = milliseconds / 1000f;
        }

        /// <inheritdoc/>
        public int Duration
        {
            get
            {
                if (_videoPlayer == null) return 0;
                return (int)(_videoPlayer.GetStreamLength() * 1000);
            }
        }

        /// <inheritdoc/>
        public int CurrentTime
        {
            get => (int)(_videoPlayer?.StreamPosition * 1000 ?? 0);
            set
            {
                if (_videoPlayer != null)
                    _videoPlayer.StreamPosition = value / 1000f;
            }
        }

        /// <inheritdoc/>
        public BlingoMediaStatus MediaStatus => _videoPlayer switch
        {
            null => BlingoMediaStatus.Closed,
            _ when _videoPlayer.IsPlaying() => BlingoMediaStatus.Playing,
            _ => BlingoMediaStatus.Paused
        };

        
        #endregion
    }
}

