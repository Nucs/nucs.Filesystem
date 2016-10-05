using System.Collections;
using System.Collections.Generic;
using System.Management;

namespace nucs.Filesystem.Enumerators {
    public class AutorunServiceFiles : IEnumerable<FileCall> {
        public IEnumerator<FileCall> GetEnumerator() {
            ManagementClass c = new ManagementClass("Win32_Service");
            foreach (ManagementObject o in c.GetInstances()) {
                if (o.GetPropertyValue("StartMode").ToString() != "Auto") //is it autostarted
                    continue;
                string p = o.GetPropertyValue("PathName").ToString();
                yield return new FileCall(p);
            }
        
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }


    }
}