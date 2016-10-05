using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using nucs.Filesystem.Enumerators;
using nucs.Windows.Processes;
using SHDocVw;
using ThreadState = System.Threading.ThreadState;

namespace nucs.Filesystem.Monitoring {
    public class AppMonitor {
        #region Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public FileInfo File { get; set; }

        /// <summary>
        /// Checks if the app is running
        /// </summary>
        public bool IsRunning => ProcessProcess() != null;

        #endregion

        #region Constructors

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public AppMonitor(int id) {
            if (id <= 0)
                throw new ArgumentException(nameof(id));
            var proc = Process.GetProcesses().FirstOrDefault(p => p.Id == id);
            if (proc == null)
                throw new NullReferenceException("The given proc ID is not found on any open process.");

            File = new FileInfo(ProcessExecutablePath(proc));
            Name = proc.ProcessName;
            Id = proc.Id;
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public AppMonitor(Process proc) {
            File = new FileInfo(ProcessExecutablePath(proc));
            Name = proc.ProcessName;
            Id = proc.Id;
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public AppMonitor(FileInfo file) {
            if (file == null || !System.IO.File.Exists(file.FullName))
                throw new ArgumentNullException(nameof(file));
            var active = new ActiveProcessFiles().Enumerate(info => info.CompareTo(file));
            if (active == null) { 
                File = new FileInfo(Paths.NormalizePath(file.FullName));
                Id = 0;
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Attempts to start the program if it doesn't exist.
        /// </summary>
        public void StartProgram() {
            var found = ProcessProcess();
            if (found == null) {
                if (File == null || System.IO.File.Exists(File.FullName) == false) {
                    throw new FileNotFoundException("Could not find a process active nor the file to start.");
                }
                var si = new ProcessStartInfo(File.FullName) {WorkingDirectory = File.Directory.FullName};
                var proc = Processy.Start(File.FullName,asAdmin:true);
                ProcessProcess(proc);
            }
        }

        public void KillProgram() {
            ProcessProcess()?.Kill();
        }

        public void Minimize() {
            var proc = ProcessProcess();
            if (proc == null)
                return;
            MinimizeWindow(proc.MainWindowHandle);
        }

        #endregion

        #region Private

        /// <summary>
        ///     Pass null and it will look for it, pass the process and it will load it.
        /// </summary>
        /// <param name="foundprocess"></param>
        /// <returns></returns>
        public Process ProcessProcess(Process foundprocess = null) {
            Process p = foundprocess;
            try {
                if (p != null)
                    return p;
                if (this.Id != 0) {
                    p = Process.GetProcesses().FirstOrDefault(proc => proc.Id.Equals(this.Id));
                    if (p != null)
                        return p;
                }

                if (this.Name != null) {
                    p = Process.GetProcesses().FirstOrDefault(proc => proc.ProcessName.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (p != null)
                        return p;
                }

                if (this.File != null && System.IO.File.Exists(this.File.FullName)) {
                    p = new ActiveProcessFiles().Enumerate(info => info.CompareTo(this.File));
                    return p;
                }
                return null;
            } finally {
                if (p != null) {
                    if (File==null)
                        File = new FileInfo(ProcessExecutablePath(p));
                    Id = p.Id;
                    Name = p.ProcessName;
                }
            }
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

        const int SW_SHOWMINNOACTIVE = 7;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void MinimizeWindow(IntPtr handle) {
            ShowWindow(handle, SW_SHOWMINNOACTIVE);
        }

        #endregion
    }
}