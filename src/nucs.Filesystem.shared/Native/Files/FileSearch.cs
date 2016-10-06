using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using nucs.SystemCore.Boolean;

namespace nucs.Filesystem {
    /// <summary>
    ///     Implementation of FileSearching
    /// </summary>
    public static class FileSearch {
        /// <summary>
        ///     Enumerates an entire drive based on given pattern.
        /// </summary>
        public static IEnumerable<FileInfo> EnumerateDrive(DriveInfo drive, string searchPattern = "*.*") {
            return EnumerateFilesDeep(drive.RootDirectory, searchPattern);
        }

        /// <summary>
        ///     Enumerates an entire drive based on given pattern.
        /// </summary>
        public static IEnumerable<FileInfo> EnumerateDrive(char drivechar, string searchPattern = "*.*") {
            var driveinfo = DriveInfo.GetDrives().FirstOrDefault(di => di.RootDirectory.FullName.First() == drivechar);
            if (driveinfo == null)
                return new FileInfo[0];
            var drive = driveinfo.RootDirectory;
            return EnumerateFilesDeep(drive, searchPattern);
        }

        /// <summary>
        ///     Enumerates all files in the given directory including all subdirectories - beware, might take time on large directories.
        /// </summary>
        public static IEnumerable<FileInfo> EnumerateFilesDeep(DirectoryInfo @base, string searchPattern = "*.*") {
            //setup queue list.
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(@base); //enqueue base directory


            while (queue.Count > 0) {
                var tries = 0;
                var dir = queue.Dequeue();
                _notrdy:
                try {
                    foreach (var subdir in dir.GetDirectories())
                        queue.Enqueue(subdir);
                } catch (UnauthorizedAccessException) {
                    continue;
                } catch (IOException e) {
                    if (e.Message.Contains("device is not ready")) {
                        Thread.Sleep(1);
                        if (++tries == 1000)
                            continue;
                        goto _notrdy;
                    }
                }
                foreach (var file in dir.GetFiles(searchPattern.Split('|')))
                    yield return file;
            }
        }

        /// <summary>
        ///     Gets all files in the given directory by the passed patterns with wildcards for e.g. *.txt
        /// </summary>
        /// <param name="this"></param>
        /// <param name="searchPatterns"></param>
        /// <returns></returns>
        public static FileInfo[] GetFiles(this DirectoryInfo @this, params string[] searchPatterns) {
            if (searchPatterns == null || searchPatterns.Length == 0)
                return @this.GetFiles();
            return searchPatterns.SelectMany(@this.GetFiles).Distinct().ToArray();
        }

        /// <summary>
        ///     Enumerates all directories in the given directory including all subdirectories - beware, might take time on large directories.
        /// </summary>
        public static IEnumerable<DirectoryInfo> EnumerateDirectoriesDeep(DirectoryInfo @base, string searchPattern = "*") {
            //setup queue list.
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(@base); //enqueue base directory


            while (queue.Count > 0) {
                var tries = 0;
                var dir = queue.Dequeue();
                _notrdy:
                DirectoryInfo[] dirs = null;
                try {
                    dirs = dir.GetDirectories();
                } catch (UnauthorizedAccessException) {
                    continue;
                } catch (IOException e) {
                    if (e.Message.Contains("device is not ready")) {
                        Thread.Sleep(1);
                        if (++tries == 1000)
                            continue;
                        goto _notrdy;
                    }
                }
                foreach (var subdir in dirs) {
                    queue.Enqueue(subdir);
                    yield return subdir;
                }
            }
        }
    }
}