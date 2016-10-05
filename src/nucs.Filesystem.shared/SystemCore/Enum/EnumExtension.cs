namespace nucs.Filesystem.SystemCore.Enum {
    public static class EnumExtension {
        public static T FromEnumObj<T>(this System.Enum e) {
            if (typeof (T).IsEnum == false || e == null) return (T)(object)-1;
            return (T)System.Enum.Parse(typeof(T), e.ToString());
        }

        public static int ToInt(this System.Enum e) {
            if (e == null) return -1;
            return (int) (object) e;
        }

        public static int ToInt(this object e) {
            if (e == null) return -1;
            return (int) (object) e;
        }

        public static T FromInt<T>(this int e) {
            return (T) System.Enum.ToObject(typeof (T), e);
        }

        public static System.Enum FromIntToEnumObj<T>(this int e) {
            return (System.Enum) System.Enum.ToObject(typeof (T), e);
        }

        public static System.Enum ToEnumObj<T>(this T e) {
            return (System.Enum)(object)e;
        }
        public static T ToEnum<T>(this object o) {
            if (o == null) return (T) (object) -1;
            return (T)o;
        }
    }
}