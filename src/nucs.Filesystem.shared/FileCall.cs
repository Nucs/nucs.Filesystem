using System;
using System.Diagnostics;
using System.IO;
using nucs.Filesystem.Distribution;
using nucs.SystemCore.Boolean;
using nucs.Windows.Processes;

namespace nucs.Filesystem {

    /// <summary>
    ///     Represents a File that is in Startup
    /// </summary>
    [Serializable]
    public class FileCall {

        public bool Exists => File.Exists(this.FileInfo.FullName);

        public DirectoryInfo BaseDirectory => this.FileInfo.Directory;

        public FileInfo FileInfo { get; }

        public string Alias { get; set; }

        public string Arguments { get; set; }

        public string FullName => FileInfo.FullName;

        public FileCall(FileInfo fi, string arguments="", string alias = null) {
            this.FileInfo = fi;
            Alias = alias;
            Arguments = arguments;
        }

        public FileCall(string executioncommand, string alias = null) {
            var _path = executioncommand;
            if (_path.EndsWith(".exe") == false) { //has extension 
                _path = _path.Substring(0, _path.IndexOf(".exe") + 4);
            }
            var args = executioncommand.Replace(_path, "").TrimStart('"').Trim();
            _path = _path.Replace("\"", "");
            this.FileInfo = new FileInfo(_path);
            
            Alias = alias;
            Arguments = args;
        }

        /// <summary>
        ///     Representive design, can be reparsed.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"\"{this.FileInfo.FullName}\" {Arguments}";
        }

        public string GetFileNameWithoutExtension() {
            return this.FileInfo.GetFileNameWithoutExtension();
        }

        // User-defined conversion from Digit to double 
        public static implicit operator FileInfo(FileCall d) {
            return d.FileInfo;
        }

        public override bool Equals(object obj) {
            return base.GetHashCode().Equals(obj.GetHashCode());
        }

        public override int GetHashCode() {
            return this.FileInfo.FullName.GetHashCode();
        }

        /// <summary>
        ///     Starts the FileCall.
        /// </summary>
        /// <param name="asAdmin"></param>
        public Process Start(bool asAdmin=false) {
            return Processy.Start(FileInfo.FullName, Arguments, true, asAdmin, ProcessWindowStyle.Normal);
        }
    }
}