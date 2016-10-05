using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;

namespace nucs.Filesystem.Enumerators {
    public class ActiveProcessFiles : IEnumerable<FileInfo>, IEnumerable<FileProcessInfo> {
        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<FileProcessInfo> IEnumerable<FileProcessInfo>.GetEnumerator() {
            var p = Process.GetProcesses();
            foreach (var proc in p) {
                FileInfo _r;
                try {
                    var path = ProcessExecutablePath(proc);
                    if (string.IsNullOrEmpty(path))
                        continue;
                    _r = new FileInfo(path);
                } catch {
                    continue;
                }
                yield return new FileProcessInfo() {FileInfo = _r, Process = proc};
            }
        }

        public IEnumerator<FileInfo> GetEnumerator() {
            var p = Process.GetProcesses();
            foreach (var proc in p) {
                FileInfo _r;
                try {
                    var path = ProcessExecutablePath(proc);
                    if (string.IsNullOrEmpty(path))
                        continue;
                    _r = new FileInfo(path);
                } catch {
                    continue;
                }
                yield return _r;
            }
        }

        /// <summary>
        ///     Will enumerate through all FileInfos and return the matching one to the comperator.
        /// </summary>
        /// <param name="comperator"></param>
        /// <returns></returns>
        public Process Enumerate(Func<FileInfo, bool> comperator) {
            var p = Process.GetProcesses();
            foreach (var proc in p) {
                FileInfo _r;
                try {
                    var path = ProcessExecutablePath(proc);
                    if (string.IsNullOrEmpty(path))
                        continue;
                    _r = new FileInfo(path);
                } catch {
                    continue;
                }
                if (comperator(_r))
                    return proc;
            }
            return null;
        }

        /// <summary>
        ///     Will enumerate through all FileInfos and return the matching one to the comperator.
        /// </summary>
        /// <param name="comperator"></param>
        /// <returns></returns>
        public Process Enumerate(Func<Process, FileInfo, bool> comperator) {
            var p = Process.GetProcesses();
            foreach (var proc in p) {
                FileInfo _r;
                try {
                    var path = ProcessExecutablePath(proc);
                    if (string.IsNullOrEmpty(path))
                        continue;
                    _r = new FileInfo(path);
                } catch {
                    continue;
                }
                if (comperator(proc, _r))
                    return proc;
            }
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        private static string ProcessExecutablePath(Process process) {
            try {
                return process.MainModule.FileName;
            } catch {
                var query = "SELECT ExecutablePath, ProcessID FROM Win32_Process";
                var searcher = new ManagementObjectSearcher(query);

                foreach (var o in searcher.Get()) {
                    var item = (ManagementObject) o;
                    var id = item["ProcessID"];
                    var path = item["ExecutablePath"];

                    if (path != null && id.ToString() == process.Id.ToString()) {
                        return path.ToString();
                    }
                }
            }

            return null;
        }
    }

    public class FileProcessInfo {
        public Process Process { get; set; }
        public FileInfo FileInfo { get; set; }
    }
}