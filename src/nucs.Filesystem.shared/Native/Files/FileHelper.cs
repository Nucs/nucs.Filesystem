using System;

namespace nucs.Filesystem {
    internal static class EnumExtension {
        public static T FromEnumObj<T>(this Enum e) {
            if (typeof(T).IsEnum == false || e == null)
                return (T) (object) -1;
            return (T) Enum.Parse(typeof(T), e.ToString());
        }

        public static int ToInt(this Enum e) {
            if (e == null)
                return -1;
            return (int) (object) e;
        }

        public static int ToInt(this object e) {
            if (e == null)
                return -1;
            return (int) (object) e;
        }

        public static T FromInt<T>(this int e) {
            return (T) Enum.ToObject(typeof(T), e);
        }

        public static Enum FromIntToEnumObj<T>(this int e) {
            return (Enum) Enum.ToObject(typeof(T), e);
        }

        public static Enum ToEnumObj<T>(this T e) {
            return (Enum) (object) e;
        }

        public static T ToEnum<T>(this object o) {
            if (o == null)
                return (T) (object) -1;
            return (T) o;
        }
    }


    public enum ApplicationType {
        WinForms,
        Console
    }
}