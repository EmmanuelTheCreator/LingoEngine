using System.Collections.Generic;
using System.IO;
using System.Text;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Director;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Director;

public class BlLegacyMovieBlingoExtensionsShould
{
    [Fact]
    public void CreateScriptResourceForScriptMembers()
    {
        var baseDto = new BlingoMemberDTO
        {
            Name = "Test Script",
            CastLibNum = 1,
            NumberInCast = 2,
            Type = BlingoMemberTypeDTO.Script,
            RegPoint = new BlingoPointDTO()
        };
        var castDto = new BlingoCastDTO { Name = "Cast 1", Number = 1 };
        var resources = new DirFilesContainerDTO();
        var usedNames = new HashSet<string>();
        var memberScript = new BlCastMemberScript
        {
            Script = "on beginSprite\n  put 42\nend",
            LinkedFileName = "inv:alid.lingo",
            IsJavascript = false
        };

        var result = baseDto.ToScriptMember(
            archive: null!,
            castResourceId: 0,
            castDto,
            resources,
            usedNames,
            NullLogger.Instance,
            memberScript);

        Assert.Single(resources.Files);
        var resource = resources.Files[0];
        Assert.Equal(DirFileResourceKind.Script, resource.Kind);
        Assert.Equal(baseDto.CastLibNum, resource.CastLibNum);
        Assert.Equal(baseDto.NumberInCast, resource.NumberInCast);
        Assert.EndsWith(".lingo", resource.FileName);
        Assert.Equal(memberScript.Script, Encoding.UTF8.GetString(resource.Bytes));
        var scriptDto = Assert.IsType<BlingoMemberScriptDTO>(result);
        Assert.Equal(resource.FileName, scriptDto.LinkedFilePath);
        Assert.Contains(resource.FileName, usedNames);
        Assert.Equal("Test Script", scriptDto.Name);
    }
}
