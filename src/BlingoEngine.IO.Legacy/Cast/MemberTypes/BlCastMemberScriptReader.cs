using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.Text;
using static BlingoEngine.IO.Data.DTO.Members.BlingoMemberScriptDTO;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberScriptReader
    {
        internal BlCastRawMemberItem Read(byte[] specificData, List<byte[]> blobs, List<int> prefixValues)
        {
            var member = new BlCastRawMemberScript();
            var scriptType = specificData.ReadInt16(0);
            switch (scriptType)
            {
                case 1: member.ScriptType = BlScriptType.Behavior; break;
                case 3: member.ScriptType = BlScriptType.MovieScript; break;
                case 7: member.ScriptType = BlScriptType.ParentScript; break;
                default:
                    break;
            }
            var scriptText = Encoding.ASCII.GetString(blobs[0]);
            member.Script = scriptText;
            // javascripts sets a 2 in the prefixValues
            if (prefixValues[4] == 2)
                member.IsJavascript = true;
            if (blobs.Count > 3)
            {
                if (blobs[1].Length > 0) member.LinkedFolder = blobs[1].Skip(1).ToArray().ReadCString(0);
                if (blobs[2].Length > 1) member.LinkedFileName = blobs[2].Skip(1).ToArray().ReadCString(0);
            }
            return member;
        }
    }
}
