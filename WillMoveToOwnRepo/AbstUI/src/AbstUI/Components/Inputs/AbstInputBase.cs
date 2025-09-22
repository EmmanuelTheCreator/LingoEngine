using System;
using AbstUI.Styles.Components;

namespace AbstUI.Components.Inputs
{
    /// <summary>
    /// Base class for all engine level input controls.
    /// </summary>
    public abstract class AbstInputBase<TFramework> : AbstNodeLayoutBase<TFramework>
        where TFramework : IAbstFrameworkNodeInput
    {
        /// <summary>Whether the control is enabled.</summary>
        public bool Enabled { get => _framework.Enabled; set => _framework.Enabled = value; }

        /// <summary>Event raised when the input value changes.</summary>
        public event Action? ValueChanged
        {
            add { _framework.ValueChanged += value; }
            remove { _framework.ValueChanged -= value; }
        }

        /// <summary>
        /// Event raised when the user commits the current edits (press enter or focus lost).
        /// </summary>
        public event Action? OnCommit
        {
            add { _framework.OnCommit += value; }
            remove { _framework.OnCommit -= value; }
        }

        protected override void OnSetStyle(AbstComponentStyle componentStyle)
        {
            base.OnSetStyle(componentStyle);
            if (componentStyle is AbstInputStyle style)
            {
                if (style.FontSize.HasValue)
                {
                    if (_framework is IAbstFrameworkInputText text)
                        text.FontSize = style.FontSize.Value;
                    if (_framework is IAbstFrameworkInputNumber number)
                        number.FontSize = style.FontSize.Value;
                }

                if (style.Font is not null && _framework is IAbstFrameworkInputText textWithFont)
                    textWithFont.Font = style.Font;

                if (style.TextColor.HasValue && _framework is IAbstFrameworkInputText textWithColor)
                    textWithColor.TextColor = style.TextColor.Value;
            }
        }

        protected override void OnGetStyle(AbstComponentStyle componentStyle)
        {
            base.OnGetStyle(componentStyle);
            if (componentStyle is AbstInputStyle style)
            {
                if (_framework is IAbstFrameworkInputText text)
                {
                    style.FontSize = text.FontSize;
                    style.Font = text.Font;
                    style.TextColor = text.TextColor;
                }

                if (_framework is IAbstFrameworkInputNumber number)
                    style.FontSize = number.FontSize;
            }
        }
    }
}
