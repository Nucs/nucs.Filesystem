using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using nucs.Monitoring;

namespace nucs.Filesystem.Monitoring.Windows {
    public class DriveMonitor : MonitorBase<DriveInfo> {
        public delegate void DriveInsertedHandler(Guid driveguid, DriveInfo driveinfo);

        public static event DriveInsertedHandler DriveInserted;

        public static IEnumerable<DriveInfo> AllDrives {
            get { return DriveInfo.GetDrives(); }
        }
        public static IEnumerable<DriveInfo> FileDrives {
            get { return DriveInfo.GetDrives().Where(d=>d.DriveType==DriveType.Fixed || d.DriveType==DriveType.Removable); }
        }
        public static IEnumerable<DriveInfo> FixedDrives {
            get { var remo = RemovableDrives.ToArray();
                return DriveInfo.GetDrives().Where(drive=>drive.DriveType==DriveType.Fixed).Where(drive=>remo.Any(rem=>rem.RootDirectory.FullName.Equals(drive.RootDirectory.FullName))==false); }
        }


        public static IEnumerable<DriveInfo> RemovableDrives {
            get {
                /*return from d in System.IO.DriveInfo.GetDrives()
                    where d.DriveType == DriveType.Removable
                    select d;*/
                return from ManagementObject drive in new ManagementObjectSearcher("select * from Win32_DiskDrive  WHERE InterfaceType='USB'").Get() 
                        from ManagementObject o in drive.GetRelated("Win32_DiskPartition") 
                        from ManagementObject i in o.GetRelated("Win32_LogicalDisk") 
                        select new DriveInfo(i["Name"].ToString()[0].ToString());
            }
        }

        public override IEnumerable<DriveInfo> FetchCurrent() {
            return RemovableDrives;
        }

        public DriveMonitor() : base(
            new DynamicEqualityComparer<DriveInfo>(
                (x, y) => x.RootDirectory.FullName.Equals(y.RootDirectory.FullName)
                , info => info.RootDirectory.FullName.First().GetHashCode())
            ,t1 => false
            ) {
            Entered += item => {
                if (DriveInserted != null) DriveInserted(item.GenerateGuid(), item);
            };
        }
    }
}