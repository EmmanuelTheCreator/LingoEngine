using Godot;
using LingoEngine.Texts;

namespace LingoEngineGodot.Texts
{
    public class LingoGodotMemberText : ILingoFrameworkMemberText, IDisposable
    {
#pragma warning disable CS8618 
        private LingoMemberText _lingoMemberText;
#pragma warning restore CS8618 

        private string _text = "";
        private readonly Label _labelNode;
        private readonly CenterContainer _parentNode;
        public Node Node2D => _parentNode;

        public string RAWTextData { get; private set; } = "";
        public string Text { get => _text; set => UpdateText(value); }
        public bool IsLoaded { get; private set; }

        public LingoGodotMemberText()
        {
            _parentNode = new CenterContainer();
            _labelNode = new Label();
            _parentNode.AddChild(_labelNode);
        }

        internal void Init(LingoMemberText lingoInstance)
        {
            _lingoMemberText = lingoInstance;
            _parentNode.Name = lingoInstance.Name;
            LoadFile();
        }
        /// <summary>
        /// Creates an ImageTexture from the ImageData byte array.
        /// </summary>
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
            _lingoMemberText.UpdateTextFromFW(value);
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
