using System.Data;

namespace LingoEngine.Texts
{
    public class LingoWord : ILingoWord
        {
            private bool _hasParsed = false;
            private string _text = "";
            private string[] _words = [];
            public string this[int index]
            {
                get
                {
                    if (!_hasParsed) Parse();
                    return _words[index - 1];
                }
            }

            public int Count => _words.Length;
            public LingoWord(string text)
            {
                SetText(text);
            }

            public override string ToString() => string.Join(Environment.NewLine, _words);

            internal void SetText(string text)
            {
                _text = text;
                _hasParsed = false;
                
            }
            private void Parse()
            {
                _words = _text.Split([' ', '.', '?', '!', ';', ':', '=', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
                           .Where(word => word != "")
                           .ToArray();
                _hasParsed = true;
            }
        }

    }

 

