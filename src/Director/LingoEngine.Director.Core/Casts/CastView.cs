using LingoEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingoEngine.Director.Core.Casts
{
    public class CastView
    {
        public LingoCast Cast { get; }
        public CastView(LingoCast cast)
        {
            Cast = cast;
        }

        public void Show()
        {

        }

    }
}
