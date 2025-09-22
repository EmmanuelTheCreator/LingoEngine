using AbstUI.Commands;
using BlingoEngine.Members;

namespace BlingoEngine.Members.Commands;

public abstract record BlingoMemberBaseCommand(BlingoMemberRef MemberReference) : IAbstCommand;
