using Director.Tools;

namespace Director.IO
{
    internal class EmbeddedArchiveLoader
    {
        /// <summary>
        /// Loads a Director archive embedded inside a parent MV93 block.
        /// </summary>
        public static void Load(byte[] embeddedData, string sourceName = "embedded")
        {
            using var ms = new MemoryStream(embeddedData, writable: false);
            using var reader = new BinaryReader(ms);

            // Wrap into Archive
            var archive = new Archive(isBigEndian: true);
            var fileReader = new ArchiveFileLoader();
            fileReader.LoadInto(archive, ms, reader, sourceName);

            // Load the archive as a Cast
            var cast = new Cast(archive, castLibID: 0);
            cast.SetArchive(archive);
            cast.LoadArchive();

            LogHelper.DebugLog(1, DebugChannel.Loading, $"Finished loading embedded archive from {sourceName}.");
        }
    }
    }
