using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace nucs.Windows.Processes {
    internal static class WindowFinder {
        /// <summary>
        /// Finds the window's handle by the process name, if it is for example "main.exe" in the Task Manager, then search for "main"
        /// </summary>
        /// <param name="name">for process "deven.exe" search for "deven"</param>
        /// <returns>If not found, <see cref="IntPtr.Zero"/></returns>
        internal static IntPtr ByProccessName(string name) {
            if (name.Contains("."))
                name = name.Split('.')[0];

            var hWnd = IntPtr.Zero;
            foreach (var proc in Process.GetProcesses())
                if (proc.ProcessName.Contains(name)) //MainWindowTitle MU ISRAEL //ProcessName main
                    hWnd = proc.MainWindowHandle;
            return hWnd;
        }

        /// <summary>
        /// Finds the window's handle by the process title, for example "Untitles - Notepad"
        /// </summary>
        /// <param name="name">for process "deven.exe" search for "deven"</param>
        /// <returns>If not found, <see cref="IntPtr.Zero"/></returns>
        internal static IntPtr ByTitle(string name) {
            if (name.Contains("."))
                name = name.Split('.')[0];

            var hWnd = IntPtr.Zero;
            foreach (var proc in Process.GetProcesses())
                if (proc.MainWindowTitle.Contains(name)) //MainWindowTitle MU ISRAEL //ProcessName main
                    hWnd = proc.MainWindowHandle;
            return hWnd;
        }

        /// <summary>
        /// Finds the window's handle by the process id, if it is for example "main.exe" in the Task Manager, then search for "main"
        /// </summary>
        /// <returns>If not found, <see cref="IntPtr.Zero"/></returns>
        internal static IntPtr ById(int id) {

            var hWnd = IntPtr.Zero;
            foreach (var proc in Process.GetProcesses())
                if (proc.Id == id) //MainWindowTitle MU ISRAEL //ProcessName main
                    hWnd = proc.MainWindowHandle;
            return hWnd;
        }

        /// <summary>
        /// Finds the window's handle by the process id, if it is for example "main.exe" in the Task Manager, then search for "main"
        /// </summary>
        /// <returns>If not found, <see cref="IntPtr.Zero"/></returns>
        internal static IntPtr ByProcessInfo(ProcessInfo info) {
            var p = info.ToProcess();
            if (p == null) return IntPtr.Zero;
            return p.MainWindowHandle;
        }


    }
}
