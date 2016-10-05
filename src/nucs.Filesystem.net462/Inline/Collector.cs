using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nucs.SystemCore;
using nucs.SystemCore.Boolean;

namespace nucs.Collections {
    public partial class Collector<T> : IEnumerable<T>, ICollectorAdder<T> {
        internal delegate void ItemAddedToApproveHandler(T item, int itemnumber, bool isLast, Bool Approve);
        internal delegate void ItemAddedApprovedHandler(T item, int itemnumber, bool isLast);

        internal event ItemAddedToApproveHandler ItemAddedToApprove;
        internal event ItemAddedApprovedHandler ItemAddedApproved;
        internal readonly ArrayQueue<T> Queue = new ArrayQueue<T>(); //explanations: http://www.tutorialspoint.com/csharp/csharp_queue.htm
        internal readonly object syncer = new object();
        /// <summary>
        /// Represents how many items were added regardless to their approval
        /// </summary>
        internal int Counter { get; private set; }

        /// <summary>
        /// Equivalent to <see cref="IList.Count"/>
        /// </summary>
        internal int ItemsLeft {
            get {
                {}
                return Queue.Count;
            }
        }

        internal bool IsClosed { get; private set; }

        internal object Syncronizer {get{ return syncer; }}

        internal Collector() {
            IsClosed = false;
            Counter = 0;
        }

        private readonly object lock_add = new object();
        public void Add(T item, bool isLast = false) {
            lock (lock_add) {
                if (IsClosed) throw new InvalidOperationException("Cannot add items after closing collector (declaring item added is last)");
                Counter++;
                if (isLast)
                    IsClosed = true;
                if (ItemAddedToApprove == null) goto _approved;
                var approval = (Bool) true;
                ItemAddedToApprove(item, Counter, isLast, approval);
                if (approval == false)
                    return;
                _approved:
                lock (syncer) Queue.Enqueue(item);
                if (ItemAddedApproved != null) ItemAddedApproved(item, Counter, isLast);
            }
        }

        public void AddRange(IEnumerable<T> items, bool isLast = false) {
            lock (lock_add) {
                foreach (var item in items) {
                    Add(item, false);
                }
            }
            if (isLast == true)
                IsClosed = true;
        }

        internal T TakeFirst() {
            lock (syncer) {
                return Queue.Dequeue();
            }
        }

        internal T Take(int index) {
            lock (syncer) {
                return Queue.Take(index);
            }
        }

        private Random _rand=null;
        internal T TakeRandom() {
            return TakeRandom(_rand ?? (_rand = new Random()));
        }

        internal T TakeRandom(Random rand) {
            lock (syncer) {
                return Queue.Take(rand.Next(0, Queue.Count - 1));
            }
        }

        internal T PeakFirst() {
            lock (syncer) {
                return Queue.PeekFirst();
            }
        }

        internal T Peak(int index) {
            lock (syncer) {
                return Queue.Peek(index);
            }
        }

        internal bool Remove(T item) {
            lock (syncer) {
                Queue.Remove(item);
            }
            return false;
        }

        internal void RemoveAt(int index) {
            lock (syncer) {
                Queue.RemoveAt(index);
            }
        }

        public IEnumerator<T> GetEnumerator() {
            T[] arr;
            lock (syncer)
                arr = Queue.ToArray();
            return ((IEnumerable<T>) arr).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            T[] arr;
            lock (syncer)
                arr = Queue.ToArray();
            return ((IEnumerable<T>)arr).GetEnumerator();
        }
    }

    internal class CollectorPump<T> {
        internal readonly Collector<T> Collector;
        internal delegate void ItemAddedHandler(CollectorPump<T> pump, T item, int itemnumber, bool isLast);
        internal object lock_output = new object();
        /// <summary>
        /// Number of items that went through the pump
        /// </summary>
        internal int Counter { get; private set; }
        internal event ItemAddedHandler ItemReceived;
        internal bool IsPumpOpen { get; private set; }
        private bool LastReceived = false;
        internal bool IsClosed { get; private set; }
        private readonly Collector<T>.ItemAddedApprovedHandler CollectorAction;
        internal CollectorPump(Collector<T> collector, ItemAddedHandler pumpOutput, bool openPump = false) {
            Collector = collector;
            Counter = 0;
            IsClosed = false;
            CollectorAction = (item, itemnumber, last) => Sucker();
            collector.ItemAddedApproved += (item, itemnumber, last) => LastReceived = last; //Last listener
            ItemReceived += pumpOutput;
            if (openPump) OpenPump();
        }

        /// <summary>
        /// Opens the flow to output event
        /// </summary>
        /// <returns></returns>
        internal bool OpenPump() {
            lock (lock_output) {
                if (IsPumpOpen) return false;
                Collector.ItemAddedApproved += CollectorAction;
                Sucker();
                return (IsPumpOpen = true);
            }
        }

        internal bool ClosePump() {
            lock (lock_output) {
                if (IsPumpOpen == false) return false;
                Collector.ItemAddedApproved -= CollectorAction;
                return !(IsPumpOpen = false);
            }
        }

        private void Sucker() {
            lock (lock_output) {
                T item;
                while (true) {
                    if (Collector.ItemsLeft == 0)
                        break;
                    try {
                        if ((item = Collector.TakeFirst()).Equals(null))
                            break;
                    } catch {
                        break;
                    }
                    Counter++;
                    if (LastReceived && Collector.Queue.Count == 0 && Collector.IsClosed) {
                        ItemReceived(this, item, Counter, true);
                        ClosePump();
                        IsClosed = true;
                        break;
                    }
                    ItemReceived(this, item, Counter, false);
                }
            }
        }
    }
}
