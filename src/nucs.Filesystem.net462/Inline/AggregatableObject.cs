using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nucs.SystemCore {
    /// <summary>
    /// Allows math operations in various tools such as DictionarySelfModifierAggreatable.
    /// </summary>
    /// <typeparam name="T">Is the object to use math operations, generally the type that is being derived.</typeparam>
    internal abstract class AggregatableObjectBase<T> : IAggregatableObject<T> where T : class, IAggregatableObject<T> {
        public abstract void Add(T b);
        public abstract void Subtract(T b);

        public static T operator +(AggregatableObjectBase<T> a, IAggregatableObject<T> b) {
            a.Add((T) b);
            return a as T;
        }

        public static T operator -(AggregatableObjectBase<T> a, IAggregatableObject<T> b) {
            a.Subtract((T)b);
            return a as T;
        }
    }

    internal interface IAggregatableObject<T> {
        void Add(T b);
        void Subtract(T b);
    }


}
