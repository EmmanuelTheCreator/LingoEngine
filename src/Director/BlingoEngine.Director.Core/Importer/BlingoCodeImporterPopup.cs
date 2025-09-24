using AbstUI.Commands;
using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.FileSystems;
using BlingoEngine.Director.Core.Importer.Commands;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Legacy.Lingo;
using BlingoEngine.Scripts;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using TextCopy;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlingoEngine.Director.Core.Importer;

public class BlingoCodeImporterPopupHandler : IAbstCommandHandler<OpenBlingoCodeImporterCommand>
{
    private readonly IAbstWindowManager _windowManager;
    private readonly IAbstComponentFactory _factory;
    private readonly IDirFolderPicker _folderPicker;

    public BlingoCodeImporterPopupHandler(IAbstWindowManager windowManager, IAbstComponentFactory componentFactory, IDirFolderPicker folderPicker)
    {
        _windowManager = windowManager;
        _factory = componentFactory;
        _folderPicker = folderPicker;
    }

    public bool CanExecute(OpenBlingoCodeImporterCommand command) => true;

    public bool Handle(OpenBlingoCodeImporterCommand command)
    {
        var component = _factory.GetRequiredService<BlingoCodeImporterPopup>();
        var popupRef = _windowManager.ShowCustomDialog("Lingo code importer", component.GetFWPanel());
        return true;
    }
}

public class BlingoCodeImporterPopup 
{
    protected readonly IBlingoFrameworkFactory _factory;
    private readonly IDirFolderPicker _folderPicker;
    private readonly AbstPanel _panel;
    private AbstWrapPanel _folderPanel = null!;
    private AbstPanel _mainPanel = null!;
    private AbstItemList _fileList = null!;
    private Dictionary<string, ViewModel> _sourcefileList = new();
    private DirCodeHighlichter _blingoHighlighter = null!;
    private DirCodeHighlichter _csharpHighlighter = null!;
    private AbstInputText _errorInput = null!;
    private string _currentFolder = string.Empty;
    private AbstWrapPanel _menuBar;
    private ViewModel? _currentFile;

    protected sealed class ViewModel
    {
        public string FileAndPath { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Lingo { get; set; } = string.Empty;
        public string CSharp { get; set; } = string.Empty;
        public string Errors { get; set; } = string.Empty;
        public BlingoScriptType ScriptType { get; set; } 
    }

    public BlingoCodeImporterPopup(IBlingoFrameworkFactory factory, IDirFolderPicker folderPicker)
    {
        _factory = factory;
        _folderPicker = folderPicker;
        var vm = new ViewModel();
        _panel = BuildPanel();
        _blingoHighlighter.Update();
        _csharpHighlighter.Update();
    }

    public void Init(IAbstFrameworkDialog framework)
    {
        _blingoHighlighter.Update();
        _csharpHighlighter.Update();
    }

    internal IAbstFrameworkPanel GetFWPanel() => _panel.Framework<IAbstFrameworkPanel>();

    protected AbstPanel BuildPanel()
    {
        var root = _factory.CreatePanel("BlingoImporterRoot");
        root.BackgroundColor = DirectorColors.BG_WhiteMenus;
        root.Width = 1000;
        root.Height = 560;

        _folderPanel = _factory.CreateWrapPanel(AOrientation.Horizontal, "FolderSelect");
        _folderPanel.Width = 1000;
        _folderPanel.Height = 40;
        var folderInput = _factory.CreateInputText("FolderInput", 0, null);
        folderInput.Width = 800;
        folderInput.Height = 30;
        _folderPanel.AddItem(folderInput);
        var browse = _factory.CreateButton("BrowseButton", "Browse");
        browse.Pressed += () =>
        {
            //_folderPicker.StartFolder = 
            _folderPicker.PickFolder(path =>
                    {
                        folderInput.Text = path;
                        ShowFiles(path);
                    }, folderInput.Text);
        };
        _folderPanel.AddItem(browse);
        var open = _factory.CreateButton("OpenButton", "Open");
        open.Pressed += () =>
        {
            ShowFiles(folderInput.Text);
        };
        _folderPanel.AddItem(open);
        root.AddItem(_folderPanel);

        //_mainPanel = _factory.CreateWrapPanel(AOrientation.Horizontal, "MainContent");
        _mainPanel = _factory.CreatePanel("MainContent");
        _mainPanel.BackgroundColor = DirectorColors.BG_WhiteMenus;
        _mainPanel.Width = 1000;
        _mainPanel.Height = 520;
        _mainPanel.Margin = new AMargin(0, 40, 0, 0);
        _mainPanel.Visibility = false;
        root.AddItem(_mainPanel);

        _fileList = _factory.CreateItemList("FilesList", key =>
        {
            if (string.IsNullOrEmpty(key)) return;
            SelectFile(key);
        });
        _fileList.Width = 200;
        _fileList.Height = 420;
        _mainPanel.AddItem(_fileList,10,40);
        _menuBar = _factory.CreateWrapPanel(AOrientation.Horizontal, "MenuBar");
        _menuBar.Width = 600;
        _menuBar.Height = 40;
        _menuBar.Margin = new AMargin(10,10, 0, 0);
        _menuBar.Visibility = false;
        root.AddItem(_menuBar);

        _menuBar.ComposeForToolBar()
            .AddButton("ConvertButton", "Convert all", () => Convert());
        var converterPanel = BuildConverterArea();
        _mainPanel.AddItem(converterPanel,220,0);

        return root;
    }

    private AbstPanel BuildConverterArea()
    {
        
        var container = _factory.CreatePanel("ConverterPanel");
        container.Width = 800;
        container.Height = 520;
        container.BackgroundColor = DirectorColors.BG_WhiteMenus;

        var content = _factory.CreatePanel( "Content");
        content.Width = 800;
        content.Height = 460;
        content.BackgroundColor = DirectorColors.BG_WhiteMenus;
        container.AddItem(content);

        var left = _factory.CreateWrapPanel(AOrientation.Vertical, "BlingoColumn");
        left.Width = 400;
        left.Height = 460;
        content.AddItem(left,0,0);

        var right = _factory.CreateWrapPanel(AOrientation.Vertical, "CSharpColumn");
        right.Width = 400;
        right.Height = 460;
        content.AddItem(right,390,0);

        var leftHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "BlingoHeader");
        left.AddItem(leftHeader);
        leftHeader.Compose()
            .AddLabel("BlingoLabel", "Lingo")
            .AddButton("CopyBlingo", "Copy", () =>
            {
                if (_currentFile != null)
                    ClipboardService.SetText(_currentFile.Lingo);
            });

        _blingoHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>();
        _blingoHighlighter.CodeLanguage = DirCodeHighlichter.SourceCodeLanguage.Lingo;
        _blingoHighlighter.Width = 380;
        _blingoHighlighter.Height = 420;
        _blingoHighlighter.TextChanged += () =>
        {
            if (_currentFile != null) _currentFile.Lingo = _blingoHighlighter.Text;
        };
        left.AddItem(_blingoHighlighter.TextComponent);

        var rightHeader = _factory.CreateWrapPanel(AOrientation.Horizontal, "CSharpHeader");
        right.AddItem(rightHeader);
        rightHeader.Compose()
            .AddLabel("CSharpLabel", "C#")
            .AddButton("CopyCSharp", "Copy", () =>
            {
                if (_currentFile != null) ClipboardService.SetText(_currentFile.CSharp);
            });

        _csharpHighlighter = _factory.ComponentFactory.CreateElement<DirCodeHighlichter>();
        _csharpHighlighter.CodeLanguage = DirCodeHighlichter.SourceCodeLanguage.CSharp;
        _csharpHighlighter.Width = 380;
        _csharpHighlighter.Height = 420;
        _csharpHighlighter.TextChanged += () =>
        {
            if (_currentFile != null) _currentFile.CSharp = _csharpHighlighter.Text;
        };
        right.AddItem(_csharpHighlighter.TextComponent);

        _errorInput = _factory.CreateInputText("ErrorsText", 0, null);
        _errorInput.Width = 700;
        _errorInput.Height = 40;
        _errorInput.IsMultiLine = true;
        _errorInput.Enabled = false;
        _errorInput.Margin = new AMargin(0, 500, 0, 0);
        container.AddItem(_errorInput, 0, 450);

        

        return container;
    }

    private void ShowFiles(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return;
        _currentFolder = folder;
        _fileList.ClearItems();
        _sourcefileList.Clear();
        var allFiles = Directory.GetFiles(folder, "*.ls", SearchOption.AllDirectories);
        foreach (var file in allFiles)
        {
            var filename = Path.GetFileName(file);
            _fileList.AddItem(file, filename);
            _sourcefileList.Add(filename,new ViewModel { FileName = filename, FileAndPath = file, Lingo = File.ReadAllText(file) });
        }
        _folderPanel.Visibility = false;
        _mainPanel.Visibility = true;
        _menuBar.Visibility = true;
    }

    private void SelectFile(string path)
    {
        //if (!File.Exists(path)) return;
        //var text = File.ReadAllText(path);
        _currentFile = _sourcefileList.First(x => x.Value.FileAndPath == path).Value;
        var text = _currentFile.Lingo.Replace("\r", "\n");
        _blingoHighlighter.SetText(text);
        _csharpHighlighter.SetText(_currentFile.CSharp);
        _errorInput.Text = _currentFile.Errors;
    }

    private void Convert()
    {
        var converter = new BlingoToCSharpConverter();
        var bundle = _sourcefileList.Values.Select(x => new BlingoScriptFile(x.FileName,x.Lingo,x.ScriptType)).ToList();
        var result = converter.Convert(bundle);
        foreach (var converted in bundle)
        {
            var vm = _sourcefileList[converted.Name];
            vm.CSharp = converted.CSharp;
        }
        //_csharpHighlighter.SetText(vm.CSharp);

        _errorInput.Text = converter.GetCurrentErrorsAndFlush();
    }

    public void Dispose()
    {
        _blingoHighlighter.Dispose();
        _csharpHighlighter.Dispose();
    }
}

