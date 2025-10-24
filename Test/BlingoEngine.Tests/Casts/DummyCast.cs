using System;
using System.Collections.Generic;
using System.Linq;
using AbstUI.Primitives;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Members;

namespace BlingoEngine.Tests.Casts;

internal class DummyCast : IBlingoCast
{
    // fields
    private readonly DummyMembersContainer _members = new();
    private IBlingoMember? _lastAddedMember;

    // properties
    public string Name { get; set; } = "Dummy";
    public string FileName { get; set; } = string.Empty;
    public int Number => 1;
    public PreLoadModeType PreLoadMode { get; set; }
    public bool IsInternal => true;
    public CastMemberSelection? Selection { get; set; }
    public IBlingoMembersContainer Member => _members;
    public IBlingoMember? LastAddedMember => _lastAddedMember;

    // constructors
    public DummyCast() { }

    // events
    public event Action<IBlingoMember>? MemberAdded;
    public event Action<IBlingoMember>? MemberDeleted;
    public event Action<IBlingoMember>? MemberNameChanged;

    // methods
    public void Dispose() { }
    public T? GetMember<T>(int number) where T :  IBlingoMember => default;
    public T? GetMember<T>(string name) where T : IBlingoMember => default;
    public int FindEmpty() => 1;
    public IBlingoMember Add(BlingoMemberType type, int numberInCast, string name, string fileName = "", APoint regPoint = default)
    {
        var member = new DummyTextMember(this) { Name = name };
        member.SetFileName(fileName);
        _lastAddedMember = member;
        MemberAdded?.Invoke(member);
        return member;
    }
    public T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T : IBlingoMember => throw new NotImplementedException();
    public IEnumerable<IBlingoMember> GetAll() => Array.Empty<IBlingoMember>();
    public void SwapMembers(int slot1, int slot2) { }
    public void Save() { }

    public IReadOnlyList<int> ResolveFreeSlotNumbers(int startSlot, int requiredCount)
        => Enumerable.Range(startSlot, requiredCount).ToArray();

    private class DummyMembersContainer : IBlingoMembersContainer
    {
        public IBlingoMember? this[int number] => null;
        public IBlingoMember? this[string name] => null;
        public event Action<IBlingoMember>? MemberAdded;
        public event Action<IBlingoMember>? MemberDeleted;
        public T? Member<T>(int number) where T : IBlingoMember => default;
        public T? Member<T>(string name) where T : IBlingoMember => default;
    }
}


