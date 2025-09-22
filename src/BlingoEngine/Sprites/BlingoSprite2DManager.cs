using System;
using AbstUI.Primitives;
using BlingoEngine.Inputs;
using BlingoEngine.Members;
using BlingoEngine.Movies;

namespace BlingoEngine.Sprites
{
    public class BlingoSprite2DManager : BlingoSpriteManager<BlingoSprite2D>, IBlingoSpriteManager<BlingoSprite2D>
    {
        //protected readonly Dictionary<int, BlingoSprite2D> _frameSpriteBehaviors = new();
        protected BlingoStageMouse _blingoMouse;
        
        internal BlingoSprite2DManager(BlingoMovie movie, BlingoMovieEnvironment environment) : base(BlingoSprite2D.SpriteNumOffset, movie, environment)
        {
            _blingoMouse = (BlingoStageMouse)environment.Mouse;
        }
        internal void SetMouse(BlingoStageMouse mouse)
        {
            _blingoMouse = mouse;
        }

        public override BlingoSpritePropertyMutation? RetrievePropertyMutation(BlingoSprite sprite, APropertyValue change)
        {
            var baseResult = base.RetrievePropertyMutation(sprite, change);
            if (baseResult != null)
                return baseResult;

            if (sprite is not BlingoSprite2D sprite2D)
                return null;

            switch (change.PropertyName)
            {
                case nameof(BlingoSprite2D.FlipH) when change.Value is bool newFlipH && sprite2D.FlipH != newFlipH:
                {
                    bool oldValue = sprite2D.FlipH;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.FlipH = newFlipH,
                        () => sprite2D.FlipH = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: false,
                        NewValue: newFlipH,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.FlipV) when change.Value is bool newFlipV && sprite2D.FlipV != newFlipV:
                {
                    bool oldValue = sprite2D.FlipV;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.FlipV = newFlipV,
                        () => sprite2D.FlipV = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: false,
                        NewValue: newFlipV,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.LocH) when change.TryGetFloat(out var newLocH) && !AreClose(sprite2D.LocH, newLocH):
                {
                    float oldValue = sprite2D.LocH;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.LocH = newLocH,
                        () => sprite2D.LocH = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newLocH,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.LocV) when change.TryGetFloat(out var newLocV) && !AreClose(sprite2D.LocV, newLocV):
                {
                    float oldValue = sprite2D.LocV;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.LocV = newLocV,
                        () => sprite2D.LocV = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newLocV,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.LocZ) when change.TryGetInt(out var newLocZ) && sprite2D.LocZ != newLocZ:
                {
                    int oldValue = sprite2D.LocZ;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.LocZ = newLocZ,
                        () => sprite2D.LocZ = oldValue,
                        RequiresStageRefresh: false,
                        IsAnimationProperty: false,
                        NewValue: newLocZ,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.Width) when change.TryGetFloat(out var newWidth) && !AreClose(sprite2D.Width, newWidth):
                {
                    float oldValue = sprite2D.Width;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.Width = newWidth,
                        () => sprite2D.Width = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newWidth,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.Height) when change.TryGetFloat(out var newHeight) && !AreClose(sprite2D.Height, newHeight):
                {
                    float oldValue = sprite2D.Height;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.Height = newHeight,
                        () => sprite2D.Height = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newHeight,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.Ink) when change.TryGetInt(out var newInk) && sprite2D.Ink != newInk:
                {
                    int oldValue = sprite2D.Ink;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.Ink = newInk,
                        () => sprite2D.Ink = oldValue,
                        RequiresStageRefresh: false,
                        IsAnimationProperty: false,
                        NewValue: newInk,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.Blend) when change.TryGetFloat(out var newBlend) && !AreClose(sprite2D.Blend, newBlend):
                {
                    float oldValue = sprite2D.Blend;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.Blend = newBlend,
                        () => sprite2D.Blend = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newBlend,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.Rotation) when change.TryGetFloat(out var newRotation) && !AreClose(sprite2D.Rotation, newRotation):
                {
                    float oldValue = sprite2D.Rotation;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.Rotation = newRotation,
                        () => sprite2D.Rotation = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newRotation,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.Skew) when change.TryGetFloat(out var newSkew) && !AreClose(sprite2D.Skew, newSkew):
                {
                    float oldValue = sprite2D.Skew;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.Skew = newSkew,
                        () => sprite2D.Skew = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newSkew,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.ForeColor) when change.Value is AColor newForeColor && !sprite2D.ForeColor.Equals(newForeColor):
                {
                    var oldValue = sprite2D.ForeColor;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.ForeColor = newForeColor,
                        () => sprite2D.ForeColor = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newForeColor,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite2D.BackColor) when change.Value is AColor newBackColor && !sprite2D.BackColor.Equals(newBackColor):
                {
                    var oldValue = sprite2D.BackColor;
                    return new BlingoSpritePropertyMutation(
                        () => sprite2D.BackColor = newBackColor,
                        () => sprite2D.BackColor = oldValue,
                        RequiresStageRefresh: true,
                        IsAnimationProperty: true,
                        NewValue: newBackColor,
                        OriginalValue: oldValue);
                }
            }

            return null;
        }

        private static bool AreClose(float left, float right) => Math.Abs(left - right) <= 0.0001f;

        public BlingoSprite2D Add(int num, int begin, int end, Action<BlingoSprite2D>? configure = null)
         => AddSprite(num, c =>
         {
             c.BeginFrame = begin;
             c.EndFrame = end;
             c.LocZ = num;
             configure?.Invoke(c);
             if (c.Puppet)
                 _newPuppetSprites.Add(c);
         });
        internal BlingoSprite2D AddSprite(int num, int begin, int end, float x, float y, Action<BlingoSprite2D>? configure = null)
           => AddSprite(num, c =>
           {
               c.BeginFrame = begin;
               c.EndFrame = end;
               c.LocH = x;
               c.LocV = y;
               c.LocZ = num;
               configure?.Invoke(c);
               if (c.Puppet)
                   _newPuppetSprites.Add(c);
           });

        protected override BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member)
        {
            var sprite = AddSprite(spriteNum, begin, end, 0, 0, null);
            if (member != null)
                sprite.SetMember(member);
            return sprite;
        }
        protected override BlingoSprite2D OnCreateSprite(BlingoMovie movie, Action<BlingoSprite2D> onRemove)
        {
            var sprite = _environment.Factory.CreateSprite2D(_movie, onRemove);
            return sprite;
        }
        protected override void SpriteJustCreated(BlingoSprite2D sprite)
        {
            sprite.LocZ = sprite.SpriteNum;
        }
        internal void ChangeSpriteChannel(BlingoSprite2D sprite, int newChannel)
        {
            if (sprite.SpriteNum - 1 == newChannel)
                return;

            int oldChannel = sprite.SpriteNum - 1;
            // _activeSpritesOrdered isn't changing
            _activeSprites.Remove(sprite.SpriteNum);
            _spriteChannels[oldChannel].RemoveSprite();

            var spriteTyped = sprite;
            spriteTyped.ChangeSpriteNumIKnowWhatImDoingOnlyInternal(newChannel + 1);
            _spriteChannels[newChannel].SetSprite(spriteTyped);
            _activeSprites[sprite.SpriteNum] = spriteTyped;

            RaiseSpriteListChanged(sprite.SpriteNum + SpriteNumChannelOffset);
        }


        protected override void SpriteEntered(BlingoSprite2D sprite)
        {
            _spriteChannels[sprite.SpriteNum].SetSprite(sprite);
        }
        protected override void SpriteExited(BlingoSprite2D sprite)
        {
            _spriteChannels[sprite.SpriteNum].RemoveSprite();
        }

        protected override void OnBeginSprite(BlingoSprite2D sprite)
        {
            sprite.FrameworkObj.Show();

            if (!_blingoMouse.IsSubscribed(sprite))
                _blingoMouse.Subscribe(sprite);
            base.OnBeginSprite(sprite);
        }
        protected override void OnEndSprite(BlingoSprite2D sprite)
        {
            base.OnEndSprite(sprite);
            if (_blingoMouse.IsSubscribed(sprite))
                _blingoMouse.Unsubscribe(sprite);

        }


        List<BlingoMember> _changedMembers = new List<BlingoMember>();
        internal void PreStepFrame()
        {
            _changedMembers.Clear();
            foreach (var sprite in _activeSpritesOrdered)
            {
                if (sprite.IsActive)
                {
                    var changedMember = sprite.DoPreStepFrame();
                    if (changedMember != null)
                        _changedMembers.Add(changedMember);
                }
            }
            foreach (var item in _changedMembers)
                item.ChangesHasBeenApplied();
        }


        internal IBlingoSpriteChannel Channel(int channelNumber) => _spriteChannels[channelNumber];
        internal IBlingoSpriteChannel GetActiveSprite(int number) => _spriteChannels[number];


        #region SendSprite
        internal void SendSprite(int spriteNumber, Action<IBlingoSpriteChannel> actionOnSprite)
        {
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.Number == spriteNumber);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }

        internal void SendAllSprites(Action<IBlingoSpriteChannel> actionOnSprite)
        {
            foreach (var channel in _spriteChannels.Values)
            {
                if (channel.Sprite != null)
                    actionOnSprite(channel);
            }
        }

        internal void SendAllSprites<T>(Action<T> actionOnSprite) where T : BlingoSpriteBehavior
        {
            foreach (var sprite in _activeSpritesOrdered)
                sprite.CallBehavior(actionOnSprite);
        }

        internal IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : BlingoSpriteBehavior
        {
            foreach (var sprite in _activeSpritesOrdered)
                yield return sprite.CallBehavior(actionOnSprite);
        }



        internal bool TrySendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
            where T : IBlingoSpriteBehavior
            => TryCallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));
        internal void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
            where T : IBlingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour)
            where T : IBlingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal void SendSprite(string name, Action<IBlingoSpriteChannel> actionOnSprite)
        {
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.Name == name);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }
        #endregion


        internal void SetSpriteMember(int number, string memberName) => CallActiveSprite(number, s => s.SetMember(memberName));
        internal void SetSpriteMember(int number, int memberNumber) => CallActiveSprite(number, s => s.SetMember(memberNumber));


        public bool RollOver(int spriteNumber)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return false;
            return sprite.IsMouseInsideBoundingBox(_blingoMouse);
        }

        public int RollOver()
        {
            var sprite = GetSpriteUnderMouse();
            return sprite?.SpriteNum ?? 0;
        }

        internal int ConstrainH(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)MathCompat.Clamp(pos, sprite.Left, sprite.Right);
        }

        internal int ConstrainV(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)MathCompat.Clamp(pos, sprite.Top, sprite.Bottom);
        }

        internal BlingoSprite2D? GetSpriteUnderMouse(bool skipLockedSprites = false)
            => GetSpritesAtPoint(_blingoMouse.MouseH, _blingoMouse.MouseV, skipLockedSprites).FirstOrDefault();

        internal IEnumerable<BlingoSprite2D> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false)
        {
            var matches = new List<BlingoSprite2D>();
            foreach (var sprite in _activeSpritesOrdered)
            {
                if (skipLockedSprites && sprite.Lock) continue;
                if (sprite.SpriteChannel != null && !sprite.SpriteChannel.Visibility) continue;
                if (sprite.IsPointInsideBoundingBox(x, y))
                    matches.Add(sprite);
            }

            return matches
                .OrderByDescending(s => s.LocH)
                .ThenByDescending(s => s.SpriteNum);
        }

        internal BlingoSprite2D? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false)
            => GetSpritesAtPoint(x, y, skipLockedSprites).FirstOrDefault();

        internal int GetMaxLocZ() => _activeSpritesOrdered.Max(x => x.LocZ);

       
    }
}

