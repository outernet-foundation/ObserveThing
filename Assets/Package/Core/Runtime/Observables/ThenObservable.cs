using System;
using System.Collections.Generic;

namespace ObserveThing
{
    // public class OnEachObservable<T> : IInitializationOperationsProvider<T> where T : Operation
    // {
    //     private IObservable<T> _source;
    //     private IObserver<T> _then;
    //     private OperationObservableOperand<T> _operand;
    //     private IDisposable _subscriptions;

    //     public OnEachObservable(IObservable<T> source, IObserver<T> then, OperationObservableOperand<T> operand)
    //     {
    //         _source = source;
    //         _then = then;
    //         _operand = operand;
    //         _subscriptions = _source.Subscribe(new Observer<T>(
    //             onNext: op =>
    //             {
    //                 _then.OnNext(op);
    //                 _operand.EnqueuePendingOperation(op);
    //             },
    //             onError: exc =>
    //             {
    //                 _then.OnError(exc);
    //                 _operand.OnError(exc);
    //             },
    //             onDispose: Dispose
    //         ));
    //     }

    //     public IReadOnlyList<T> GetInitializationOperations()
    //         => _source.GetInitializationOperations();

    //     public void Dispose()
    //     {
    //         _subscriptions?.Dispose();
    //         _subscriptions = null;

    //         _then.OnDispose();
    //         _operand.OnDisposed();
    //     }
    // }
}