using System;
using System.Linq;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public sealed class XmedTokenGrouperTests
{
    [Fact]
    public void Grouper_should_expand_padding_and_repeat_tokens_before_grouping()
    {
        var block = GetMainBlock("0006");

        Assert.Equal(block.DeclaredTokenCount, block.RawTokens.Count);
        Assert.All(block.RawTokens, t => Assert.NotEqual(BlXmedToken.TokenType.C1, t.Type));
        Assert.All(block.RawTokens, t => Assert.NotEqual(BlXmedToken.TokenType.B_81, t.Type));
    }

    [Fact]
    public void Run_style_block_should_pair_tokens_and_preserve_tail()
    {
        var block = GetMainBlock("0004");

        Assert.Single(block.PreTokens);
        Assert.Equal(BlXmedToken.TokenType.PrefixedHex, block.PreTokens[0].Type);

        var entries = block.Items.OfType<XmedTokenGroup>().ToList();
        Assert.NotEmpty(entries);
        Assert.All(entries, entry => Assert.Equal(2, entry.Items.OfType<BlXmedToken>().Count()));

        Assert.Single(block.PostTokens);
        Assert.Equal(BlXmedToken.TokenType.PrefixedHex, block.PostTokens[0].Type);
    }

    [Fact]
    public void Run_paragraph_block_should_pair_tokens_and_preserve_tail()
    {
        var block = GetMainBlock("0005");

        Assert.Single(block.PreTokens);
        var entries = block.Items.OfType<XmedTokenGroup>().ToList();
        Assert.NotEmpty(entries);
        Assert.All(entries, entry => Assert.Equal(2, entry.Items.OfType<BlXmedToken>().Count()));
        Assert.Single(block.PostTokens);
    }

    [Fact]
    public void Style_block_should_split_on_82_and_promote_c2_groups()
    {
        var block = GetMainBlock("0006");

        Assert.NotEmpty(block.PreTokens);
        var pre = block.PreTokens[0];
        Assert.Equal(BlXmedToken.TokenType.PrefixedHex, pre.Type);
        Assert.Equal(0x01, pre.TypeValue);

        Assert.Equal(block.DeclaredItemCount, block.Items.Count);

        var entries = block.Items.OfType<XmedTokenGroup>().ToList();
        Assert.Contains(entries, entry => entry.Items.OfType<XmedTokenGroup>().Any(g => g.Type == BlXmedToken.TokenType.C2));

        foreach (var entry in entries)
            Assert.Single(entry.PreTokens);
    }

    [Fact]
    public void Paragraph_block_should_split_declared_structs()
    {
        var block = GetMainBlock("0007");

        Assert.Equal(block.DeclaredItemCount, block.Items.Count);

        foreach (var entry in block.Items.OfType<XmedTokenGroup>())
        {
            Assert.NotEmpty(entry.PreTokens);
            Assert.DoesNotContain(entry.EnumerateTokens(), token => token.Type == BlXmedToken.TokenType.B_82);
            Assert.Contains(entry.EnumerateC2Groups(), group => group.TypeValue.HasValue);
        }
    }

    [Fact]
    public void Font_block_should_materialize_named_entries()
    {
        var block = GetMainBlock("0008");

        Assert.Equal(block.DeclaredItemCount, block.Items.Count);

        foreach (var entry in block.Items.OfType<XmedTokenGroup>())
        {
            Assert.NotEmpty(entry.PreTokens);
            Assert.All(entry.PreTokens, token => Assert.Equal(BlXmedToken.TokenType.Block00, token.Type));
            Assert.DoesNotContain(entry.EnumerateTokens(), token => token.Type == BlXmedToken.TokenType.B_82);
        }

        var terminal = block.Items.OfType<XmedTokenGroup>()
            .FirstOrDefault(entry => entry.PreTokens.Any(token => string.Equals(token.Ascii, "Terminal", StringComparison.OrdinalIgnoreCase)));

        Assert.NotNull(terminal);
        Assert.Contains(terminal!.EnumerateC2Groups(), group => group.TypeValue == 0x03);
    }

    private static XmedMainTokenGroup GetMainBlock(string id)
    {
        var path = TestContextHarness.GetAssetPath("Texts_Fields/Text_Multi_Line_Multi_Style_13.xmed.bin");
        var bytes = System.IO.File.ReadAllBytes(path);
        var tokens = BlXmedTokenizer.Tokenize(bytes).Tokens;
        var groups = new XmedTokenGrouper().CreateGroups(tokens);
        var mainGroups = groups.OfType<XmedMainTokenGroup>().ToList();
        Assert.NotEmpty(mainGroups);

        var match = mainGroups.FirstOrDefault(g => g.BlockId == id);
        Assert.True(match != null, $"Block {id} not found. Known blocks: {string.Join(",", mainGroups.Select(g => g.BlockId))}");
        return match!;
    }
}
