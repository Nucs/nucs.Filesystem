using System;

namespace nucs.Filesystem {
   /* public static class FileHelper {


        /// <summary>
        ///     Exploits windows's file attribute System|Hidden, making it totally invisiable when browsing with explorer.exe.
        /// </summary>
        public static void SetVisibility(this FileInfo fi, bool visible) {
            //todo handle unaccessable files.
            fi.Attributes = visible
                ? fi.Attributes.Remove(FileAttributes.Hidden).Remove(FileAttributes.System)
                : fi.Attributes.Add(FileAttributes.Hidden).Add(FileAttributes.System);
        }

        /// <summary>
        ///     Exploits windows's file attribute System|Hidden, making it totally invisiable when browsing with explorer.exe.
        /// </summary>
        public static void SetVisibility(this string fi, bool visible) {
            SetVisibility(new FileInfo(fi), visible);
        }

        public static void SetReadOnly(this FileInfo fi, bool @readonly) {
            //todo handle unaccessable files.
            fi.Attributes = @readonly
                ? fi.Attributes.Remove(FileAttributes.ReadOnly)
                : fi.Attributes.Add(FileAttributes.ReadOnly);
        }


    }*/

    internal static class EnumExtension
    {
        public static T FromEnumObj<T>(this Enum e)
        {
            if (typeof(T).IsEnum == false || e == null) return (T)(object)-1;
            return (T)Enum.Parse(typeof(T), e.ToString());
        }

        public static int ToInt(this Enum e)
        {
            if (e == null) return -1;
            return (int)(object)e;
        }

        public static int ToInt(this object e)
        {
            if (e == null) return -1;
            return (int)(object)e;
        }

        public static T FromInt<T>(this int e)
        {
            return (T)Enum.ToObject(typeof(T), e);
        }

        public static Enum FromIntToEnumObj<T>(this int e)
        {
            return (Enum)Enum.ToObject(typeof(T), e);
        }

        public static Enum ToEnumObj<T>(this T e)
        {
            return (Enum)(object)e;
        }
        public static T ToEnum<T>(this object o)
        {
            if (o == null) return (T)(object)-1;
            return (T)o;
        }
    }


    public enum ApplicationType {
        WinForms,
        Console
    }
}