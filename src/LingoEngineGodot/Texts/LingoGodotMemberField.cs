using Godot;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberField : LingoGodotMemberTextBase<LingoMemberField> , ILingoFrameworkMemberField
    {


        public bool WordWrap
        {
            get => _labelNode.AutowrapMode != TextServer.AutowrapMode.Off;
            set => _labelNode.AutowrapMode = value ? TextServer.AutowrapMode.Word : TextServer.AutowrapMode.Off;
        }

        public int ScrollTop
        {
            get => _labelNode.LinesSkipped;
            set => _labelNode.LinesSkipped = value;
        }

        private LingoTextStyle _textStyle = LingoTextStyle.None;

        public LingoTextStyle TextStyle
        {
            get => _textStyle;
            set
            {
                _textStyle = value;

                // todo : implement a way for rtf

                //var rtl = new RichTextLabel();
                //// Bold/Italic handled via FontStyle
                //TextServer.FontStyle style = TextServer.FontStyle.Normal;

                //if (value.HasFlag(LingoTextStyle.Bold))
                //    style |= TextServer.FontStyle.Bold;

                //if (value.HasFlag(LingoTextStyle.Italic))
                //    style |= TextServer.FontStyle.Italic;

                //rtl.FontStyle = style;

                //// Underline handled separately
                //_LabelSettings.UnderlineMode = value.HasFlag(LingoTextStyle.Underline)
                //    ? UnderlineMode.Always
                //    : UnderlineMode.Disabled;
            }
        }

        private int _margin = 0;
        public int Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                _labelNode.AddThemeConstantOverride("margin_left", value);
                _labelNode.AddThemeConstantOverride("margin_right", value);
                _labelNode.AddThemeConstantOverride("margin_top", value);
                _labelNode.AddThemeConstantOverride("margin_bottom", value);
            }
        }
        public LingoTextAlignment Alignment
        {
            get
            {
                return _labelNode.HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => LingoTextAlignment.Left,
                    HorizontalAlignment.Center => LingoTextAlignment.Center,
                    HorizontalAlignment.Right => LingoTextAlignment.Right,
                    _ => LingoTextAlignment.Left // Default fallback
                };
            }
            set
            {
                _labelNode.HorizontalAlignment = value switch
                {
                    LingoTextAlignment.Left => HorizontalAlignment.Left,
                    LingoTextAlignment.Center => HorizontalAlignment.Center,
                    LingoTextAlignment.Right => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Left // Default fallback
                };
            }
        }

        private LingoColor _lingoColor = LingoColor.FromRGB(0, 0, 0);
        public LingoColor TextColor
        {
            get => _lingoColor; set
            {
                _lingoColor = value;
                _LabelSettings.SetLingoColor(value);
            }
        }
        public int TextSize
        {
            get => _LabelSettings.FontSize; set
            {
                
                _LabelSettings.SetLingoFontSize(value);
            }
        }

        private string _fontName = "";
        public string TextFont { get => _fontName;
            set
            {
                _fontName = value;
                _LabelSettings.SetLingoFont(_fontManager, value);
            }
        }

        public LingoGodotMemberField(ILingoFontManager lingoFontManager) : base(lingoFontManager)
        {
        }

    }
}
