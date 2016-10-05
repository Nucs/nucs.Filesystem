using System;
using System.Collections.Generic;
using System.IO;

namespace nucs.Filesystem.Distribution.Methods {
    public class ProgramFileDistributer : SubdirectoriesDistributer {

        protected override IEnumerable<DirectoryInfo> FetchBaseDirs() {
            var a = new List<DirectoryInfo>();
            if (8 == IntPtr.Size || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))) {
                yield return new DirectoryInfo(Environment.GetEnvironmentVariable("ProgramFiles(x86)"));
            }

            yield return new DirectoryInfo(Environment.GetEnvironmentVariable("ProgramFiles"));
        }

        protected override uint SearchDepth() {
            return 1;
        }

        public override bool IsStatic => false;
    }
}
