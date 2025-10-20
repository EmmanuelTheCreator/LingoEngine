using BlingoEngine.IO.Legacy.Texts.Data;

using System.Collections.Generic;
using System.Linq;

namespace BlingoEngine.IO.Legacy.Texts
{
    internal sealed class XmedTokenGroupIndex
    {
        private readonly Dictionary<string, List<XmedTokenGroup>> _groupsById = new(StringComparer.OrdinalIgnoreCase);

        public XmedTokenGroupIndex(IEnumerable<XmedTokenGroup> groups)
        {
            foreach (var group in groups)
            {
                var id = GetBlockId(group);
                if (string.IsNullOrEmpty(id))
                    continue;

                if (!_groupsById.TryGetValue(id, out var list))
                {
                    list = new List<XmedTokenGroup>();
                    _groupsById[id] = list;
                }

                list.Add(group);
            }
        }

        public XmedTokenGroup? FindFirst(string id)
        {
            if (_groupsById.TryGetValue(id, out var list) && list.Count > 0)
                return list[0];
            return null;
        }

        public IEnumerable<XmedTokenGroup> FindAll(string id)
        {
            if (_groupsById.TryGetValue(id, out var list))
                return list;
            return Enumerable.Empty<XmedTokenGroup>();
        }

        public static string? GetBlockId(XmedTokenGroup group)
        {
            if (group.Type == BlXmedToken.TokenType.PrefixedHex &&
                group.TypeValue == 0x03 &&
                !string.IsNullOrEmpty(group.Ascii) &&
                group.Ascii.Length >= 4)
                return group.Ascii[..4];

            return null;
        }
    }
}
