using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a combobox input.
    /// </summary>
    public class LingoInputCombobox : LingoInputBase<ILingoFrameworkGfxInputCombobox>
    {
        public IReadOnlyList<KeyValuePair<string,string>> Items => _framework.Items;
        public void AddItem(string key, string value) => _framework.AddItem(key, value);
        public void ClearItems() => _framework.ClearItems();
        public int SelectedIndex { get => _framework.SelectedIndex; set => _framework.SelectedIndex = value; }
        public string? SelectedKey { get => _framework.SelectedKey; set => _framework.SelectedKey = value; }
        public string? SelectedValue { get => _framework.SelectedValue; set => _framework.SelectedValue = value; }
    }
}
