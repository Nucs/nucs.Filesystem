using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using nucs.SystemCore;
using nucs.Collections.Extensions;
using nucs.SystemCore.Boolean;

#if NET45
using System.Threading;
using System.Threading.Tasks;
#else
using System.Threading;
#endif

namespace nucs.Collections {

    /// <summary>
    ///     A list that have item-add handling using events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerStepThrough]    
    [Serializable]
    public sealed class ImprovedList<T> : List<T> {
        internal delegate bool CompareItems(T item);
        internal delegate void ItemAddedHandler(T Item);
        internal delegate void ItemToBeAddedHandler(T Item, Bool Approval);
        internal delegate void ListAccessedHandler();
        /// <summary>
        ///     Post approval->item added to list
        /// </summary>
        internal event ItemAddedHandler ItemAdded = null;
        /// <summary>
        ///     Will ask for approval for adding the item to the list
        /// </summary>
        internal event ItemToBeAddedHandler ItemToBeAdded = null;
        /// <summary>
        ///     Will invoke when item has been accessed. Read, written, added, delete and so on.
        /// </summary>
        internal event ListAccessedHandler ListAccessed = null; 

        internal bool IsEmpty { get { return Count == 0; } }

        public ImprovedList(IEnumerable<T> source) : base(source) {}
        internal ImprovedList() : base() {}
        internal ImprovedList(int capacity) : base(capacity) {}
        internal ImprovedList(IList<T> list) {AddRange(list);}


        internal T this[long index] {
            get {
                invokeAccessed();
                return base[Convert.ToInt32(index)];
            }
            set {
                var i = Convert.ToInt32(index);
                base[i] = value;
            }
        }

        internal new void Add(T item) { //DONE
            if (ItemToBeAdded == null) {
                //No need to check approval
                base.Add(item);
                if (ItemAdded != null)
                    ItemAdded(item);
                return;
            }
            //Check for approval
            var approval = (Bool)true;
            ItemToBeAdded(item, approval);

            if (approval == false)
                return;

            base.Add(item);
            if (ItemAdded != null)
                ItemAdded(item);
            invokeAccessed();
        }
        internal new void AddRange(IEnumerable<T> range) {
            var items = range.ToArray();
            if (ItemToBeAdded == null) {
                //No need to be checked
                base.AddRange(items);
                if (ItemAdded != null)
                    Tasky.Run(() => items.ForEach(item => ItemAdded(item)));
                return;
            }

            var approved = new List<T>();
            foreach (var item in items) {
                var app = (Bool) true;
                ItemToBeAdded(item, app);
                if (app)
                    approved.Add(item);
            }
            base.AddRange(approved);
            if (ItemAdded != null)
                Tasky.Run(() => items.ForEach(item => ItemAdded(item)));
        }

        internal new void Insert(int index, T item) {
            if (ItemToBeAdded == null) {
                //No need to check approval
                base.Insert(index, item);
                if (ItemAdded != null)
                    ItemAdded(item);
                return;
            }
            //Check for approval
            var approval = (Bool)true;
            ItemToBeAdded(item, approval);

            if (approval == false)
                return;

            base.Insert(index, item);
            if (ItemAdded != null)
                ItemAdded(item);
            invokeAccessed();
        }
        internal new void InsertRange(int index, IEnumerable<T> range) {
            var items = range.ToArray();
            if (ItemToBeAdded == null) {
                //No need to be checked
                base.InsertRange(index, items);
                if (ItemAdded != null)
                    Tasky.Run(() => items.ForEach(item => ItemAdded(item)));
                return;
            }

            var approved = new List<T>();
            foreach (var item in items) {
                var app = (Bool) true;
                ItemToBeAdded(item, app);
                if (app)
                    approved.Add(item);
            }
            base.InsertRange(index, approved);
            if (ItemAdded != null)
                Tasky.Run(() => items.ForEach(item => ItemAdded(item)));
            invokeAccessed();
        }

        internal T TakeFirst() {
            return Take(0);
        }

        internal T TakeLast() {
            return Take(Count - 1);
        }

        internal T Take(int index) {
            if (IsEmpty)
                throw new InvalidOperationException("Count equals to 0, can't take what does not exist.");
            if (0 > index || index >= Count)
                throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");
            var item = this[index];
            RemoveAt(index);
            invokeAccessed();
            return item;
        }

        internal bool[] RemoveMultiple(IEnumerable<T> range) {
            return range.Select(Remove).ToArray();
        }

        /// <summary>
        ///     Waits for arrival of an item for <param name="timeout"> milliseconds. </param>
        /// </summary>
        /// <param name="comparer">The comparator for indentifying the wanted item</param>
        /// <param name="timeout">The time in milliseconds to wait. set it to -1 for infinite</param>
        /// <returns></returns>
        internal T WaitFor(CompareItems comparer, int timeout = -1) { 
            T res = default(T); //ignore default(T), it doesnt matter anyway
            var firstOrDefault = this.FirstOrDefault(t=>comparer(t));
            if (firstOrDefault != null && firstOrDefault.Equals(default(T)) == false)
                return firstOrDefault;
            //not found.. wait for it.
            var waiter = new ManualResetEventSlim(false);
           
            var itemAddedHandler = new ItemAddedHandler(item => { if (comparer(item)) {res = item; waiter.Set();} });
            ItemAdded += itemAddedHandler;
            var fod = this.FirstOrDefault(t => comparer(t));
            if (fod != null && fod.Equals(default(T)) == false) {
                ItemAdded -= itemAddedHandler;
                return fod;
            }
                
            if (timeout > -1)
                waiter.Wait(timeout);
            else 
                waiter.Wait();
            return res;

        }


        internal void ClearEventRegisterations() {
            ItemAdded = null;
            ItemToBeAdded = null;
            ListAccessed = null;
        }

        private void invokeAccessed() {
            if (ListAccessed != null)
                Tasky.Run(() => ListAccessed());
        }

        internal void Distinct() {
            var newly = this.ToArray().Distinct();
            this.Clear();
            base.AddRange(newly);
        }

        internal void Distinct(IEqualityComparer<T> comparer) {
            var newly = this.ToArray().Distinct(comparer);
            this.Clear();
            base.AddRange(newly);
        }

        #region Operators
        public static ImprovedList<T> operator +(ImprovedList<T> source, IEnumerable<T> toAdd) {
            source.AddRange(toAdd);
            return source;
        }

        public static ImprovedList<T> operator +(ImprovedList<T> source, T item) {
            source.Add(item);
            return source;
        }

        public static ImprovedList<T> operator -(ImprovedList<T> source, IEnumerable<T> ToRemove) {
            foreach (var item in ToRemove)
                source.Remove(item);
            return source;
        }

        public static ImprovedList<T> operator -(ImprovedList<T> source, T item) {
            source.Remove(item);
            return source;
        }

        #endregion

    }
}
