
using LingoEngine.Core;
using LingoEngine.Primitives;
using LingoEngine.Tools;

namespace LingoEngine.Texts
{

    public class LingoMemberField : LingoMemberTextBase<ILingoFrameworkMemberField>, ILingoMemberField
    {
      
        private bool _isFocused;

       
        #region Properties

        public bool Editable { get; set; }
        public bool MultiLine { get; set; }
        public bool WordWrap
        {
            get => _frameworkMember.WordWrap;
            set => _frameworkMember.WordWrap = value;
        }

        public int ScrollTop
        {
            get => _frameworkMember.ScrollTop;
            set => _frameworkMember.ScrollTop = value;
        }

        public string TextFont
        {
            get => _frameworkMember.TextFont;
            set => _frameworkMember.TextFont = value;
        }

        public int TextSize
        {
            get => _frameworkMember.TextSize;
            set => _frameworkMember.TextSize = value;
        }


        public LingoColor TextColor
        {
            get => _frameworkMember.TextColor;
            set => _frameworkMember.TextColor = value;
        }

        public LingoTextStyle TextStyle
        {
            get => _frameworkMember.TextStyle;
            set => _frameworkMember.TextStyle = value;
        }
        public bool Bold
        {
            get => (_frameworkMember.TextStyle & LingoTextStyle.Bold) != 0;
            set
            {
                var style = _frameworkMember.TextStyle;
                if (value)
                    style |= LingoTextStyle.Bold;
                else
                    style &= ~LingoTextStyle.Bold;
                _frameworkMember.TextStyle = style;
            }
        }

        public bool Italic
        {
            get => (_frameworkMember.TextStyle & LingoTextStyle.Italic) != 0;
            set
            {
                var style = _frameworkMember.TextStyle;
                if (value)
                    style |= LingoTextStyle.Italic;
                else
                    style &= ~LingoTextStyle.Italic;
                _frameworkMember.TextStyle = style;
            }
        }

        public bool Underline
        {
            get => (_frameworkMember.TextStyle & LingoTextStyle.Underline) != 0;
            set
            {
                var style = _frameworkMember.TextStyle;
                if (value)
                    style |= LingoTextStyle.Underline;
                else
                    style &= ~LingoTextStyle.Underline;
                _frameworkMember.TextStyle = style;
            }
        }
        public LingoTextAlignment Alignment
        {
            get => _frameworkMember.Alignment;
            set => _frameworkMember.Alignment = value;
        }

        public int Margin
        {
            get => _frameworkMember.Margin;
            set => _frameworkMember.Margin = value;
        }


        public bool IsFocused => _isFocused;

        #endregion


        public LingoMemberField(LingoCast cast, ILingoFrameworkMemberField frameworkMember, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default) : base(LingoMemberType.Field, cast, frameworkMember, numberInCast, name, fileName, regPoint)
        {
        }

        public override void LoadFile()
        {
            base.LoadFile();
            var rtf = _frameworkMember.ReadTextRtf();
            if (!string.IsNullOrWhiteSpace(rtf))
            {
                var rtfInfo = RtfExtracter.Parse(rtf);
                if (rtfInfo != null)
                {
                    if (rtfInfo.Size > 0) TextSize = rtfInfo.Size;
                    if (rtfInfo.Color != null) TextColor = rtfInfo.Color.Value;
                    if (!string.IsNullOrWhiteSpace(rtfInfo.FontName)) TextFont = rtfInfo.FontName;
                }
            }
        }


        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new LingoMemberText(_cast, _lingoFrameworkMember, newNumber, Name);
            //clone.Text = Text;
            //return clone;
        }
        /// <summary>




    }

}



