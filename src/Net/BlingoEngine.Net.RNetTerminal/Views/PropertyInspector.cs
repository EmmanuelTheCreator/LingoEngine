using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetTerminal.Datas;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Terminal.Gui;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Attribute = Terminal.Gui.Drawing.Attribute;

namespace BlingoEngine.Net.RNetTerminal.Views;

public enum PropertyTarget
{
    Sprite,
    Member
}

internal sealed class PropertyInspector : View
{
    private static readonly Scheme SectionTitleScheme = new()
    {
        Normal = new Attribute(Color.Black, Color.White),
        Focus = new Attribute(Color.Black, Color.White),
        HotNormal = new Attribute(Color.Black, Color.White),
        HotFocus = new Attribute(Color.Black, Color.White)
    };

    private readonly View _contentContainer;
    private readonly Label _selectionLabel;
    private readonly LineView _verticalSeparator;
    private readonly List<PropertySection> _sections = new();
    private readonly List<PropertySection> _visibleSections = new();
    private readonly DataTable _memberTable = new();
    private readonly List<PropertySpec> _memberSpecs = new();
    private readonly TableView _memberTableView;
    private readonly DataTable _spriteTable = new();
    private readonly List<PropertySpec> _spriteSpecs = new();
    private readonly TableView _spriteTableView;
    private readonly PropertySection _memberSection;
    private readonly PropertySection _spriteSection;
    private readonly PropertySection _bitmapSection;
    private readonly PropertySection _soundSection;
    private readonly PropertySection _movieSection;
    private readonly PropertySection _castSection;
    private readonly PropertySection _textSection;
    private readonly PropertySection _shapeSection;
    private readonly PropertySection _guidesSection;
    private readonly PropertySection _behaviorSection;
    private readonly PropertySection _filmLoopSection;
    private Blingo2DSpriteDTO? _sprite;
    private BlingoMemberDTO? _member;
    private string _selectionText = "";

    public BlingoMemberDTO? CurrentMember => _member;

    public bool DelayPropertyUpdates { get; set; }

    public event Action<PropertyTarget, string, string>? PropertyChanged;

    public PropertyInspector()
    {
        CanFocus = true;
        Text = "Properties";

        _selectionLabel = new Label
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill() - 1,
            Height = 1
        };
        _selectionLabel.Text = string.Empty;
        Add(_selectionLabel);

        _verticalSeparator = new LineView(Orientation.Vertical)
        {
            X = 0,
            Y = 1,
            Width = 1,
            Height = Dim.Fill()
        };
        Add(_verticalSeparator);

        _contentContainer = new View
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 1,
            Height = Dim.Fill() - 1,
            ContentSizeTracksViewport = false
        };
        var verticalScrollBar = _contentContainer.VerticalScrollBar;
        if (verticalScrollBar != null)
        {
            verticalScrollBar.AutoShow = true;
        }
        var horizontalScrollBar = _contentContainer.HorizontalScrollBar;
        if (horizontalScrollBar != null)
        {
            horizontalScrollBar.AutoShow = false;
            horizontalScrollBar.Visible = false;
        }
        Add(_contentContainer);

        _spriteSpecs.AddRange(new[]
        {
            new PropertySpec("Lock", typeof(bool)),
            new PropertySpec("FlipH", typeof(bool)),
            new PropertySpec("FlipV", typeof(bool)),
            new PropertySpec("Name", typeof(string)),
            new PropertySpec("LocH", typeof(int)),
            new PropertySpec("LocV", typeof(int)),
            new PropertySpec("LocZ", typeof(int)),
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Ink", typeof(int)),
            new PropertySpec("Blend", typeof(float)),
            new PropertySpec("BeginFrame", typeof(int)),
            new PropertySpec("EndFrame", typeof(int)),
            new PropertySpec("Rotation", typeof(float)),
            new PropertySpec("Skew", typeof(float)),
            new PropertySpec("ForeColor", typeof(Color)),
            new PropertySpec("BackColor", typeof(Color)),
        });
        var spriteSection = CreateSection("Sprite", _spriteSpecs, PropertyTarget.Sprite);
        _spriteSection = spriteSection.Section;
        _spriteTableView = spriteSection.View;
        _spriteTable = spriteSection.Data;

        _memberSpecs.AddRange(new[]
        {
            new PropertySpec("Name", typeof(string)),
            new PropertySpec("CastLibNum", typeof(int), true),
            new PropertySpec("NumberInCast", typeof(int), true),
            new PropertySpec("Type", typeof(string), true),
            new PropertySpec("RegPointX", typeof(float), true),
            new PropertySpec("RegPointY", typeof(float), true),
            new PropertySpec("Width", typeof(int), true),
            new PropertySpec("Height", typeof(int), true),
            new PropertySpec("Size", typeof(int), true),
            new PropertySpec("Comments", typeof(string)),
            new PropertySpec("FileName", typeof(string), true),
            new PropertySpec("PurgePriority", typeof(int), true)
        });

        var memberSection = CreateSection("Member", _memberSpecs, PropertyTarget.Member);
        _memberSection = memberSection.Section;
        _memberTableView = memberSection.View;
        _memberTable = memberSection.Data;

        var (bitmapSection, _, _) = CreateSection("Bitmap", new[]
        {
            new PropertySpec("Dimensions", typeof(string), true),
            new PropertySpec("Highlight", typeof(bool)),
            new PropertySpec("RegPointX", typeof(int)),
            new PropertySpec("RegPointY", typeof(int))
        }, PropertyTarget.Member);
        _bitmapSection = bitmapSection;

        var (soundSection, _, _) = CreateSection("Sound", new[]
        {
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("Duration", typeof(float), true),
            new PropertySpec("SampleRate", typeof(int), true),
            new PropertySpec("BitDepth", typeof(int), true),
            new PropertySpec("Channels", typeof(int), true),
            new PropertySpec("Play", typeof(bool)),
            new PropertySpec("Stop", typeof(bool))
        }, PropertyTarget.Member);
        _soundSection = soundSection;

        var (movieSection, _, _) = CreateSection("Movie", new[]
        {
            new PropertySpec("StageWidth", typeof(int)),
            new PropertySpec("StageHeight", typeof(int)),
            new PropertySpec("Resolution", typeof(float)),
            new PropertySpec("Channels", typeof(int)),
            new PropertySpec("BackgroundColor", typeof(Color)),
            new PropertySpec("About", typeof(string)),
            new PropertySpec("Copyright", typeof(string))
        }, PropertyTarget.Member);
        _movieSection = movieSection;

        var (castSection, _, _) = CreateSection("Cast", new[]
        {
            new PropertySpec("Number", typeof(int)),
            new PropertySpec("Name", typeof(string))
        }, PropertyTarget.Member);
        _castSection = castSection;

        var (textSection, _, _) = CreateSection("Text", new[]
        {
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        }, PropertyTarget.Member);
        _textSection = textSection;

        var (shapeSection, _, _) = CreateSection("Shape", new[]
        {
            new PropertySpec("Shape", typeof(string)),
            new PropertySpec("Filled", typeof(bool)),
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        }, PropertyTarget.Member);
        _shapeSection = shapeSection;

        var (guidesSection, _, _) = CreateSection("Guides", new[]
        {
            new PropertySpec("GuidesColor", typeof(Color)),
            new PropertySpec("GuidesVisible", typeof(bool)),
            new PropertySpec("GuidesSnap", typeof(bool)),
            new PropertySpec("GuidesLock", typeof(bool)),
            new PropertySpec("GridColor", typeof(Color)),
            new PropertySpec("GridVisible", typeof(bool)),
            new PropertySpec("GridSnap", typeof(bool)),
            new PropertySpec("AddVerticalGuide", typeof(bool)),
            new PropertySpec("AddHorizontalGuide", typeof(bool)),
            new PropertySpec("RemoveGuides", typeof(bool)),
            new PropertySpec("GridWidth", typeof(int)),
            new PropertySpec("GridHeight", typeof(int))
        }, PropertyTarget.Member);
        _guidesSection = guidesSection;

        var (behaviorSection, _, _) = CreateSection("Behavior", new[] { new PropertySpec("Behaviors", typeof(string)) }, PropertyTarget.Member);
        _behaviorSection = behaviorSection;

        var (filmLoopSection, _, _) = CreateSection("FilmLoop", new[]
        {
            new PropertySpec("Framing", typeof(string)),
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("FrameCount", typeof(int))
        }, PropertyTarget.Member);
        _filmLoopSection = filmLoopSection;

        SubViewLayout += (_, _) => UpdateSectionLayout();

        var store = TerminalDataStore.Instance;
        UpdateSelection(store.GetSelectedSprite());
        store.SelectedSpriteChanged += UpdateSelection;
        store.MemberChanged += m =>
        {
            var sel = store.GetSelectedSprite();
            if (sel.HasValue)
            {
                var sprite = store.FindSprite(sel.Value);
                if (sprite != null && sprite.Member!.CastLibNum == m.CastLibNum && sprite.Member.MemberNum == m.NumberInCast)
                {
                    ShowMember(m);
                    return;
                }
            }
            if (_member != null && _member.CastLibNum == m.CastLibNum && _member.NumberInCast == m.NumberInCast)
            {
                ShowMember(m);
            }
        };
        store.SpriteChanged += sprite =>
        {
            var current = _sprite;
            if (current == null)
            {
                return;
            }

            if (sprite.SpriteNum == current.SpriteNum && sprite.BeginFrame == current.BeginFrame)
            {
                ShowSprite(sprite);
                if (sprite.Member != null)
                {
                    ShowMember(store.FindMember(sprite.Member.CastLibNum, sprite.Member.MemberNum));
                }
            }
        };
    }
  

    private void UpdateSelection(SpriteRef? sel)
    {
        var store = TerminalDataStore.Instance;
        var sprite = sel.HasValue ? store.FindSprite(sel.Value) : null;
        ShowSprite(sprite);
        BlingoMemberDTO? member = null;
        var memberText = ""; 
        if (sprite != null)
        {
            member = store.FindMember(sprite.Member!.CastLibNum, sprite.Member.MemberNum);
            if (member != null)
                memberText = member.Name + " ("+ member.CastLibNum + "." + member.NumberInCast + ")";
            ShowMember(store.FindMember(sprite.Member!.CastLibNum, sprite.Member.MemberNum));
        }
        else
        {
            ShowMember(null);
        }
        _selectionText = (sprite != null ? "Sprite " + sprite.SpriteNum + ": " : "") + memberText;
        _selectionLabel.Text = _selectionText;
        _selectionLabel.SetNeedsDraw();
    }

    public void ShowSprite(Blingo2DSpriteDTO? sprite)
    {
        _sprite = sprite;
        for (var i = 0; i < _spriteSpecs.Count; i++)
        {
            var spec = _spriteSpecs[i];
            string value;
            if (sprite == null)
            {
                value = GetDefaultValue(spec.Type);
            }
            else
            {
                value = spec.Name switch
                {
                    "Lock" => sprite.Lock.ToString(),
                    "FlipH" => sprite.FlipH.ToString(),
                    "FlipV" => sprite.FlipV.ToString(),
                    "Name" => sprite.Name,
                    "LocH" => ((int)sprite.LocH).ToString(CultureInfo.InvariantCulture),
                    "LocV" => ((int)sprite.LocV).ToString(CultureInfo.InvariantCulture),
                    "LocZ" => sprite.LocZ.ToString(CultureInfo.InvariantCulture),
                    "Width" => ((int)sprite.Width).ToString(CultureInfo.InvariantCulture),
                    "Height" => ((int)sprite.Height).ToString(CultureInfo.InvariantCulture),
                    "Ink" => sprite.Ink.ToString(CultureInfo.InvariantCulture),
                    "Blend" => sprite.Blend.ToString(CultureInfo.InvariantCulture),
                    "BeginFrame" => sprite.BeginFrame.ToString(CultureInfo.InvariantCulture),
                    "EndFrame" => sprite.EndFrame.ToString(CultureInfo.InvariantCulture),
                    "Rotation" => sprite.Rotation.ToString(CultureInfo.InvariantCulture),
                    "Skew" => sprite.Skew.ToString(CultureInfo.InvariantCulture),
                    _ => _spriteTable.Rows[i][1]?.ToString() ?? GetDefaultValue(spec.Type)
                };
            }
            _spriteTable.Rows[i][1] = value;
        }
        //_spriteTableView.SetNeedsDraw();
    }



    private void SetVisibleSections(params PropertySection[] sections)
    {
        _visibleSections.Clear();
        foreach (var section in sections)
        {
            if (section != null && !_visibleSections.Contains(section))
            {
                _visibleSections.Add(section);
            }
        }

        UpdateSectionLayout();
        var verticalScrollBar = _contentContainer.VerticalScrollBar;
        if (verticalScrollBar != null)
        {
            verticalScrollBar.Position = 0;
        }
    }

    private void UpdateSectionLayout()
    {
        foreach (var section in _sections)
        {
            section.Container.Visible = false;
        }

        var y = 0;
        foreach (var section in _visibleSections)
        {
            section.Container.Visible = true;
            section.Container.X = 0;
            section.Container.Y = y;
            y += section.Height;
        }

        var width = _contentContainer.Frame.Width;
        if (width <= 0)
        {
            width = Frame.Width;
        }
        if (width <= 0)
        {
            width = 1;
        }

        var height = y;
        var visibleHeight = _contentContainer.Frame.Height;
        if (visibleHeight > 0)
        {
            height = Math.Max(height, visibleHeight);
        }
        else if (height <= 0)
        {
            height = 1;
            visibleHeight = height;
        }
        else
        {
            visibleHeight = height;
        }

        _contentContainer.SetContentSize(new System.Drawing.Size(width, height));

        var verticalScrollBar = _contentContainer.VerticalScrollBar;
        if (verticalScrollBar != null)
        {
            var viewportHeight = visibleHeight > 0 ? visibleHeight : height;
            verticalScrollBar.VisibleContentSize = viewportHeight;
            verticalScrollBar.ScrollableContentSize = height;
            var maxPosition = Math.Max(0, height - viewportHeight);
            if (verticalScrollBar.Position > maxPosition)
            {
                verticalScrollBar.Position = maxPosition;
            }
        }

        _contentContainer.SetNeedsDraw();
    }

    private static string? EditValue(Type type, string name, string value)
    {
        string? result = null;
        if (type == typeof(bool))
        {
            var check = RUI.NewCheckBox(12, 1, string.Empty, bool.TryParse(value, out var b) && b);
            check.KeyDown += (_,e) =>
            {
                if (e.KeyEventKey() == Key.Space)
                {
                    var state = check.CheckedState == CheckState.Checked;
                    // inverse selection
                    check.CheckedState = state ? CheckState.UnChecked : CheckState.Checked; 
                    
                    result = (!state).ToString();
                    Application.RequestStop();
                    e.Handled = true;
                }
            };
            var ok = RUI.NewButton("Ok", true, () =>
            {
                result = check.Checked().ToString();
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog($"Edit {name}", 30, 7, ok);
            dialog.Add(RUI.NewLabel(name + ":", 1, 1 ), check);
            check.SetFocus();
            Application.Run(dialog);
        }
        else if (type == typeof(int))
        {
            var field = RUI.NewTextField(value,12, 1,  20);
            var ok = RUI.NewButton("Ok", true, () =>
            {
                if (int.TryParse(field.Text.ToString(), out var v))
                {
                    result = v.ToString();
                }
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog($"Edit {name}", 40, 7, ok);
            dialog.Add(RUI.NewLabel(name + ":",1, 1), field);
            field.SetFocus();
            Application.Run(dialog);
        }
        else if (type == typeof(float))
        {
            var field = RUI.NewTextField(value, 12, 1, 20);
            var ok = RUI.NewButton("Ok", true, () =>
            {
                if (float.TryParse(field.Text.ToString(), out var v))
                {
                    result = v.ToString();
                }
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog($"Edit {name}", 40, 7, ok);
            dialog.Add(RUI.NewLabel(name + ":",1, 1), field);
            field.SetFocus();
            Application.Run(dialog);
        }
        else if (type == typeof(Color))
        {
            var colors = Enum.GetNames< ColorName16>();
            var list = RUI.NewListView(colors);
            list.Width = Dim.Fill();
            list.Height = Dim.Fill();
            var ok = RUI.NewButton("Ok", true, () =>
            {
                result = colors[list.SelectedItem];
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog($"Edit {name}", 30, 15, ok);
            dialog.Add(list);
            Application.Run(dialog);
        }
        else
        {
            var field = RUI.NewTextField(value,12,1, 20);
            var ok = RUI.NewButton("Ok", true, () =>
            {
                result = field.Text.ToString();
                Application.RequestStop();
            });
            var dialog = RUI.NewDialog($"Edit {name}", 40, 7, ok);
            dialog.Add(RUI.NewLabel(name + ":",1,1), field);
            field.SetFocus();
            Application.Run(dialog);
        }
        return result;
    }

    public void ShowMember(BlingoMemberDTO? member)
    {
        _member = member;
        for (var i = 0; i < _memberSpecs.Count; i++)
        {
            var spec = _memberSpecs[i];
            string value;
            if (member == null)
            {
                value = GetDefaultValue(spec.Type);
            }
            else
            {
                value = spec.Name switch
                {
                    "Name" => member.Name ?? string.Empty,
                    "CastLibNum" => member.CastLibNum.ToString(CultureInfo.InvariantCulture),
                    "NumberInCast" => member.NumberInCast.ToString(CultureInfo.InvariantCulture),
                    "Type" => member.Type.ToString(),
                    "RegPointX" => member.RegPoint.X.ToString(CultureInfo.InvariantCulture),
                    "RegPointY" => member.RegPoint.Y.ToString(CultureInfo.InvariantCulture),
                    "Width" => member.Width.ToString(CultureInfo.InvariantCulture),
                    "Height" => member.Height.ToString(CultureInfo.InvariantCulture),
                    "Size" => member.Size.ToString(CultureInfo.InvariantCulture),
                    "Comments" => member.Comments ?? string.Empty,
                    "FileName" => member.FileName ?? string.Empty,
                    "PurgePriority" => member.PurgePriority.ToString(CultureInfo.InvariantCulture),
                    _ => _memberTable.Rows[i][1]?.ToString() ?? GetDefaultValue(spec.Type)
                };
            }
            _memberTable.Rows[i][1] = value;
        }

        if (member == null)
        {
            var sections = new List<PropertySection>();
            if (_sprite != null)
            {
                sections.Add(_spriteSection);
            }
            sections.Add(_memberSection);
            SetVisibleSections(sections.ToArray());
            _memberTableView.SetNeedsDraw();
            return;
        }

        _memberTableView.SetNeedsDraw();

        var visibleSections = new List<PropertySection>();
        if (_sprite != null)
        {
            visibleSections.Add(_spriteSection);
            if (_sprite.Behaviors.Any())
            {
                visibleSections.Add(_behaviorSection);
            }
        }
        visibleSections.Add(_memberSection);
        visibleSections.Add(_castSection);

        switch (member.Type)
        {
            case BlingoMemberTypeDTO.Bitmap:
            case BlingoMemberTypeDTO.Picture:
                visibleSections.Add(_bitmapSection);
                break;
            case BlingoMemberTypeDTO.Sound:
                visibleSections.Add(_soundSection);
                break;
            case BlingoMemberTypeDTO.Text:
            case BlingoMemberTypeDTO.Field:
                visibleSections.Add(_textSection);
                break;
            case BlingoMemberTypeDTO.Shape:
                visibleSections.Add(_shapeSection);
                break;
            case BlingoMemberTypeDTO.FilmLoop:
                visibleSections.Add(_filmLoopSection);
                break;
            case BlingoMemberTypeDTO.Movie:
                visibleSections.Add(_movieSection);
                break;
        }

        SetVisibleSections(visibleSections.ToArray());
    }

 
    private (PropertySection Section, TableView View, DataTable Data) CreateSection(string title, IList<PropertySpec> props, PropertyTarget target)
    {
        var table = CreateTable(props);
        AttachEditPopup(props, table, target);
        var section = BuildSection(title, table.View, table.Data);
        return (section, table.View, table.Data);
    }

    private PropertySection BuildSection(string title, TableView tableView, DataTable data)
    {
        var tableHeight = Math.Max(1, data.Rows.Count);
        tableView.Height = tableHeight;
        tableView.Width = Dim.Fill();

        var container = new View
        {
            Width = Dim.Fill(),
            Height = tableHeight + 2
        };

        var separator = new LineView(Orientation.Horizontal)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };
        container.Add(separator);

        var titleLabel = new Label
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = 1
        };
        titleLabel.Text = $" {title} ";
        titleLabel.SetScheme(SectionTitleScheme);
        container.Add(titleLabel);

        tableView.X = 0;
        tableView.Y = 2;
        container.Add(tableView);

        var section = new PropertySection(container, tableView, data, tableHeight + 2);
        _sections.Add(section);
        _contentContainer.Add(container);

        return section;
    }

    private void AttachEditPopup(IList<PropertySpec> props, (TableView View, DataTable Data) table, PropertyTarget target)
    {
        table.View.CellActivated += (_, args) =>
        {
            var spec = props[args.Row];
            if (spec.ReadOnly)
            {
                return;
            }
            var value = table.Data.Rows[args.Row][1]?.ToString() ?? string.Empty;
            var newValue = EditValue(spec.Type, spec.Name, value);
            if (newValue != null)
            {
                if (!DelayPropertyUpdates)
                    table.Data.Rows[args.Row][1] = newValue;
                PropertyChanged?.Invoke(target, spec.Name, newValue);
            }
            //view.SetNeedsDisplay();
            //view.SetNeedsDraw();
        };
    }

    private (TableView View, DataTable Data) CreateTable(IEnumerable<PropertySpec> props)
    {
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Value");
        var i = 0;
        foreach (var prop in props)
        {
            i++;
            table.Rows.Add(prop.Name, GetDefaultValue(prop.Type));
        }

        var view = CreateTableView(table);
        return (view, table);
    }
    private static string GetDefaultValue(Type type)
    {
        if (type == typeof(bool))
        {
            return bool.FalseString;
        }
        if (type == typeof(int) || type == typeof(float))
        {
            return "0";
        }
        return string.Empty;
    }

    private TableView CreateTableView(DataTable datas)
    {
        var tableView = new TableView
        {
            Width = Dim.Fill(),
            Table = new DataTableSource(datas),
            FullRowSelect = true
        };
        RNetTerminalStyle.SetForTableView(tableView);
        tableView.Style.AlwaysShowHeaders = false;
        tableView.Style.ShowHeaders = false;
        tableView.Style.ShowHorizontalHeaderUnderline = false;
        tableView.Style.ShowHorizontalHeaderOverline = false;
        tableView.Style.ShowVerticalHeaderLines = false;
        tableView.Style.ShowVerticalCellLines = false;
        tableView.MultiSelect = false;
        //tableView.SelectedColumn = 1;
        //tableView.SelectedCellChanged += (_, _) => tableView.SelectedColumn = 1;
        //tableView.KeyDown += (_, e) =>
        //{
        //    if (e.KeyEventKey() == Key.CursorLeft || e.KeyEventKey() == Key.CursorRight)
        //    {
        //        e.Handled = true;
        //    }
        //};
        tableView.Style.ColumnStyles.Add(0, new ColumnStyle { Alignment = Alignment.Start });
        tableView.Style.ColumnStyles.Add(1, new ColumnStyle { Alignment = Alignment.End });
        return tableView;
    }

    private sealed class PropertySection
    {
        public PropertySection(View container, TableView tableView, DataTable data, int height)
        {
            Container = container;
            TableView = tableView;
            Data = data;
            Height = height;
        }

        public View Container { get; }
        public TableView TableView { get; }
        public DataTable Data { get; }
        public int Height { get; }
    }

    private sealed class PropertySpec
    {
        public string Name { get; }
        public Type Type { get; }
        public bool ReadOnly { get; }

        public PropertySpec(string name, Type type, bool readOnly = false)
        {
            Name = name;
            Type = type;
            ReadOnly = readOnly;
        }
    }
}


