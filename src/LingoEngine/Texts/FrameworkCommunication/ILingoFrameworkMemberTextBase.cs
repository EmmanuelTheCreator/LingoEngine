using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Texts
{
    public interface ILingoFrameworkMemberTextBase : ILingoFrameworkMember
    {
        /// <summary>
        /// The raw text contents of the member. This property can be used to read or write the full contents.
        /// </summary>
        string Text { get; set; }

    }
}
