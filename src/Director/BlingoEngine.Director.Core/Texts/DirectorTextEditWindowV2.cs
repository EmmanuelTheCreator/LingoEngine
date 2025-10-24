using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Components.Graphics;
using AbstUI.Components.Inputs;
using AbstUI.Components.Texts;
using AbstUI.Primitives;
using AbstUI.Styles;
using AbstUI.Texts;
using AbstUI.Windowing;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Icons;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.UI;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Texts;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlingoEngine.Director.Core.Texts
{
    public class DirectorTextEditWindowV2 : DirectorWindow<IDirFrameworkTextEditWindow>
    {
        private readonly AbstInputText _markdownInput;
        private readonly AbstGfxCanvas _previewCanvas;
        private readonly AbstMarkdownRenderer _renderer;
        private readonly AbstPanel _rootPanel;
        private readonly AbstScrollContainer _markdownScroller;
        private readonly AbstScrollContainer _markdownScrollerContainer;
        private readonly IAbstLayoutNode _markdownInputContainer;
        private CancellationTokenSource? _renderCts;
        private SynchronizationContext? _uiContext;
        private IBlingoMemberTextBase? _member;
        private IAbstImagePainter _painter;
        private MemberNavigationBar<IBlingoMemberTextBase> _navBar;
        private bool _isSettingMemberValues = false;
        private readonly AbstLabel _caretLabel;
        private readonly IAbstLayoutNode _caretLabelContainer;

        public TextEditIconBar IconBar { get; }
        public AbstTextStyle CurrentStyle => IconBar.CurrentStyle;
        public AbstPanel RootPanel => _rootPanel;

        public DirectorTextEditWindowV2(IBlingoFrameworkFactory factory, IServiceProvider serviceProvider, IDirectorEventMediator mediator, IBlingoPlayer player, IDirectorIconManager iconManager) : base(serviceProvider, DirectorMenuCodes.TextEditWindow)
        {
            var fontManager = factory.ComponentFactory.GetRequiredService<IAbstFontManager>();
            _navBar = new MemberNavigationBar<IBlingoMemberTextBase>(mediator, player, iconManager, factory, 20);
            IconBar = new TextEditIconBar(factory);
            _renderer = new AbstMarkdownRenderer(fontManager);
            IconBar.SetFonts(fontManager.GetAllNames());
            _rootPanel = factory.CreatePanel("TextEditWindowV2Root");
            _rootPanel.BackgroundColor = DirectorColors.BG_WhiteMenus;
            _rootPanel.AddItem(_navBar.Panel, 0, 0);
            _rootPanel.AddItem(IconBar.Panel, 0, 25);
            // Previously disposed preview texture here; no longer required.
            _previewCanvas = factory.CreateGfxCanvas("MarkdownPreview", 400, 400);
            _markdownScroller = factory.CreateScrollContainer("MarkdownScroller");
            _markdownScroller.Width = 400;
            _markdownScroller.Height = 400;
            _markdownScroller.AddItem(_previewCanvas);
            _markdownScroller.ClipContents = true;
            _markdownScrollerContainer = (AbstScrollContainer)_rootPanel.AddItem(_markdownScroller, 420, 55);
            _painter = _factory.ComponentFactory.CreateImagePainterToTexture((int)_previewCanvas.Width, (int)_previewCanvas.Height);
            _painter.AutoResizeWidth = true;
            _painter.AutoResizeHeight = true;

            ConfigureIconBar();

            _markdownInput = factory.CreateInputText("MarkdownInput", onChange: OnTextChanged, multiLine: true);
            _markdownInput.Width = 400;
            _markdownInput.Height = 400;
            _markdownInputContainer = (IAbstLayoutNode)_rootPanel.AddItem(_markdownInput, 10, 55);

            _caretLabel = factory.CreateLabel("CaretPositionLabel", "Ln:0 Ch:0");
            _caretLabelContainer = (IAbstLayoutNode)_rootPanel.AddItem(_caretLabel, 0, 475);
            _markdownInput.OnCaretChanged += (l, c) => _caretLabel.Text = $"Ln:{l} Ch:{c}";

            Width = 820;
            Height = 520;
            MinimumWidth = 400;
            MinimumHeight = 460;
            X = 50;
            Y = 700;
        }

        private void ConfigureIconBar()
        {
            IconBar.BoldChanged += v =>
            {
                if (v)
                {
                    InsertAroundSelection("**", "**");
                    IconBar.SetBold(false);
                }
            };
            IconBar.ItalicChanged += v =>
            {
                if (v)
                {
                    InsertAroundSelection("*", "*");
                    IconBar.SetItalic(false);
                }
            };
            IconBar.UnderlineChanged += v =>
            {
                if (v)
                {
                    InsertAroundSelection("__", "__");
                    IconBar.SetUnderline(false);
                }
            };
            IconBar.AlignmentChanged += a =>
            {
                var style = EnsureParagraphStyle();
                style.Alignment = a;
                SyncMemberText();
                ScheduleRender(_markdownInput.Text);
            };
            IconBar.FontSizeChanged += v =>
            {
                var style = EnsureParagraphStyle();
                style.FontSize = v;
                SyncMemberText();
                ScheduleRender(_markdownInput.Text);
            };
            IconBar.FontChanged += n =>
            {
                if (string.IsNullOrEmpty(n)) return;
                var style = EnsureParagraphStyle();
                style.Font = n;
                SyncMemberText();
                ScheduleRender(_markdownInput.Text);
            };
            IconBar.ColorChanged += c =>
            {
                if (_markdownInput.HasSelection)
                {
                    var styleName = IconBar.CreateStyle(s => s.Color = c);
                    InsertAroundSelection($"{{{{STYLE:{styleName}}}}}", "{{/STYLE}}");
                }
                else
                {
                    var style = EnsureParagraphStyle();
                    style.Color = c;
                    SyncMemberText();
                    ScheduleRender(_markdownInput.Text);
                }
            };
        }

        protected override void OnInit(IAbstFrameworkWindow frameworkWindow)
        {
            base.OnInit(frameworkWindow);
            _uiContext = SynchronizationContext.Current;
            Content = _rootPanel;
            Title = "Text Editor";
        }

        protected override void OnDispose()
        {
            _renderCts?.Cancel();
            _renderCts?.Dispose();
            _painter?.Dispose();
            _markdownScroller?.Dispose();
            base.OnDispose();
        }

        protected override void OnResizing(bool firstLoad, int width, int height)
        {
            base.OnResizing(firstLoad, width, height);
            IconBar.OnResizing(width, height);
            _rootPanel.Width = width;
            _rootPanel.Height = height - 10;
            var contentHeight = (int)(height - IconBar.Panel.Height - 40);
            var innerWidth = (int)(width - 20);
            _markdownScroller.Width = innerWidth / 2;
            _markdownScroller.Height = (int)contentHeight;
            _markdownInput.Width = innerWidth / 2;
            _markdownInput.Height = contentHeight;
            _previewCanvas.Width = innerWidth / 2;
            _previewCanvas.Height = contentHeight;
            _markdownScrollerContainer.X = 10 + innerWidth / 2;
            _markdownScrollerContainer.Y = 55;
            _markdownInputContainer.X = 10;
            _markdownInputContainer.Y = 55;
            _caretLabelContainer.Y = height - 25;
        }

        public void SetMemberValues(IBlingoMemberTextBase textMember)
        {
            if (_isSettingMemberValues) return;
            _isSettingMemberValues = true;
            _member = textMember;
            _markdownInput.Text = textMember.Text;

            IconBar.SetMemberValues(textMember);
            RenderMarkdown(_markdownInput.Text);
            _navBar.SetMember(textMember);
            _isSettingMemberValues = false;
        }

        private void OnTextChanged(string text)
        {
            if (_isSettingMemberValues) return;
            if (_member != null)
                _member.Text = text;
            ScheduleRender(text);
        }

        private void SyncMemberText()
        {
            if (_isSettingMemberValues) return;
            if (_member != null)
                _member.Text = _markdownInput.Text;
        }

        private void ScheduleRender(string text)
        {
            _renderCts?.Cancel();
            var cts = new CancellationTokenSource();
            _renderCts = cts;
            Task.Delay(250, cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    if (_uiContext != null)
                        _uiContext.Post(_ => RenderMarkdown(text), null);
                    else
                        RenderMarkdown(text);
                }
            }, TaskScheduler.Default);
        }

        private void RenderMarkdown(string text)
        {
            _painter.Clear(AColors.Transparent);
            _painter.AutoResizeWidth = false;
            _painter.AutoResizeHeight = true;
            _painter.Render();
            _renderer.Reset();
            _renderer.SetText(text, IconBar.Styles);
            _renderer.Render(_painter, new APoint(0, 0));
            var previewTexture = _painter.GetTexture("MarkdownPreview");
            _previewCanvas.Clear(AColors.White);
            if (previewTexture != null)
                _previewCanvas.DrawPicture(previewTexture, previewTexture.Width, previewTexture.Height, new APoint(0, 0));
        }

        private AbstTextStyle EnsureParagraphStyle()
        {
            var (caretLine, caretColumn) = _markdownInput.GetCaretPosition();
            var text = _markdownInput.Text;
            int caret = GetOffset(text, caretLine, caretColumn);
            int lineStart = text.LastIndexOf('\n', Math.Max(0, caret - 1)) + 1;
            int lineEnd = text.IndexOf('\n', caret);
            if (lineEnd < 0) lineEnd = text.Length;

            var segment = text.Substring(lineStart, lineEnd - lineStart);
            var paraMatch = Regex.Match(segment, "^\\{\\{PARA:([^}]+)\\}\\}");
            string styleName;
            bool inserted = false;
            if (paraMatch.Success)
            {
                styleName = paraMatch.Groups[1].Value;
            }
            else
            {
                styleName = IconBar.CreateStyle();
                string tag = $"{{{{PARA:{styleName}}}}}";
                text = text.Insert(lineStart, tag);
                _markdownInput.Text = text;
                int newCaret = caret + tag.Length;
                var (nLine, nCol) = GetLineColumn(text, newCaret);
                _markdownInput.SetCaretPosition(nLine, nCol);
                SyncMemberText();
                inserted = true;
            }

            var style = IconBar.EnsureStyle(styleName);
            if (inserted)
                ScheduleRender(_markdownInput.Text);
            return style;
        }

        private void InsertAroundSelection(string prefix, string suffix)
        {
            var text = _markdownInput.Text;
            var (endLine, endCol) = _markdownInput.GetCaretPosition();
            if (_markdownInput.HasSelection)
            {
                var endIndex = GetOffset(text, endLine, endCol);
                _markdownInput.DeleteSelection();
                var (startLine, startCol) = _markdownInput.GetCaretPosition();
                int startIndex = GetOffset(text, startLine, startCol);
                var selected = text.Substring(startIndex, endIndex - startIndex);
                _markdownInput.InsertText(prefix + selected + suffix);
                int newIndex = startIndex + prefix.Length + selected.Length + suffix.Length;
                var (nLine, nCol) = GetLineColumn(_markdownInput.Text, newIndex);
                _markdownInput.SetCaretPosition(nLine, nCol);
            }
            else
            {
                _markdownInput.InsertText(prefix + suffix);
                int endIndex = GetOffset(text, endLine, endCol);
                int newIndex = endIndex + prefix.Length;
                var (nLine, nCol) = GetLineColumn(_markdownInput.Text, newIndex);
                _markdownInput.SetCaretPosition(nLine, nCol);
            }
            SyncMemberText();
            ScheduleRender(_markdownInput.Text);
        }

        private static int GetOffset(string text, int line, int column)
        {
            int index = 0;
            int currentLine = 0;
            while (index < text.Length && currentLine < line)
            {
                if (text[index] == '\n')
                    currentLine++;
                index++;
            }
            return Math.Clamp(index + column, 0, text.Length);
        }

        private static (int line, int column) GetLineColumn(string text, int index)
        {
            int line = 0;
            int column = 0;
            for (int i = 0; i < index && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }
            }
            return (line, column);
        }
    }
}

