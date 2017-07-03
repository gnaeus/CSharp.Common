using System;
using System.IO;
using System.Linq;

namespace Common.Helpers
{
    public static class FileSystemHelper
    {
        /// <summary>
        /// Reqursively delete all files and folders from directory.
        /// </summary>
        public static void CleanDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles()) {
                file.Delete();
            }
            foreach (DirectoryInfo subDirectory in di.GetDirectories()) {
                subDirectory.Delete(true);
            }
        }

        /// <summary>
        /// Cleanup <paramref name="fileName"/> from invalid characters.
        /// </summary>
        public static string RemoveInvalidCharsFromFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();

            return new String(fileName
                .Where(x => !invalidChars.Contains(x))
                .ToArray());
        }
    }
}
