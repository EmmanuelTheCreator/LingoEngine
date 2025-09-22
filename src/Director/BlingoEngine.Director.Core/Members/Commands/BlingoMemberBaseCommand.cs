using AbstUI.Commands;
using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Members.Commands;

public abstract record BlingoMemberBaseCommand(BlingoMemberRef MemberReference) : IAbstCommand;
