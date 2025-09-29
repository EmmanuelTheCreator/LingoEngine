using System;
using System.Collections.Generic;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Core;
using BlingoEngine.FilmLoops;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Scripts;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using BlingoEngine.Transitions;

namespace BlingoEngine.Casts
{
    public class CastMemberSelection
    {
        // Todo
    }

    /// <inheritdoc/>
    public class BlingoCast : IBlingoCast
    {

        private const int MaxCastSlotNumber = 999;

        private readonly BlingoCastLibsContainer _castLibsContainer;
        private readonly IBlingoFrameworkFactory _factory;
        private readonly BlingoMembersContainer _membersContainer = new(false);

        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public string FileName { get; set; } = "";
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public PreLoadModeType PreLoadMode { get; set; } = PreLoadModeType.WhenNeeded;
        /// <inheritdoc/>
        public bool IsInternal { get; }
        /// <inheritdoc/>
        public CastMemberSelection? Selection { get; set; } = null;

        public IBlingoMembersContainer Member => _membersContainer;

        public event Action<IBlingoMember>? MemberAdded { add => _membersContainer.MemberAdded +=value; remove => _membersContainer.MemberAdded -= value; }
        public event Action<IBlingoMember>? MemberDeleted { add => _membersContainer.MemberDeleted += value; remove => _membersContainer.MemberDeleted -= value; }
        public event Action<IBlingoMember>? MemberNameChanged;
        

        internal BlingoCast(BlingoCastLibsContainer castLibsContainer, IBlingoFrameworkFactory factory, string name, bool isInternal)
        {
            _castLibsContainer = castLibsContainer;
            _factory = factory;
            Name = name;
            Number = castLibsContainer.GetNextCastNumber();
            IsInternal = isInternal;
        }

        /// <inheritdoc/>
        public T? GetMember<T>(int number) where T : IBlingoMember => _membersContainer.Member<T>(number);
        /// <inheritdoc/>
        public T? GetMember<T>(string name) where T : IBlingoMember => _membersContainer.Member<T>(name);
        /// <inheritdoc/>
        internal IBlingoCast Add(BlingoMember member)
        {
#if DEBUG
            if (member.Name.Contains("blokred") || member.NumberInCast == 30)
            {

            }
#endif
            _castLibsContainer.AddMember(member);
            _membersContainer.Add(member);
            return this;
        }
        public IBlingoCast Remove(IBlingoMember member)
        {
            member.Dispose();

            _castLibsContainer.RemoveMember(member);
            _membersContainer.Remove(member);
            return this;
        }
        internal void MemberNameHasChanged(string oldName, BlingoMember member)
        {
            _castLibsContainer.MemberNameChanged(oldName, member);
            _membersContainer.MemberNameChanged(oldName, member);
            MemberNameChanged?.Invoke(member);
        }
        /// <inheritdoc/>
        public int FindEmpty() => _membersContainer.FindEmpty();

        public IReadOnlyList<int> ResolveFreeSlotNumbers(int startSlot, int requiredCount)
        {
            if (requiredCount <= 0)
                return Array.Empty<int>();

            var slots = new List<int>(requiredCount);
            var used = new HashSet<int>();

            int start = startSlot;
            if (start < 1 || start > MaxCastSlotNumber)
            {
                var fallback = FindEmpty();
                start = fallback >= 1 && fallback <= MaxCastSlotNumber ? fallback : 1;
            }

            for (int slot = start; slot <= MaxCastSlotNumber && slots.Count < requiredCount; slot++)
            {
                if (_membersContainer[slot] == null && used.Add(slot))
                    slots.Add(slot);
            }

            for (int slot = 1; slot < start && slots.Count < requiredCount; slot++)
            {
                if (_membersContainer[slot] == null && used.Add(slot))
                    slots.Add(slot);
            }

            return slots;
        }
        internal int GetUniqueNumber(int numberInCast)
        {
            //if (Number == 1 ? ((member.CastLibNum - 1) * 131114 : numberInCast : _cast.GetUniqueNumber();
            return _castLibsContainer.GetNextMemberNumber(Number, numberInCast);
        }

        internal void RemoveAll()
        {
            var allMembers = _membersContainer.All;
            foreach (var member in allMembers)
                Remove(member);
        }

        public IBlingoMember Add(BlingoMemberType type, int numberInCast, string name, string fileName = "", APoint regPoint = default)
        {
            switch (type)
            {
                case BlingoMemberType.Bitmap: return _factory.CreateMemberBitmap(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.Sound: return _factory.CreateMemberSound(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.FilmLoop: return _factory.CreateMemberFilmLoop(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.Text: return _factory.CreateMemberText(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.Field: return _factory.CreateMemberField(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.Shape: return _factory.CreateMemberShape(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.Script: return _factory.CreateScript(this, numberInCast, name, fileName, regPoint);
                case BlingoMemberType.Palette: return new BlingoColorPaletteMember(this, numberInCast, name);
                case BlingoMemberType.Transition: return new BlingoTransitionMember(this, numberInCast, name);
                default:
                    return _factory.CreateEmpty(this, numberInCast, name, fileName, regPoint);
            }

        }
        public T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T : IBlingoMember
        {
            var member = (T)Add(GetByType<T>(), numberInCast, name, "", default);
            if (configure != null)
                configure(member);
            
            return member;
        }

        private static BlingoMemberType GetByType<T>()
            where T : IBlingoMember
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(BlingoMemberBitmap):return BlingoMemberType.Bitmap;
                case Type t when t == typeof(BlingoMemberSound):return BlingoMemberType.Sound;
                case Type t when t == typeof(BlingoFilmLoopMember):return BlingoMemberType.FilmLoop;
                case Type t when t == typeof(BlingoMemberText):return BlingoMemberType.Text;
                case Type t when t == typeof(BlingoMemberField):return BlingoMemberType.Field;
                case Type t when t == typeof(BlingoMemberShape):return BlingoMemberType.Shape;
                case Type t when t == typeof(BlingoMemberScript):return BlingoMemberType.Script;
                case Type t when t == typeof(BlingoColorPaletteMember):return BlingoMemberType.Palette;
                case Type t when t == typeof(BlingoTransitionMember):return BlingoMemberType.Transition;
                default:
                    return BlingoMemberType.Unknown;
            }
        }
        public IEnumerable<IBlingoMember> GetAll() => _membersContainer.All;

        public void SwapMembers(int slot1, int slot2)
        {
            if (slot1 == slot2) return;

            var tempSlot = _membersContainer.FindEmpty();
            _membersContainer.ChangeNumber(slot1, tempSlot);
            _membersContainer.ChangeNumber(slot2, slot1);
            _membersContainer.ChangeNumber(tempSlot, slot2);
        }
        public void Save()
        {
            // todo : save castlib 
        }
        public void Dispose() => RemoveAll();
    }
}

