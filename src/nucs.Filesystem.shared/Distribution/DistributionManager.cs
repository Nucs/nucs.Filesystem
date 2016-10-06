using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nucs.SystemCore.Boolean;

namespace nucs.Filesystem.Distribution {
    public static class DistributionManager {

        #region Cache
        static DistributionManager() {
            methods_cache = new List<Type>();
            var baseType = typeof(DistributerBase);
            var assembly = baseType.Assembly;
            methods_cache.AddRange(assembly.GetTypes()
                .Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract));
        }


        private static readonly List<Type> methods_cache;
        #endregion

        /// <summary>
        ///     Gets an instance from all of the distributers that are available.
        /// </summary>
        public static List<DistributerBase> GetDistributers(bool onlystaticdirs) {
            var a = methods_cache.Select(Activator.CreateInstance).Cast<DistributerBase>();
            if (onlystaticdirs) a = a.Where(db => db.IsStatic);
            return a.ToList();
        }

/*        /// <summary>
        ///     Gets the path to the best of choice path.
        /// </summary>
        /// <returns></returns>
        public static FileInfo DistributableBestFile() {
            Paths.RandomLocation
            return file;
        }*/

        /// <summary>
        ///     Gets the path to the best of choice path.
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo DistributableBestDir() {
            //var dd = new WindowsDistributer().Distributables();
            var dir = new DirectoryInfo(Path.Combine(Paths.WindowsDirectory.FullName, "he-IL\\"));
            dir.EnsureDirectoryExists();
            
            return dir;
        }
    }
}