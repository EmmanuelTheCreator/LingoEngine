using Godot;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Primitives;
using LingoEngine.Texts;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberField : ILingoFrameworkMemberField, IDisposable
    {
        private LingoMemberField _lingoMember;

        private string _text = "";
        private readonly Label _labelNode;
        private readonly CenterContainer _parentNode;
        private readonly ILingoFontManager _fontManager;

        public Node Node2D => _parentNode;

        public string RAWTextData { get; private set; } = "";
        public string Text { get => _text; set => UpdateText(value); }
        public bool IsLoaded { get; private set; }
        public bool WordWrap { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int ScrollTop { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string TextFont { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int TextSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public LingoTextStyle TextStyle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public LingoColor TextColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int FontSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public LingoTextAlignment Alignment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Margin { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

#pragma warning disable CS8618
        public LingoGodotMemberField(ILingoFontManager fontManager)
#pragma warning restore CS8618 
        {
            _fontManager = fontManager;
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
            var rtfVersion = _lingoMember.FileName.Replace(".txt", ".rtf");
            _labelNode.TryParseRtfFont(rtfVersion, _fontManager);
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

        public void SetSelection(int start, int end)
        {
            throw new NotImplementedException();
        }

        public void ReplaceSelection(string replacement)
        {
            throw new NotImplementedException();
        }

        public void InsertText(string text)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Copy(string text)
        {
            throw new NotImplementedException();
        }

        public void Cut()
        {
            throw new NotImplementedException();
        }

        public void Paste()
        {
            throw new NotImplementedException();
        }

        public string PasteClipboard()
        {
            throw new NotImplementedException();
        }
    }
}
