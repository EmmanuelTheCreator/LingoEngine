using Director.IO;
using System.IO;
using Director.Primitives;
using Director.ScummVM;
using Director.Tools;

namespace Director
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // var filePath = args.Length > 0 ? args[0] : "ScoresExt.cst";
            var filePath = args.Length > 0 ? args[0] : "arkanoi3.cxt";
            //var filePath = args.Length > 0 ? args[0] : "AutoDismisser.dir";
            //var filePath = args.Length > 0 ? args[0] : "pinballV2_21.dir";

            //if (args.Length >= 2 && args[0] == "export-cast")
            //{
                var output = args.Length >= 3 ? args[2] : "Export";
                CastExporter.Export(filePath, output);
                Console.WriteLine($"Cast exported to '{output}'.");
                return;
            //}

            
            ConfMan.SetBool("dump_scripts", true);
            LogHelper.SetLevel(-1); // or a specific level like 3 or 5
            LogHelper.Enable(DebugChannel.Loading);
            LogHelper.Enable(DebugChannel.Compile);
            LogHelper.Enable(DebugChannel.ImGui);
            LogHelper.Enable(DebugChannel.NoBytecode);

            var reader = new ArchiveFileLoader();
            var fileData = reader.ReadFile(filePath);
            if (IsMovieArchive(fileData.Archive))
            {
                //var movie = new Movie(archive);
                //movie.Load();
            }
            else
            {
                var cast = new Cast(fileData.Archive, castLibID: 0);
                cast.SetArchive(fileData.Archive);
                cast.LoadArchive();
            }

            Console.WriteLine("Done.");
        }
        private static bool IsMovieArchive(Archive archive)
        {
            // Placeholder heuristic:
            // e.g. presence of specific tags, or a 'MV93' block inside, or Score/Cast resources
            return archive.HasResource(ResourceTags.MV93, 1)
                || archive.ListTags().Contains(ResourceTags.Lctx)  // 'Lctx' compiled script context
                || archive.ListTags().Contains(ResourceTags.FCRD); // Frame/score data
        }
    }
}
