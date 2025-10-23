using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Tools;
using System.Text;

namespace BlingoEngine.IO.Legacy.Cast.MemberTypes
{
    internal class BlCastMemberScriptReader
    {
        internal BlCastMemberItem Read(byte[] specificData, List<byte[]> blobs, List<int> prefixValues)
        {
            var member = new BlCastMemberScript();
            var scriptType = specificData.ReadInt16(0);
            switch (scriptType)
            {
                case 1: member.ScriptType = BlCastMemberScript.BlScriptType.Behavior; break;
                case 3: member.ScriptType = BlCastMemberScript.BlScriptType.MovieScript; break;
                case 7: member.ScriptType = BlCastMemberScript.BlScriptType.ParentScript; break;
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
                if (blobs[1].Length > 0) 
                    member.LinkName = blobs[1].ReadCString(0); 
            }
            return member;
        }
    }
}
