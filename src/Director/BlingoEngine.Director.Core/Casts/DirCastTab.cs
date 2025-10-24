using System;
using System.Collections.Generic;
using AbstUI.Commands;
using AbstUI.Components.Buttons;
using AbstUI.Components.Containers;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using AbstUI.Windowing.Commands;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Casts.Commands;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Scripts.Commands;
using BlingoEngine.Director.Core.Members.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.Events;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Scripts;
using BlingoEngine.Texts;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Director.Core.Casts
{
    public class DirCastTab : IDisposable
    {
        private int _itemMargin = 2;
        private readonly List<DirCastItem> _items = new();
        private readonly List<DirCastListItem> _listItems = new();
        private readonly AbstScrollContainer _scroll;
        private readonly AbstTabItem _tabItem;
        private readonly AbstWrapPanel _wrap;
        private readonly AbstWrapPanel _listWrap;
        private readonly AbstPanel _root;
        private readonly AbstPanel _topBar;
        private readonly AbstStateButton _viewButton;
        private readonly IBlingoCast _cast;
        private readonly IAbstCommandManager _commandManager;
        private readonly IBlingoFrameworkFactory _factory;
        private readonly IDirectorIconManager _iconManager;
        private readonly MemberNavigationBar _navBar;
        private readonly AbstContextMenu _contextMenu;
        private readonly IDirectorEventMediator _mediator;
        private readonly ILogger _logger;
        private IDirCastItem? _selected;
        private IDirCastItem? _hoveredItem;
        private DirCastItem? _dragItem;
        private bool _dragging;
        private float _dragStartX, _dragStartY;
        private int _columns = 1;
        private bool _listView;
        private int _contextSlot;
        private static bool _openingEditor;
        private static readonly object _lock = new();

        public event Action<IBlingoMember, IDirCastItem>? MemberSelected;
        internal AbstTabItem TabItem => _tabItem;

        public int Width { get; private set; }
        internal IBlingoCast Cast => _cast;

        public DirCastTab(
            IBlingoFrameworkFactory factory,
            IBlingoCast cast,
            IDirectorIconManager iconManager,
            IAbstCommandManager commandManager,
            IDirectorEventMediator mediator,
            IBlingoPlayer player,
            Func<AbstContextMenu> contextMenuFactory, ILogger logger)
        {
            _commandManager = commandManager;
            _factory = factory;
            _iconManager = iconManager;
            _mediator = mediator;
            _logger = logger;
            _cast = cast;
            _contextMenu = (contextMenuFactory ?? throw new ArgumentNullException(nameof(contextMenuFactory)))();
            _contextMenu
                .AddItemFluent(string.Empty, "Find cast member", () =>
                        _contextSlot > 0 && _cast.Member[_contextSlot] != null,
                        FindCastMember)
                .AddItemFluent(string.Empty, "Import member", () => _contextSlot > 0, StartImportMembers);
            var tabName = cast.Name ?? $"Cast{cast.Number}";
            _tabItem = factory.CreateTabItem("Cast_" + tabName, tabName);
            _root = factory.CreatePanel(tabName + "_Root");
            _root.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _topBar = factory.CreatePanel(tabName + "_Top");
            _topBar.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _topBar.Height = 24;
            _viewButton = factory.CreateStateButton(tabName + "_ViewMode", null, "", v => SetListView(v));
            _viewButton.TextureOff = iconManager.Get(DirectorIcon.ViewGrid);
            _viewButton.TextureOn = iconManager.Get(DirectorIcon.ViewList);
            _viewButton.Width = 22;
            _viewButton.Height = 22;
            _topBar.AddItem(_viewButton,5,0);
            _navBar = new MemberNavigationBar(mediator, player, iconManager, factory, (int)_topBar.Height);
            _topBar.AddItem(_navBar.Panel,30,0);
            _wrap = factory.CreateWrapPanel(AOrientation.Horizontal, tabName + "_Wrap");
            _listWrap = factory.CreateWrapPanel(AOrientation.Vertical, tabName + "_ListWrap");
            _listWrap.ItemMargin = new APoint(0, 0);
            _wrap.ItemMargin = new APoint(_itemMargin, _itemMargin);
            _scroll = factory.CreateScrollContainer(tabName + "_Scroll");
            _scroll.ClipContents = true;
            _scroll.ScollbarModeH = AbstScrollbarMode.Hidden;
            _scroll.AddItem(_wrap);
            _root.AddItem(_topBar, 0, 0);
            _root.AddItem(_scroll, 0, _topBar.Height);

            _tabItem.TopHeight = _topBar.Height;
            _tabItem.Content = _root;
            _cast.MemberAdded += MemberAdded;
            _cast.MemberDeleted += MemberDeleted;
            _cast.MemberNameChanged += MemberNameChanged;
        }
        public void Dispose()
        {
            _cast.MemberAdded -= MemberAdded;
            _cast.MemberDeleted -= MemberDeleted;
            _cast.MemberNameChanged -= MemberNameChanged;
            _contextMenu.Dispose();
        }

        private void MemberNameChanged(IBlingoMember member)
        {
            var idx = _items.FindIndex(i => i.Member == member);
            if (idx < 0) return;
            _items[idx].SetMember(member);
            _listItems[idx].SetMember(member);
            if (_selected?.Member == member)
                _navBar.SetMember(member);
        }

        private void MemberDeleted(IBlingoMember member)
        {
            var idx = _items.FindIndex(i => i.Member == member);
            if (idx < 0) return;
            _items[idx].MakeEmpty();
            _listItems.RemoveAt(idx);
            if (_selected?.Member == member)
                Select(null);
        }

        private void MemberAdded(IBlingoMember member)
        {
            var index = member.NumberInCast - 1;
            if (index >= 0 && index < _items.Count)
            {
                _items[index].SetMember(member);
                _listItems[index].SetMember(member);
            }
        }

        internal void LoadAllMembers()
        {
            _items.Clear();
            _listItems.Clear();
            _wrap.RemoveAll();
            _listWrap.RemoveAll();
            for (int i = 1; i <= 999; i++)
            {
                var member = _cast.Member[i];
                var item = new DirCastItem(_factory, member, i, _iconManager);
                _items.Add(item);
                _wrap.AddItem(item.Canvas);
                if (member != null)
                {
                    var listItem = new DirCastListItem(_factory, member, _iconManager);
                    _listItems.Add(listItem);
                    _listWrap.AddItem(listItem.Panel);
                }
            }
        }

        public void SetViewportSize(int width, int height)
        {
            Width = width;
            _tabItem.Width = width;
            //_tabItem.Height = height-20;
            _root.Width = width;
            //_root.Height = height-20;
            _topBar.Width = width;
            _navBar.Panel.Width = width - _viewButton.Width;
            _scroll.Width = width;
            _scroll.Height = height - (int)_tabItem.TopHeight-20;
            _wrap.Width = Width;
            _listWrap.Width = Width;
            var itemSize = DirCastItem.Width + _itemMargin;
            var div = (double)(Width + 2) / itemSize;
            _columns = Math.Max(1, (int)Math.Floor(div));
            foreach (var item in _listItems)
                item.SetWidth(width);
        }


        public void Select(IDirCastItem? selected)
        {
            _selected = selected;
            foreach (var it in _items)
                it.SetSelected(it == selected);
            foreach (var it in _listItems)
                it.SetSelected(it == selected);
            if (selected?.Member is IBlingoMember member)
                _navBar.SetMember(member);
        }

        public int IndexOfMember(IBlingoMember member)
        {
            for (int i = 0; i < _items.Count; i++)
                if (_items[i].Member == member)
                    return i;
            return -1;
        }

        public void ScrollToIndex(int index)
        {
            if (_listView)
            {
                _scroll.ScrollVertical = index * DirCastListItem.RowHeight;
            }
            else
            {
                int row = index / _columns;
                _scroll.ScrollVertical = row * DirCastItem.Height;
            }
        }

        public IDirCastItem? GetItem(int index)
            => _listView
                ? index >= 0 && index < _listItems.Count ? _listItems[index] : null
                : index >= 0 && index < _items.Count ? _items[index] : null;

        public void HandleMouseEvent(AbstMouseEvent e)
        {
            float x = e.Mouse.MouseH + _scroll.ScrollHorizontal;
            float y = e.Mouse.MouseV + _scroll.ScrollVertical -  _navBar.Height - AbstTabItem.TopTabHeaderHeight;
            if (y < 0 || x < 0 || x > Width || e.MouseV < _navBar.Height + AbstTabItem.TopTabHeaderHeight)
            {
                if (_hoveredItem != null)
                {
                    _hoveredItem.SetHovered(false);
                    _hoveredItem = null;
                }
                return;
            }
            //Console.WriteLine(x + "x" + y + " \t"+ _scroll.ScrollVertical+ " \t"+ e.Mouse.MouseV);
            IBlingoMember? memberHover;
            IDirCastItem? hoverItem;
            if (_listView)
                memberHover = HitTestList(y, out hoverItem);
            else
                memberHover = HitTestGrid(x, y, out hoverItem);
            if (!ReferenceEquals(hoverItem, _hoveredItem))
            {
                _hoveredItem?.SetHovered(false);
                _hoveredItem = hoverItem;
                _hoveredItem?.SetHovered(true);
            }
            switch (e.Type)
            {
                case AbstMouseEventType.MouseDown:
                    {
                        var member = memberHover;
                        var item = hoverItem;
                        int slotNumber = item is DirCastItem gridItem
                            ? gridItem.SlotNumber
                            : member?.NumberInCast ?? 0;
                        bool isRightClick = e.Mouse.RightMouseDown;
                        bool isDoubleClick = e.Mouse.DoubleClick;

                        if (item != null)
                        {
                            Select(item);
                            if (member != null)
                            {
                                if (isDoubleClick && !isRightClick)
                                    OpenEditor(member);

                                MemberSelected?.Invoke(member, item);

                                if (isRightClick)
                                {
                                    ShowContextMenu(slotNumber);
                                }
                                else if (!isDoubleClick)
                                {
                                    _dragStartX = x;
                                    _dragStartY = y;
                                    _dragging = false;
                                    _dragItem = item as DirCastItem;
                                }
                            }
                            else if (isRightClick && slotNumber > 0)
                            {
                                ShowContextMenu(slotNumber);
                            }
                        }
                    }
                    break;
                case AbstMouseEventType.MouseMove:
                    if (!_listView && e.Mouse.MouseDown && _selected is DirCastItem sel && sel.Member != null)
                    {
                        float dx = x - _dragStartX;
                        float dy = y - _dragStartY;
                        if (!_dragging && dx * dx + dy * dy > 16)
                        {
                            _dragging = true;
                            DirectorDragDropHolder.StartDrag(sel.Member!, "CastItem");
                        }
                    }
                    break;
                case AbstMouseEventType.MouseUp:
                    if (!_listView && _dragging && _dragItem?.Member != null && hoverItem is DirCastItem di && di != _dragItem)
                    {
                        SwapItems(_dragItem, di);
                    }
                    _dragItem = null;
                    _dragging = false;
                    _selected = null;
                    lock (_lock) _openingEditor = false;
                    DirectorDragDropHolder.EndDrag();
                    break;
            }
        }
        private void SwapItems(DirCastItem source, DirCastItem target)
        {
            int srcIndex = _items.IndexOf(source);
            int dstIndex = _items.IndexOf(target);
            if (srcIndex < 0 || dstIndex < 0) return;

            _cast.SwapMembers(srcIndex + 1, dstIndex + 1);

            var srcMember = source.Member;
            source.SetMember(target.Member);
            target.SetMember(srcMember);
        }

        private IBlingoMember? HitTestGrid(float x, float y, out IDirCastItem? item)
        {
            int col = (int)(x / (DirCastItem.Width + _itemMargin));
            int row = (int)(y / (DirCastItem.Height + _itemMargin));
            int idx = row * _columns + col;
            if (idx >= 0 && idx < _items.Count)
            {
                item = _items[idx];
                return _items[idx].Member;
            }
            item = null;
            return null;
        }

        private IBlingoMember? HitTestList(float y, out IDirCastItem? item)
        {
            int idx = (int)(y / DirCastListItem.RowHeight);
            if (idx >= 0 && idx < _listItems.Count)
            {
                item = _listItems[idx];
                return _listItems[idx].Member;
            }
            item = null;
            return null;
        }

        private void SetListView(bool list)
        {
            _listView = list;
            _scroll.RemoveItem(list ? _wrap : _listWrap);
            _scroll.AddItem(list ? _listWrap : _wrap);
            Select(null);
        }

        private void ShowContextMenu(int slotNumber)
        {
            if (slotNumber <= 0)
                return;

            _contextSlot = slotNumber;
            _contextMenu.Popup();
        }

        private void StartImportMembers()
        {
            if (_contextSlot <= 0)
                return;

            var slot = _contextSlot;
            _contextSlot = 0;
            _commandManager.Handle(new OpenCastImportDialogCommand(_cast, slot));
        }

        private void FindCastMember()
        {
            if (_contextSlot <= 0)
                return;

            var slot = _contextSlot;
            _contextSlot = 0;

            var member = _cast.Member[slot];
            if (member == null)
                return;

            //_mediator.RaiseFindMember(member);
            _mediator.RaiseMemberSelected(member);
            _commandManager.Handle(new FindCastMemberInScoreCommand(member));
        }

        private void OpenEditor(IBlingoMember member)
        {
            lock (_lock)
            {
                if (_openingEditor) return;
                _openingEditor = true;
            }
            try
            {
                string? windowCode = member switch
                {
                    IBlingoMemberTextBase => DirectorMenuCodes.TextEditWindow,
                    BlingoMemberBitmap => DirectorMenuCodes.PictureEditWindow,
                    BlingoMemberScript => null,
                    _ => null
                };
                if (windowCode != null)
                    _commandManager.Handle(new OpenWindowCommand(windowCode));
                else if (member is BlingoMemberScript script)
                    _commandManager.Handle(new OpenScriptCommand(script));
            }
            catch (Exception ex)
            {
                // todo : add logging
                _logger.LogError(ex, "Error opening member editor:"+ex.Message+":"+ex);
            }

        }
    }
}


