﻿using System.Linq;

namespace nucs.SystemCore.Boolean {

    /// <summary>
    /// A delegate that returns boolean for feedback. can be used for validating, finding and more.
    /// </summary>
    internal delegate bool BoolAction();
    /// <summary>
    /// A delegate that returns boolean for feedback. can be used for validating, finding and more.
    /// </summary>
    public delegate bool BoolAction<in T1>(T1 t1);
    /// <summary>
    /// A delegate that returns boolean for feedback. can be used for validating, finding and more.
    /// </summary>
    internal delegate bool BoolAction<in T1, in T2>(T1 t1, T2 t2);
    /// <summary>
    /// A delegate that returns boolean for feedback. can be used for validating, finding and more.
    /// </summary>
    internal delegate bool BoolAction<in T1, in T2, in T3>(T1 t1, T2 t2, T3 t3);

    /// <summary>
    /// A simple class that holds boolean value.<remarks>You can implicit to Boolean and explicit boolean to Bool</remarks>
    /// </summary>
    internal class Bool {
        internal bool value = false; //default

        internal Bool(bool b) {
            value = b;
        }

        internal Bool(Bool b) {
            value = b.value;
        }

        public static explicit operator Bool(bool b) {
            return new Bool(b);
        }

        public static implicit operator bool(Bool b) {
            return b.value;
        }

        internal static bool EqualsAny(object obj, params object[] objs) {
            return objs.Any(o => o.Equals(obj));
        }

        internal static bool EqualsAny<T>(T obj, params T[] objs) {
            return objs.Any(o => o.Equals(obj));
        }
    }
}
