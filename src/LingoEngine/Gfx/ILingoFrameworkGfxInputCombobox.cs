using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific combo box input.
    /// </summary>
    public interface ILingoFrameworkGfxInputCombobox : ILingoFrameworkGfxNodeInput
    {
        IReadOnlyList<KeyValuePair<string,string>> Items { get; }
        void AddItem(string key, string value);
        void ClearItems();
        int SelectedIndex { get; set; }
        string? SelectedKey { get; set; }
        string? SelectedValue { get; set; }
    }
}
