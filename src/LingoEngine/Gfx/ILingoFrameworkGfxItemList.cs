using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific list widget allowing selection of items.
    /// </summary>
    public interface ILingoFrameworkGfxItemList : ILingoFrameworkGfxNodeInput
    {
        /// <summary>Current items in the list.</summary>
        IReadOnlyList<KeyValuePair<string,string>> Items { get; }
        /// <summary>Adds an item to the list.</summary>
        void AddItem(string key, string value);
        /// <summary>Removes all items.</summary>
        void ClearItems();
        /// <summary>Selected item index.</summary>
        int SelectedIndex { get; set; }
        /// <summary>Selected item key.</summary>
        string? SelectedKey { get; set; }
        /// <summary>Selected item value.</summary>
        string? SelectedValue { get; set; }
    }
}
