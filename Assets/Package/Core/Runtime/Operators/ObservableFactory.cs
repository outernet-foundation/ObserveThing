// using System;
// using System.Collections.Generic;

// namespace ObserveThing
// {
//     public class ObservableFactory : IObservable
//     {
//         private Func<IObserver<IOperation>, IDisposable> _subscribe;

//         public ObservableFactory(Func<IObserver<IOperation>, IDisposable> subscribe)
//         {
//             _subscribe = subscribe;
//         }

//         public IDisposable Subscribe(IObserver<IOperation> observer)
//             => _subscribe(observer);
//     }

//     public class ObservableFactory<T> : IObservable<T>
//     {
//         private Func<IObserver<T>, IDisposable> _subscribe;
//         private Queue<Operation<T>> _operationPool = new Queue<Operation<T>>();
//         private List<Operation<T>> _opList = new List<Operation<T>>();

//         public ObservableFactory(Func<IObserver<T>, IDisposable> subscribe)
//         {
//             _subscribe = subscribe;
//         }

//         public IDisposable Subscribe(IObserver<T> observer)
//             => _subscribe(observer);

//         public IDisposable Subscribe(IObserver<IOperation> observer)
//             => Subscribe(new Observer<T>(
//                 onOperation: ops =>
//                 {
//                     if (ops == null)
//                     {
//                         observer.OnOperation(null);
//                         return;
//                     }

//                     foreach (var op in ops)
//                     {
//                         if (!_operationPool.TryDequeue(out var operation))
//                             operation = new Operation<T>(this);

//                         operation.value = op;
//                         _opList.Add(operation);
//                     }

//                     observer.OnOperation(_opList);

//                     foreach (var op in _opList)
//                     {
//                         op.value = default;
//                         _operationPool.Enqueue(op);
//                     }

//                     _opList.Clear();
//                 },
//                 observer.OnError,
//                 observer.OnDispose,
//                 immediate: observer.immediate
//             ));
//     }
// }