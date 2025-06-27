using System;
using System.Collections.Generic;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxInputCombobox : ILingoFrameworkGfxInputCombobox, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;

        private readonly List<KeyValuePair<string,string>> _items = new();
        public IReadOnlyList<KeyValuePair<string,string>> Items => _items;
        public int SelectedIndex { get; set; } = -1;
        public string? SelectedKey { get; set; }
        public string? SelectedValue { get; set; }

        public event Action? ValueChanged;

        public void AddItem(string key, string value)
        {
            _items.Add(new KeyValuePair<string,string>(key,value));
        }

        public void ClearItems()
        {
            _items.Clear();
            SelectedIndex = -1;
            SelectedKey = null;
            SelectedValue = null;
        }

        public void Dispose() { }
    }
}
