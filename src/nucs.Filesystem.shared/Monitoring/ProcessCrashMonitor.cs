using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using nucs.Filesystem.Enumerators;

namespace nucs.Filesystem.Monitoring {
    /// <summary>
    ///     Monitor a process crashing
    /// </summary>
    public class ProcessCrashMonitor {
        private string lasttitle = "";

        static ProcessCrashMonitor() {
            WerFaultKiller.Start();
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public ProcessCrashMonitor(int id) {
            if (id <= 0)
                throw new ArgumentException(nameof(id));
            var proc = Process.GetProcesses().FirstOrDefault(p => p.Id == id);
            if (proc == null)
                throw new NullReferenceException("The given proc ID is not found on any open process.");

            _load(proc);
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public ProcessCrashMonitor(Process proc) {
            _load(proc);
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public ProcessCrashMonitor(FileInfo file) {
            if (file == null || !System.IO.File.Exists(file.FullName))
                throw new ArgumentNullException(nameof(file));
            var active = new ActiveProcessFiles().Enumerate(info => info.CompareTo(file));
            if (active == null)
                Id = 0;
            else {
                _load(active);
            }


        }

        private void _load(Process proc) {
            File = new FileInfo(ProcessExecutablePath(proc));
            Name = proc.ProcessName;
            Id = proc.Id;
            if (this.HasStopped)
            Thread = new Thread(Monitor);
            Thread.Start(this);
        }

        /// <summary>
        ///     Has Stop() been called, if thread has already stopped it will be true.
        /// </summary>
        public bool IsStopping { get; private set; }

        /// <summary>
        ///     Once stopped and thread is dead, its true
        /// </summary>
        public bool HasStopped => IsStopping && !Thread.IsAlive;

        /// <summary>
        ///     File taken from the process
        /// </summary>
        public FileInfo File { get; private set; }

        /// <summary>
        ///     Id that changes with every rebind.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        ///     Attempts to find the current process that is monitored, null if faultly not available atm.
        /// </summary>
        public Process RetrieveProcess {
            get {
                try {
                    return Process.GetProcessById(Id);
                } catch {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Name of the active process.
        /// </summary>
        public string Name { get; private set; }

        public string Title {
            get {
                var p = RetrieveProcess;
                var tit = p.MainModule.FileVersionInfo.FileDescription;
                if (tit == "")
                    tit = p.ProcessName;
                lasttitle = tit;
                return lasttitle;
            }
        }

        /// <summary>
        ///     Active monitoring thread.
        /// </summary>
        private Thread Thread { get; set; }

        /// <summary>
        ///     Called after the process has been restarted.
        /// </summary>
        public event Action<Process> ProcessCrashed;

        /// <summary>
        ///     When process crashed and a new one has started.
        /// </summary>
        public event Action<Process> ProcessRebound;

        /// <summary>
        ///     Signals the thread to stop.
        /// </summary>
        public void Stop() {
            IsStopping = true;
        }

        /// <summary>
        /// Tries to open from the existing 
        /// </summary>
        public void OpenIfClosed() {
            if (Id == 0) {
                var proc = Process.Start(File.FullName);
                _load(proc);
            }
        }

        public void Open() {
            if (Id == 0) {
                var proc = Process.Start(File.FullName);
                _load(proc);
            }
        }

        private void Monitor(object o) {
            var parent = o as ProcessCrashMonitor;
            var proc = RetrieveProcess;
            while (true) {
                if (IsStopping) {
                    break;
                }
                _rewait:
                if (!proc.WaitForExit(100)) {
                    if (IsStopping)
                        break;
                    goto _rewait;
                }
                if (IsStopping)
                    break;

                ProcessCrashed?.Invoke(proc);
                //bind to new thread
                Process np;
                while ((np = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == this.Name && ProcessExecutablePath(p).Equals(File.FullName, StringComparison.InvariantCultureIgnoreCase))) == null) {
                    Thread.Sleep(300);
                }
                proc = np;
                Name = np.ProcessName;
                Id = np.Id;
                ProcessRebound?.Invoke(proc);
            }
        }

        public static string ProcessExecutablePath(Process process) {
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

            return "";
        }
    }
}