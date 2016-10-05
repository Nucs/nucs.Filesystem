using System;
using System.Linq;

namespace nucs.SystemCore.String {
    internal static class StringToOthers {
        #region Numbers
        internal static long ToInt64(this string toConvert) {
            try { return Convert.ToInt64(toConvert); } catch { return 0; }
        }
        internal static int ToInt32(this string toConvert) {
            try { return Convert.ToInt32(toConvert); } catch { return 0; }
        }
        internal static short ToInt16(this string toConvert) {
            try { return Convert.ToInt16(toConvert); } catch { return 0; }
        }

        internal static ulong ToUInt64(this string toConvert) {
            try { return Convert.ToUInt64(toConvert); } catch { return 0; }
        }

        internal static uint ToUInt32(this string toConvert) {
            try { return Convert.ToUInt32(toConvert); } catch { return 0; }
        }

        internal static ushort ToUInt16(this string toConvert) {
            try { return Convert.ToUInt16(toConvert); } catch { return 0; }
        }

        internal static decimal ToDecimal(this string toConvert) {
            try { return Convert.ToDecimal(toConvert); } catch { return 0; }
        }

        internal static double ToDouble(this string toConvert) {
            try { return Convert.ToDouble(toConvert); } catch { return 0; }
        }

        internal static float ToFloat(this string toConvert) {
            try { return Convert.ToSingle(toConvert); } catch { return 0; }
        }

        #endregion

        internal static char[] ToChars(this string toConvert) {
            try { return toConvert.ToCharArray(); } catch { return null; }
        }

        internal static string ConvertToString(this char[] chars) {
            return new string(chars);
        }

    }
}
