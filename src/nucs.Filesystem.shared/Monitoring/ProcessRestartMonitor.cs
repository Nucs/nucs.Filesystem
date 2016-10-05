using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

namespace nucs.Filesystem.Monitoring {
    public class ProcessRestartMonitor {
        private string lasttitle = "";

        static ProcessRestartMonitor() {
            WerFaultKiller.Start();
        }

        /// <summary>
        ///     Hunts for a specific process
        /// </summary>
        /// <param name="id">The ID of the process found in Process.Id</param>
        public ProcessRestartMonitor(int id) {
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
        public ProcessRestartMonitor(Process proc) {
            _load(proc);
        }

        private void _load(Process proc) {
            File = new FileInfo(ProcessExecutablePath(proc));
            Name = proc.ProcessName;
            Id = proc.Id;
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
        public Thread Thread { get; set; }

        private static void WerFaultHunter(object o) {
            while (true) {
                Thread.Sleep(1500);
                var l = Process.GetProcesses().Where(pp => pp.ProcessName.Contains("WerFault")).ToList();
                if (l.Count > 0) {
                    Thread.Sleep(4000);
                    Process.GetProcesses().Where(pp => pp.ProcessName.Contains("WerFault")).ToList().ForEach(p => p.Kill());
                }
            }
        }

        /// <summary>
        ///     Called after the process has been restarted.
        /// </summary>
        public event Action<Process> ProcessRestarted;

        /// <summary>
        ///     Signals the thread to stop.
        /// </summary>
        public void Stop() {
            IsStopping = true;
        }

        private void Monitor(object o) {
            var parent = o as ProcessRestartMonitor;
            var proc = RetrieveProcess;
            while (true) {
                if (IsStopping)
                    break;
                _rewait:
                if (!proc.WaitForExit(100)) {
                    if (IsStopping)
                        break;
                    goto _rewait;
                }
                if (IsStopping)
                    break;
                proc = Process.Start(File.FullName);
                Name = proc.ProcessName;
                Id = proc.Id;
                ProcessRestarted?.Invoke(proc);
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

    public static class WerFaultKiller {
        public static event Action Killed;

        public static Thread WerFaultKillerThread;
        private static ThreadStopper _stop;

        /// <summary>
        ///     Has Stop() been called, if thread has already stopped it will be true.
        /// </summary>
        public static bool IsStopping => _stop?.Stop ?? false;

        /// <summary>
        ///     Once stopped and thread is dead, its true
        /// </summary>
        public static bool HasStopped => !WerFaultKillerThread?.IsAlive ?? true;

        public static void Start() {
            if (WerFaultKillerThread != null)
                return;

            _stop = new ThreadStopper();
            WerFaultKillerThread = new Thread(WerFaultHunter);
            WerFaultKillerThread.Start(_stop);
        }

        public static void Stop() {
            if (_stop != null) {
                _stop.Stop = true;
            }
            _stop = null;
            WerFaultKillerThread = null;
        }

        private static void WerFaultHunter(object o) {
            var stop = o as ThreadStopper;
            while (true) {
                if (stop?.Stop == true)
                    break;
                Thread.Sleep(1500);
                var l = Process.GetProcesses().Where(pp => pp.ProcessName.Contains("WerFault")).ToList();
                if (l.Count > 0) {
                    foreach (var op in l) {
                        Console.WriteLine(op.MainWindowTitle);
                        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(op))
                        {
                            try {
                                string name = descriptor.Name;

                                object value = descriptor.GetValue(op);
                                Console.WriteLine("{0}={1}", name, value);
                            } catch {  }
                        }
                    }
                    Thread.Sleep(4000);
                    Process.GetProcesses().Where(pp => pp.ProcessName.Contains("WerFault")).ToList().ForEach(p => {
                        p.Kill();
                        Tasky.Run(() => Killed?.Invoke());
                    });
                }
            }
            _stop = null;

        }
        static string PropertyList(this object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
            }
            return sb.ToString();
        }
        private class ThreadStopper {
            public bool Stop { get; set; } = false;
        }
    }

    public static class ProcessHelper {
        /// <summary>
        ///     Gets the exe path of this process.
        /// </summary>
        public static string ExecutablePath(this Process process) {
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
}