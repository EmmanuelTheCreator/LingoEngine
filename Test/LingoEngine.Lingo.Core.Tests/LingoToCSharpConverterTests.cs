using LingoEngine.Lingo.Core.Tokenizer;
using LingoEngine.Lingo.Core;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class LingoToCSharpConverterTests
{
    [Fact]
    public void PutStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("put 1 into x");
        Assert.Equal("x = 1;", result.Trim());
    }

    [Fact]
    public void AssignmentStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("x = 5");
        Assert.Equal("x = 5;", result.Trim());
    }

    [Fact]
    public void CallStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("myFunc");
        Assert.Equal("myFunc();", result.Trim());
    }

    [Fact]
    public void IfStatementIsConverted()
    {
        var lingo = "if 1 then\nput 2 into x\nend if";
        var result = LingoToCSharpConverter.Convert(lingo);
        var expected = string.Join('\n',
            "if (1)",
            "{",
            "x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void RepeatWhileStatementIsConverted()
    {
        var lingo = "repeat while 1\nput 2 into x\nend repeat";
        var result = LingoToCSharpConverter.Convert(lingo);
        var expected = string.Join('\n',
            "while (1)",
            "{",
            "x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void ExitRepeatIfStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("exit repeat if 1");
        Assert.Equal("if (1) break;", result.Trim());
    }

    [Fact]
    public void NextRepeatStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("next repeat");
        Assert.Equal("continue;", result.Trim());
    }

    [Fact]
    public void SendSpriteStatementIsConverted()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "B1",
                Source = "on beginSprite\r\n sendSprite 2, #doIt\r\nend\r\n",
                Type = LingoScriptType.Behavior
            },
            new LingoScriptFile
            {
                Name = "B2",
                Source = "on doIt\r\n end\r\n",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = LingoToCSharpConverter.Convert(scripts);
        Assert.Equal("SendSprite<B2>(2, b2 => b2.doIt());", batch.ConvertedScripts["B1"].Trim());
    }

    [Fact]
    public void UnknownMethodCallsMovieScript()
    {
        var scripts = new[]
        {
            new LingoScriptFile
            {
                Name = "M1",
                Source = "on myMovieHandler\r\n end\r\n",
                Type = LingoScriptType.Movie
            },
            new LingoScriptFile
            {
                Name = "P1",
                Source = "on beginSprite\r\n myMovieHandler\r\nend\r\n",
                Type = LingoScriptType.Behavior
            }
        };
        var batch = LingoToCSharpConverter.Convert(scripts);
        Assert.Equal("CallMovieScript<M1>(m1 => m1.myMovieHandler());", batch.ConvertedScripts["P1"].Trim());
    }

    [Fact]
    public void MemberTextAccessIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("member(\"T_Text\").text");
        Assert.Equal("Member<LingoMemberText>(\"T_Text\").Text", result.Trim());
    }

    [Fact]
    public void NewMemberCallIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("_movie.newMember(#bitmap)");
        Assert.Equal("_movie.New.Picture()", result.Trim());
    }

    [Fact]
    public void NewMemberAssignmentIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("img = _movie.newMember(#bitmap)");
        Assert.Equal("img = _movie.New.Picture();", result.Trim());
    }

    [Fact(Skip = "Converter does not yet fully match the reference implementation")]
    public void DemoNewGameScriptMatchesConvertedOutput()
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", ".."));
        string ls = File.ReadAllText(Path.Combine(root, "Demo", "TetriGrounds",
            "TetriGrounds.Lingo.Original", "13_B_NewGame.ls"));
        string expected = File.ReadAllText(Path.Combine(root, "Demo",
            "TetriGrounds", "LingoEngine.Demo.TetriGrounds.Core", "Sprites",
            "Behaviors", "NewGameBehavior.cs"));

        string converted = LingoToCSharpConverter.Convert(ls);

        static string Normalize(string s) => string.Join('\n', s.Split('\n', '\r').Select(l => l.Trim()).Where(l => l.Length > 0));

        Assert.Equal(Normalize(expected), Normalize(converted));
    }

    [Fact]
    public void CaseStatementIsConverted()
    {
        var label1 = new LingoCaseLabelNode
        {
            Value = new LingoDatumNode(new LingoDatum(1)),
            Block = new LingoBlockNode { Children = { new LingoCallNode { Name = "DoOne" } } }
        };
        var label2 = new LingoCaseLabelNode
        {
            Value = new LingoDatumNode(new LingoDatum(2)),
            Block = new LingoBlockNode { Children = { new LingoCallNode { Name = "DoTwo" } } }
        };
        label1.NextLabel = label2;
        var caseNode = new LingoCaseStmtNode
        {
            Value = new LingoDatumNode(new LingoDatum(1)),
            FirstLabel = label1,
            Otherwise = new LingoOtherwiseNode
            {
                Block = new LingoBlockNode { Children = { new LingoCallNode { Name = "DoDefault" } } }
            }
        };
        var result = CSharpWriter.Write(caseNode).Trim();
        var expected = string.Join('\n',
            "switch (1)",
            "{",
            "case 1:",
            "DoOne();",
            "case 2:",
            "DoTwo();",
            "default:",
            "DoDefault();",
            "}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TheExpressionIsConverted()
    {
        var node = new LingoTheExprNode { Prop = "mouseH" };
        Assert.Equal("_Mouse.MouseH", CSharpWriter.Write(node).Trim());
        node.Prop = "actorList";
        Assert.Equal("_Movie.ActorList", CSharpWriter.Write(node).Trim());
        node.Prop = "banana";
        Assert.Equal("/* the banana */", CSharpWriter.Write(node).Trim());
    }

    [Fact]
    public void ObjectPropertyExpressionIsConverted()
    {
        var node = new LingoObjPropExprNode
        {
            Object = new LingoVarNode { VarName = "foo" },
            Property = new LingoVarNode { VarName = "bar" }
        };
        Assert.Equal("foo.bar", CSharpWriter.Write(node).Trim());
    }

    [Fact]
    public void PlayAndSoundNodesAreConverted()
    {
        var play = new LingoPlayCmdStmtNode { Command = new LingoVarNode { VarName = "demo" } };
        Assert.Equal("Play(demo);", CSharpWriter.Write(play).Trim());
        var soundProp = new LingoSoundPropExprNode
        {
            Sound = new LingoVarNode { VarName = "s" },
            Property = new LingoVarNode { VarName = "p" }
        };
        Assert.Equal("Sound(s).p", CSharpWriter.Write(soundProp).Trim());
        var menuProp = new LingoMenuPropExprNode
        {
            Menu = new LingoVarNode { VarName = "m" },
            Property = new LingoVarNode { VarName = "p" }
        };
        Assert.Equal("MenuProp(m, p)", CSharpWriter.Write(menuProp).Trim());
        var del = new LingoChunkDeleteStmtNode { Chunk = new LingoVarNode { VarName = "c" } };
        Assert.Equal("DeleteChunk(c);", CSharpWriter.Write(del).Trim());
    }

    [Fact]
    public void DeclarationStatementsAreConverted()
    {
        var global = new LingoGlobalDeclStmtNode();
        global.Names.AddRange(new[] { "g1", "g2" });
        Assert.Equal("var g1, g2;", CSharpWriter.Write(global).Trim());

        var prop = new LingoPropertyDeclStmtNode();
        prop.Names.AddRange(new[] { "p1", "p2" });
        Assert.Equal("var p1, p2;", CSharpWriter.Write(prop).Trim());

        var inst = new LingoInstanceDeclStmtNode();
        inst.Names.AddRange(new[] { "i1" });
        Assert.Equal("var i1;", CSharpWriter.Write(inst).Trim());
    }

    [Fact]
    public void RepeatStatementsAreConverted()
    {
        var withIn = new LingoRepeatWithInStmtNode
        {
            Variable = "v",
            List = new LingoVarNode { VarName = "lst" },
            Body = new LingoBlockNode { Children = { new LingoCallNode { Name = "Do" } } }
        };
        var expectedIn = string.Join('\n',
            "foreach (var v in lst)",
            "{",
            "Do();",
            "}");
        Assert.Equal(expectedIn, CSharpWriter.Write(withIn).Trim());

        var withTo = new LingoRepeatWithToStmtNode
        {
            Variable = "i",
            Start = new LingoDatumNode(new LingoDatum(1)),
            End = new LingoDatumNode(new LingoDatum(3)),
            Body = new LingoBlockNode { Children = { new LingoCallNode { Name = "Step" } } }
        };
        var expectedTo = string.Join('\n',
            "for (var i = 1; i <= 3; i++)",
            "{",
            "Step();",
            "}");
        Assert.Equal(expectedTo, CSharpWriter.Write(withTo).Trim());

        var until = new LingoRepeatUntilStmtNode(
            new LingoVarNode { VarName = "done" },
            new LingoBlockNode { Children = { new LingoCallNode { Name = "Work" } } });
        var expectedUntil = string.Join('\n',
            "do",
            "{",
            "Work();",
            "} while (!(done));");
        Assert.Equal(expectedUntil, CSharpWriter.Write(until).Trim());

        var forever = new LingoRepeatForeverStmtNode(
            new LingoBlockNode { Children = { new LingoCallNode { Name = "Loop" } } });
        var expectedForever = string.Join('\n',
            "while (true)",
            "{",
            "Loop();",
            "}");
        Assert.Equal(expectedForever, CSharpWriter.Write(forever).Trim());

        var times = new LingoRepeatTimesStmtNode(
            new LingoDatumNode(new LingoDatum(2)),
            new LingoBlockNode { Children = { new LingoCallNode { Name = "T" } } });
        var expectedTimes = string.Join('\n',
            "for (int i = 1; i <= 2; i++)",
            "{",
            "T();",
            "}");
        Assert.Equal(expectedTimes, CSharpWriter.Write(times).Trim());
    }

    [Fact]
    public void ExpressionNodesAreConverted()
    {
        var within = new LingoSpriteWithinExprNode
        {
            SpriteA = new LingoVarNode { VarName = "a" },
            SpriteB = new LingoVarNode { VarName = "b" }
        };
        Assert.Equal("SpriteWithin(a, b)", CSharpWriter.Write(within).Trim());

        var last = new LingoLastStringChunkExprNode { Source = new LingoVarNode { VarName = "txt" } };
        Assert.Equal("LastChunkOf(txt)", CSharpWriter.Write(last).Trim());

        var inter = new LingoSpriteIntersectsExprNode
        {
            SpriteA = new LingoVarNode { VarName = "x" },
            SpriteB = new LingoVarNode { VarName = "y" }
        };
        Assert.Equal("SpriteIntersects(x, y)", CSharpWriter.Write(inter).Trim());

        var cnt = new LingoStringChunkCountExprNode { Source = new LingoVarNode { VarName = "str" } };
        Assert.Equal("ChunkCount(str)", CSharpWriter.Write(cnt).Trim());

        var notOp = new LingoNotOpNode { Expr = new LingoVarNode { VarName = "ok" } };
        Assert.Equal("!(ok)", CSharpWriter.Write(notOp).Trim());
    }

    [Fact]
    public void BinaryOperationIsConverted()
    {
        var inner = new LingoBinaryOpNode
        {
            Left = new LingoDatumNode(new LingoDatum(2)),
            Right = new LingoDatumNode(new LingoDatum(3)),
            Opcode = LingoBinaryOpcode.Multiply
        };
        var outer = new LingoBinaryOpNode
        {
            Left = new LingoDatumNode(new LingoDatum(1)),
            Right = inner,
            Opcode = LingoBinaryOpcode.Add
        };
        Assert.Equal("1 + (2 * 3)", CSharpWriter.Write(outer).Trim());
    }

    [Fact]
    public void ReturnAndExitStatementsAreConverted()
    {
        var retVal = new LingoReturnStmtNode(new LingoDatumNode(new LingoDatum(5)));
        Assert.Equal("return 5;", CSharpWriter.Write(retVal).Trim());
        var ret = new LingoReturnStmtNode(null);
        Assert.Equal("return;", CSharpWriter.Write(ret).Trim());
        Assert.Equal("return;", CSharpWriter.Write(new LingoExitStmtNode()).Trim());
    }

    [Fact]
    public void LoopControlStatementsAreConverted()
    {
        Assert.Equal("break;", CSharpWriter.Write(new LingoExitRepeatStmtNode()).Trim());
        var nextIf = new LingoNextRepeatIfStmtNode(new LingoDatumNode(new LingoDatum(1)));
        Assert.Equal("if (1) continue;", CSharpWriter.Write(nextIf).Trim());
    }

    [Fact]
    public void PropertyAndMiscNodesAreConverted()
    {
        var bracket = new LingoObjBracketExprNode
        {
            Object = new LingoVarNode { VarName = "arr" },
            Index = new LingoDatumNode(new LingoDatum(3))
        };
        Assert.Equal("arr[3]", CSharpWriter.Write(bracket).Trim());

        var propIndex = new LingoObjPropIndexExprNode
        {
            Object = new LingoVarNode { VarName = "o" },
            PropertyIndex = new LingoDatumNode(new LingoDatum(1))
        };
        Assert.Equal("o.prop[1]", CSharpWriter.Write(propIndex).Trim());

        var spriteProp = new LingoSpritePropExprNode
        {
            Sprite = new LingoDatumNode(new LingoDatum(2)),
            Property = new LingoVarNode { VarName = "locH" }
        };
        Assert.Equal("Sprite(2).locH", CSharpWriter.Write(spriteProp).Trim());

        var menuItemProp = new LingoMenuItemPropExprNode
        {
            MenuItem = new LingoVarNode { VarName = "file" },
            Property = new LingoVarNode { VarName = "enabled" }
        };
        Assert.Equal("menuItem(file).enabled", CSharpWriter.Write(menuItemProp).Trim());

        var theProp = new LingoThePropExprNode { Property = new LingoVarNode { VarName = "version" } };
        Assert.Equal("TheProp(version)", CSharpWriter.Write(theProp).Trim());

        var memberExpr = new LingoMemberExprNode { Expr = new LingoVarNode { VarName = "foo" } };
        Assert.Equal("Member(foo)", CSharpWriter.Write(memberExpr).Trim());

        var soundCmd = new LingoSoundCmdStmtNode { Command = new LingoVarNode { VarName = "play" } };
        Assert.Equal("Sound(play);", CSharpWriter.Write(soundCmd).Trim());

        var hilite = new LingoChunkHiliteStmtNode { Chunk = new LingoVarNode { VarName = "c" } };
        Assert.Equal("Hilite(c);", CSharpWriter.Write(hilite).Trim());
    }
}
