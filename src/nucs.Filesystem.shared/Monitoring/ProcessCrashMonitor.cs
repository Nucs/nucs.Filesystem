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
    public class ProcessCrashMonitor : IDisposable {
        public Func<Process, bool> AdditionalCheck { get; set; }
        private string lasttitle = "";

        static ProcessCrashMonitor() {
            WerFaultKiller.Start();
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        /// <param name="additionalCheck"></param>
        public ProcessCrashMonitor(int id, Func<Process, bool> additionalCheck = null) {
            AdditionalCheck = additionalCheck;
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
        /// <param name="additionalCheck"></param>
        public ProcessCrashMonitor(Process proc, Func<Process, bool> additionalCheck=null) {
            AdditionalCheck = additionalCheck;
            _load(proc);
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        /// <param name="additionalCheck"></param>
        public ProcessCrashMonitor(FileInfo file, Func<Process, bool> additionalCheck = null) {
            AdditionalCheck = additionalCheck;
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
            lock (this) {
                if (_cancel == null)
                    _cancel = new CancellationTokenSource();
                var path = ProcessExecutablePath(proc);
                if (string.IsNullOrEmpty(path) == false)
                    File = new FileInfo(path);
                Name = proc.ProcessName;
                Id = proc.Id;
                this.Thread = new Thread(Monitor);
                this.Thread.Start(this);
            }
        }

        private CancellationTokenSource _cancel { get; set; }

        /// <summary>
        ///     Has Stop() been called, if thread has already stopped it will be true.
        /// </summary>
        public bool IsStopping => _cancel.IsCancellationRequested;

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
                lock (this)
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
                lock (this) {
                    var p = RetrieveProcess;
                    var tit = p.MainModule.FileVersionInfo.FileDescription;
                    if (tit == "")
                        tit = p.ProcessName;
                    lasttitle = tit;
                    return lasttitle;
                }
            }
        }

        /// <summary>
        ///     Active monitoring thread.
        /// </summary>
        private Thread Thread { get; set; }

        /// <summary>
        ///     Called after the process has been restarted.
        /// </summary>
        public event Action<ProcessCrashMonitor, Process> ProcessCrashed;

        /// <summary>
        ///     When process crashed and a new one has started.
        /// </summary>
        public event Action<ProcessCrashMonitor, Process> ProcessRebound;

        /// <summary>
        ///     Signals the thread to stop.
        /// </summary>
        public void Stop() {
            _cancel.Cancel(false);
        }

        /// <summary>
        /// Tries to open from the existing 
        /// </summary>
        public void OpenIfClosed() {
            lock (this) {
                if (RetrieveProcess == null) {
                    var proc = Process.Start(File.FullName);
                    proc.WaitForInputIdle();
                    while (!proc.Responding) Thread.Sleep(10);
                    _load(proc);
                }
            }
        }

        public void Open() {
            lock (this) {
                var proc = Process.Start(File.FullName);
                proc.WaitForInputIdle();
                while (!proc.Responding) Thread.Sleep(10);
                Stop();
                _load(proc);
            }
        }

        private void Monitor(object o) {
            if (o == null) throw new ArgumentNullException(nameof(o));
            var parent = (ProcessCrashMonitor) o;
            var stopsource = parent._cancel;
            if (stopsource == null)
                throw new InvalidOperationException();
            bool _isStopping() => stopsource.IsCancellationRequested;

            var proc = RetrieveProcess;

            while (true) {
                if (_isStopping()) {
                    break;
                }
                if (proc == null)
                    goto _rebind;
                _rewait:
                if (!proc.WaitForExit(100) && parent.AdditionalCheck?.Invoke(proc) != true) {
                    if (_isStopping())
                        break;
                    goto _rewait;
                }
                if (_isStopping())
                    break;
                _rebind:
                if (proc?.HasExited == false) {
                    try {proc.Kill();} catch {}
                    try {proc.Kill();} catch {}
                }
                ProcessCrashed?.Invoke(parent, proc);
                //bind to new thread
                Process np;
                while ((np = Process.GetProcesses().FirstOrDefault(p => p.ProcessName == this.Name && ProcessExecutablePath(p).Equals(File.FullName, StringComparison.InvariantCultureIgnoreCase))) == null) {
                    Thread.Sleep(300);
                }
                proc = np;
                Name = np.ProcessName;
                Id = np.Id;
                ProcessRebound?.Invoke(parent, proc);
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

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() {
            Stop();
        }

        #endregion
    }
}