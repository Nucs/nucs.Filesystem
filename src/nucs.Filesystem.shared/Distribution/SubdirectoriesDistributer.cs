using System.Collections.Generic;
using System.IO;

namespace nucs.Filesystem.Distribution {
    public abstract class SubdirectoriesDistributer : DistributerBase {

        /// <summary>
        ///     Enumerate the base directories inwhich subdirs will be searched.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<DirectoryInfo> FetchBaseDirs();

        /// <summary>
        ///     How deep to return subdirs; depth = 0-base, 1-base+subs, 2-base+subs+subs
        /// </summary>
        protected abstract uint SearchDepth();

        /// <summary>
        ///     Recusivly digs up the directories.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="depth">depth = 0-base, 1-base+subs, 2-base+subs+subs</param>
        /// <returns></returns>
        public static IEnumerable<DirectoryInfo> DigDirectories(DirectoryInfo root, int depth = -1) {
            yield return root;
            if (depth <= 0) yield break;
            foreach (var dir in root.GetDirectories())
                foreach (var subdir in DigDirectories(dir, depth - 1))
                    yield return subdir;
        }


        protected override List<FileInfo> FindDistributed() {
            return null;
        }

        public override IEnumerable<DirectoryInfo> Distributables() {
            var depth = SearchDepth();
            if (depth < 1) depth = 1;
            var dirs = new List<DirectoryInfo>();

            foreach (var @base in FetchBaseDirs())
                dirs.AddRange(DigDirectories(@base, (int)depth));
            
            foreach (var dir in dirs)
                yield return dir;
           
        }
    }
}