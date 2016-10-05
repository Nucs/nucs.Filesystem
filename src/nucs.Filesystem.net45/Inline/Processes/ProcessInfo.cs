using System;
using System.Diagnostics;
using System.Linq;
using nucs.SystemCore.String;


namespace nucs.Windows.Processes {

    
    internal class ProcessInfo {
        
        internal int UniqueID { get; private set; }

        
        internal string Name { get; private set; }

        
        internal string MachineName { get; private set; }

        
        private long _handleSerialize {
            get { return Handle.ToInt64(); }
            set { Handle = new IntPtr(value);}
        }



        internal IntPtr Handle = IntPtr.Zero;

        internal ProcessInfo(Process proc) {
            if (proc == null)
                return;
            UniqueID = proc.Id;
            Name = proc.ProcessName;
            MachineName = proc.MachineName;
            Handle = proc.Handle;
        }

        private ProcessInfo() { }

        internal ProcessInfo(int UniqueID, string Name, string MachineName) {
            this.UniqueID = UniqueID;
            this.Name = Name;
            this.MachineName = MachineName;
        }
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcessInfo) obj);
        }
        internal bool Equals(Process proc) {
            try {
                return (UniqueID == proc.Id && Name == proc.ProcessName && MachineName == proc.MachineName);
            } catch {
                return false;
            }
        }

        protected bool Equals(ProcessInfo proc) {
            return UniqueID == proc.UniqueID && string.Equals(Name, proc.Name) && string.Equals(MachineName, proc.MachineName);
        }

        internal bool Exists() {
            return ProcessFinder.Exists(ProcessSearchMethod.ProcessInfo, this) > 0;
        }

        /// <summary>
        /// Attemts to find the process using the function <see cref="ProcessFinder.FindProcess"/>. If not found, null is returned
        /// </summary>
        /// <returns></returns>
        internal Process ToProcess() {
            return ProcessFinder.Find(ProcessSearchMethod.ProcessInfo, this).FirstOrDefault();
        }

        internal bool WaitForExit(uint timeout = 0) {
            try {
                var proc = this.ToProcess();
                if (proc == null) return true;
                if (timeout == 0) {
                    proc.WaitForExit();
                    return true;
                }
                return proc.WaitForExit((int) timeout);
            } catch {
                return true;
            }
        }

        public override string ToString() {
            return Name + "↔" + UniqueID + "↔" + MachineName;
        }

        public override int GetHashCode() {
            unchecked {
                int hashCode = UniqueID;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MachineName != null ? MachineName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ProcessInfo left, ProcessInfo right) {
            return Equals(left, right);
        }

        public static bool operator !=(ProcessInfo left, ProcessInfo right) {
            return !Equals(left, right);
        }

        internal static ProcessInfo TryParse(string toString) {
            try {
                var s = toString.Split('↔');
                return new ProcessInfo(s[1].ToInt32(), s[0], s[2]);

            }
            catch {
                return null;
            }
        }

    }
}
