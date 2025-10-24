using System.Collections.Generic;
using AbstUI.Components.Containers;
using AbstUI.Primitives;
using AbstUI.Tools;
using BlingoEngine.Bitmaps;
using BlingoEngine.Director.Core.Members.Commands;
using BlingoEngine.FilmLoops;
using BlingoEngine.Members;
using BlingoEngine.Medias;
using BlingoEngine.Shapes;
using BlingoEngine.Sounds;
using BlingoEngine.Texts;
using BlingoEngine.Members.Commands;

namespace BlingoEngine.Director.Core.Inspector;

public partial class DirectorPropertyInspectorWindow
{
    private void AddMemberTabs(IBlingoMember member)
    {
        AddMemberTab(member);
        switch (member)
        {
            case BlingoMemberText text:
                AddTextTab(text);
                break;
            case BlingoMemberField text:
                AddTextTab(text);
                break;
            case BlingoMemberBitmap bitmap:
                AddBitmapTab(bitmap);
                break;
            case BlingoMemberSound sound:
                AddSoundTab(sound);
                break;
            case BlingoMemberShape shape:
                AddShapeTab(shape);
                break;
            case BlingoMemberMedia media:
                AddMediaTab(media);
                break;
            case BlingoFilmLoopMember film:
                AddTab(PropetyTabNames.FilmLoop, film);
                break;
        }
    }

    private void AddMemberTab(IBlingoMember member)
    {
        var wrapContainer = AddTab(PropetyTabNames.Member);
        var container = _factory.CreatePanel("MemberDetailPanel");
        var memberAdapter = new MemberCommandAdapter(this, member);
        wrapContainer
            .AddItem(container);

        container.Compose(_factory.ComponentFactory)
               .Columns(4)
               .AddTextInput("MemberName", "Name:", memberAdapter, s => s.Name, inputSpan: 3)
               .Columns(4)
               .AddLabel("MemberSize", "Size: ", 2)
               .AddLabel("MemberSizeV", CommonExtensions.BytesToShortString(member.Size), 2)
               .AddLabel("MemberCreationDate", "Created: ", 2)
               .AddLabel("MemberCreationDateV", member.CreationDate.ToString("dd/MM/yyyy HH:mm"), 2)
               .AddLabel("MemberModifyDate", "Modified: ", 2)
               .AddLabel("MemberModifyDateV", member.ModifiedDate.ToString("dd/MM/yyyy HH:mm"), 2)
               .Columns(4)
               .AddTextInput("MemberFileName", "FileName:", memberAdapter, s => s.FileName, inputSpan: 3)
               .Columns(4)
               .AddTextInput("MemberComments", "Comments:", memberAdapter, s => s.Comments, inputSpan: 3)
               .Finalize();
    }

    private void DispatchMemberCommand(IBlingoMember member, IReadOnlyList<APropertyValue> changes)
    {
        if (changes == null || changes.Count == 0)
            return;

        _commandManager.Handle(new BlingoUpdateMemberPropertiesCommand(BlingoMemberRef.FromMember(member), changes));
    }

    private abstract class MemberCommandAdapterBase<TMember> : PropertyCommandAdapterBase<TMember>
        where TMember : IBlingoMember
    {
        protected MemberCommandAdapterBase(DirectorPropertyInspectorWindow window, TMember member)
            : base(window, member)
        {
        }

        protected TMember Member => Target;

        protected override void DispatchChanges(TMember target, IReadOnlyList<APropertyValue> changes)
            => Window.DispatchMemberCommand(target, changes);
    }

    private sealed class MemberCommandAdapter : MemberCommandAdapterBase<IBlingoMember>
    {
        public MemberCommandAdapter(DirectorPropertyInspectorWindow window, IBlingoMember member)
            : base(window, member)
        {
        }

        public string Name
        {
            get => Member.Name;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMember.Name), Member.Name, sanitized);
            }
        }

        public string FileName
        {
            get => Member.FileName;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMember.FileName), Member.FileName, sanitized);
            }
        }

        public string Comments
        {
            get => Member.Comments;
            set
            {
                var sanitized = value ?? string.Empty;
                DispatchIfChanged(nameof(BlingoMember.Comments), Member.Comments, sanitized);
            }
        }
    }
}
