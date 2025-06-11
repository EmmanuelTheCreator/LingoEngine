using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Director.Scripts
{
    /// <summary>
    /// Represents a single entry in the Lingo script context table.
    /// </summary>
    public class LingoContextEntry
    {
        public int Index { get; set; }
        public int NextUnused { get; set; }
        public bool Unused { get; set; }

        public LingoContextEntry(int index, int nextUnused)
        {
            Index = index;
            NextUnused = nextUnused;
            Unused = false;
        }
    }

}
