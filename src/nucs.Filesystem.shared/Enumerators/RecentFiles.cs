using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nucs.Filesystem.Enumerators {
    /// <summary>
    ///     Enumerate through all recent files.
    /// </summary>
    public class RecentFiles : IEnumerable<FileInfo> {
        public IEnumerator<FileInfo> GetEnumerator() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
            var files = Directory.GetFiles(path).OrderByDescending(f => new FileInfo(f).LastAccessTimeUtc);
            IWshRuntimeLibrary.IWshShell wsh = new IWshRuntimeLibrary.WshShell();
            foreach (var file in files) {
                if (file.EndsWith(".lnk")) { 
                    IWshRuntimeLibrary.IWshShortcut sc = (IWshRuntimeLibrary.IWshShortcut)wsh.CreateShortcut(file);
                    var tp = sc.TargetPath;
                    if (!File.Exists(tp))
                        continue;
                    yield return new FileInfo(tp);
                } else
                    yield return new FileInfo(file);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}