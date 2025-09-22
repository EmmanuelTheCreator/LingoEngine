using System;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Common interface for all framework input controls.
    /// </summary>
    public interface IAbstFrameworkNodeInput : IAbstFrameworkLayoutNode
    {
        /// <summary>Whether the control is enabled.</summary>
        bool Enabled { get; set; }

        /// <summary>Raised whenever the input text/value changes.</summary>
        event Action? ValueChanged;

        /// <summary>Raised when an edit is committed (enter pressed or focus lost).</summary>
        event Action? OnCommit;
    }
}
