// Type: System.Collections.Generic.Queue`1
// Assembly: System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.dll


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using nucs.Collections.Extensions;
using Interlocked = System.Threading.Interlocked;

namespace nucs.Collections {
    /// <summary>
    /// Represents a first-in, first-out collection of objects.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam><filterpriority>1</filterpriority>
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    internal sealed class ArrayQueue<T> : IEnumerable<T>, ICollection {
        private const int _MinimumGrow = 4;
        private const int _ShrinkThreshold = 32;
        private const int _GrowFactor = 200;
        private const int _DefaultCapacity = 4;
        private static readonly T[] _emptyArray = new T[0];
        private T[] _array;
        private int _head;
        private int _size;
        [NonSerialized] private object _syncRoot;
        private int _tail;
        private int _version;
        static ArrayQueue() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1"/> class that is empty and has the default initial capacity.
        /// </summary>
        internal ArrayQueue() {
            _array = _emptyArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1"/> class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Queue`1"/> can contain.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero.</exception>
        internal ArrayQueue(int capacity) {
            if (capacity < 0) {
                throw new ArgumentOutOfRangeException();
            }
            _array = new T[capacity];
            _head = 0;
            _tail = 0;
            _size = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Collections.Generic.Queue`1"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="T:System.Collections.Generic.Queue`1"/>.</param><exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is null.</exception>
        internal ArrayQueue(IEnumerable<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException();
            }
            _array = new T[4];
            _size = 0;
            _version = 0;
            foreach (T obj in collection) {
                Enqueue(obj);
            }
        }

        #region ICollection Members

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </returns>
        public int Count {
            get { return _size; }
        }


        bool ICollection.IsSynchronized {
            get { return false; }
        }


        object ICollection.SyncRoot {
            get {
                if (_syncRoot == null) {
                    Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                }
                return _syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException();
            }
            if (array.Rank != 1) {
                throw new ArgumentException();
            }
            if (array.GetLowerBound(0) != 0) {
                throw new ArgumentException();
            }
            int length1 = array.Length;
            if (index < 0 || index > length1) {
                throw new ArgumentOutOfRangeException();
            }
            if (length1 - index < _size) {
                throw new ArgumentException();
            }
            int num = length1 - index < _size ? length1 - index : _size;
            if (num == 0) {
                return;
            }
            try {
                int length2 = _array.Length - _head < num ? _array.Length - _head : num;
                Array.Copy(_array, _head, array, index, length2);
                int length3 = num - length2;
                if (length3 <= 0) {
                    return;
                }
                Array.Copy(_array, 0, array, index + _array.Length - _head, length3);
            }
            catch (ArrayTypeMismatchException) {
                throw new ArgumentException();
            }
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }


        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        #endregion

        /// <summary>
        /// Removes all objects from the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        internal void Clear() {
            if (_head < _tail) {
                Array.Clear(_array, _head, _size);
            }
            else {
                Array.Clear(_array, _head, _array.Length - _head);
                Array.Clear(_array, 0, _tail);
            }
            _head = 0;
            _tail = 0;
            _size = 0;
            ++_version;
        }

        /// <summary>
        /// Copies the <see cref="T:System.Collections.Generic.Queue`1"/> elements to an existing one-dimensional <see cref="T:System.Array"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.Queue`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than zero.</exception><exception cref="T:System.ArgumentException">The number of elements in the source <see cref="T:System.Collections.Generic.Queue`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        internal void CopyTo(T[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException();
            }
            if (arrayIndex < 0 || arrayIndex > array.Length) {
                throw new ArgumentOutOfRangeException();
            }
            int length1 = array.Length;
            if (length1 - arrayIndex < _size) {
                throw new ArgumentException();
            }
            int num = length1 - arrayIndex < _size ? length1 - arrayIndex : _size;
            if (num == 0) {
                return;
            }
            int length2 = _array.Length - _head < num ? _array.Length - _head : num;
            Array.Copy(_array, _head, array, arrayIndex, length2);
            int length3 = num - length2;
            if (length3 <= 0) {
                return;
            }
            Array.Copy(_array, 0, array, arrayIndex + _array.Length - _head, length3);
        }


        /// <summary>
        /// Adds an object to the end of the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.Queue`1"/>. The value can be null for reference types.</param>
        internal void Enqueue(T item) {
            if (_size == _array.Length) {
                var capacity = (int) (_array.Length*200L/100L);
                if (capacity < _array.Length + 4) {
                    capacity = _array.Length + 4;
                }
                SetCapacity(capacity);
            }
            _array[_tail] = item;
            _tail = (_tail + 1)%_array.Length;
            ++_size;
            ++_version;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.Queue`1.Enumerator"/> for the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </returns>
        internal Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The object that is removed from the beginning of the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Generic.Queue`1"/> is empty.</exception>
        internal T Dequeue() {
            if (_size == 0)
                throw new InvalidOperationException("Count equals to 0, can't Dequeue what does not exist.");
            T obj = _array[_head];
            _array[_head] = default (T);
            _head = (_head + 1)%_array.Length;
            --_size;
            ++_version;
            return obj;
        }

        /// <summary>
        /// Returns the object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1"/> without removing it.
        /// </summary>
        /// 
        /// <returns>
        /// The object at the beginning of the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Collections.Generic.Queue`1"/> is empty.</exception>
        internal T PeekFirst() {
            if (_size == 0) {
                throw new InvalidOperationException("Count equals to 0, can't peek what does not exist.");
            }
            return _array[_head];
        }

        /// <summary>
        /// Determines whether an element is in the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        /// 
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.Queue`1"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.Queue`1"/>. The value can be null for reference types.</param>
        internal bool Contains(T item) {
            int index = _head;
            int num = _size;
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            while (num-- > 0) {
                if (item == null) {
                    if (_array[index] == null) {
                        return true;
                    }
                }
                else if (_array[index] != null && @default.Equals(_array[index], item)) {
                    return true;
                }
                index = (index + 1)%_array.Length;
            }
            return false;
        }

        internal T GetElement(int i) {
            return _array[(_head + i)%_array.Length];
        }

        /// <summary>
        /// Copies the <see cref="T:System.Collections.Generic.Queue`1"/> elements to a new array.
        /// </summary>
        /// 
        /// <returns>
        /// A new array containing elements copied from the <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </returns>
        internal T[] ToArray() {
            if (_size == 0)
                return _emptyArray;
            var objArray = new T[_size];
            
            if (_head < _tail) {
                Array.Copy(_array, _head, objArray, 0, _size);
            } else {
                Array.Copy(_array, _head, objArray, 0, _array.Length - _head);
                Array.Copy(_array, 0, objArray, _array.Length - _head, _tail);
            }
            return objArray;
        }

        internal ImprovedList<T> ToImprList() {
            return ToArray().ToImprList();
        }

        private void SetCapacity(int capacity) {
            var objArray = new T[capacity];
            if (_size > 0) {
                if (_head < _tail) {
                    Array.Copy(_array, _head, objArray, 0, _size);
                }
                else {
                    Array.Copy(_array, _head, objArray, 0, _array.Length - _head);
                    Array.Copy(_array, 0, objArray, _array.Length - _head, _tail);
                }
            }
            _array = objArray;
            _head = 0;
            _tail = _size == capacity ? 0 : _size;
            ++_version;
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="T:System.Collections.Generic.Queue`1"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        internal void TrimExcess() {
            if (_size >= (int) (_array.Length*0.9)) {
                return;
            }
            SetCapacity(_size);
        }

        internal T Peek(int index) {
            if (_size == 0)
                throw new InvalidOperationException("Count equals to 0, can't peek what does not exist.");
            if (index < 0  || index >= _size)
                throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");
            return _array[index];
        }

        internal T Take(int index) {
            if (_size == 0)
                throw new InvalidOperationException("Count equals to 0, can't take what does not exist.");
            var s = ToImprList();
            var item = s.Take(index);
            _array = s.ToArray();
            --_tail;
            --_size;
            ++_version;
            return item;
        }

        internal bool Remove(T item) {
            if (_size == 0)
                return false;
            var s = ToArray().ToImprList();
            if (s.Remove(item)) {
                _array = s.ToArray();
                --_tail;
                --_size;
                ++_version;
                return true;
            }
            return false;
        }

        internal bool RemoveAt(int index) {
            if (_size == 0)
                return false;

            if (0 > index || index >= _size)
                throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");
            var s = ToArray().ToList();
            s.RemoveAt(index);
            _array = s.ToArray();
            --_tail;
            --_size;
            ++_version;
            return true;
        }

        #region Nested type: Enumerator

        /// <summary>
        /// Enumerates the elements of a <see cref="T:System.Collections.Generic.Queue`1"/>.
        /// </summary>
        [Serializable]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator {
            private readonly ArrayQueue<T> _q;
            private readonly int _version;
            private T _currentElement;
            private int _index;

            internal Enumerator(ArrayQueue<T> q) {
                _q = q;
                _version = _q._version;
                _index = -1;
                _currentElement = default (T);
            }

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            /// 
            /// <returns>
            /// The element in the <see cref="T:System.Collections.Generic.Queue`1"/> at the current position of the enumerator.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element. </exception>
            public T Current {
                get {
                    if (_index < 0) {
                        if (_index == -1) {
                            throw new InvalidOperationException();
                        }
                        else {
                            throw new InvalidOperationException();
                        }
                    }
                    return _currentElement;
                }
            }


            object IEnumerator.Current {
                get {
                    if (_index < 0) {
                        if (_index == -1) {
                            throw new InvalidOperationException();
                        }
                        else {
                            throw new InvalidOperationException();
                        }
                    }
                    return _currentElement;
                }
            }

            /// <summary>
            /// Releases all resources used by the <see cref="T:System.Collections.Generic.Queue`1.Enumerator"/>.
            /// </summary>
            public void Dispose() {
                _index = -2;
                _currentElement = default (T);
            }

            /// <summary>
            /// Advances the enumerator to the next element of the <see cref="T:System.Collections.Generic.Queue`1"/>.
            /// </summary>
            /// 
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
            public bool MoveNext() {
                if (_version != _q._version) {
                    throw new InvalidOperationException();
                }
                if (_index == -2) {
                    return false;
                }
                ++_index;
                if (_index == _q._size) {
                    _index = -2;
                    _currentElement = default (T);
                    return false;
                }
                else {
                    _currentElement = _q.GetElement(_index);
                    return true;
                }
            }


            void IEnumerator.Reset() {
                if (_version != _q._version) {
                    throw new InvalidOperationException();
                }
                _index = -1;
                _currentElement = default (T);
            }

            #endregion
        }

        #endregion
    }
}