using System.Collections.Generic;
using System.IO;

namespace nucs.Filesystem.Distribution.Methods {
    public class UniquePathDistributer : SpecificDirectoryDistributer {
        protected override IEnumerable<DirectoryInfo> FetchBaseDirs() {
            yield return Paths.UniqueWritableLocation;
        }

        public override bool IsStatic => true;
    }
}