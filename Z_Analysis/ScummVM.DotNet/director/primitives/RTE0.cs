using Director.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Director.Primitives
{
    public class RTE0
    {
        public short Top { get; set; }
        public short Left { get; set; }
        public short Bottom { get; set; }
        public short Right { get; set; }
        public short RegX { get; set; }
        public short RegY { get; set; }

        public RTE0() { }

        public RTE0(SeekableReadStreamEndian reader)
        {
            Top = reader.ReadInt16BE();
            Left = reader.ReadInt16BE();
            Bottom = reader.ReadInt16BE();
            Right = reader.ReadInt16BE();
            RegX = reader.ReadInt16BE();
            RegY = reader.ReadInt16BE();
        }
    }
}
