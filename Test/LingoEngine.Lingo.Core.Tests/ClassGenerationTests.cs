using LingoEngine.Lingo.Core;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class ClassGenerationTests
{
    [Fact]
    public void BehaviorScriptGeneratesClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyBehavior",
            Source = "",
            Type = LingoScriptType.Behavior
        };
        var result = LingoToCSharpConverter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : LingoSpriteBehavior",
            "{",
            "    public MyBehaviorBehavior(ILingoMovieEnvironment env) : base(env) { }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParentScriptGeneratesClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyParent",
            Source = "",
            Type = LingoScriptType.Parent
        };
        var result = LingoToCSharpConverter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyParentParentScript : LingoParentScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyParentParentScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void MovieScriptGeneratesClass()
    {
        var file = new LingoScriptFile
        {
            Name = "MyMovie",
            Source = "",
            Type = LingoScriptType.Movie
        };
        var result = LingoToCSharpConverter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyMovieMovieScript : LingoMovieScript",
            "{",
            "    private readonly GlobalVars _global;",
            "",
            "    public MyMovieMovieScript(ILingoMovieEnvironment env, GlobalVars global) : base(env)",
            "    {",
            "        _global = global;",
            "    }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BehaviorScriptWithAllPropertyDescriptionHandlersImplementsInterface()
    {
        var file = new LingoScriptFile
        {
            Name = "MyBehavior",
            Source = string.Join('\n',
                "on getPropertyDescriptionList",
                "end",
                "on getBehaviorDescription",
                "end",
                "on getBehaviorTooltip",
                "end",
                "on runPropertyDialog",
                "end",
                "on isOKToAttach",
                "end"),
            Type = LingoScriptType.Behavior
        };
        var result = LingoToCSharpConverter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : LingoSpriteBehavior, ILingoPropertyDescriptionList",
            "{",
            "    public MyBehaviorBehavior(ILingoMovieEnvironment env) : base(env) { }",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BehaviorScriptWithPartialPropertyDescriptionHandlersDoesNotImplementInterface()
    {
        var file = new LingoScriptFile
        {
            Name = "MyBehavior",
            Source = "on getPropertyDescriptionList\nend",
            Type = LingoScriptType.Behavior
        };
        var result = LingoToCSharpConverter.ConvertClass(file).Trim();
        var expected = string.Join('\n',
            "public class MyBehaviorBehavior : LingoSpriteBehavior",
            "{",
            "    public MyBehaviorBehavior(ILingoMovieEnvironment env) : base(env) { }",
            "}");
        Assert.Equal(expected, result);
    }
}
