using System;
using System.Collections.Generic;

namespace ObserveThing
{
    // public interface IOperand
    // {
    //     void OnError(Exception error);
    //     void OnDisposed();
    // }

    // public interface ObservableValue<T> : IOperand
    // {
    //     T value { get; set; }
    // }

    // public interface ObservableCollection<T> : IOperand
    // {
    //     void Add(uint id, T element);
    //     void Remove(uint id);
    //     void Clear();
    // }

    // public interface ObservableDictionary<TKey, TValue> : IOperand
    // {
    //     void Add(TKey key, TValue value);
    //     void Remove(TKey key);
    //     void Clear();
    // }

    // public interface ObservableSet<T> : IOperand
    // {
    //     void Add(T value);
    //     void Remove(T value);
    //     void Clear();
    // }

    // public interface Observable<T> : IOperand where T : IOperation
    // {
    //     void EnqueuePendingOperation(T operation);
    // }

    // public interface IBatchOperand<T> : IOperand where T : IOperation
    // {
    //     void EnqueuePendingOperation(IReadOnlyList<T> operation);
    // }

    public interface IInitializationOperationsProvider<T> : IDisposable where T : IOperation
    {
        IReadOnlyList<T> GetInitializationOperations();
    }
}
