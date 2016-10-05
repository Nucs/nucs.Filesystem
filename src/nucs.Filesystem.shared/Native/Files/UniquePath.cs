using System;
using System.Collections.Generic;
using System.IO;
using nucs.Startup.Internal;
using nucs.SystemCore.Boolean;

namespace nucs.Filesystem {
    /// <summary>
    ///     This class generates a unique directory/file per PC (hardwares)
    /// </summary>
    public static class UniquePath {
        public static readonly List<DirectoryInfo> PotentialPaths = new List<DirectoryInfo> {
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)),
            new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)),
        };

        public static DirectoryInfo GetUniqueDirectory {
            get {
                var rand = UniquePcId.NewUniqueRandomizer;
                _retry:
                var random_dir = PotentialPaths[rand.Next(PotentialPaths.Count)];
                if (random_dir.IsDirectoryWritable() == false)
                    goto _retry;
                return random_dir;
            }
        }

        public static string GetUniqueFileName {
            get {
                var rand = UniquePcId.NewUniqueRandomizer;
                return StringGenerator.Generate(rand, 10);
            }
        }

        /// <summary>
        ///     Is GetUniqueFileName is the exe that is running
        /// </summary>
        public static bool IsExecutingTheUnique => new FileInfo(GetUniqueFileName).FullName.Equals(Paths.ExecutingExe.FullName);

        /// <summary>
        ///     Is The running exe inside unique dir.
        /// </summary>
        public static bool IsExecutingInUniqueDir => GetUniqueDirectory.FullName.Equals(Paths.ExecutingExe.Directory?.FullName);

        /// <summary>
        ///     Fetches a unique file location.
        /// </summary>
        /// <returns></returns>
        public static FileInfo FetchPath() {
            return new FileInfo(Path.Combine(GetUniqueDirectory.FullName, GetUniqueFileName));
        }
         
    }
}