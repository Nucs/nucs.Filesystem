using System.Collections.Generic;
using System.IO;

namespace nucs.Filesystem.Distribution.Methods {
    public class WindowsDistributer : SubdirectoriesDistributer {

        private static DirectoryInfo IdentifyOrNullify(string path) {
            try {
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
            } catch {
               return null;
            }
            return new DirectoryInfo(path);
        }

        protected override IEnumerable<DirectoryInfo> FetchBaseDirs() {
            var windir = Paths.WindowsDirectory;
            yield return windir;
            yield return IdentifyOrNullify(Path.Combine(windir.FullName, "/system/"));
            yield return IdentifyOrNullify(Path.Combine(windir.FullName, "/System32/"));
            yield return IdentifyOrNullify(Path.Combine(windir.FullName, "/SysWOW64/"));
        }

        protected override uint SearchDepth() {
            return 1;
        }


        public override bool IsStatic => true;
    }
}