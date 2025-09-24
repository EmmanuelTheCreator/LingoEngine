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

namespace BlingoEngine.Net.RNetTerminal.Views;

public enum PropertyTarget
{
    Sprite,
    Member
}

internal sealed class PropertyInspector : View
{
    private readonly TabView _tabs;
    private readonly DataTable _memberTable = new();
    private readonly List<PropertySpec> _memberSpecs = new();
    private readonly TableView _memberTableView;
    private readonly DataTable _spriteTable = new();
    private readonly List<PropertySpec> _spriteSpecs = new();
    private readonly TableView _spriteTableView;
    private readonly Tab _memberTab;
    private readonly Tab _spriteTab;
    private readonly Tab _bitmapTab;
    private readonly Tab _soundTab;
    private readonly Tab _movieTab;
    private readonly Tab _castTab;
    private readonly Tab _textTab;
    private readonly Tab _shapeTab;
    private readonly Tab _guidesTab;
    private readonly Tab _behaviorTab;
    private readonly Tab _filmLoopTab;
    private Blingo2DSpriteDTO? _sprite;
    private BlingoMemberDTO? _member;
    private string _lastTab = "Sprite";
    private string _selectionText = ""; 

    public BlingoMemberDTO? CurrentMember => _member;

    public bool DelayPropertyUpdates { get; set; }

    public event Action<PropertyTarget, string, string>? PropertyChanged;

    public PropertyInspector()
    {
        CanFocus = true;
        Text = "Properties";
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };
        if (_tabs.Border != null)
            _tabs.Border.Thickness = new Thickness(0);
        var tabsMargin = _tabs.Margin;
        if (tabsMargin != null)
            tabsMargin.Thickness = new Thickness(-1, 0, -1, -1);
        var tabsPadding = _tabs.Padding;
        if (tabsPadding != null)
            tabsPadding.Thickness = new Thickness(0);
        _tabs.SelectedTabChanged += (_, e) =>
        {
            if (e.NewTab != null)
            {
                _lastTab = e.NewTab.Text?.ToString() ?? _lastTab;
            }
        };
        Add(_tabs);

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
        var tabSpriteTuple = CreateTab("Sprite", _spriteSpecs, PropertyTarget.Sprite);
        _spriteTab = tabSpriteTuple.Tab;
        _spriteTable = tabSpriteTuple.Data;
        _spriteTableView = tabSpriteTuple.View;
        _tabs.AddTab(_spriteTab,true);

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

        var tabMemberTuple = CreateTab("Member", _memberSpecs, PropertyTarget.Member);
        _memberTab = tabMemberTuple.Tab;
        _memberTableView = tabMemberTuple.View;
        _memberTable = tabMemberTuple.Data;
        _tabs.AddTab(_memberTab, true);

        _bitmapTab = CreateTab("Bitmap", new[]
        {
            new PropertySpec("Dimensions", typeof(string), true),
            new PropertySpec("Highlight", typeof(bool)),
            new PropertySpec("RegPointX", typeof(int)),
            new PropertySpec("RegPointY", typeof(int))
        }, PropertyTarget.Member);
        _soundTab = CreateTab("Sound", new[]
        {
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("Duration", typeof(float), true),
            new PropertySpec("SampleRate", typeof(int), true),
            new PropertySpec("BitDepth", typeof(int), true),
            new PropertySpec("Channels", typeof(int), true),
            new PropertySpec("Play", typeof(bool)),
            new PropertySpec("Stop", typeof(bool))
        }, PropertyTarget.Member);
        _movieTab = CreateTab("Movie", new[]
        {
            new PropertySpec("StageWidth", typeof(int)),
            new PropertySpec("StageHeight", typeof(int)),
            new PropertySpec("Resolution", typeof(float)),
            new PropertySpec("Channels", typeof(int)),
            new PropertySpec("BackgroundColor", typeof(Color)),
            new PropertySpec("About", typeof(string)),
            new PropertySpec("Copyright", typeof(string))
        }, PropertyTarget.Member);
        _castTab = CreateTab("Cast", new[]
        {
            new PropertySpec("Number", typeof(int)),
            new PropertySpec("Name", typeof(string))
        }, PropertyTarget.Member);
        _textTab = CreateTab("Text", new[]
        {
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        }, PropertyTarget.Member);
        _shapeTab = CreateTab("Shape", new[]
        {
            new PropertySpec("Shape", typeof(string)),
            new PropertySpec("Filled", typeof(bool)),
            new PropertySpec("Width", typeof(int)),
            new PropertySpec("Height", typeof(int)),
            new PropertySpec("Edit", typeof(bool))
        }, PropertyTarget.Member);
        _guidesTab = CreateTab("Guides", new[]
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
        _behaviorTab = CreateTab("Behavior", new[] { new PropertySpec("Behaviors", typeof(string)) }, PropertyTarget.Member);
        _filmLoopTab = CreateTab("FilmLoop", new[]
        {
            new PropertySpec("Framing", typeof(string)),
            new PropertySpec("Loop", typeof(bool)),
            new PropertySpec("FrameCount", typeof(int))
        }, PropertyTarget.Member);

        SetTabs( _movieTab);
        var initial = _tabs.Tabs.FirstOrDefault(t => t.Text.ToString() == _lastTab) ?? _spriteTab;
        _tabs.SelectedTab = initial;

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
    }
    private void DrawSelectionHeader()
    {
        Move(2, 2);
        AddStr(_selectionText);
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



    private void SetTabs(params Tab[] tabs)
    {
        foreach (var existing in _tabs.Tabs.ToList())
            _tabs.RemoveTab(existing);

        foreach (var tab in tabs)
            _tabs.AddTab(tab, false);
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
            var tabsEmpty = new List<Tab>();
            if (_sprite != null)
            {
                tabsEmpty.Add(_spriteTab);
            }
            tabsEmpty.Add(_memberTab);
            SetTabs(tabsEmpty.ToArray());
            var target = _tabs.Tabs.FirstOrDefault(t => t.Text.ToString() == _lastTab);
            _tabs.SelectedTab = target ?? (_sprite != null ? _spriteTab : _memberTab);
            _memberTableView.SetNeedsDraw();
            return;
        }

        _memberTableView.SetNeedsDraw();

        var tabs = new List<Tab>();
        if (_sprite != null)
        {
            tabs.Add(_spriteTab);
            if (_sprite.Behaviors.Any())
                tabs.Add(_behaviorTab);
        }
        tabs.Add(_memberTab);
        tabs.Add(_castTab);

        switch (member.Type)
        {
            case BlingoMemberTypeDTO.Bitmap:
            case BlingoMemberTypeDTO.Picture:
                tabs.Add(_bitmapTab);
                break;
            case BlingoMemberTypeDTO.Sound:
                tabs.Add(_soundTab);
                break;
            case BlingoMemberTypeDTO.Text:
            case BlingoMemberTypeDTO.Field:
                tabs.Add(_textTab);
                break;
            case BlingoMemberTypeDTO.Shape:
                tabs.Add(_shapeTab);
                break;
            case BlingoMemberTypeDTO.FilmLoop:
                tabs.Add(_filmLoopTab);
                break;
            case BlingoMemberTypeDTO.Movie:
                tabs.Add(_movieTab);
                break;
        }

        SetTabs(tabs.ToArray());
        var desired = _tabs.Tabs.FirstOrDefault(t => t.Text.ToString() == _lastTab);
        _tabs.SelectedTab = desired ?? (_sprite != null ? _spriteTab : _memberTab);
        _tabs.SetNeedsDraw();
    }

 
    protected override bool OnDrawingContent()
    {
        // not working yet
        var ok = base.OnDrawingContent();
        DrawSelectionHeader();
        return ok;
    }



    private Tab CreateTab(string title, PropertySpec[] props, PropertyTarget target)
    {
        var tab = new Tab();
        RNetTerminalStyle.SetForTableView(tab);
        tab.DisplayText = title;
        var tableView = BuildPropertyTableView(props);
        AttachEditPopup(props, tableView, target);
        tab.View = tableView.View;
        return (tab);
    }
    private (Tab Tab, TableView View, DataTable Data) CreateTab(string name, IList<PropertySpec> data, PropertyTarget target)
    {
        var tab = new Tab();
        tab.CanFocus = true;
        RNetTerminalStyle.SetForTableView(tab);
        tab.DisplayText = name;
        var tableView = CreateTable(data);
        AttachEditPopup(data, tableView, target);
        tab.View = tableView.View;
        return (tab, tableView.View, tableView.Data);
    }
    private (TableView View, DataTable Data) BuildPropertyTableView(PropertySpec[] props)
    {
        var table = CreateTable(props);
        
        return table;
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
            Height = Dim.Fill(),
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


