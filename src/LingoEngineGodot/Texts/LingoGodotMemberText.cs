using Godot;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Texts;
using LingoEngine.Tools;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberText : ILingoFrameworkMemberText, IDisposable
    {
        private LingoMemberText _lingoMemberText;

        private string _text = "";
        private ILingoFontManager _fontManager;
        private readonly Label _labelNode;
        private readonly CenterContainer _parentNode;
        public Node Node2D => _parentNode;

        public string RAWTextData { get; private set; } = "";
        public string Text { get => _text; set => UpdateText(value); }
        public bool IsLoaded { get; private set; }

#pragma warning disable CS8618 
        public LingoGodotMemberText(ILingoFontManager lingoFontManager)
#pragma warning restore CS8618 
        {
            _fontManager = lingoFontManager;
            _parentNode = new CenterContainer();
            _labelNode = new Label();
            _parentNode.AddChild(_labelNode);
            var labelSettings = new LabelSettings
            {
                Font = _fontManager.Get<FontFile>("Earth"),
                FontColor = new Color(1, 0, 0),
                FontSize = 40,
            };
            _labelNode.LabelSettings = labelSettings;
        }

     
        internal void Init(LingoMemberText lingoInstance)
        {
            _lingoMemberText = lingoInstance;
            _parentNode.Name = lingoInstance.Name;
            LoadFile();
        }
        public void LoadFile()
        {
#if DEBUG
            if (_lingoMemberText.FileName.Contains("blockT") )
            {
            }
#endif
            if (!File.Exists(_lingoMemberText.FileName))
            {
                GD.PrintErr("File not found for Text :"+_lingoMemberText.FileName);
                return;
            }
            RAWTextData = File.ReadAllText(_lingoMemberText.FileName);
            var rtfVersion = _lingoMemberText.FileName.Replace(".txt", ".rtf");
            _labelNode.TryParseRtfFont(rtfVersion, _fontManager);
            UpdateRAWTextData(RAWTextData);
        }
        private void UpdateRAWTextData(string rawTextData)
        {
            RAWTextData = rawTextData;
            Text = rawTextData;
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
            RAWTextData = "";
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

        public void Dispose()
        {
            _labelNode.Dispose();
            _parentNode.Dispose();
        }
        public void CopyToClipboard()
        {
            DisplayServer.ClipboardSet(RAWTextData);
        }
        public void PasteClipboardInto()
        {
            var _RAWTextData = DisplayServer.ClipboardGet();
            UpdateRAWTextData(_RAWTextData);
        }

        public void ImportFileInto()
        {
        }
    } 
}
