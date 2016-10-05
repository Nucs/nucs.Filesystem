using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace nucs.Filesystem {
    public static class DriveInfoHelper {
        /// <summary>
        /// Generates a unique ID for the current drive.
        /// </summary>
        public static Guid GenerateGuid(this DriveInfo di) {
            var uniquetext = $"{di.VolumeLabel ?? ""},{di.TotalSize},{""},{di.DriveType},{di.DriveFormat}";

            using (SHA1 sha = SHA1.Create()) {
                byte[] hash = sha.ComputeHash(Encoding.Default.GetBytes(uniquetext));
                var guid = new Guid(hash.Take(16).ToArray());
                return guid;
            }
        }
         
    }
}