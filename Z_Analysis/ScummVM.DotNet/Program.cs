using Director.IO;
using Director.ScummVM;
using Director.Tools;

namespace Director
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //var filePath = args.Length > 0 ? args[0] : "ScoresExt.cst";
            var filePath = args.Length > 0 ? args[0] : "arkanoi3.cxt";
            //var filePath = args.Length > 0 ? args[0] : "AutoDismisser.dir";
            //var filePath = args.Length > 0 ? args[0] : "pinballV2_21.dir";
            ConfMan.SetBool("dump_scripts", true);
            LogHelper.SetLevel(-1); // or a specific level like 3 or 5
            LogHelper.Enable(DebugChannel.Loading);
            LogHelper.Enable(DebugChannel.Compile);
            LogHelper.Enable(DebugChannel.ImGui);
            LogHelper.Enable(DebugChannel.NoBytecode);

            var reader = new ArchiveFileLoader();
            reader.ReadFile(filePath);

            Console.WriteLine("Done.");
        }
    }
}
