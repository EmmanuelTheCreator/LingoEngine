using System.Collections.Generic;
using System.Data;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Net.RNetTerminal.Datas;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace BlingoEngine.Net.RNetTerminal.Views;

internal sealed class CastView : View
{
    private TabView _tabs;
    private bool _keyWasDown;

    public event System.Action<BlingoMemberDTO>? MemberSelected;

    public CastView()
    {
        CanFocus = true;
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_tabs);
        var store = TerminalDataStore.Instance;
        store.CastsChanged += ReloadData;
        store.MemberChanged += _ => ReloadData();
        ReloadData();
    }

    public void ReloadData()
    {
        var casts = TerminalDataStore.Instance.GetCasts();
        _tabs.RemoveAll();
        Remove(_tabs);
        _tabs = new TabView
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        Add(_tabs);
        var first = true;
        foreach (var cast in casts)
        {
            var members = cast.Value;
            var tableView = new TableView
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                Table = new DataTableSource(CreateTable(members)),
                FullRowSelect = true
            };
            RNetTerminalStyle.SetForTableView(tableView);
            tableView.BorderStyle = Terminal.Gui.Drawing.LineStyle.None;
            tableView.Style.AlwaysShowHeaders = false;
            tableView.Style.ShowHeaders = false;
            tableView.Style.ShowHorizontalHeaderOverline = false;
            tableView.Style.ShowHorizontalHeaderUnderline = false;
            tableView.MultiSelect = false;
            tableView.SelectedColumn = 0;
            tableView.SelectedCellChanged += (_,_) => tableView.SelectedColumn = 0;
            tableView.KeyDown += (_,e) =>
            {
                
                if (e.KeyCode == Key.CursorLeft || e.KeyCode == Key.CursorRight)
                {
                    e.Handled = true;
                }
            };
            tableView.KeyDown += (sender, e) => _keyWasDown = true;
            tableView.KeyUp += (sender, e) => _keyWasDown = false;
            tableView.CellActivated += (sender,e) =>
            {
                if (e.Row >= 0 && e.Row < members.Count)
                {
                    var member = members[e.Row];
                    MemberSelected?.Invoke(member);
                    if (e.Col == 4)
                    {
                        if (member.Type == BlingoMemberTypeDTO.Text)
                        {
                            var text = new TextView
                            {
                                Width = Dim.Fill(),
                                Height = Dim.Fill(),
                                Text = member.Comments
                            };
                            var ok = RUI.NewButton("OK", true, () =>
                            {
                                member.Comments = text.Text.ToString() ?? string.Empty;
                                TerminalDataStore.Instance.UpdateMember(member);
                                Application.RequestStop();
                            });
                            var dialog = new Dialog();
                            dialog.Text = $"Edit {member.Name}";
                            dialog.Width = 60;
                            dialog.Height = 20;
                            dialog.AddButton(ok);
                            dialog.Add(text);
                            text.SetFocus();
                            Application.Run(dialog);
                        }
                    }
                }
            };
            var tab = new Tab();
            RNetTerminalStyle.SetForTableView(tab);
            tab.DisplayText = cast.Key;
            tab.View = tableView;
            _tabs.AddTab(tab, first);
            first = false;
        }
        _tabs.SetNeedsDraw();
    }

  

    private static DataTable CreateTable(IEnumerable<BlingoMemberDTO> members)
    {
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Number", typeof(int));
        table.Columns.Add("Type");
        table.Columns.Add("Comments");
        table.Columns.Add(" ");
        foreach (var member in members)
        {
            var canEdit = member.Type == BlingoMemberTypeDTO.Text || member.Type == BlingoMemberTypeDTO.Field;
            table.Rows.Add(member.Name, member.NumberInCast, member.Type.ToString(), member.Comments, canEdit?"Edit":"");
        }
        return table;
    }
}

