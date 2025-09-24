using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetTerminal.Datas;
using BlingoEngine.Net.RNetTerminal;
using BlingoEngine.Net.RNetTerminal.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace BlingoEngine.Net.RNetTerminal.Views
{
    internal class RootWindow
    {
       
        private readonly List<string> _logs = new();

        private TextView? _logTextView;
        private PropertyInspector? _propertyInspector;
        private Label? _connectionStatusLabel;
        private Label? _infoItem;

        private Action<int> _setPort = p => { };
        //private View _leftZone;
        private Tile _leftTopZone;
        private ScoreView _score;
        private StageView _stage;
        private CastView _cast;
        private PropertyInspector _propInsp;
        private MenuItemv2 _stageBtn;
        private MenuItemv2 _castBtn;
        private Button? _rewindButton;
        private Button? _playPauseButton;
        private bool _isMoviePlaying;
        private Func<RNetCommand, CancellationToken?, Task>? _sendCommandAsync;
        private int _lastRequestedFrame = -1;
        private bool _suppressNextFrameCommand;
        private CheckBox? _showFirstBehaviorCheckBox;
        private CheckBox? _showMemberNameCheckBox;
        private CheckBox? _showSpriteNameCheckBox;
        private bool _updatingShowMenu;

        private int _port => BlingoRNetTerminal.Port;
        public RootWindow()
        {
           // _leftZone = new View();
        }


        public void BuildUi(Func<RNetCommand, CancellationToken?, Task> sendCommandAsync, Func<Task> toggleConnectionAsync, Action<int> setPort)
        {
            _setPort = setPort;
            _sendCommandAsync = sendCommandAsync;
            _score = BuildScoreWindow();
            _stage = BuildStageWindow();
            _cast = BuildCastWindow();
            _propInsp = CreatePropertyInspector(sendCommandAsync);
            var logs = CreateLog();
            var top = new Window
            {
                BorderStyle = LineStyle.None,
            }; // Application.Top;
            top.Border!.Width = 0;
            var menu = new MenuBarv2(new[]
            {
                new MenuBarItemv2("_Host", new[]
                {
                    NewMenuItemv2("_Connect/Disconnect", string.Empty, async () => await toggleConnectionAsync()),
                    NewMenuItemv2("_Host Port", string.Empty, SetPort),
                    NewMenuItemv2("_Quit", string.Empty, () => Application.RequestStop())
                }),
                new MenuBarItemv2("_Show", CreateShowMenuItems()),
                new MenuBarItemv2("_Help", Array.Empty<MenuItemv2>())
            });
            _stageBtn = NewMenuItemv2("_Stage", string.Empty, () => SwitchToStageMode());
            _castBtn = NewMenuItemv2("_Cast", string.Empty, () => SwitchToCastMode());
            _stageBtn.X = Pos.Absolute(15);
            _castBtn.X = Pos.Absolute(15);
            top.Add(menu);
            top.Add(_stageBtn);
            top.Add(_castBtn);
            RNetTerminalStyle.SetMenuSchema(menu);
            UpdateShowMenuChecks();
            

            _connectionStatusLabel = RUI.NewLabel(string.Empty, Pos.AnchorEnd(15), 0, 15);
            _connectionStatusLabel.TextAlignment = Alignment.End;
            _connectionStatusLabel.SetScheme(RNetTerminalStyle.MenuScheme);

            top.Add(_connectionStatusLabel);

            _rewindButton = RUI.NewButton("Rewind", false, OnRewindClicked);
            _rewindButton.X = Pos.Left(_connectionStatusLabel!) - 18;
            _rewindButton.Y = 0;
            _rewindButton.SetScheme(RNetTerminalStyle.MenuScheme);
            top.Add(_rewindButton);

            _playPauseButton = RUI.NewButton("Play", false, OnPlayPauseClicked);
            _playPauseButton.X = Pos.Right(_rewindButton) + 1;
            _playPauseButton.Y = 0;
            _playPauseButton.SetScheme(RNetTerminalStyle.MenuScheme);
            top.Add(_playPauseButton);

            var tv = new TileView
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()!-1,
                Orientation = Orientation.Vertical,
                BorderStyle = LineStyle.None
            };
            var tvMargin = tv.Margin;
            if (tvMargin != null)
            {
                tvMargin.Thickness = new Thickness(0);
            }
            var tvPadding = tv.Padding;
            if (tvPadding != null)
            {
                tvPadding.Thickness = new Thickness(0);
            }
            RNetTerminalStyle.SetTileViewSchema(tv);
            tv.Border!.Width = 0;
            tv.Border.Visible = false;
            tv.TrySplitTile(0, 2, out var tvLeft);
            tv.TrySplitTile(1, 2, out var tv2);
            var leftMargin = tvLeft.Margin;
            if (leftMargin != null)
            {
                leftMargin.Thickness = new Thickness(0);
            }
            var leftPadding = tvLeft.Padding;
            if (leftPadding != null)
            {
                leftPadding.Thickness = new Thickness(0);
            }
            var rightMargin = tv2.Margin;
            if (rightMargin != null)
            {
                rightMargin.Thickness = new Thickness(0);
            }
            var rightPadding = tv2.Padding;
            if (rightPadding != null)
            {
                rightPadding.Thickness = new Thickness(0);
            }

            _stage.Visible = true;
            _cast.Visible = false;

            tv.LineStyle = LineStyle.Single;
            tv2.LineStyle = LineStyle.Single;
            tv2.Tiles.ElementAt(0).Title = "Property Inspector";
            tv2.Tiles.ElementAt(0).ContentView!.Add(_propInsp);
            tv2.Tiles.ElementAt(1).Title = "Log";
            tv2.Tiles.ElementAt(1).ContentView!.Add(logs);

            tvLeft.Orientation = Orientation.Horizontal;
            _leftTopZone = tvLeft.Tiles.ElementAt(0);
            _leftTopZone.Title = "Stage";
            _leftTopZone.ContentView!.Add(_stage);
            tvLeft.Tiles.ElementAt(1).Title = "Score";
            tvLeft.Tiles.ElementAt(1).ContentView!.Add(_score);


            RNetTerminalStyle.SetTileViewSchema(tv);
            top.Add(tv);

            _infoItem = new Label { Text = "Frame:0 Channel:0 Sprite:- Member:" , Y= Pos.Align(Alignment.End)};
            RNetTerminalStyle.SetStatusBar(_infoItem);
            top.Add(_infoItem);

            var store = TerminalDataStore.Instance;
            store.MovieStateChanged += OnMovieStateChanged;
            OnMovieStateChanged(store.MovieState);

            UpdateConnectionStatus(BlingoNetConnectionState.Disconnected);
            Log("Starting...");
            Application.Run(top);
            top.Dispose();
            store.MovieStateChanged -= OnMovieStateChanged;
        }

        private IEnumerable<View> CreateShowMenuItems()
        {
            return new View[]
            {
                CreateShowMenuCheck("_Show first Behavior", () => _score?.ShowFirstBehavior ?? false, value =>
                {
                    if (_score != null)
                    {
                        _score.ShowFirstBehavior = value;
                    }
                }, checkBox => _showFirstBehaviorCheckBox = checkBox),
                CreateShowMenuCheck("Show _Member Name", () => _score?.ShowMemberName ?? false, value =>
                {
                    if (_score != null)
                    {
                        _score.ShowMemberName = value;
                    }
                }, checkBox => _showMemberNameCheckBox = checkBox),
                CreateShowMenuCheck("Show _Sprite Name", () => _score?.ShowSpriteName ?? false, value =>
                {
                    if (_score != null)
                    {
                        _score.ShowSpriteName = value;
                    }
                }, checkBox => _showSpriteNameCheckBox = checkBox)
            };
        }

        private MenuItemv2 CreateShowMenuCheck(string label, Func<bool> getter, Action<bool> setter, Action<CheckBox> register)
        {
            var checkBox = new CheckBox
            {
                Title = label,
                CheckedState = getter() ? CheckState.Checked : CheckState.UnChecked
            };
            register(checkBox);
            var menuItem = new MenuItemv2 { CommandView = checkBox };
            menuItem.SetScheme(RNetTerminalStyle.MenuScheme);
            checkBox.CheckedStateChanged += (_, _) =>
            {
                if (_updatingShowMenu)
                {
                    return;
                }

                setter(checkBox.CheckedState == CheckState.Checked);
                UpdateShowMenuChecks();
            };
            return menuItem;
        }

        private void UpdateShowMenuChecks()
        {
            if (_score == null)
            {
                return;
            }

            _updatingShowMenu = true;
            try
            {
                if (_showFirstBehaviorCheckBox != null)
                {
                    _showFirstBehaviorCheckBox.CheckedState = _score.ShowFirstBehavior ? CheckState.Checked : CheckState.UnChecked;
                }

                if (_showMemberNameCheckBox != null)
                {
                    _showMemberNameCheckBox.CheckedState = _score.ShowMemberName ? CheckState.Checked : CheckState.UnChecked;
                }

                if (_showSpriteNameCheckBox != null)
                {
                    _showSpriteNameCheckBox.CheckedState = _score.ShowSpriteName ? CheckState.Checked : CheckState.UnChecked;
                }
            }
            finally
            {
                _updatingShowMenu = false;
            }
        }

        private MenuItemv2 NewMenuItemv2(string text, string helperText, Action action)
        {
            var menuItem = new MenuItemv2(text, helperText, action);
            menuItem.SetScheme(RNetTerminalStyle.MenuScheme);
            return menuItem;
        }

        private ScoreView BuildScoreWindow()
        {
            var scoreView = new ScoreView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            scoreView.PlayFromHere += f =>
            {
                Log($"Play from {f}");
                QueueGoToFrame(f, force: true);
                if (!_isMoviePlaying)
                {
                    var store = TerminalDataStore.Instance;
                    if (store.ApplyLocalChanges)
                    {
                        ToggleLocalPlayback(true);
                    }
                    else
                    {
                        SendMovieCommand(new ResumeCmd());
                        var state = store.MovieState;
                        if (!state.IsPlaying || state.Frame != f)
                        {
                            store.UpdateMovieState(state with { Frame = f, IsPlaying = true });
                        }
                    }
                }
            };
            scoreView.InfoChanged += (f, ch, sp, mem) =>
            {
                UpdateInfo(f, ch, sp, mem);
                //TerminalDataStore.Instance.SetFrame(f);
                //QueueGoToFrame(f, force: true);
            };
            return scoreView;
        }

        private StageView BuildStageWindow()
        {
            var stageView = new StageView();
            stageView.X = 0;
            stageView.Y = 0;
            stageView.Width = Dim.Fill();
            stageView.Height = Dim.Fill();
            return stageView;
        }

        private CastView BuildCastWindow()
        {
            var castView = new CastView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            castView.MemberSelected += m =>
            {
                Log($"memberSelected {m.Name}");
                _propertyInspector?.ShowMember(m);
            };
            return castView;
        }

        private PropertyInspector CreatePropertyInspector(Func<RNetCommand, CancellationToken?, Task> sendCommandAsync)
        {
            //_propertyWindow = RUI.NewWindow("Properties", Pos.AnchorEnd(_propertyInspectorWidth + _logExpandedWidth), 1, _propertyInspectorWidth, Dim.Fill() - 1);

            _propertyInspector = new PropertyInspector
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            _propertyInspector.PropertyChanged += (target, n, v) =>
            {
                Log($"propertyChanged {n}={v}");
                var store = TerminalDataStore.Instance;
                store.PropertyHasChanged(target, n, v, _propertyInspector?.CurrentMember);
                if (target == PropertyTarget.Sprite)
                {
                    var sel = store.GetSelectedSprite();
                    if (sel.HasValue)
                    {
                        var spriteType = store.GetSpriteType(sel.Value);
                        _ = sendCommandAsync(new SetSpritePropCmd(sel.Value.SpriteNum, sel.Value.BeginFrame, spriteType, n, v), null);
                    }
                }
                else if (target == PropertyTarget.Member && _propertyInspector?.CurrentMember != null)
                {
                    var member = _propertyInspector.CurrentMember;
                    var memberType = member.Type.ToRNet();
                    _ = sendCommandAsync(new SetMemberPropCmd(member.CastLibNum, member.NumberInCast, memberType, n, v), null);
                }
            };
            return _propertyInspector;
        }
        private View CreateLog()
        {

            _logTextView = new TextView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                ReadOnly = true,
                Multiline = true,
                WordWrap = true
            };
            _logTextView.VerticalScrollBar.AutoShow = true;
            _logTextView.VerticalScrollBar.Visible = true;
            _logTextView.HorizontalScrollBar.AutoShow = false;
            _logTextView.HorizontalScrollBar.Visible = false;
            return _logTextView;
        }

      
        private void SwitchToStageMode()
        {
            if (_stage == null || _score == null)
                return;
            if (_stage.Visible) return;

            _stage.Visible = true;
            _cast.Visible = false;
            _stageBtn.Visible = false;
            _castBtn.Visible = true;
            _leftTopZone.ContentView!.Remove(_cast);
            _leftTopZone.ContentView!.Add(_stage);
            _score.SetFocus();
            _suppressNextFrameCommand = true;
            _score.TriggerInfo();
            _leftTopZone.Title = _stage.Visible ? "Stage" : "Cast";
            _stage.Draw();

        }

        private void SwitchToCastMode()
        {
            if (_cast == null || _score == null)
                return;
            if (_cast.Visible) return;
            _cast.Visible = true;
            _stage.Visible = false;
            _stageBtn.Visible = true;
            _castBtn.Visible = false;
            _leftTopZone.ContentView!.Remove(_stage);
            _leftTopZone.ContentView!.Add(_cast);
            _cast.SetFocus();
            _suppressNextFrameCommand = true;
            _score.TriggerInfo();
            _leftTopZone.Title = _stage.Visible ? "Stage" : "Cast";
        }

        public void UpdateConnectionStatus(BlingoNetConnectionState connected)
        {
            if (_connectionStatusLabel == null)
                return;
            
            _connectionStatusLabel.Text = connected == BlingoNetConnectionState.Connected ? "Connected" : "Disconnected";
            _connectionStatusLabel.SetNeedsDraw();
        }

        public void UpdateInfo(int frame, int channel, SpriteRef? sprite, BlingoMemberRefDTO? member)
        {
            if (_infoItem != null)
            {
                var store = TerminalDataStore.Instance;
                var memName = member!= null ? store.FindMember(member.CastLibNum, member.MemberNum)?.Name : null;
                _infoItem.Title = $"Frame:{frame} Channel:{channel} Sprite:{sprite?.SpriteNum.ToString() ?? "-"} Member:{memName ?? string.Empty}";
            }
            _score?.SetFocus();
        }

        private void OnMovieStateChanged(MovieStateDto state)
        {
            _isMoviePlaying = state.IsPlaying;
            _lastRequestedFrame = state.Frame;
            UpdatePlayPauseButton();
        }

        private void UpdatePlayPauseButton()
        {
            if (_playPauseButton == null)
            {
                return;
            }

            _playPauseButton.Text = _isMoviePlaying ? "Stop" : "Play";
            _playPauseButton.SetNeedsDraw();
        }

        private void QueueGoToFrame(int frame, bool force = false)
        {
            var store = TerminalDataStore.Instance;
            if (_suppressNextFrameCommand)
            {
                _suppressNextFrameCommand = false;
                return;
            }
            if (!force && !store.ApplyLocalChanges && _lastRequestedFrame == frame)
            {
                return;
            }

            _lastRequestedFrame = frame;

            if (!store.ApplyLocalChanges)
            {
                SendMovieCommand(new GoToFrameCmd(frame));
            }
        }

        private void SendMovieCommand(RNetCommand command)
        {
            var sender = _sendCommandAsync;
            if (sender == null)
            {
                return;
            }

            var task = sender(command, null);
            if (!task.IsCompletedSuccessfully)
            {
                _ = task.ContinueWith(t =>
                {
                    if (t.Exception is { } ex)
                    {
                        Log($"Command error: {ex.GetBaseException().Message}");
                    }
                }, TaskScheduler.Default);
            }
        }

        private void OnRewindClicked()
        {
            const int firstFrame = 1;
            var store = TerminalDataStore.Instance;
            store.SetFrame(firstFrame);

            if (store.ApplyLocalChanges)
            {
                ToggleLocalPlayback(false);
                return;
            }

            SendMovieCommand(new RewindCmd());
            ToggleLocalPlayback(false);
        }

        private void OnPlayPauseClicked()
        {
            var store = TerminalDataStore.Instance;
            if (store.ApplyLocalChanges)
            {
                ToggleLocalPlayback(!_isMoviePlaying);
                return;
            }

            if (_isMoviePlaying)
            {
                SendMovieCommand(new PauseCmd());
            }
            else
            {
                SendMovieCommand(new ResumeCmd());
            }
        }

        private void ToggleLocalPlayback(bool playing)
        {
            var store = TerminalDataStore.Instance;
            var state = store.MovieState with { IsPlaying = playing };
            store.UpdateMovieState(state);
        }



        private void SetPort()
        {
            var dialog = PortDialog.Create(_port, p =>
            {
                Log($"Port set to {p}.");
                _setPort(p);
            });
            Application.Run(dialog);
        }



        public void Log(string message)
        {
            void AddLog()
            {
                _logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                if (_logs.Count > 100)
                    _logs.RemoveAt(0);
                
                if (_logTextView != null)
                {
                    _logTextView.Text = string.Join(Environment.NewLine, _logs);
                    _logTextView.MoveEnd();
                }
            }

            Application.AddTimeout(TimeSpan.Zero, () =>
            {
                AddLog();
                return false; // do not repeat
            });
        }

        internal void SetPlayFrame(int f)
        {
            _score?.SetPlayFrame(f);
        }

        internal void UpdateIsRemove(bool remote)
        {
            if (_propertyInspector != null)
                _propertyInspector.DelayPropertyUpdates = remote;
        }
    }
}

