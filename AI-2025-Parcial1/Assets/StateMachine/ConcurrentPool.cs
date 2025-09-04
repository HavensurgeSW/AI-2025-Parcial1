using System;
using System.Collections.Concurrent;

public class ConcurrentPool
{
    private static readonly ConcurrentDictionary<Type, ConcurrentStack<IResetable>> concurrentPool =
        new ConcurrentDictionary<Type, ConcurrentStack<IResetable>>();

    public static TResetable Get<TResetable>() where TResetable : IResetable, new()
    {
        if (!concurrentPool.ContainsKey(typeof(TResetable)))
            concurrentPool.TryAdd(typeof(TResetable), new ConcurrentStack<IResetable>());

        TResetable value;
        if (concurrentPool[typeof(TResetable)].Count > 0)
        {
            concurrentPool[typeof(TResetable)].TryPop(out IResetable resetable);
            value = (TResetable)resetable;
        }
        else
        {
            value = new TResetable();
        }
        return value;
    }

    public static void Release<TResetable>(TResetable obj) where TResetable : IResetable, new()
    {
        obj.Reset();
        concurrentPool[typeof(TResetable)].Push(obj);
    }
}

