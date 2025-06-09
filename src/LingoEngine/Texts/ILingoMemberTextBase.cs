
using LingoEngine.Core;

namespace LingoEngine.Texts
{
    public interface ILingoMemberTextBase : ILingoMember
    {
        /// <summary>
        /// The raw text contents of the member. This property can be used to read or write the full contents.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Returns or sets individual lines of the text, 1-based indexing.
        /// </summary>
        ILingoLine Line { get; }

        /// <summary>
        /// Returns or sets individual words of the text, 1-based indexing.
        /// </summary>
        ILingoWord Word { get; }

        /// <summary>
        /// Returns or sets individual characters of the text, 1-based indexing.
        /// </summary>
        ILingoChar Char { get; }
    }

}



