using System;
using AbstUI.Primitives;
using BlingoEngine.Members;
using BlingoEngine.Movies;
using System.Collections.Generic;

namespace BlingoEngine.Sprites
{

    /// <summary>
    /// Lingo Sprite Manager interface.
    /// </summary>
    public interface IBlingoSpriteManager
    {
        int MaxSpriteChannelCount { get; }
        int SpriteTotalCount { get; }
        int SpriteMaxNumber { get; }
        int SpriteNumChannelOffset { get; }
        void MoveSprite(int number, int newFrame);
        void MuteChannel(int channel, bool state);
        BlingoSpritePropertyMutation? RetrievePropertyMutation(BlingoSprite sprite, APropertyValue change);
        BlingoSprite? GetSprite(BlingoSpriteRef spriteRef);
    }
    /// <summary>
    /// Lingo Sprite Manager interface.
    /// </summary>
    public interface IBlingoSpriteManager<TSprite> : IBlingoSpriteManager
    {

        event Action<int>? SpriteListChanged;
        event Action<TSprite>? SpriteAdded;
        event Action<TSprite>? SpriteRemoved;
        event Action? SpritesCleared;

        IEnumerable<TSprite> GetAllSprites();
        IEnumerable<TSprite> GetAllSpritesByChannel(int channel);
        IEnumerable<TSprite> GetAllSpritesBySpriteNumAndChannel(int spriteNumAndChannel);

        void MoveSprite(TSprite sprite, int newFrame);
    }



    public abstract class BlingoSpriteManager : IBlingoSpriteManager
    {
        protected readonly BlingoMovieEnvironment _environment;
        protected readonly BlingoMovie _movie;
        protected readonly List<int> _mutedSprites = new List<int>();


        protected int _maxSpriteNum = 0;
        protected int _maxSpriteChannelCount;

        public abstract int MaxSpriteChannelCount { get; set; }
        public abstract int SpriteTotalCount { get; }
        public abstract int SpriteMaxNumber { get; }
        public int SpriteNumChannelOffset { get; protected set; }

        protected BlingoSpriteManager(BlingoMovie movie, BlingoMovieEnvironment environment)
        {
            _movie = movie;
            _environment = environment;
        }

        public abstract void MoveSprite(int number, int newFrame);
        public abstract void MuteChannel(int channel, bool state);
        public abstract BlingoSprite? GetSprite(BlingoSpriteRef spriteRef);

        public virtual BlingoSpritePropertyMutation? RetrievePropertyMutation(BlingoSprite sprite, APropertyValue change)
        {
            switch (change.PropertyName)
            {
                case nameof(BlingoSprite.Lock) when change.Value is bool newLock && sprite.Lock != newLock:
                {
                    bool oldValue = sprite.Lock;
                    return new BlingoSpritePropertyMutation(
                        () => sprite.Lock = newLock,
                        () => sprite.Lock = oldValue,
                        RequiresStageRefresh: false,
                        IsAnimationProperty: false,
                        NewValue: newLock,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite.Name) when change.Value is string newName && sprite.Name != newName:
                {
                    string oldValue = sprite.Name;
                    return new BlingoSpritePropertyMutation(
                        () => sprite.Name = newName,
                        () => sprite.Name = oldValue,
                        RequiresStageRefresh: false,
                        IsAnimationProperty: false,
                        NewValue: newName,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite.BeginFrame) when change.TryGetInt(out var newBegin) && sprite.BeginFrame != newBegin:
                {
                    int oldValue = sprite.BeginFrame;
                    return new BlingoSpritePropertyMutation(
                        () => sprite.BeginFrame = newBegin,
                        () => sprite.BeginFrame = oldValue,
                        RequiresStageRefresh: false,
                        IsAnimationProperty: false,
                        NewValue: newBegin,
                        OriginalValue: oldValue);
                }

                case nameof(BlingoSprite.EndFrame) when change.TryGetInt(out var newEnd) && sprite.EndFrame != newEnd:
                {
                    int oldValue = sprite.EndFrame;
                    return new BlingoSpritePropertyMutation(
                        () => sprite.EndFrame = newEnd,
                        () => sprite.EndFrame = oldValue,
                        RequiresStageRefresh: false,
                        IsAnimationProperty: false,
                        NewValue: newEnd,
                        OriginalValue: oldValue);
                }
            }

            return null;
        }

        internal abstract void UpdateActiveSprites(int currentFrame, int lastFrame);
        internal abstract void BeginSprites();
        internal virtual BlingoSprite? Add(int spriteNumWithChannel, int begin, int end, IBlingoMember? member)
        {
            return OnAdd(spriteNumWithChannel - SpriteNumChannelOffset, begin, end, member);
        }
        protected abstract BlingoSprite? OnAdd(int spriteNum, int begin, int end, IBlingoMember? member);
        internal abstract void EndSprites();
    }




    public abstract class BlingoSpriteManager<TSprite> : BlingoSpriteManager
        where TSprite : BlingoSprite, IBlingoSpriteBase
    {

        protected readonly Dictionary<int, BlingoSpriteChannel> _spriteChannels = new();
        protected readonly Dictionary<string, TSprite> _spritesByName = new();
        protected readonly List<TSprite> _allTimeSprites = new();
        protected readonly List<TSprite> _newPuppetSprites = new();
        protected readonly Dictionary<int, TSprite> _activeSprites = new();
        protected readonly List<TSprite> _activeSpritesOrdered = new();
        protected readonly List<TSprite> _enteredSprites = new();
        protected readonly List<TSprite> _exitedSprites = new();
        private Dictionary<int, TSprite> _deletedPuppetSpritesCache = new();

        internal List<TSprite> AllTimeSprites => _allTimeSprites;


        public override int MaxSpriteChannelCount
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
                            _spriteChannels.Add(i, new BlingoSpriteChannel(i, _movie));
                    }
                }
            }
        }

        public override int SpriteTotalCount => _activeSprites.Count;
        public override int SpriteMaxNumber => _activeSprites.Keys.DefaultIfEmpty(0).Max();



        public event Action<int>? SpriteListChanged;
        public event Action<TSprite>? SpriteAdded;
        public event Action<TSprite>? SpriteRemoved;
        public event Action? SpritesCleared;
        protected void RaiseSpriteListChanged(int spritenumChannel) => SpriteListChanged?.Invoke(spritenumChannel);
        protected void RaiseSpriteAdded(TSprite sprite) => SpriteAdded?.Invoke(sprite);
        protected void RaiseSpriteRemoved(TSprite sprite) => SpriteRemoved?.Invoke(sprite);
        protected void RaiseSpritesCleared() => SpritesCleared?.Invoke();


        protected BlingoSpriteManager(int spritenumChannelOffset, BlingoMovie movie, BlingoMovieEnvironment environment)
           : base(movie, environment)
        {
            SpriteNumChannelOffset = spritenumChannelOffset;
        }

        public IEnumerable<TSprite> GetAllSprites() => _allTimeSprites.ToArray();
        public IEnumerable<TSprite> GetAllSpritesByChannel(int spriteNum) => _allTimeSprites.Where(x => x.SpriteNum == spriteNum).ToArray();
        public IEnumerable<TSprite> GetAllSpritesBySpriteNumAndChannel(int spriteNumAndChannel) => _allTimeSprites.Where(x => x.SpriteNumWithChannel == spriteNumAndChannel).ToArray();

        public override BlingoSprite? GetSprite(BlingoSpriteRef spriteRef) => FindSprite(spriteRef);

        protected virtual TSprite? FindSprite(BlingoSpriteRef spriteRef)
        {
            TSprite? fallback = null;
            foreach (var sprite in _allTimeSprites)
            {
                if (sprite.SpriteNumWithChannel != spriteRef.SpriteNum)
                    continue;

                if (sprite.BeginFrame == spriteRef.BeginFrame)
                    return sprite;

                fallback ??= sprite;
            }

            return fallback;
        }

        internal TSprite AddSprite(string name, Action<TSprite>? configure = null)
        {
            _maxSpriteNum++;
            var num = _maxSpriteNum;
            return AddSprite(num, name, configure);
        }

        internal TSprite AddSprite(int num, Action<TSprite>? configure = null) => AddSprite(num, "Sprite_" + num, configure);

        internal TSprite AddSprite(int num, string name, Action<TSprite>? configure = null)
        {
            var sprite = OnCreateSprite(_movie, s =>
            {
                // On Remove
                _newPuppetSprites.Remove(s);
                var index = _allTimeSprites.IndexOf(s);
                if (index > -1)
                {
                    _allTimeSprites.RemoveAt(index);
                    _spritesByName.Remove(name);
                    RaiseSpriteListChanged(s.SpriteNumWithChannel);
                    RaiseSpriteRemoved(s);
                    if (_allTimeSprites.Count == 0)
                        RaiseSpritesCleared();
                }
            });
            sprite.Init(num, name);
            _allTimeSprites.Add(sprite);
            if (!_spritesByName.ContainsKey(name))
                _spritesByName.Add(name, sprite);
            if (num > _maxSpriteNum)
                _maxSpriteNum = num;
            SpriteJustCreated(sprite);

            configure?.Invoke(sprite);

            sprite.InitialState = sprite.GetState();

            RaiseSpriteListChanged(sprite.SpriteNumWithChannel);
            RaiseSpriteAdded(sprite);
            return sprite;
        }
        protected abstract TSprite OnCreateSprite(BlingoMovie movie, Action<TSprite> onRemove);

        protected virtual void SpriteJustCreated(TSprite sprite)
        {

        }

        public override void MoveSprite(int number, int newFrame)
        {
            if (number <= 0 || number > _movie.MaxSpriteChannelCount)
                return;
            var sprite = _allTimeSprites[number - 1];
            if (sprite == null) return;
            MoveSprite(sprite, newFrame);
        }
        public void MoveSprite(TSprite sprite, int newFrame)
        {
            if (newFrame <= 0 || newFrame > _movie.FrameCount)
                return;
            var duration = sprite.EndFrame - sprite.BeginFrame;
            sprite.BeginFrame = newFrame;
            sprite.EndFrame = newFrame + duration;
            RaiseSpriteListChanged(sprite.SpriteNumWithChannel);
        }

        internal bool RemoveSprite(string name)
        {
            if (!_spritesByName.TryGetValue(name, out var sprite))
                return false;
            sprite.RemoveMe();
            return true;
        }
        internal bool RemoveSprite(BlingoSprite2D sprite)
        {
            sprite.RemoveMe();
            return true;
        }

        internal bool TryGetAllTimeSprite(string name, out TSprite? sprite)
        {
            if (_spritesByName.TryGetValue(name, out var sprite1))
            {
                sprite = sprite1;
                return true;
            }
            sprite = null;
            return false;
        }

        internal bool TryGetAllTimeSprite(int number, out TSprite? sprite)
        {
            if (number <= 0 || number > _allTimeSprites.Count)
            {
                sprite = null;
                return false;
            }
            sprite = _allTimeSprites[number - 1];
            return true;
        }


        protected void CallActiveSprites(Action<TSprite> actionOnAllActiveSprites)
        {
            foreach (var sprite in _activeSpritesOrdered)
                actionOnAllActiveSprites(sprite);
        }
        protected void CallActiveSprite(int number, Action<TSprite> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return;
            spriteAction(sprite);
        }
        protected bool TryCallActiveSprite(int number, Action<TSprite> spriteAction)
        {
            if (!_activeSprites.ContainsKey(number)) return false;
            var sprite = _activeSprites[number];
            if (sprite == null) return false;
            spriteAction(sprite);
            return true;
        }
        protected TResult? CallActiveSprite<TResult>(int number, Func<TSprite, TResult?> spriteAction)
        {
            var sprite = _activeSprites[number];
            if (sprite == null) return default;
            return spriteAction(sprite);
        }
        internal override void UpdateActiveSprites(int currentFrame, int lastFrame)
        {
            _enteredSprites.Clear();
            _exitedSprites.Clear();

            foreach (var sprite in _activeSpritesOrdered.ToArray()) // make a copy of the array
            {

                bool stillActive = sprite.BeginFrame <= currentFrame && sprite.EndFrame >= currentFrame;
                if (!stillActive || sprite.IsPuppetCached)
                {
                    _exitedSprites.Add(sprite);
                    _activeSprites.Remove(sprite.SpriteNum);
                    _activeSpritesOrdered.Remove(sprite);
                    SpriteExited(sprite);
                }
            }
            foreach (var sprite in _allTimeSprites)
            {
                if (sprite.IsActive) continue;
                var isActive = sprite.BeginFrame <= currentFrame && sprite.EndFrame >= currentFrame;
                if (isActive)
                {
                    if (sprite.IsPuppetCached) continue;
                    sprite.IsActive = true;
                    _enteredSprites.Add(sprite);
                    if (_activeSprites.TryGetValue(sprite.SpriteNum, out var existingSprite))
                        //throw new Exception($"Operlapping sprites:{existingSprite.Name}:{existingSprite.Member?.Name} and {sprite.Name}:{sprite.Member?.Name}");
                        throw new Exception($"Operlapping sprites:{existingSprite.SpriteNum}) {existingSprite.Name} and {sprite.SpriteNum}) {sprite.Name}");
                    _activeSprites.Add(sprite.SpriteNum, sprite);
                    _activeSpritesOrdered.Add(sprite);
                    SpriteEntered(sprite);

                }
            }
            foreach (var sprite in _exitedSprites)
                sprite.IsActive = false;
        }

        protected virtual void SpriteEntered(TSprite sprite)
        {
            //_spriteChannels[sprite.SpriteNum].SetSprite(sprite);
        }
        protected virtual void SpriteExited(TSprite sprite)
        {
            //_spriteChannels[sprite.SpriteNum].RemoveSprite();
        }


        internal override void BeginSprites()
        {
            foreach (var sprite in _enteredSprites)
                OnBeginSprite(sprite);
        }
        protected virtual void OnBeginSprite(TSprite sprite) => sprite.DoBeginSprite();

        internal override void EndSprites()
        {
            foreach (var sprite in _exitedSprites)
                OnEndSprite(sprite);
        }
        protected virtual void OnEndSprite(TSprite sprite) => sprite.DoEndSprite();



        public override void MuteChannel(int channel, bool state)
        {
            if (state)
            {
                if (!_mutedSprites.Contains(channel))
                    _mutedSprites.Add(channel);
            }
            else
            {
                if (_mutedSprites.Contains(channel))
                    _mutedSprites.Remove(channel);
            }
        }

        public int GetNextSpriteStart(int channel, int frame)
        {
            int next = int.MaxValue;
            foreach (var sp in _allTimeSprites)
            {
                if (sp.SpriteNum - 1 == channel && sp.BeginFrame > frame)
                    next = Math.Min(next, sp.BeginFrame);
            }
            return next == int.MaxValue ? -1 : next;
        }

        public int GetPrevSpriteEnd(int channel, int frame)
        {
            int prev = -1;
            foreach (var sp in _allTimeSprites)
            {
                if (sp.SpriteNum - 1 == channel && sp.EndFrame < frame)
                    prev = Math.Max(prev, sp.EndFrame);
            }
            return prev;
        }

        internal void DoPuppetSprites()
        {
            if (_newPuppetSprites.Count == 0) return;
            foreach (var sprite in _newPuppetSprites)
                if (sprite.Puppet) // Important, ensure is still puppet, it can be changed programmatically.
                    OnBeginSprite(sprite);
            _newPuppetSprites.Clear();
        }


        #region Deleted PuppetSprite cache
        internal void PuppetSpriteCacheAdd(int number, TSprite sprite)
        {
            if (!_deletedPuppetSpritesCache.ContainsKey(number))
            {
                sprite.IsPuppetCached = true;
                _deletedPuppetSpritesCache.Add(number, sprite);
                _allTimeSprites.Remove(sprite);
            }
        }

        internal TSprite? PuppetSpriteCacheTryGet(int channel)
        {
            if (_deletedPuppetSpritesCache.TryGetValue(channel, out var sprite))
            {
                sprite.IsPuppetCached = false;
                _allTimeSprites.Add(sprite);
                return sprite;
            }
            return null;
        }

        private void DeleteAllCachedPuppetSprites()
        {
            foreach (var sprite in _deletedPuppetSpritesCache.Values.ToList())
                DeletePuppetSprite(sprite);
            _deletedPuppetSpritesCache.Clear();

        }
        private void DeletePuppetSprite(TSprite sprite)
        {
            //var channel = (BlingoSpriteChannel)Channel(sprite.SpriteNumWithChannel);
            //channel.RemoveSprite();
            sprite.DoEndSprite();
            sprite.RemoveMe();
        }
        #endregion
    }
}

