using AbstUI.Components;
using AbstUI.Primitives;
using AbstUI.Commands;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Legacy.Lingo;
using TextCopy;
using AbstUI.Windowing;
using AbstUI.Components.Containers;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Director.Core.Importer.Commands;
using BlingoEngine.Director.Core.Styles;

namespace BlingoEngine.Director.Core.Importer;

public class BlingoCSharpConverterPopupHandler : IAbstCommandHandler<OpenBlingoCSharpConverterCommand> 
{
    private readonly IAbstWindowManager _windowManager;
    private readonly IAbstComponentFactory _componentFactory;

    public BlingoCSharpConverterPopupHandler(IAbstWindowManager windowManager, IAbstComponentFactory componentFactory)
    {
        _windowManager = windowManager;
        _componentFactory = componentFactory;
    }
    public bool CanExecute(OpenBlingoCSharpConverterCommand command) => true;

    public virtual bool Handle(OpenBlingoCSharpConverterCommand command)
    {
        var component = _componentFactory.GetRequiredService<BlingoCSharpConverterPopup>();
        _windowManager.ShowCustomDialog("Lingo to C#", component.GetFWPanel());
        return true;
    }
}
public class BlingoCSharpConverterPopup 
{
    protected readonly IBlingoFrameworkFactory _factory;
    private readonly AbstPanel _panel;
    private DirCodeHighlichter _csharpHighlighter = null!;
    private DirCodeHighlichter _blingoHighlighter = null!;

    protected sealed class ViewModel
    {
        public string Lingo { get; set; } = string.Empty;
        public string CSharp { get; set; } = string.Empty;
        public string Errors { get; set; } = string.Empty;
    }

    public BlingoCSharpConverterPopup(IBlingoFrameworkFactory factory)
    {
        _factory = factory;
        var vm = new ViewModel();
        _panel = BuildPanel(vm);
        _blingoHighlighter.Update();
        _csharpHighlighter.Update();
    }

  

 
    internal IAbstFrameworkPanel GetFWPanel() => _panel.Framework<IAbstFrameworkPanel>();
    protected AbstPanel BuildPanel(ViewModel vm)
    {
        var root = _factory.CreatePanel("BlingoCSharpRoot");
        root.Width = 810;
        root.Height = 560;
        root.BackgroundColor = DirectorColors.BG_WhiteMenus;

        //var content = _factory.CreateWrapPanel(AOrientation.Horizontal, "Content");
        var content = _factory.CreatePanel("Content");
        content.Width = 820;
        content.Height = 470;
        content.BackgroundColor = DirectorColors.BG_WhiteMenus;
        root.AddItem(content);

        var left = _factory.CreateWrapPanel(AOrientation.Vertical, "BlingoColumn");
        left.Width = 400;
        left.Height = 460;
        content.AddItem(left);

        var right = _factory.CreateWrapPanel(AOrientation.Vertical, "CSharpColumn");
        right.Width = 400;
        right.Height = 460;
        content.AddItem(right,405,0);

        var leftHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "BlingoHeader");
        left.AddItem(leftHeader);
        leftHeader.Compose()
            .AddLabel("BlingoLabel", "Lingo")
            .AddButton("CopyBlingo", "Copy", () => ClipboardService.SetText(vm.Lingo));

        _blingoHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>(); // new DirCodeHighlichter(_factory, DirCodeHighlichter.Language.Lingo);
        _blingoHighlighter.CodeLanguage = DirCodeHighlichter.SourceCodeLanguage.Lingo;
        _blingoHighlighter.Width = 380;
        _blingoHighlighter.Height = 420;
        _blingoHighlighter.TextChanged += () => vm.Lingo = _blingoHighlighter.Text;
        
        left.AddItem(_blingoHighlighter.TextComponent);

        var rightHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "CSharpHeader");
        right.AddItem(rightHeader);
        rightHeader.Compose()
            .AddLabel("CSharpLabel", "C#")
            .AddButton("CopyCSharp", "Copy", () => ClipboardService.SetText(vm.CSharp));

        _csharpHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>();
        _csharpHighlighter.CodeLanguage = DirCodeHighlichter.SourceCodeLanguage.CSharp;
        _csharpHighlighter.Width = 380;
        _csharpHighlighter.Height = 420;
        _csharpHighlighter.TextChanged += () => vm.CSharp = _csharpHighlighter.Text;
        
        right.AddItem(_csharpHighlighter.TextComponent);

        var errorInput = _factory.CreateInputText("ErrorsText", 0, null);
        errorInput.Width = 800;
        errorInput.Height = 60;
        errorInput.IsMultiLine = true;
        errorInput.Enabled = false;
        errorInput.Margin = new AMargin(0, 500, 0, 0);
        //prop?.SetValue(framework, DirectorColors.Notification_Error_Border);
        root.AddItem(errorInput, 0, 450);

        var menuBar = _factory.CreateWrapPanel(AOrientation.Horizontal, "BottomBar");
        menuBar.Width = 800;
        menuBar.Margin = new AMargin(40, 520, 0, 0);
        root.AddItem(menuBar); // _factory.CreateLayoutWrapper(menuBar, 0, 860));

        menuBar.ComposeForToolBar()
            .AddButton("ConvertButton", "Convert", () =>
            {
                var converter = new BlingoToCSharpConverter();
                vm.CSharp = converter.Convert(vm.Lingo);
                _csharpHighlighter.SetText(vm.CSharp);
                vm.Errors = converter.GetCurrentErrorsAndFlush();
                errorInput.Text = vm.Errors;
            }, c => { c.Width = 150; c.Height = 30; });

        return root;
    }

    public void Dispose()
    {
        _blingoHighlighter.Dispose();
        _csharpHighlighter.Dispose();
    }

   

    public DirCodeHighlichter BlingoHighlighter => _blingoHighlighter;
    public DirCodeHighlichter CSharpHighlighter => _csharpHighlighter;
}

