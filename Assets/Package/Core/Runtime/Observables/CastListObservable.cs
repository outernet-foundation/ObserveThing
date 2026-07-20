using System;

namespace ObserveThing
{
    // public class CastListObservable<T> : IDisposable
    // {
    //     private IListOperand<T> _operand;
    //     private IDisposable _subscription;

    //     public CastListObservable(IListObservable source, IListOperand<T> operand)
    //     {
    //         _operand = operand;
    //         _subscription = source.Subscribe(
    //             onAdd: (index, element) => _operand.Insert(index, (T)element),
    //             onRemove: (index, element) => _operand.RemoveAt(index),
    //             onError: _operand.OnError,
    //             onDispose: Dispose,
    //             immediate: true
    //         );
    //     }

    //     public void Dispose()
    //     {
    //         _subscription?.Dispose();
    //         _subscription = null;
    //         _operand.OnDisposed();
    //     }
    // }
}