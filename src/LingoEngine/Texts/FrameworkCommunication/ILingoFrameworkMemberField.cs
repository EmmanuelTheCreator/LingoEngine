
using LingoEngine.Primitives;

namespace LingoEngine.Texts
{
    public interface ILingoFrameworkMemberField : ILingoFrameworkMemberTextBase
    {

        /// <summary>
        /// Enables or disables word wrapping in the field.
        /// </summary>
        bool WordWrap { get; set; }

        /// <summary>
        /// Gets or sets the scroll position of the field.
        /// </summary>
        int ScrollTop { get; set; }
        string TextFont { get; set; }
        int TextSize { get; set; }
        LingoTextStyle TextStyle { get; set; }
        LingoColor TextColor { get; set; }
        LingoTextAlignment Alignment { get; set; }
        int Margin { get; set; }

       
    }
}
