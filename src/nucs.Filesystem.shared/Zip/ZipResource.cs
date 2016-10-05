using System;
using System.IO;

namespace nucs.Filesystem.Zip {
    public class ZipResource {
        /// <summary>
        ///     Exports the zip stored file to wanted dir.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="resourcename"></param>
        public static void Export(DirectoryInfo dir, string resourcename) {
            if (dir==null) throw new ArgumentNullException(nameof(dir));
            if (dir.Exists==false) dir.Create();
            Export(dir.FullName, resourcename);
        }

        /// <summary>
        ///     Exports the zip stored file to wanted dir.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="resourcename"></param>
        public static void Export(string dir, string resourcename) {
            var rs = ResourceHelper.GetResource(resourcename);
            var zip = ZipStorer.Open(rs, FileAccess.Write);
            foreach (var entry in zip.ReadCentralDir()) {
                zip.ExtractFile(entry, Path.Combine(dir, entry.FilenameInZip));
            }
            rs.Close();
        }

        /// <summary>
        ///     Exports the zip stored file to wanted dir.
        /// </summary>
        public static void Export(DirectoryInfo dir, string resourcename, string rijpass) {
            if (dir==null) throw new ArgumentNullException(nameof(dir));
            if (dir.Exists==false) dir.Create();
            Export(dir.FullName, resourcename);
        }

/*
        /// <summary>
        ///     Exports the zip stored file to wanted dir.
        /// </summary>
        public static void Export(string dir, string resourcename, string rijpass) {
            var rs = ResourceHelper.GetResource(resourcename);
            var sr = new StreamReader(rs);
            var ms = new MemoryStream(new RijndaelEnhanced(rijpass).DecryptToBytes(sr.ReadToEnd()));
            sr.Close();
            rs.Close();
            var zip = ZipStorer.Open(ms, FileAccess.Write);
            foreach (var entry in zip.ReadCentralDir()) {
                zip.ExtractFile(entry, Path.Combine(dir, entry.FilenameInZip));
            }
            ms.Close();
        }
*/
    }
}