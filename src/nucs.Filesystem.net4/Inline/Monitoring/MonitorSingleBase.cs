﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using nucs.SystemCore.Boolean;

namespace nucs.Monitoring {
    internal abstract class MonitorSingleBase<T> : IMonitorSingle<T>, IDisposable {
        private readonly DynamicEqualityComparer<T> comparer;
        private readonly BoolAction<T> isblacklisted;

        /// <summary>
        ///     When an exception occurs on fetching, howmuch to sleep.
        /// </summary>
        protected int exceptionOnFetchDelayMS = 50;

        private int last_hashsum = -1;

        /// <summary>
        ///     When the @new list equals to @last, how much to sleep.
        /// </summary>
        protected int noChangeDelayMs = 200;

        private bool stopflag;


        private Thread t;

        protected MonitorSingleBase() {
            comparer = new DynamicEqualityComparer<T>(((x, y) => x.Equals(y)), x => x.GetHashCode());
        }

        protected MonitorSingleBase(DynamicEqualityComparer<T> comparer, BoolAction<T> isblacklisted) {
            this.comparer = comparer;
            this.isblacklisted = isblacklisted;
        }

        public abstract T FetchCurrent();

        public event ChangedHandler<T> Changed;

        /// <summary>
        ///     Begins the monitoring thread.
        /// </summary>
        internal virtual void Start() {
            if (t != null)
                return;
            t = new Thread(detector);
            t.Name = "Monitor";
            t.Start();
        }

        /// <summary>
        ///     Signals that the monitoring thread should be stopped.
        /// </summary>
        internal virtual void Stop() {
            stopflag = true;
        }
        [DebuggerStepThrough]
        private void detector() {
            while (true) {
                if (stopflag)
                    break;
                T @new;
                try {
                    @new = FetchCurrent();
                } catch (Exception) {
                    Thread.Sleep(exceptionOnFetchDelayMS);
                    continue;
                }

                var hashsum = @new?.GetHashCode() ?? 0;
                if (hashsum == last_hashsum) {
                    Thread.Sleep(noChangeDelayMs);
                    continue;
                }
                //not equals

                last_hashsum = hashsum;

                Changed?.Invoke(@new);
            }
            stopflag = false;
            t = null;
        }

        public static MonitorSingleBase<T> operator +(MonitorSingleBase<T> c1, ChangedHandler<T> c2) {
            c1.Changed += c2;
            return c1;
        }
        public static MonitorSingleBase<T> operator +(MonitorSingleBase<T> c1, Action<T> c2) {
            c1.Changed += item => c2(item);
            return c1;
        }
        public void Dispose() {
            this.Stop();
        }
    }
}