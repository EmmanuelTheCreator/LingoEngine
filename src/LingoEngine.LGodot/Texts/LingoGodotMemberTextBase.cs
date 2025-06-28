using Godot;
using LingoEngine.LGodot.Helpers;
using LingoEngine.Primitives;
using LingoEngine.Styles;
using LingoEngine.Texts;
using LingoEngine.Texts.FrameworkCommunication;
using Microsoft.Extensions.Logging;
using System.Resources;

namespace LingoEngine.LGodot.Texts
{
    public abstract class LingoGodotMemberTextBase<TLingoText> : ILingoFrameworkMemberTextBase, IDisposable
         where TLingoText : ILingoMemberTextBase
    {
        protected TLingoText _lingoMemberText;
        protected string _text = "";
        protected ILingoFontManager _fontManager;
        private readonly ILogger _logger;
        protected LabelSettings _LabelSettings = new LabelSettings();
        protected readonly Label _labelNode;
        protected readonly CenterContainer _parentNode;

        public Node Node2D => _parentNode;


        #region Properties

        public string Text { get => _text; set => UpdateText(value); }

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

        public LingoTextStyle FontStyle
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
        public int FontSize
        {
            get => _LabelSettings.FontSize; set
            {

                _LabelSettings.SetLingoFontSize(value);
            }
        }

        private string _fontName = "";
        public string FontName
        {
            get => _fontName;
            set
            {
                _fontName = value;
                _LabelSettings.SetLingoFont(_fontManager, value);
            }
        }


        public bool IsLoaded { get; private set; }
        #endregion

#pragma warning disable CS8618
        public LingoGodotMemberTextBase(ILingoFontManager lingoFontManager, ILogger logger)
#pragma warning restore CS8618
        {
            _fontManager = lingoFontManager;
            _logger = logger;
            _parentNode = new CenterContainer();
            _labelNode = new Label();
            _parentNode.AddChild(_labelNode);
            //var labelSettings = new LabelSettings
            //{
            //    Font = _fontManager.Get<FontFile>("Earth"),
            //    FontColor = new Color(1, 0, 0),
            //    FontSize = 40,
            //};
            _labelNode.LabelSettings = _LabelSettings;
        }
        public void Dispose()
        {
            _labelNode.Dispose();
            _parentNode.Dispose();
        }

        internal void Init(TLingoText lingoInstance)
        {
            _lingoMemberText = lingoInstance;
            if (!string.IsNullOrWhiteSpace(lingoInstance.Name))
                _parentNode.Name = lingoInstance.Name;
        }

        public string ReadText()
        {
            var file = GodotHelper.ReadFile(_lingoMemberText.FileName);
            if (file == null)
            {
                _logger.LogWarning("File not found for Text :" + _lingoMemberText.FileName);
                return "";
            }
            return file;
        }
        public string ReadTextRtf()
        {
            var rtfVersion = GodotHelper.ReadFile(_lingoMemberText.FileName.Replace(".txt", ".rtf"));
            if (rtfVersion == null)
                return "";
            return rtfVersion;
        }
        private void UpdateText(string value)
        {
            if (_text == value) return;
            _text = value;
            _labelNode.Text = value;
        }

        public void Erase()
        {
            Unload();
            IsLoaded = false;
        }


        public void Preload()
        {
            IsLoaded = true;
        }

        public void Unload()
        {
            IsLoaded = false;
        }



        #region Clipboard
        public void ImportFileInto()
        {
        }

        public void CopyToClipboard()
        {
            DisplayServer.ClipboardSet(Text);
        }
        public void PasteClipboardInto()
        {
            var _RAWTextData = DisplayServer.ClipboardGet();
            _lingoMemberText.Text = _RAWTextData;
        }
        public void Copy(string text) => DisplayServer.ClipboardSet(text);
        public string PasteClipboard() => DisplayServer.ClipboardGet();
        #endregion
    }
}
