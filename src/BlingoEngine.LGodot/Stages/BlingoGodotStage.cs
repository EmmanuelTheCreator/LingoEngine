using AbstUI.Components;
using AbstUI.LGodot.Bitmaps;
using AbstUI.LGodot.Primitives;
using AbstUI.Primitives;
using Godot;
using BlingoEngine.Core;
using BlingoEngine.Movies;
using BlingoEngine.Stages;
using static Godot.OpenXRCompositionLayer;

namespace BlingoEngine.LGodot.Movies
{
    public partial class BlingoGodotStage : Node2D, IBlingoFrameworkStage, IDisposable
    {
        private Action<IAbstTexture2D>? _pendingShot;
        private bool _shotArmed;
        private bool _isDrawingForShot = false;
        private BlingoStage _blingoStage = null!;
        private readonly BlingoClock _blingoClock;
        private readonly BlingoDebugOverlay _overlay;
        private readonly ColorRect _bg;
        private BlingoPlayer? _player;
        private bool _f1Down;

        private BlingoGodotMovie? _activeMovie;
        private SubViewport _stageSV = null!;
        private SubViewportContainer _stageSVC = null!;
        private Node2D _stageRoot = null!;
        private Node2D _spriteLayer = null!;
        private Sprite2D _transitionStartSprite = null!;
        private Sprite2D _transitionSprite = null!;
        float IBlingoFrameworkStage.Scale { get => base.Scale.X; set => base.Scale = new Vector2(value, value); }
        public BlingoStage BlingoStage => _blingoStage;
        public float X { get => BlingoStage.X; set => BlingoStage.X = value; }
        public float Y { get => BlingoStage.Y; set => BlingoStage.Y = value; }
        string IAbstFrameworkNode.Name { get => Name; set => Name = value; }
        public bool Visibility { get => Visible; set => Visible = value; }
        public float Width { get; set; }
        public float Height { get; set; }
        public AMargin Margin { get; set; } = AMargin.Zero;

        public object FrameworkNode => this;

        public BlingoGodotStage(BlingoPlayer blingoPlayer)
        {
            _blingoClock = (BlingoClock)blingoPlayer.Clock;
            _overlay = new BlingoDebugOverlay(new Core.BlingoGodotDebugOverlay(this), blingoPlayer);


            _stageSV = new SubViewport
            {
                Disable3D = true,
                TransparentBg = false,
                RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
                RenderTargetClearMode = SubViewport.ClearMode.Always,
                
            };

            _bg = new ColorRect
            {
                Color = Colors.Black,
                Size = new Vector2(300, 300),
                CustomMinimumSize = new Vector2(300, 300)
            };
            _stageSVC = new SubViewportContainer { Stretch = true, Name = "StageView" };
            AddChild(_stageSVC);
            // mount SubViewport inside the container
            _stageSVC.AddChild(_stageSV);

            _stageSV.AddChild(_bg);
            _stageRoot = new Node2D { Name = "StageRoot" };
            _stageSV.AddChild(_stageRoot);

            _spriteLayer = new Node2D { Name = "SpriteLayer" };
            _stageRoot.AddChild(_spriteLayer);

            _transitionStartSprite = new Sprite2D { Visible = false, ZIndex = 1000 };
            AddChild(_transitionStartSprite);
            _transitionSprite = new Sprite2D { Visible = false, ZIndex = 1001 };
            AddChild(_transitionSprite);
        }

        public override void _Ready()
        {
            base._Ready();
        }
        public override void _Process(double delta)
        {
            base._Process(delta);
            _blingoClock.Tick((float)delta);
            if (_blingoStage.IsDirty)
                _bg.Color = _blingoStage.BackgroundColor.ToGodotColor();
            if (_player != null)
            {
                _overlay.Update((float)delta);
                bool f1 = _player.Key.KeyPressed((int)Key.F1);
                if (f1 && !_f1Down)
                    _overlay.Toggle();
                _f1Down = f1;
                _overlay.Render();
            }
        }



        internal void Init(BlingoStage blingoInstance, BlingoPlayer blingoPlayer)
        {
            _blingoStage = blingoInstance;
            _player = blingoPlayer;
            _bg.Color = blingoInstance.BackgroundColor.ToGodotColor();
            var size = new Vector2I((int)_blingoStage.Width, (int)_blingoStage.Height);
            _stageSV.Size = size;
            UpdateSize();
        }

        public void UpdateSize()
        {
            var size = new Vector2I((int)_blingoStage.Width, (int)_blingoStage.Height);
            
            _stageSVC.CustomMinimumSize = size;
            _bg.Size = size;
            _bg.CustomMinimumSize = size;
            var center = new Vector2(BlingoStage.Width / 2f, BlingoStage.Height / 2f);
            _transitionStartSprite.Position = center;
            _transitionSprite.Position = center;
        }

        internal void ShowMovie(BlingoGodotMovie blingoGodotMovie)
        {
            var node = blingoGodotMovie.GetNode2D();
            // Avoid adding the same node multiple times which results in an error
            if (node.GetParent() != _spriteLayer)
            {
                _spriteLayer.AddChild(node);
            }
            UpdateSize();
        }

        internal void HideMovie(BlingoGodotMovie blingoGodotMovie)
        {
            var node = blingoGodotMovie.GetNode2D();
            if (node.GetParent() == _spriteLayer)
                _spriteLayer.RemoveChild(node);
        }

        public void SetActiveMovie(BlingoMovie? blingoMovie)
        {
            if (_activeMovie != null)
                _activeMovie.Hide();
            if (blingoMovie == null)
            {
                _activeMovie = null;
                return;
            }
            if (blingoMovie == null) return;
            var godotMovie = blingoMovie.Framework<BlingoGodotMovie>();
            _activeMovie = godotMovie;
            godotMovie.Show();
        }

        internal void SetScale(float scale)
        {
            Scale = new Vector2(scale, scale);
        }

        public void ApplyPropertyChanges()
        {
        }

        public void RequestNextFrameScreenshot(Action<IAbstTexture2D> onCaptured)
        {
            _pendingShot = onCaptured;
            if (!_shotArmed)
            {
                _shotArmed = true;
                RenderingServer.FramePostDraw += OnFramePostDraw_Screenshot; // capture & restore
            }
        }

        private void OnFramePostDraw_Screenshot()
        {
            try
            {
                if (_pendingShot is null) return;
                var shot = GetScreenshot();   // off-screen: no window presentation occurred
                _pendingShot?.Invoke(shot);
            }
            finally
            {
                // one-shot unsubscribe
                RenderingServer.FramePostDraw -= OnFramePostDraw_Screenshot;
                _pendingShot = null;
                _shotArmed = false;
            }
        }


        public IAbstTexture2D GetScreenshot()
        {
            var texx = _stageSV.GetTexture().GetImage();
            ImageTexture tex2 = ImageTexture.CreateFromImage(texx);
            var wrap2 = new AbstGodotTexture2D(tex2, $"StageShot_{_activeMovie!.CurrentFrame}");
#if DEBUG
            //wrap2.DebugWriteToDiskInc();
#endif
            return wrap2;
        }




        public void ShowTransition(IAbstTexture2D startTexture)
        {
#if DEBUG
            AbstGodotTexture2D.ResetDebuggerInc();
#endif
            var godotTex = (AbstGodotTexture2D)startTexture;
            _transitionStartSprite.Texture = godotTex.Texture;
            _transitionStartSprite.Position = new Vector2(startTexture.Width / 2f, startTexture.Height / 2f);
            _transitionStartSprite.Visible = true;

            var img = godotTex.Texture.GetImage();
           // godotTex.DebugWriteToDiskInc();
            var overlayTex = ImageTexture.CreateFromImage(img);
            _transitionSprite.Texture = overlayTex;
            _transitionSprite.RegionEnabled = true;
            _transitionSprite.RegionRect = new Rect2(0, 0, startTexture.Width, startTexture.Height);
            _transitionSprite.Position = new Vector2(startTexture.Width / 2f, startTexture.Height / 2f);
            _transitionSprite.Visible = false;
        }


        public void UpdateTransitionFrame(IAbstTexture2D texture, ARect targetRect)
        {
            var godotTex = (AbstGodotTexture2D)texture;
            if (_transitionSprite.Texture is ImageTexture imgTex)
            {
                // reuse existing ImageTexture, update its data
                imgTex.Update(godotTex.Texture.GetImage());
                _transitionSprite.RegionRect = targetRect.ToRect2();
                _transitionSprite.Position = new Vector2(targetRect.Left + targetRect.Width / 2f,
                    targetRect.Top + targetRect.Height / 2f);
                _transitionSprite.Visible = true;
#if DEBUG
                //godotTex.DebugWriteToDiskInc();
#endif
            }
        }


        public void HideTransition()
        {
            _transitionStartSprite.Texture = null;
            _transitionStartSprite.Visible = false;
            _transitionSprite.Texture = null;
            _transitionSprite.Visible = false;
        }
    }
}

