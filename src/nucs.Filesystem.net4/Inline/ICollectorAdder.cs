using System.Collections.Generic;

namespace nucs.Collections {
    internal interface ICollectorAdder<T> {
        void Add(T item, bool isLast = false);
        void AddRange(IEnumerable<T> items, bool isLast = false);
    }
}