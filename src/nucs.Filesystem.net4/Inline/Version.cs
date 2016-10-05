/*
using System;
using System.Collections.Generic;
using System.Linq;
using nucs.Collections.Extensions;
using Z.ExtensionMethods.Object;

namespace nucs.SystemCore {
    internal sealed class Version : ISizeComparable<Version> {
        private double _primaryVersion=1;
        private uint _changesVersion=0;

        internal double PrimaryVersion {
            get { return _primaryVersion; }
            set { _primaryVersion = value; }
        }
        
        // ReSharper disable once ConvertToAutoProperty
        internal uint ChangesVersion {
            get { return _changesVersion; }
            set { _changesVersion = value; }
        }

        internal Version() { }

        internal Version(double primaryVersion, uint changesVersion) {
            _primaryVersion = primaryVersion;
            _changesVersion = changesVersion;
        }

        internal Version(string s) {
            var spl = s.Split(':');
            if (spl.Length > 2) {
                throw new IndexOutOfRangeException("The version string is invalid in format. format: '1.15:45315' or just '1.05'");
            } 
            if (spl.Length >= 1) {
                _primaryVersion = Convert.ToDouble(spl[0]);
            } 
            if (spl.Length == 2) {
                _changesVersion = Convert.ToUInt32(spl[1]);
            }
        }

        internal static explicit operator Version(string d) {
            return new Version(d);
        }

        internal static explicit operator string(Version d) {
            return d.ToString();
        }


        internal override int GetHashCode() {
            var res = PrimaryVersion*(ChangesVersion == 0 ? 1 : ChangesVersion);
            if (res > Int32.MaxValue)
                res -= Int32.MaxValue;
            
            return Convert.ToInt32(res);
        }

        internal SizeCompare Compare(Version o) {
            if (o.IsNull())
                return SizeCompare.Uncomparable;
            if (PrimaryVersion > o.PrimaryVersion)
                return SizeCompare.Larger;
            if (PrimaryVersion < o.PrimaryVersion)
                return SizeCompare.Smaller;
            if (PrimaryVersion == o.PrimaryVersion) {
                if (ChangesVersion == o.ChangesVersion)
                    return SizeCompare.Equals;
                if (ChangesVersion > o.ChangesVersion)
                    return SizeCompare.Larger;
                if (ChangesVersion < o.ChangesVersion)
                    return SizeCompare.Smaller;
            }
            
            return SizeCompare.Uncomparable;
        }

        internal override string ToString() {
            return string.Format("{0}:{1}", PrimaryVersion.ToString("##.0####"), ChangesVersion);
        }
    }

    internal static class VersionExtensions {
        /// <summary>
        /// Turns a list of versions to numbers that are comparable.
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        internal static IEnumerable<double> ToValues(this IEnumerable<Version> vals) {
            return vals.Select(v => v.ToString()).Select(v => v.Split(':'))
                .Select(v => v.Length == 2 
                        ? v[0].Split('.').StringJoin("")+"."+v[1] 
                        : v[0])
                .Select(Convert.ToDouble);
        }

        /// <summary>
        /// Turns a version to numbers that are comparable.
        /// </summary>
        /// <param name="vals"></param>
        /// <returns></returns>
        internal static double ToValue(this Version vals) {
            return vals.ToEnumerable().ToValues().FirstOrDefault();
        }
    }
}
*/
