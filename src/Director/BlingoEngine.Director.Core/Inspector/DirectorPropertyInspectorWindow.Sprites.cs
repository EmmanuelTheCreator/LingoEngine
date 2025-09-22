using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Inspector.Commands;
using BlingoEngine.Director.Core.Members.Commands;
using BlingoEngine.Director.Core.Sprites;
using BlingoEngine.Director.Core.Sprites.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Primitives;
using BlingoEngine.Scripts;
using BlingoEngine.Members;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    #region Sprite Tab
    private void AddSpriteTab(BlingoSprite sprite)
    {
        CreateBehaviorPanel();
        var wrapContainer = AddTab(PropetyTabNames.Sprite);
        var containerIcons = _factory.CreateWrapPanel(AOrientation.Horizontal, "SpriteDetailIcons");
        var container = _factory.CreatePanel("SpriteDetailPanel");
        container.BackgroundColor = DirectorColors.BG_WhiteMenus;

        var spriteAdapter = new SpriteCommandAdapter(this, sprite);
        var sprite2D = sprite as BlingoSprite2D;
        var sprite2DAdapter = sprite2D != null ? new Sprite2DCommandAdapter(this, sprite2D) : null;

        containerIcons.Margin = new AMargin(5, 5, 5, 5);
        var composer0 = containerIcons.Compose()
            .AddStateButton("SpriteLock", spriteAdapter, _iconManager.Get(DirectorIcon.Lock), c => c.Lock);
        if (sprite2DAdapter != null)
        {
            composer0
                .AddStateButton("SpriteFlipH", sprite2DAdapter, _iconManager.Get(DirectorIcon.FlipHorizontal), c => c.FlipH, "")
                .AddStateButton("SpriteFlipV", sprite2DAdapter, _iconManager.Get(DirectorIcon.FlipVertical), c => c.FlipV);
        }
        composer0.Finalize();

        var composer = container.Compose(_factory.ComponentFactory)
               .Columns(4)
               .AddTextInput("SpriteName", "Name:", spriteAdapter, s => s.Name, inputSpan: 3)
               .Columns(8);
        if (sprite2DAdapter != null)
        {
            composer
                   .AddNumericInputFloat("SpriteLocH", "X:", sprite2DAdapter, s => s.LocH)
                   .AddNumericInputFloat("SpriteLocV", "Y:", sprite2DAdapter, s => s.LocV)
                   .AddNumericInputFloat("SpriteLocZ", "Z:", sprite2DAdapter, s => s.LocZ, inputSpan: 3)
                   .AddNumericInputFloat("SpriteLeft", "L:", sprite2DAdapter, s => s.Left)
                   .AddNumericInputFloat("SpriteTop", "T:", sprite2DAdapter, s => s.Top)
                   .AddNumericInputFloat("SpriteRight", "R:", sprite2DAdapter, s => s.Right)
                   .AddNumericInputFloat("SpriteBottom", "B:", sprite2DAdapter, s => s.Bottom)
                   .AddNumericInputFloat("SpriteWidth", "W:", sprite2DAdapter, s => s.Width)
                   .AddNumericInputFloat("SpriteHeight", "H:", sprite2DAdapter, s => s.Height, inputSpan: 5)
                   .AddEnumInput<Sprite2DCommandAdapter, BlingoInkType>("SpriteInk", "Ink:", sprite2DAdapter, s => s.Ink, inputSpan: 6)
                   .AddNumericInputFloat("SpriteBlend", "%", sprite2DAdapter, s => s.Blend, showLabel: false);
        }
        composer
               .AddNumericInputInt("SpriteBeginFrame", "StartFrame:", spriteAdapter, s => s.BeginFrame, labelSpan: 3)
               .AddNumericInputInt("SpriteEndFrame", "End:", spriteAdapter, s => s.EndFrame, inputSpan: 1, labelSpan: 3);
        _behaviorList.ClearItems();
        _behaviors.Clear();
        if (sprite2DAdapter != null && sprite2D != null)
        {
            composer
               .AddNumericInputFloat("SpriteRotation", "Rotation:", sprite2DAdapter, s => s.Rotation, labelSpan: 3)
               .AddNumericInputFloat("SpriteSkew", "Skew:", sprite2DAdapter, s => s.Skew, inputSpan: 1, labelSpan: 3)
               .AddColorPicker("SpriteForeColor", "Foreground:", sprite2DAdapter, s => s.ForeColor, inputSpan: 1, labelSpan: 3)
               .AddColorPicker("SpriteBackColor", "Background:", sprite2DAdapter, s => s.BackColor, inputSpan: 1, labelSpan: 3);

            var index = 0;
            _behaviors = sprite2D.Behaviors.ToDictionary(b =>
            {
                index++;
                return $"{index}.{b.Name} {(b.ScriptMember != null ? $"{b.ScriptMember.CastLibNum},{b.ScriptMember.NumberInCast}" : "")}";
            });
        }
        if (sprite is BlingoFrameScriptSprite frameScript && frameScript.Behavior != null)
            _behaviors.Add("1." + frameScript.Behavior.Name + $"{(frameScript.Member != null ? $"{frameScript.Member.CastLibNum},{frameScript.Member.NumberInCast}" : "")}", frameScript.Behavior);

        foreach (var item in _behaviors)
            _behaviorList.AddItem(item.Key, item.Value.Name);

        composer.Finalize();
        wrapContainer
            .AddItem(containerIcons)
            .AddHLine("SpriteSplitterIconHLine", _lastWidh - 10, 5)
            .AddItem(container)
            .AddHLine("SpriteSplitterIconHLine", _lastWidh - 10, 5)
            .AddItem(_behaviorPanel);
    }

    private void DispatchSpriteCommand(BlingoSprite sprite, IReadOnlyList<APropertyValue> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        _commandManager.Handle(new BlingoUpdateSpritePropertiesCommand(BlingoSpriteRef.FromSprite(sprite), changes));
    }

    private void DispatchMemberCommand(IBlingoMember member, IReadOnlyList<APropertyValue> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        _commandManager.Handle(new BlingoUpdateMemberPropertiesCommand(BlingoMemberRef.FromMember(member), changes));
    }

    private abstract class PropertyCommandAdapterBase<TTarget>
    {
        protected PropertyCommandAdapterBase(DirectorPropertyInspectorWindow window, TTarget target)
        {
            Window = window;
            Target = target;
        }

        protected DirectorPropertyInspectorWindow Window { get; }

        protected TTarget Target { get; }

        protected void Dispatch(params APropertyValue[] changes)
        {
            if (changes == null || changes.Length == 0)
                return;

            DispatchChanges(Target, changes);
        }

        protected void Dispatch(IReadOnlyList<APropertyValue> changes)
        {
            if (changes == null || changes.Count == 0)
                return;

            if (changes is APropertyValue[] array)
                DispatchChanges(Target, array);
            else
                DispatchChanges(Target, changes.ToArray());
        }

        protected bool DispatchIfChanged<T>(string propertyName, T currentValue, T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
                return false;

            Dispatch(new APropertyValue(propertyName, newValue));
            return true;
        }

        protected abstract void DispatchChanges(TTarget target, IReadOnlyList<APropertyValue> changes);
    }

    private abstract class SpriteCommandAdapterBase<TSprite> : PropertyCommandAdapterBase<TSprite>
        where TSprite : BlingoSprite
    {
        protected SpriteCommandAdapterBase(DirectorPropertyInspectorWindow window, TSprite sprite)
            : base(window, sprite)
        {
        }

        protected TSprite Sprite => Target;

        protected override void DispatchChanges(TSprite target, IReadOnlyList<APropertyValue> changes)
            => Window.DispatchSpriteCommand(target, changes);
    }

    private sealed class SpriteCommandAdapter : SpriteCommandAdapterBase<BlingoSprite>
    {
        public SpriteCommandAdapter(DirectorPropertyInspectorWindow window, BlingoSprite sprite)
            : base(window, sprite)
        {
        }

        public bool Lock
        {
            get => Sprite.Lock;
            set => DispatchIfChanged(nameof(BlingoSprite.Lock), Sprite.Lock, value);
        }

        public string Name
        {
            get => Sprite.Name;
            set => DispatchIfChanged(nameof(BlingoSprite.Name), Sprite.Name, value);
        }

        public int BeginFrame
        {
            get => Sprite.BeginFrame;
            set => DispatchIfChanged(nameof(BlingoSprite.BeginFrame), Sprite.BeginFrame, value);
        }

        public int EndFrame
        {
            get => Sprite.EndFrame;
            set => DispatchIfChanged(nameof(BlingoSprite.EndFrame), Sprite.EndFrame, value);
        }
    }

    private sealed class Sprite2DCommandAdapter : SpriteCommandAdapterBase<BlingoSprite2D>
    {
        public Sprite2DCommandAdapter(DirectorPropertyInspectorWindow window, BlingoSprite2D sprite)
            : base(window, sprite)
        {
        }

        public bool FlipH
        {
            get => Sprite.FlipH;
            set => DispatchIfChanged(nameof(BlingoSprite2D.FlipH), Sprite.FlipH, value);
        }

        public bool FlipV
        {
            get => Sprite.FlipV;
            set => DispatchIfChanged(nameof(BlingoSprite2D.FlipV), Sprite.FlipV, value);
        }

        public float LocH
        {
            get => Sprite.LocH;
            set
            {
                if (Math.Abs(Sprite.LocH - value) <= float.Epsilon)
                    return;
                DispatchPosition(value, Sprite.LocV, Sprite.LocZ);
            }
        }

        public float LocV
        {
            get => Sprite.LocV;
            set
            {
                if (Math.Abs(Sprite.LocV - value) <= float.Epsilon)
                    return;
                DispatchPosition(Sprite.LocH, value, Sprite.LocZ);
            }
        }

        public float LocZ
        {
            get => Sprite.LocZ;
            set
            {
                var newValue = Convert.ToInt32(value);
                if (Sprite.LocZ == newValue)
                    return;
                DispatchPosition(Sprite.LocH, Sprite.LocV, newValue);
            }
        }

        public float Left
        {
            get => Sprite.Left;
            set
            {
                if (Math.Abs(Sprite.Left - value) <= float.Epsilon)
                    return;

                float delta = value - Sprite.Left;
                float newLocH = Sprite.LocH + delta;
                DispatchPosition(newLocH, Sprite.LocV, Sprite.LocZ);
            }
        }

        public float Top
        {
            get => Sprite.Top;
            set
            {
                if (Math.Abs(Sprite.Top - value) <= float.Epsilon)
                    return;

                float delta = value - Sprite.Top;
                float newLocV = Sprite.LocV + delta;
                DispatchPosition(Sprite.LocH, newLocV, Sprite.LocZ);
            }
        }

        public float Right
        {
            get => Sprite.Right;
            set
            {
                if (Math.Abs(Sprite.Right - value) <= float.Epsilon)
                    return;

                float newLeft = value - Sprite.Width;
                float delta = newLeft - Sprite.Left;
                float newLocH = Sprite.LocH + delta;
                DispatchPosition(newLocH, Sprite.LocV, Sprite.LocZ);
            }
        }

        public float Bottom
        {
            get => Sprite.Bottom;
            set
            {
                if (Math.Abs(Sprite.Bottom - value) <= float.Epsilon)
                    return;

                float newTop = value - Sprite.Height;
                float delta = newTop - Sprite.Top;
                float newLocV = Sprite.LocV + delta;
                DispatchPosition(Sprite.LocH, newLocV, Sprite.LocZ);
            }
        }

        public float Width
        {
            get => Sprite.Width;
            set
            {
                if (Math.Abs(Sprite.Width - value) <= float.Epsilon)
                    return;
                DispatchSize(value, Sprite.Height);
            }
        }

        public float Height
        {
            get => Sprite.Height;
            set
            {
                if (Math.Abs(Sprite.Height - value) <= float.Epsilon)
                    return;
                DispatchSize(Sprite.Width, value);
            }
        }

        public int Ink
        {
            get => Sprite.Ink;
            set => DispatchIfChanged(nameof(BlingoSprite2D.Ink), Sprite.Ink, value);
        }

        public float Blend
        {
            get => Sprite.Blend;
            set => DispatchIfChanged(nameof(BlingoSprite2D.Blend), Sprite.Blend, value);
        }

        public float Rotation
        {
            get => Sprite.Rotation;
            set => DispatchIfChanged(nameof(BlingoSprite2D.Rotation), Sprite.Rotation, value);
        }

        public float Skew
        {
            get => Sprite.Skew;
            set => DispatchIfChanged(nameof(BlingoSprite2D.Skew), Sprite.Skew, value);
        }

        public AColor ForeColor
        {
            get => Sprite.ForeColor;
            set => DispatchIfChanged(nameof(BlingoSprite2D.ForeColor), Sprite.ForeColor, value);
        }

        public AColor BackColor
        {
            get => Sprite.BackColor;
            set => DispatchIfChanged(nameof(BlingoSprite2D.BackColor), Sprite.BackColor, value);
        }

        private void DispatchPosition(float x, float y, int z)
        {
            Dispatch(
                new APropertyValue(nameof(BlingoSprite2D.LocH), x),
                new APropertyValue(nameof(BlingoSprite2D.LocV), y),
                new APropertyValue(nameof(BlingoSprite2D.LocZ), z));
        }

        private void DispatchSize(float width, float height)
        {
            Dispatch(
                new APropertyValue(nameof(BlingoSprite2D.Width), width),
                new APropertyValue(nameof(BlingoSprite2D.Height), height));
        }
    }

    private void CreateBehaviorPanel()
    {
        _behaviorPanel = _factory.CreateWrapPanel(AOrientation.Vertical, "InspectorBehaviors");

        _behaviorList = _factory.CreateItemList("BehaviorList", x =>
        {
            if (x != null && _behaviors.TryGetValue(x, out var behavior))
                _commandManager.Handle(new OpenBehaviorPopupCommand(behavior));
        });
        _behaviorList.Height = 45;
        _behaviorList.Width = _lastWidh - 15;
        _behaviorList.Margin = new AMargin(5, 0, 0, 0);
        _behaviorPanel.AddItem(_behaviorList);

    }


    public IAbstWindowDialogReference? BuildBehaviorPopup(BlingoSpriteBehavior behavior)
        => _descriptionManager.BuildBehaviorPopup(behavior, () =>
        {
            _behaviorList.SelectedIndex = -1;
        });

    public void ShowBehaviorPopup(IAbstWindowDialogReference window)
    {
        _behaviorWindow?.Dialog?.Dispose();
        _behaviorWindow = window;
        //window.PopupCentered();
    }

    public bool CanExecute(OpenBehaviorPopupCommand command) => true;

    public bool Handle(OpenBehaviorPopupCommand command)
    {
        var win = BuildBehaviorPopup(command.Behavior);
        if (win == null) return true;
        ShowBehaviorPopup(win);
        return true;
    }
    #endregion
}
