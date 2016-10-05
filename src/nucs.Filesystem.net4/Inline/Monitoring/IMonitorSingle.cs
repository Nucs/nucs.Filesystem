using System.Collections.Generic;

namespace nucs.Monitoring {
    /// <summary>
    ///     When Item changes, it is passed through this delegate
    /// </summary>
    internal delegate void ChangedHandler<in T>(T item);

    /// <summary>
    ///     Monitors a single value that changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IMonitorSingle<out T> {

        /// <summary>
        ///     Core function to this pattern - fetches the items that are monitored to change.
        /// </summary>
        T FetchCurrent();
        /// <summary>
        ///     When FetchCurrent changes, this is invoked.
        /// </summary>
        event ChangedHandler<T> Changed;
    }
}