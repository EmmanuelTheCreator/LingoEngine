using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Texts;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Texts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace BlingoEngine.Director.Core.Texts;

/// <summary>
/// Reusable toolbar for editing text formatting properties.
/// Creates all UI elements through the framework factory and
/// exposes strongly typed events for all value changes.
/// </summary>
public class TextEditIconBar
{
    private AbstInputCombobox _stylesCombo;
    private AbstButton _addStyleButton;
    private AbstButton _removeStyleButton;
    private AbstStateButton _alignLeft;
    private AbstStateButton _alignCenter;
    private AbstStateButton _alignRight;
    private AbstStateButton _alignJustified;
    private AbstStateButton _boldButton;
    private AbstStateButton _italicButton;
    private AbstStateButton _underlineButton;
    private AbstColorPicker _colorPicker;
    private AbstInputNumber<int> _fontSize;
    private AbstInputNumber<int> _lineHeight;
    private AbstInputNumber<int> _marginLeft;
    private AbstInputNumber<int> _marginRight;
    private AbstInputCombobox _fontsCombo;
    private bool _isSettingMemberValues;

    private IBlingoMemberTextBase? _member;
    private bool _isAligning;

    private readonly Dictionary<string, AbstTextStyle> _styles = new();
    private readonly IBlingoFrameworkFactory _factory;
    private AbstTextStyle _currentStyle;
    private AbstInputNumber<int> _memberWidth;
    public const string DefaultStyleName = "Default";

    /// <summary>Container panel holding the toolbar items.</summary>
    public AbstPanel Panel { get; }

    /// <summary>Raised when the text alignment changes.</summary>
    public event Action<AbstTextAlignment>? AlignmentChanged;
    /// <summary>Raised when bold style is toggled.</summary>
    public event Action<bool>? BoldChanged;
    /// <summary>Raised when italic style is toggled.</summary>
    public event Action<bool>? ItalicChanged;
    /// <summary>Raised when underline style is toggled.</summary>
    public event Action<bool>? UnderlineChanged;
    /// <summary>Raised when font size changes.</summary>
    public event Action<int>? FontSizeChanged;
    /// <summary>Raised when a font is selected.</summary>
    public event Action<string>? FontChanged;
    /// <summary>Raised when color changes.</summary>
    public event Action<AColor>? ColorChanged;
    /// <summary>Raised when line height changes.</summary>
    public event Action<int>? LineHeightChanged;
    /// <summary>Raised when left margin changes.</summary>
    public event Action<int>? MarginLeftChanged;
    /// <summary>Raised when right margin changes.</summary>
    public event Action<int>? MarginRightChanged;
    public event Action<int>? MemberWidthChanged;

    /// <summary>Returns whether bold is currently selected.</summary>
    public bool IsBold => _boldButton.IsOn;
    /// <summary>Returns whether italic is currently selected.</summary>
    public bool IsItalic => _italicButton.IsOn;
    /// <summary>Returns whether underline is currently selected.</summary>
    public bool IsUnderline => _underlineButton.IsOn;
    /// <summary>Currently selected font name.</summary>
    public string SelectedFont => _fontsCombo.SelectedValue ?? string.Empty;

    /// <summary>Enumerates all defined text styles.</summary>
    public IEnumerable<AbstTextStyle> Styles => _styles.Values;

    /// <summary>Returns the style at the current caret position.</summary>
    public AbstTextStyle CurrentStyle => _currentStyle;

    public TextEditIconBar(IBlingoFrameworkFactory factory)
    {
        const int actionBarHeight = 22;
        _factory = factory;
        // Ensure default style exists
        var defaultStyle = new AbstTextStyle
        {
            Name = DefaultStyleName,
            FontSize = 12,
            Font = string.Empty,
            Color = AColors.Black,
            Alignment = AbstTextAlignment.Left,
            Bold = false,
            Italic = false,
            Underline = false,
            LineHeight = 0,
            MarginLeft = 0,
            MarginRight = 0
        };
        _styles.Add(defaultStyle.Name, defaultStyle);
        _currentStyle = defaultStyle;

        Panel = factory.CreatePanel("TextEditIconBar");
        Panel.Height = actionBarHeight;
        Panel.BackgroundColor = DirectorColors.BG_WhiteMenus;

        var container = factory.CreateWrapPanel(AOrientation.Horizontal, "TextEditIconBarContainer");
        Panel.AddItem(container);

        CreateStyleSelection(factory, container);
        CreateAlignment(factory, container);
        CreateStyling(factory, container);
        CreateFontButtons(factory, actionBarHeight, container);

        RefreshStylesCombo();


    }

  
    private void CreateStyleSelection(IBlingoFrameworkFactory factory, AbstWrapPanel container)
    {
        _stylesCombo = factory.CreateInputCombobox("TextStyles", s =>
        {
            if (s != null) ApplyStyle(s);
        });
        _stylesCombo.Width = 100;
        container.AddItem(_stylesCombo);

        _addStyleButton = factory.CreateButton("AddTextStyle", "+");
        _addStyleButton.Width = 20;
        _addStyleButton.Pressed += AddStyle;
        container.AddItem(_addStyleButton);

        _removeStyleButton = factory.CreateButton("RemoveTextStyle", "-");
        _removeStyleButton.Width = 20;
        _removeStyleButton.Pressed += RemoveCurrentStyle;
        container.AddItem(_removeStyleButton);

        container.AddVLine("VLine1",16,2);

        _stylesCombo.SelectedKey = DefaultStyleName;
    }

    private void CreateStyling(IBlingoFrameworkFactory factory, AbstWrapPanel container)
    {
        _boldButton = factory.CreateStateButton("Bold", null, "B", v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.Bold = v;
            BoldChanged?.Invoke(v);
        });
        _italicButton = factory.CreateStateButton("Italic", null, "I", v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.Italic = v;
            ItalicChanged?.Invoke(v);
        });
        _underlineButton = factory.CreateStateButton("Underline", null, "U", v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.Underline = v;
            UnderlineChanged?.Invoke(v);
        });

        container.AddItem(_boldButton);
        container.AddItem(_italicButton);
        container.AddItem(_underlineButton);
        container.AddVLine("VLine3",16,2);
    }

    private void CreateAlignment(IBlingoFrameworkFactory factory, AbstWrapPanel container)
    {
        _alignLeft = factory.CreateStateButton("AlignLeft", null, "L", _ =>
        {
            if (_isSettingMemberValues) return;
            SetAlignment(AbstTextAlignment.Left);
            AlignmentChanged?.Invoke(AbstTextAlignment.Left);
        });
        _alignCenter = factory.CreateStateButton("AlignCenter", null, "C", _ =>
        {
            if (_isSettingMemberValues) return;
            SetAlignment(AbstTextAlignment.Center);
            AlignmentChanged?.Invoke(AbstTextAlignment.Center);
        });
        _alignRight = factory.CreateStateButton("AlignRight", null, "R", _ =>
        {
            if (_isSettingMemberValues) return;
            SetAlignment(AbstTextAlignment.Right);
            AlignmentChanged?.Invoke(AbstTextAlignment.Right);
        });
        _alignJustified = factory.CreateStateButton("AlignJustified", null, "J", _ =>
        {
            if (_isSettingMemberValues) return;
            SetAlignment(AbstTextAlignment.Justified);
            AlignmentChanged?.Invoke(AbstTextAlignment.Justified);
        });
        container.AddItem(_alignLeft);
        container.AddItem(_alignCenter);
        container.AddItem(_alignRight);
        container.AddItem(_alignJustified);
        container.AddVLine("VLine2",16,2);
    }

    private void CreateFontButtons(IBlingoFrameworkFactory factory, int actionBarHeight, AbstWrapPanel container)
    {
        _memberWidth = factory.CreateInputNumberInt("MemberWidth", 0, 800, v =>
        {
            if (_member == null || _isSettingMemberValues)
                return;
            
            _member.Width = v;
            MemberWidthChanged?.Invoke(v);
        });
        _memberWidth.Width = 35;
        container.AddItem(CreateLabel("LabelMember Width", "Width:"));
        container.AddItem(_memberWidth);


        _fontSize = factory.CreateInputNumberInt("FontSize", 1, 200, v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.FontSize = (int)v;
            FontSizeChanged?.Invoke((int)v);
        });
        _fontSize.Width = 25;
        container.AddItem(CreateLabel("LabelFontSize", "FontSize:"));
        container.AddItem(_fontSize);

        _lineHeight = factory.CreateInputNumberInt("LineHeight", 0, 500, v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.LineHeight = (int)v;
            LineHeightChanged?.Invoke((int)v);
        });
        _lineHeight.Width = 25;
        container.AddItem(CreateLabel("LabelLineHeight", "LineHeight:"));
        container.AddItem(_lineHeight);

        _marginLeft = factory.CreateInputNumberInt("MarginLeft", 0, 500, v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.MarginLeft = (int)v;
            MarginLeftChanged?.Invoke((int)v);
        });
        _marginLeft.Width = 30;
        container.AddItem(CreateLabel("LabelMargin", "Margin:"));
        container.AddItem(_marginLeft);

        _marginRight = factory.CreateInputNumberInt("MarginRight", 0, 500, v =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.MarginRight = (int)v;
            MarginRightChanged?.Invoke((int)v);
        });
        _marginRight.Width = 30;
        container.AddItem(_marginRight);

        _fontsCombo = factory.CreateInputCombobox("FontsCombo", s =>
        {
            if (_isSettingMemberValues) return;
            if (s != null)
            {
                _currentStyle.Font = s;
                FontChanged?.Invoke(s);
            }
        });
        _fontsCombo.Width = 100;
        container.AddItem(_fontsCombo);

        _colorPicker = factory.CreateColorPicker("ColorPicker", c =>
        {
            if (_isSettingMemberValues) return;
            _currentStyle.Color = c;
            ColorChanged?.Invoke(c);
        });
        _colorPicker.Width = actionBarHeight;
        _colorPicker.Height = actionBarHeight;
        container.AddItem(_colorPicker);
    }


    private AbstUI.Components.Texts.AbstLabel CreateLabel(string name,string text)
    {
        var lbl = _factory.CreateLabel(name,text);
        lbl.FontColor = AColors.Black;
        lbl.FontSize = 10;
        return lbl;
    }

    /// <summary>Populate fonts available for selection.</summary>
    public void SetFonts(IEnumerable<string> fonts)
    {
        _fontsCombo.ClearItems();
        foreach (var font in fonts)
            _fontsCombo.AddItem(font, font);
    }

    /// <summary>Set the currently selected font.</summary>
    public void SetFont(string font)
    {
        _fontsCombo.SelectedKey = font;
        _currentStyle.Font = font;
    }

    /// <summary>Set current font size.</summary>
    public void SetFontSize(int size)
    {
        _fontSize.Value = size;
        _currentStyle.FontSize = size;
    }

    /// <summary>Set the current line height.</summary>
    public void SetLineHeight(int value)
    {
        _lineHeight.Value = value;
        _currentStyle.LineHeight = value;
    }

    /// <summary>Set the left margin.</summary>
    public void SetMarginLeft(int value)
    {
        _marginLeft.Value = value;
        _currentStyle.MarginLeft = value;
    }

    /// <summary>Set the right margin.</summary>
    public void SetMarginRight(int value)
    {
        _marginRight.Value = value;
        _currentStyle.MarginRight = value;
    }

   

    /// <summary>Set the alignment state.</summary>
    public void SetAlignment(AbstTextAlignment alignment)
    {
        if (_isAligning) return;
        _isAligning = true;
        _alignLeft.IsOn = alignment == AbstTextAlignment.Left;
        _alignCenter.IsOn = alignment == AbstTextAlignment.Center;
        _alignRight.IsOn = alignment == AbstTextAlignment.Right;
        _alignJustified.IsOn = alignment == AbstTextAlignment.Justified;
        _currentStyle.Alignment = alignment;
        _isAligning = false;
    }

    public void SetBold(bool on)
    {
        _boldButton.IsOn = on;
        _currentStyle.Bold = on;
    }

    public void SetItalic(bool on)
    {
        _italicButton.IsOn = on;
        _currentStyle.Italic = on;
    }

    public void SetUnderline(bool on)
    {
        _underlineButton.IsOn = on;
        _currentStyle.Underline = on;
    }

    /// <summary>Set the current font color.</summary>
    public void SetColor(AColor color)
    {
        _colorPicker.Color = color;
        _currentStyle.Color = color;
    }

    /// <summary>
    /// Update all UI controls to reflect the values of the given text member.
    /// </summary>
    public void SetMemberValues(IBlingoMemberTextBase member)
    {
        _isSettingMemberValues = true;
        SetFontSize(member.FontSize);
        SetFont(member.Font);
        SetColor(member.Color);
        SetAlignment(member.Alignment);
        SetBold(member.Bold);
        SetItalic(member.Italic);
        SetUnderline(member.Underline);
        SetLineHeight(0);
        SetMarginLeft(0);
        SetMarginRight(0);

        // Update default style from member
        var style = _styles[DefaultStyleName];
        style.FontSize = member.FontSize;
        style.Font = member.Font;
        style.Color = member.Color;
        style.Alignment = member.Alignment;
        style.Bold = member.Bold;
        style.Italic = member.Italic;
        style.Underline = member.Underline;
        style.LineHeight = 0;
        style.MarginLeft = 0;
        style.MarginRight = 0;

        ApplyStyle(DefaultStyleName);
        _stylesCombo.SelectedKey = DefaultStyleName;
        _member = member;
        _memberWidth.Value = member.Width;
        _isSettingMemberValues = false;
    }

    public void OnResizing(float x, float y)
    {
        Panel.Width = x;
    }

    /// <summary>
    /// Ensure a style with the given name exists. If missing, a new style
    /// is created from the current toolbar values and added to the list.
    /// </summary>
    public AbstTextStyle EnsureStyle(string name)
    {
        if (_styles.TryGetValue(name, out var style))
            return style;

        style = new AbstTextStyle
        {
            Name = name,
            FontSize = _currentStyle.FontSize,
            Font = _currentStyle.Font,
            Color = _currentStyle.Color,
            Alignment = _currentStyle.Alignment,
            Bold = _currentStyle.Bold,
            Italic = _currentStyle.Italic,
            Underline = _currentStyle.Underline,
            LineHeight = _currentStyle.LineHeight,
            MarginLeft = _currentStyle.MarginLeft,
            MarginRight = _currentStyle.MarginRight
        };
        _styles.Add(name, style);
        RefreshStylesCombo();
        return style;
    }

    /// <summary>
    /// Programmatically select a style in the toolbar. Creates the style if needed.
    /// </summary>
    public void SelectStyle(string name)
    {
        if (string.IsNullOrEmpty(name))
            name = DefaultStyleName;

        if (!_styles.ContainsKey(name))
            EnsureStyle(name);

        var previousSettingMemberValues = _isSettingMemberValues;
        _isSettingMemberValues = true;

        if (_currentStyle.Name != name)
            ApplyStyle(name);

        _stylesCombo.SelectedKey = name;
        _isSettingMemberValues = previousSettingMemberValues;
    }

    /// <summary>
    /// Create a new style based on the current settings and return its name.
    /// An optional configure action allows adjusting the new style.
    /// </summary>
    public string CreateStyle(Action<AbstTextStyle>? configure = null)
    {
        string newName = GenerateStyleName();
        var style = EnsureStyle(newName);
        configure?.Invoke(style);
        return newName;
    }

    /// <summary>Try to get a style by name.</summary>
    public bool TryGetStyle(string name, out AbstTextStyle style)
        => _styles.TryGetValue(name, out style);

    private void ApplyStyle(string name)
    {
        if (_styles.TryGetValue(name, out var style))
        {
            _currentStyle = style;
            SetFontSize(style.FontSize);
            SetFont(style.Font);
            SetColor(style.Color);
            SetAlignment(style.Alignment);
            SetBold(style.Bold);
            SetItalic(style.Italic);
            SetUnderline(style.Underline);
            SetLineHeight(style.LineHeight);
            SetMarginLeft(style.MarginLeft);
            SetMarginRight(style.MarginRight);
        }
    }

    private void AddStyle()
    {
        string newName = GenerateStyleName();
        var style = new AbstTextStyle
        {
            Name = newName,
            FontSize = _currentStyle.FontSize,
            Font = _currentStyle.Font,
            Color = _currentStyle.Color,
            Alignment = _currentStyle.Alignment,
            Bold = _currentStyle.Bold,
            Italic = _currentStyle.Italic,
            Underline = _currentStyle.Underline,
            LineHeight = _currentStyle.LineHeight,
            MarginLeft = _currentStyle.MarginLeft,
            MarginRight = _currentStyle.MarginRight
        };
        _styles.Add(newName, style);
        RefreshStylesCombo();
        ApplyStyle(newName);
        _stylesCombo.SelectedKey = newName;
    }

    private void RemoveCurrentStyle()
    {
        if (_currentStyle.Name == DefaultStyleName) return;
        _styles.Remove(_currentStyle.Name);
        RefreshStylesCombo();
        ApplyStyle(DefaultStyleName);
        _stylesCombo.SelectedKey = DefaultStyleName;
    }

    private void RefreshStylesCombo()
    {
        _stylesCombo.ClearItems();
        foreach (var style in _styles.Values)
            _stylesCombo.AddItem(style.Name, style.Name);
    }

    private string GenerateStyleName()
    {
        int i = 1;
        string name;
        do
        {
            name = $"Style{i++}";
        } while (_styles.ContainsKey(name));
        return name;
    }
}


