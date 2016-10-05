using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

internal static class Tasky {
    internal static Task<T> FromResult<T>(T value)
    {
#if NET4
        var tcs = new TaskCompletionSource<T>();
        tcs.SetResult(value);
        return tcs.Task;
#else
        return Task.FromResult(value);
#endif

    }

    internal static Task<T> Run<T>(Func<T> func) {
#if NET4
        return Task.Factory.StartNew(func);
#else
        return Task.Run<T>(func);
#endif
    }


    internal static Task Run(Action func) {
#if NET4
        return Task.Factory.StartNew(func);
#else
        return Task.Run(func);
#endif
    }

#if !NET4
    internal static async Task Yield() {
        await Task.Yield();
    }
#endif

    internal static Task WhenAll(IEnumerable<Task> tasks) {
#if NET4 
        var t = tasks as Task[] ?? tasks.ToArray();
        return Task.Factory.ContinueWhenAll(t, tt=> {});
#else
        return Task.WhenAll(tasks as Task[] ?? tasks.ToArray());
#endif
    }

    internal static Task WhenAll<T>(IEnumerable<Task<T>> tasks) {
#if NET4 
        var t = tasks as Task<T>[] ?? tasks.ToArray();
        return Task.Factory.ContinueWhenAll(t, tt=> {});
#else
        return Task.WhenAll(tasks as Task<T>[] ?? tasks.ToArray());
#endif
    }

    internal static Task<Task> WhenAny(IEnumerable<Task> tasks) {
#if NET4 
        var t = tasks as Task[] ?? tasks.ToArray();
        return Task.Factory.ContinueWhenAny(t, tt=> tt);
#else
        return Task.WhenAny(tasks as Task[] ?? tasks.ToArray());
#endif
    }

    internal static Task<Task<T>> WhenAny<T>(IEnumerable<Task<T>> tasks) {
#if NET4 
        var t = tasks as Task<T>[] ?? tasks.ToArray();
        return Task.Factory.ContinueWhenAny(t, tt=> tt);
#else
        return Task.WhenAny(tasks as Task<T>[] ?? tasks.ToArray());
#endif
    }
}

