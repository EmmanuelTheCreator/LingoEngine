using System.Linq;
using FluentAssertions;
using System;
using System.Text.RegularExpressions;
using BlingoEngine.Lingo.Core;
using Xunit;

namespace BlingoEngine.Lingo.Core.Tests;

public class ClassGenerationTests
{
    private readonly BlingoToCSharpConverter _converter = new();
    [Fact]
    public void BehaviorScriptGeneratesClass()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyBehavior",
            Source = "",
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : BlingoSpriteBehavior",
            "{",
            "    public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }",
            "}");
        ShouldBeEqual(expected, result);
    }

    [Fact]
    public void ParentScriptGeneratesClass()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyParent",
            Source = "",
            Type = BlingoScriptType.Parent
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyParentParent : BlingoParentScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyParentParent(IBlingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
            "    }",
            "}");
        ShouldBeEqual(expected, result);
    }

    [Fact]
    public void NewHandlerBecomesConstructor()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyParent",
            Source = string.Join('\n',
                "property myVar",
                "on new me, x",
                "  myVar = x",
                "end"),
            Type = BlingoScriptType.Parent
        };
        var result = _converter.Convert(file).Trim();
        var expected = @"using System;
using BlingoEngine.Lingo.Core;

namespace Generated;

public class MyParentParent : BlingoParentScript
{
    public object myVar { get; set; }

    private readonly GlobalVars _global;

    public MyParentParent(IBlingoMovieEnvironment env, GlobalVars global, object x) : base(env)
    {
        _global = global;
        myVar = x;
    }
}";
        ShouldBeEqual(expected, result);
    }

    [Fact]
    public void MovieScriptGeneratesClass()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyMovie",
            Source = "",
            Type = BlingoScriptType.Movie
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyMovieMovieScript : BlingoMovieScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyMovieMovieScript(IBlingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
            "    }",
            "}");
        ShouldBeEqual(expected, result);
    }

    [Fact]
    public void BehaviorScriptWithPropertyDescriptionListImplementsInterface()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyBehavior",
            Source = "on getPropertyDescriptionList\nend",
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : BlingoSpriteBehavior, IBlingoPropertyDescriptionList",
            "{",
            "    public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }",
            "",
            "    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()",
            "    {",
            "        return new BehaviorPropertyDescriptionList()",
            "        ;",
            "    }",
            "}");
        ShouldBeEqual(expected, result);
    }

    [Fact]
    public void ScriptWithPropertyDescriptionListForcesBehaviorType()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyScript",
            Source = "on getPropertyDescriptionList\nend",
            Type = BlingoScriptType.Movie
        };
        var result = _converter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyScriptBehavior : BlingoSpriteBehavior, IBlingoPropertyDescriptionList",
            "{",
            "    public MyScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }",
            "",
            "    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()",
            "    {",
            "        return new BehaviorPropertyDescriptionList()",
            "        ;",
            "    }",
            "}");
        ShouldBeEqual(expected, result);
    }

    [Fact]
    public void ClassNameStripsLsSuffix()
    {
        var file = new BlingoScriptFile
        {
            Name = "Stop_Menu_ls",
            Source = string.Empty,
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("class Stop_MenuBehavior", result);
    }

    [Fact]
    public void ClassNameDoesNotStartWithUnderscore()
    {
        var file = new BlingoScriptFile
        {
            Name = "p__MyBehavior",
            Source = string.Empty,
            Type = BlingoScriptType.Behavior
        };

        var result = _converter.ConvertClass(file);
        var match = Regex.Match(result, @"class\s+(\w+)");
        Assert.True(match.Success);
        Assert.NotEqual('_', match.Groups[1].Value[0]);
    }

    [Fact]
    public void CustomSuffixesAreApplied()
    {
        var settings = new BlingoToCSharpConverterSettings
        {
            BehaviorSuffix = "Beh",
            ParentSuffix = "Par",
            MovieScriptSuffix = "Mov"
        };
        var converter = new BlingoToCSharpConverter(settings);

        var b = converter.ConvertClass(new BlingoScriptFile { Name = "Foo_ls", Source = "", Type = BlingoScriptType.Behavior });
        var p = converter.ConvertClass(new BlingoScriptFile { Name = "Bar_ls", Source = "", Type = BlingoScriptType.Parent });
        var m = converter.ConvertClass(new BlingoScriptFile { Name = "Baz_ls", Source = "", Type = BlingoScriptType.Movie });

        Assert.Contains("class FooBeh", b);
        Assert.Contains("class BarPar", p);
        Assert.Contains("class BazMov", m);
    }

    [Fact]
    public void ScriptWithFullPropertyDescriptionListCreatesBehaviorClass()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyComplexScript",
            Source = @"on getPropertyDescriptionList
  description = [:]
  addProp description,#myStartMembernum, [#default:0, #format:#integer, #comment:""My Start membernum:""]
  addProp description,#myEndMembernum, [#default:10, #format:#integer, #comment:""My End membernum:""]
  addProp description,#myValue, [#default:-1, #format:#integer, #comment:""My Start Value:""]
  addProp description,#mySlowDown, [#default:1, #format:#integer, #comment:""mySlowDown:""]
  addProp description,#myDataSpriteNum, [#default:1, #format:#integer, #comment:""My Sprite that contains info\n(set value to -1):""]
  addProp description,#myDataName, [#default:1, #format:#string, #comment:""Name Info:""]
  addProp description,#myWaitbeforeExecute, [#default:0, #format:#integer, #comment:""WaitTime before execute:""]
  addProp description,#myFunction, [#default:70, #format:#symbol, #comment:""function to execute:""]
  return description
end",
            Type = BlingoScriptType.Movie
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("public class MyComplexScriptBehavior : BlingoSpriteBehavior, IBlingoPropertyDescriptionList", result);
        Assert.Contains("public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()", result);
        Assert.Contains("return new BehaviorPropertyDescriptionList()", result);
        Assert.Contains(".Add(this, x => x.myStartMembernum, \"My Start membernum:\", 0)", result);
        Assert.Contains(".Add(this, x => x.myFunction, \"function to execute:\", \"70\")", result);
        Assert.Contains("public int myStartMembernum { get; set; } = 0;", result);
        Assert.Contains("public string myFunction { get; set; } = \"70\";", result);

        var addMatches = Regex.Matches(result, @"\.Add\(this,\s*x => x\.(?<name>\w+)");
        var propertyNames = addMatches.Cast<Match>().Select(m => m.Groups["name"].Value).ToList();
        Assert.Equal(8, propertyNames.Count);
        Assert.Equal(propertyNames.Count, propertyNames.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void LeadingCommentsArePreserved()
    {
        var source = string.Join('\n',
            "-- Copyright Foo",
            "-- This file was written in 2005",
            string.Empty,
            "property myVar",
            "on startMovie",
            "end");
        var file = new BlingoScriptFile
        {
            Name = "Commented",
            Source = source,
            Type = BlingoScriptType.Behavior
        };

        var result = _converter.Convert(file).Replace("\r", string.Empty);
        var commentIndex = result.IndexOf("// Copyright Foo", StringComparison.Ordinal);
        Assert.True(commentIndex >= 0);
        var classMatch = Regex.Match(result, @"public class (\w+)");
        Assert.True(classMatch.Success);
        Assert.True(classMatch.Index > commentIndex);
        Assert.Contains("// This file was written in 2005", result);
    }

    [Fact]
    public void DuplicatePropertyDescriptionEntriesAreOverwrittenInClassGeneration()
    {
        var file = new BlingoScriptFile
        {
            Name = "DupScript",
            Source = @"on getPropertyDescriptionList
  description = [:]
  addProp description,#myValue, [#default:1, #format:#integer, #comment:""First:""]
  addProp description,#myValue, [#default:2, #format:#integer, #comment:""Second:""]
  return description
end",
            Type = BlingoScriptType.Behavior
        };

        var result = _converter.ConvertClass(file);

        var matches = Regex.Matches(result, @"\.Add\(this,\s*x => x\.(?<name>\w+)");
        var propertyNames = matches.Cast<Match>().Select(m => m.Groups["name"].Value).ToList();
        Assert.Single(propertyNames);

        var valueMatch = Regex.Match(result, @"\.Add\(this,\s*x => x\.myValue,\s*""(?<comment>[^""]*)"",\s*(?<default>[^\)]+)\)");
        Assert.True(valueMatch.Success);
        Assert.Equal("Second:", valueMatch.Groups["comment"].Value);
        Assert.Equal("2", valueMatch.Groups["default"].Value.Trim());
    }

    [Fact]
    public void PropertyDeclarationsBecomeProperties()
    {
        var file = new BlingoScriptFile
        {
            Name = "MyVars",
            Source = @"property myVar1, myVar2
on startMovie
end",
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("public object myVar1 { get; set; }", result);
        Assert.Contains("public object myVar2 { get; set; }", result);
    }

    [Fact]
    public void PropertyTypesAreInferredFromAssignments()
    {
        var file = new BlingoScriptFile
        {
            Name = "InferTypes",
            Source = @"property a, b, c, d
on startMovie
  a = 1
  b = ""hi""
  c = member(""foo"").text
  d = false
end",
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.ConvertClass(file);
        Assert.Contains("public int a { get; set; }", result);
        Assert.Contains("public string b { get; set; }", result);
        Assert.Contains("public string c { get; set; }", result);
        Assert.Contains("public bool d { get; set; }", result);
    }

    [Fact]
    public void AppendInfersBlingoList()
    {
        var file = new BlingoScriptFile
        {
            Name = "Score",
            Source = @"property tempScore
on startMovie
  append tempScore, 5
end",
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.Convert(file);
        Assert.Contains("public BlingoList<int> tempScore { get; set; } = new();", result);
        Assert.Contains("tempScore.Add(5);", result);
    }

    [Fact]
    public void AddAtInfersBlingoList()
    {
        var file = new BlingoScriptFile
        {
            Name = "Numbers",
            Source = @"property nums
on startMovie
  addAt nums, 1, 10
end",
            Type = BlingoScriptType.Behavior
        };
        var result = _converter.Convert(file);
        Assert.Contains("public BlingoList<int> nums { get; set; } = new();", result);
        Assert.Contains("nums.AddAt(1, 10);", result);
    }

    [Theory]
    [InlineData("blur", "IHasBlurEvent", null)]
    [InlineData("focus", "IHasFocusEvent", null)]
    [InlineData("keyDown", "IHasKeyDownEvent", "BlingoKeyEvent key")]
    [InlineData("keyUp", "IHasKeyUpEvent", "BlingoKeyEvent key")]
    [InlineData("mouseWithin", "IHasMouseWithinEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseLeave", "IHasMouseLeaveEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseDown", "IHasMouseDownEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseUp", "IHasMouseUpEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseMove", "IHasMouseMoveEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseWheel", "IHasMouseWheelEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseEnter", "IHasMouseEnterEvent", "BlingoMouseEvent mouse")]
    [InlineData("mouseExit", "IHasMouseExitEvent", "BlingoMouseEvent mouse")]
    [InlineData("beginSprite", "IHasBeginSpriteEvent", null)]
    [InlineData("endSprite", "IHasEndSpriteEvent", null)]
    [InlineData("stepFrame", "IHasStepFrameEvent", null)]
    [InlineData("prepareFrame", "IHasPrepareFrameEvent", null)]
    [InlineData("enterFrame", "IHasEnterFrameEvent", null)]
    [InlineData("exitFrame", "IHasExitFrameEvent", null)]

    public void EventHandlersImplementInterfaces(string handler, string expectedInterface, string? param)
    {
        var methodName = expectedInterface[4..^5];
        var variants = new[]
        {
            handler,
            handler.ToLowerInvariant(),
            handler.ToUpperInvariant(),
            new string(handler.Select((c, i) => i % 2 == 0 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c)).ToArray())
        };

        foreach (var h in variants)
        {
            var file = new BlingoScriptFile
            {
                Name = "MyScript",
                Source = $"on {h} me\nend",
                Type = BlingoScriptType.Behavior
            };

            var result = _converter.Convert(file);
            var classDecl = $"public class MyScriptBehavior : BlingoSpriteBehavior, {expectedInterface}";
            Assert.Contains(classDecl, result);
            if (param is null)
                Assert.Contains($"public void {methodName}()", result);
            else
                Assert.Contains($"public void {methodName}({param})", result);
        }

    }
    private static string ShouldBeEqual(string generated,string expected)
    {
        var normGen = NormalizeLineEndings(generated).Trim();
        var normExp = NormalizeLineEndings(expected).Trim();
        normExp.Should().BeEquivalentTo(normGen);
        return normGen;
    }
    private static string NormalizeLineEndings(string input) => input.Replace("\r\n", "\n").Replace("\r", "\n");
}

