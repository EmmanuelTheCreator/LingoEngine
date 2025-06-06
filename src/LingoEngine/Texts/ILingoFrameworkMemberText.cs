using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Texts
{
    public interface ILingoFrameworkMemberText : ILingoFrameworkMember
    {
        /// <summary>
        /// Indicates whether this image is loaded into memory.
        /// Corresponds to: member.text.loaded
        /// </summary>
        bool IsLoaded { get; }
        string Text { get; set; }
       
    }
}
