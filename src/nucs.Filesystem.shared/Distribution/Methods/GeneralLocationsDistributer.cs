using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nucs.Filesystem.Distribution.Methods {
    public class GeneralLocationsDistributer : SpecificDirectoryDistributer {
        public override bool IsStatic { get { return true; } }
        protected override IEnumerable<DirectoryInfo> FetchBaseDirs() {
            return System.Enum.GetValues(typeof (Environment.SpecialFolder))
                .Cast<Environment.SpecialFolder>()
                .Where(en => !(en == Environment.SpecialFolder.StartMenu || en == Environment.SpecialFolder.Startup))
                .Select(Environment.GetFolderPath)
                .Where(p => string.IsNullOrEmpty(p) == false)
                .Distinct()
                .Select(p => new DirectoryInfo(p));
        }
    }
}