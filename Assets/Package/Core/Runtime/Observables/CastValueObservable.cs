using System;

namespace ObserveThing
{
    // public class CastValueObservable<T> : IDisposable
    // {
    //     private IValueOperand<T> _operand;
    //     private IDisposable _subscription;

    //     public CastValueObservable(IObservable source, IValueOperand<T> operand)
    //     {
    //         _operand = operand;
    //         _subscription = source.Subscribe(
    //             onNext: x => operand.value = (T)x,
    //             onError: operand.OnError,
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