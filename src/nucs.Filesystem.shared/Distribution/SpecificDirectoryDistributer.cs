using System.Collections.Generic;
using System.IO;

namespace nucs.Filesystem.Distribution {
    public abstract class SpecificDirectoryDistributer : DistributerBase {

        /// <summary>
        ///     Enumerate the base directories inwhich subdirs will be searched.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<DirectoryInfo> FetchBaseDirs();
        
        protected override List<FileInfo> FindDistributed() {
            /*var dirs = FetchBaseDirs();
            return dirs.SelectMany(dir => dir.GetFiles()).Where(TrojanFile.IsCopy).ToList();*/
            return null;
        }

        public override IEnumerable<DirectoryInfo> Distributables() {
            return FetchBaseDirs();
        }
    }
}