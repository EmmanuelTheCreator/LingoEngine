using LingoEngine.Inputs;
using LingoEngine.Sprites;
using LingoEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LingoEngine.Movies
{
    internal class LingoSpriteManager
    {
        private readonly LingoMovieEnvironment _environment;
        private readonly LingoMovie _movie;
        private readonly LingoMouse _lingoMouse;
        private readonly Action _raiseSpriteListChanged;

        private int _maxSpriteNum = 0;
        private int _maxSpriteChannelCount;
        private readonly Dictionary<int, LingoSpriteChannel> _spriteChannels = new();
        private readonly Dictionary<string, LingoSprite> _spritesByName = new();
        private readonly List<LingoSprite> _allTimeSprites = new();
        private readonly Dictionary<int, LingoSprite> _frameSpriteBehaviors = new();
        private readonly Dictionary<int, LingoSprite> _activeSprites = new();
        private readonly List<LingoSprite> _enteredSprites = new();
        private readonly List<LingoSprite> _exitedSprites = new();
        private LingoSprite? _currentFrameSprite;

        internal List<LingoSprite> AllTimeSprites => _allTimeSprites;

        internal LingoSpriteManager(LingoMovie movie, LingoMovieEnvironment environment, Action raiseSpriteListChanged)
        {
            _movie = movie;
            _environment = environment;
            _lingoMouse = (LingoMouse)environment.Mouse;
            _raiseSpriteListChanged = raiseSpriteListChanged;
        }

        internal int MaxSpriteChannelCount
        {
            get => _maxSpriteChannelCount;
            set
            {
                if (value > 0)
                {
                    _maxSpriteChannelCount = value;
                    if (_spriteChannels.Count < _maxSpriteChannelCount)
                    {
                        for (int i = _spriteChannels.Count; i < _maxSpriteChannelCount; i++)
                            _spriteChannels.Add(i, new LingoSpriteChannel(i));
                    }
                }
            }
        }

        internal int SpriteTotalCount => _activeSprites.Count;
        internal int SpriteMaxNumber => _activeSprites.Keys.DefaultIfEmpty(0).Max();

        internal ILingoSpriteChannel Channel(int channelNumber) => _spriteChannels[channelNumber];
        internal ILingoSpriteChannel GetActiveSprite(int number) => _spriteChannels[number];

        internal LingoSprite AddSprite(string name, Action<LingoSprite>? configure = null) => AddSprite<LingoSprite>(name, configure);

        internal T AddSprite<T>(string name, Action<LingoSprite>? configure = null) where T : LingoSprite
        {
            _maxSpriteNum++;
            var num = _maxSpriteNum;
            return AddSprite<T>(num, name, configure);
        }

        internal LingoSprite AddSprite(int num, Action<LingoSprite>? configure = null) => AddSprite<LingoSprite>(num, configure);

        internal LingoSprite AddFrameBehavior<TBehaviour>(int frameNumber, Action<TBehaviour>? configureBehaviour = null, Action<LingoSprite>? configure = null) where TBehaviour : LingoSpriteBehavior
        {
            var sprite = _environment.Factory.CreateSprite<LingoSprite>(_movie, s =>
            {
                _frameSpriteBehaviors.Remove(frameNumber);
                _raiseSpriteListChanged();
            });
            sprite.Init(0, $"FrameSprite_{frameNumber}");
            if (_frameSpriteBehaviors.ContainsKey(frameNumber))
                _frameSpriteBehaviors[frameNumber] = sprite;
            else
                _frameSpriteBehaviors.Add(frameNumber, sprite);
            sprite.BeginFrame = frameNumber;
            sprite.EndFrame = frameNumber;

            var behaviour = sprite.SetBehavior<TBehaviour>();
            configureBehaviour?.Invoke(behaviour);
            configure?.Invoke(sprite);
            _raiseSpriteListChanged();
            return sprite;
        }

        internal LingoSprite AddSprite(int num, int begin, int end, float x, float y, Action<LingoSprite>? configure = null)
            => AddSprite<LingoSprite>(num, c =>
            {
                c.BeginFrame = begin;
                c.EndFrame = end;
                c.LocH = x; c.LocV = y;
                configure?.Invoke(c);
            });

        internal T AddSprite<T>(int num, Action<LingoSprite>? configure = null) where T : LingoSprite => AddSprite<T>(num, "Sprite_" + num, configure);

        internal T AddSprite<T>(int num, string name, Action<LingoSprite>? configure = null) where T : LingoSprite
        {
            var sprite = _environment.Factory.CreateSprite<T>(_movie, s =>
            {
                var index = _allTimeSprites.IndexOf(s);
                _allTimeSprites.RemoveAt(index);
                _spritesByName.Remove(name);
                _raiseSpriteListChanged();
            });
            sprite.Init(num, name);
            _allTimeSprites.Add(sprite);
            if (!_spritesByName.ContainsKey(name))
                _spritesByName.Add(name, sprite);
            if (num > _maxSpriteNum)
                _maxSpriteNum = num;
            configure?.Invoke(sprite);
            _raiseSpriteListChanged();
            return sprite;
        }

        internal bool RemoveSprite(string name)
        {
            if (!_spritesByName.TryGetValue(name, out var sprite))
                return false;
            sprite.RemoveMe();
            return true;
        }

        internal bool TryGetAllTimeSprite(string name, out ILingoSprite? sprite)
        {
            if (_spritesByName.TryGetValue(name, out var sprite1))
            {
                sprite = sprite1;
                return true;
            }
            sprite = null;
            return false;
        }

        internal bool TryGetAllTimeSprite(int number, out ILingoSprite? sprite)
        {
            if (number <= 0 || number > _allTimeSprites.Count)
            {
                sprite = null;
                return false;
            }
            sprite = _allTimeSprites[number - 1];
            return true;
        }

        internal void SetSpriteMember(int number, string memberName) => CallActiveSprite(number, s => s.SetMember(memberName));
        internal void SetSpriteMember(int number, int memberNumber) => CallActiveSprite(number, s => s.SetMember(memberNumber));

        internal void PuppetSprite(int number, bool isPuppetSprite) => CallActiveSprite(number, sprite => sprite.Puppet = isPuppetSprite);

        internal void SendSprite<T>(int spriteNumber, Action<T> actionOnSpriteBehaviour)
            where T : LingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal TResult? SendSprite<T, TResult>(int spriteNumber, Func<T, TResult> actionOnSpriteBehaviour)
            where T : LingoSpriteBehavior
            => CallActiveSprite(spriteNumber, s => s.CallBehavior(actionOnSpriteBehaviour));

        internal void SendSprite(string name, Action<ILingoSpriteChannel> actionOnSprite)
        {
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.Name == name);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }

        internal void SendSprite(int spriteNumber, Action<ILingoSpriteChannel> actionOnSprite)
        {
            var sprite = _spriteChannels.Values.FirstOrDefault(x => x.SpriteNum == spriteNumber);
            if (sprite == null) return;
            actionOnSprite(sprite);
        }

        internal void SendAllSprites(Action<ILingoSpriteChannel> actionOnSprite)
        {
            foreach (var channel in _spriteChannels.Values)
            {
                if (channel.Sprite != null)
                    actionOnSprite(channel);
            }
        }

        internal void SendAllSprites<T>(Action<T> actionOnSprite) where T : LingoSpriteBehavior
        {
            foreach (var sprite in _activeSprites.Values)
                sprite.CallBehavior(actionOnSprite);
        }

        internal IEnumerable<TResult?> SendAllSprites<T, TResult>(Func<T, TResult> actionOnSprite) where T : LingoSpriteBehavior
        {
            foreach (var sprite in _activeSprites.Values)
                yield return sprite.CallBehavior(actionOnSprite);
        }

        internal bool RollOver(int spriteNumber)
        {
            var sprite = _activeSprites[spriteNumber];
            return sprite.IsMouseInsideBoundingBox(_lingoMouse);
        }

        internal int RollOver()
        {
            var sprite = GetSpriteUnderMouse();
            return sprite?.SpriteNum ?? 0;
        }

        internal int ConstrainH(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)Math.Clamp(pos, sprite.Left, sprite.Right);
        }

        internal int ConstrainV(int spriteNumber, int pos)
        {
            if (!_activeSprites.TryGetValue(spriteNumber, out var sprite))
                return pos;
            return (int)Math.Clamp(pos, sprite.Top, sprite.Bottom);
        }

        internal LingoSprite? GetSpriteUnderMouse(bool skipLockedSprites = false)
            => GetSpritesAtPoint(_lingoMouse.MouseH, _lingoMouse.MouseV, skipLockedSprites).FirstOrDefault();

        internal IEnumerable<LingoSprite> GetSpritesAtPoint(float x, float y, bool skipLockedSprites = false)
        {
            var matches = new List<LingoSprite>();
            foreach (var sprite in _activeSprites.Values)
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

        internal LingoSprite? GetSpriteAtPoint(float x, float y, bool skipLockedSprites = false)
            => GetSpritesAtPoint(x, y, skipLockedSprites).FirstOrDefault();

        private void CallActiveSprites(Action<LingoSprite> actionOnAllActiveSprites)
        {
            foreach (var sprite in _activeSprites.Values)
                actionOnAllActiveSprites(sprite);
        }
        private void CallActiveSprite(int number, Action<LingoSprite> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return;
            spriteAction(sprite);
        }
        private TResult? CallActiveSprite<TResult>(int number, Func<LingoSprite, TResult?> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return default;
            return spriteAction(sprite);
        }

        internal void ChangeSpriteChannel(ILingoSprite sprite, int newChannel)
        {
            if (sprite.SpriteNum - 1 == newChannel)
                return;

            int oldChannel = sprite.SpriteNum - 1;
            _activeSprites.Remove(sprite.SpriteNum);
            _spriteChannels[oldChannel].RemoveSprite();

            var spriteTyped = (LingoSprite)sprite;
            spriteTyped.ChangeSpriteNumIKnowWhatImDoingOnlyInternal(newChannel + 1);
            _spriteChannels[newChannel].SetSprite(spriteTyped);
            _activeSprites[sprite.SpriteNum] = spriteTyped;

            _raiseSpriteListChanged();
        }

        internal void UpdateActiveSprites(int currentFrame, int lastFrame)
        {
            _enteredSprites.Clear();
            _exitedSprites.Clear();

            foreach (var sprite in _allTimeSprites)
            {
                if (sprite == null) continue;
                sprite.IsActive = sprite.BeginFrame <= currentFrame && sprite.EndFrame >= currentFrame;

                bool wasActive = sprite.BeginFrame <= lastFrame && sprite.EndFrame >= lastFrame;
                bool isActive = sprite.IsActive;

                if (!wasActive && isActive)
                {
                    sprite.FrameworkObj.Show();
                    _enteredSprites.Add(sprite);
                    if (_activeSprites.TryGetValue(sprite.SpriteNum, out var existingSprite))
                        throw new Exception($"Operlapping sprites:{existingSprite.Name}:{existingSprite.Member?.Name} and {sprite.Name}:{sprite.Member?.Name}");
                    _spriteChannels[sprite.SpriteNum].SetSprite(sprite);
                    _activeSprites.Add(sprite.SpriteNum, sprite);
                    if (!_lingoMouse.IsSubscribed(sprite))
                        _lingoMouse.Subscribe(sprite);
                }
                else if (wasActive && !isActive)
                    _exitedSprites.Add(sprite);
            }

            if (_frameSpriteBehaviors.TryGetValue(currentFrame, out var frameSprite))
                _currentFrameSprite = frameSprite;
            else
                _currentFrameSprite = null;
        }

        internal void BeginSprites()
        {
            foreach (var sprite in _enteredSprites)
                sprite.DoBeginSprite();

            _currentFrameSprite?.DoBeginSprite();
        }

        internal void EndSprites()
        {
            _currentFrameSprite?.DoEndSprite();
            foreach (var sprite in _exitedSprites)
            {
                sprite.FrameworkObj.Hide();
                sprite.DoEndSprite();
                _activeSprites.Remove(sprite.SpriteNum);
                _spriteChannels[sprite.SpriteNum].RemoveSprite();
                if (_lingoMouse.IsSubscribed(sprite))
                    _lingoMouse.Unsubscribe(sprite);
            }
        }

        internal int GetMaxLocZ() => _activeSprites.Values.Max(x => x.LocZ);

        internal IReadOnlyDictionary<int, LingoSprite> FrameSpriteBehaviors => _frameSpriteBehaviors;

        internal void MoveFrameBehavior(int previousFrame, int newFrame)
        {
            if (previousFrame == newFrame) return;
            if (!_frameSpriteBehaviors.TryGetValue(previousFrame, out var sprite))
                return;

            _frameSpriteBehaviors.Remove(previousFrame);

            if (_frameSpriteBehaviors.TryGetValue(newFrame, out var existing))
                existing.RemoveMe();

            _frameSpriteBehaviors[newFrame] = sprite;
            sprite.BeginFrame = newFrame;
            sprite.EndFrame = newFrame;

            _raiseSpriteListChanged();
        }
    }
}
