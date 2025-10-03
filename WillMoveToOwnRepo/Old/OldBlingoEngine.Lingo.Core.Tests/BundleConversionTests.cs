using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using OldBlingoEngine.Lingo.Core;

namespace BlingoEngine.Lingo.Core.Tests;

public class BundleConversionTests
{
    private readonly BlingoToCSharpConverter _converter = new();

    [Fact]
    public void BundleConversionTranslatesSimpleScript()
    {
        var script = new BlingoScriptFile("Simple", "on startMovie\nput 1 into x\nend");
        _converter.Convert(new[] { script });
        Assert.Contains("x = 1;", script.CSharp);
    }

    [Fact]
    public void DemoScriptsAreConvertedToValidClasses()
    {
        var baseDir = TetriGroundsTestData.GetOriginalScriptsDirectory();
        var files = Directory.GetFiles(baseDir, "*.ls");
        var scripts = files
            .Select(f =>
            {
                var rel = Path.GetRelativePath(baseDir, Path.GetDirectoryName(f)!);
                rel = rel == "." ? null : rel;
                return new BlingoScriptFile(Path.GetFileNameWithoutExtension(f), File.ReadAllText(f))
                {
                    RelativeDirectory = rel
                };
            })
            .ToList();
        _converter.Convert(scripts, new ConversionOptions { Namespace = "Demo.TetriGrounds" });
        var successful = scripts.Where(s => string.IsNullOrEmpty(s.Errors)).ToList();
        Assert.NotEmpty(successful);
        Assert.All(successful, s =>
        {
            var match = Regex.Match(s.CSharp, @"class\s+(\w+)");
            Assert.True(match.Success);
            Assert.True(char.IsLetter(match.Groups[1].Value[0]));
            Assert.DoesNotMatch(@"^L\d", match.Groups[1].Value);
        });
    }

    [Fact]
    public void DemoScriptsAreWrittenToGeneratedFolder()
    {
        var baseDir = TetriGroundsTestData.GetOriginalScriptsDirectory();
        var generatedDir = Path.Combine(baseDir, "Generated");
        if (Directory.Exists(generatedDir))
            Directory.Delete(generatedDir, true);
        Directory.CreateDirectory(generatedDir);

        var files = Directory.GetFiles(baseDir, "*.ls");
        var scripts = files
            .Select(f =>
            {
                var rel = Path.GetRelativePath(baseDir, Path.GetDirectoryName(f)!);
                rel = rel == "." ? null : rel;
                return new BlingoScriptFile(Path.GetFileNameWithoutExtension(f), File.ReadAllText(f))
                {
                    RelativeDirectory = rel
                };
            })
            .ToList();
        _converter.Convert(scripts, new ConversionOptions { Namespace = "Demo.TetriGrounds" });

        foreach (var script in scripts)
        {
            var safeName = Regex.Replace(script.Name, @"[^A-Za-z0-9_]+", "_");
            File.WriteAllText(Path.Combine(generatedDir, $"{safeName}.cs"), script.CSharp ?? string.Empty);
        }

        Assert.Equal(scripts.Count, Directory.GetFiles(generatedDir, "*.cs").Length);
    }

    [Fact]
    public void SpriteManagerScriptIsConverted()
    {
        var baseDir = TetriGroundsTestData.GetOriginalScriptsDirectory();
        var path = Path.Combine(baseDir, "3_SpriteManager.ls");
        var script = new BlingoScriptFile("3_SpriteManager", File.ReadAllText(path)) { RelativeDirectory = null };
        _converter.Convert(new[] { script }, new ConversionOptions { Namespace = "Demo.TetriGrounds" });
        Assert.True(string.IsNullOrEmpty(script.Errors));
        Assert.False(string.IsNullOrWhiteSpace(script.CSharp));
    }

    [Fact]
    public void PropertyTypesAreInferredFromAssignments()
    {
        var source = @"property myMember, myMembers, myMemberNumAnim, myDestroyAnim, myColor\n" +
                     "on getPropertyDescriptionList\n" +
                     "  addProp description,#myColor,[#default:rgb(0),#format:#color]\n" +
                     "  return description\n" +
                     "end\n" +
                     "on startMovie\n" +
                     "  myMembers = [\"Block1\", \"Block2\"]\n" +
                     "  myMember = myMembers[1]\n" +
                     "  myMemberNumAnim = 0\n" +
                     "  myDestroyAnim = true\n" +
                     "end";
        var script = new BlingoScriptFile("10_Block", source, ScriptDetectionType.Behavior);
        _converter.Convert(new[] { script }, new ConversionOptions { Namespace = "Demo.TetriGrounds" });
        Assert.Contains("class BlockBehavior", script.CSharp);
        Assert.Contains("public string myMember { get; set; }", script.CSharp);
        Assert.Contains("public BlingoList<string> myMembers { get; set; } = new();", script.CSharp);
        Assert.Contains("public int myMemberNumAnim { get; set; }", script.CSharp);
        Assert.Contains("public bool myDestroyAnim { get; set; }", script.CSharp);
        Assert.Contains("public AColor myColor { get; set; } = AColor.FromCode(0);", script.CSharp);
    }

    [Fact]
    public void ConstructorParameterTypesAreInferred()
    {
        var baseDir = TetriGroundsTestData.GetOriginalScriptsDirectory();
        var path = Path.Combine(baseDir, "3_SpriteManager.ls");
        var script = new BlingoScriptFile("3_SpriteManager", File.ReadAllText(path), ScriptDetectionType.Parent);
        _converter.Convert(new[] { script });
        Assert.Contains("public int pNum { get; set; }", script.CSharp);
        Assert.Contains("SpriteManagerParent(IBlingoMovieEnvironment env, GlobalVars global, int _beginningsprite)", script.CSharp, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParameterAssignedAfterVoidCheckIsInferred()
    {
        var baseDir = TetriGroundsTestData.GetOriginalScriptsDirectory();
        var path = Path.Combine(baseDir, "10_Block.ls");
        var script = new BlingoScriptFile("10_Block", File.ReadAllText(path), ScriptDetectionType.Parent);
        _converter.Convert(new[] { script });
        Assert.Contains("BlockParent(IBlingoMovieEnvironment env, GlobalVars global, object _Gfx, int ChosenType)", script.CSharp);
    }

    [Fact]
    public void ScriptNewCallInstantiatesWithEnvAndGlobals()
    {
        var spriteMgr = new BlingoScriptFile("3_SpriteManager", "on startMovie\nend", ScriptDetectionType.Parent);
        var bgSource = string.Join('\n',
            "on startMovie",
            "  gSpriteManager = script(\"SpriteManager\")",
            "  .new(100)",
            "end");
        var bg = new BlingoScriptFile("2_Bg_Script", bgSource);
        _converter.Convert(new[] { spriteMgr, bg }, new ConversionOptions { Namespace = "Demo.TetriGrounds" });
        Assert.Contains("gSpriteManager = new SpriteManagerParent(_env, _globalvars, 100)", bg.CSharp);
    }

    [Fact]
    public void NamespaceReflectsDirectoryStructure()
    {
        var script = new BlingoScriptFile("My", "on startMovie\nend")
        {
            RelativeDirectory = Path.Combine("foo", "bar")
        };
        _converter.Convert(new[] { script }, new ConversionOptions { Namespace = "Base" });
        Assert.Contains("namespace Base.Foo.Bar;", script.CSharp);
    }
}

