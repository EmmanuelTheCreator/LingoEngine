using AbstUI.Inputs;
using AbstUI.SDL2.Core;

namespace AbstUI.SDL2
{
    public interface IAbstSDLRootContext
    {
        IAbstGlobalMouse GlobalMouse { get; set; }
        IAbstGlobalKey GlobalKey { get; set; }
        AbstSDLComponentContainer ComponentContainer { get; }
    }
}
