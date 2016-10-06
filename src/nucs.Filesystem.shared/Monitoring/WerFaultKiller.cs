using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace nucs.Filesystem.Monitoring {
    /// <summary>
    ///       Automatically close any Window's WerFault reporting of an application crashing
    /// </summary>
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
                        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(op)) {
                            try {
                                string name = descriptor.Name;

                                object value = descriptor.GetValue(op);
                                Console.WriteLine("{0}={1}", name, value);
                            } catch {}
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

        static string PropertyList(this object obj) {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props) {
                sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
            }
            return sb.ToString();
        }

        private class ThreadStopper {
            public bool Stop { get; set; } = false;
        }
    }
}