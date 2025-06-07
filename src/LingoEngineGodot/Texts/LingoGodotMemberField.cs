using Godot;
using LingoEngine.Texts;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberField : ILingoFrameworkMemberField, IDisposable
    {
        private LingoMemberField _lingoMember;

        private string _text = "";
        private readonly Label _labelNode;
        private readonly CenterContainer _parentNode;
        public Node Node2D => _parentNode;

        public string RAWTextData { get; private set; } = "";
        public string Text { get => _text; set => UpdateText(value); }
        public bool IsLoaded { get; private set; }

#pragma warning disable CS8618 
        public LingoGodotMemberField()
#pragma warning restore CS8618 
        {
            _parentNode = new CenterContainer();
            _labelNode = new Label();
            _parentNode.AddChild(_labelNode);
            var fontFile = GD.Load<FontFile>("res://Fonts/YourFont.ttf");
            var font = new FontFile(); // Or load a `.ttf`/`.otf` from resources
            var labelSettings = new LabelSettings
            {
                Font = fontFile,
                FontColor = new Color(1, 0, 0),
                FontSize = 40,
            };
            _labelNode.LabelSettings = labelSettings;
        }

        internal void Init(LingoMemberField lingoInstance)
        {
            _lingoMember = lingoInstance;
            _parentNode.Name = lingoInstance.Name;
            LoadFile();
        }
        public void LoadFile()
        {
#if DEBUG
            if (_lingoMember.FileName.Contains("blockT") )
            {
            }
#endif
            if (!File.Exists(_lingoMember.FileName))
            {
                GD.PrintErr("File not found for Text :"+_lingoMember.FileName);
                return;
            }
            RAWTextData = File.ReadAllText(_lingoMember.FileName);
            //var error = _image.Load($"res://{_lingoMemberText.FileName}");
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
            _lingoMember.UpdateTextFromFW(value);
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
        public void CopyToClipBoard()
        {
            DisplayServer.ClipboardSet(RAWTextData);
        }
        public void PasteClipBoardInto()
        {
            var _RAWTextData = DisplayServer.ClipboardGet();
            UpdateRAWTextData(_RAWTextData);
        }

        public void ImportFileInto()
        {
        }
    }
}
