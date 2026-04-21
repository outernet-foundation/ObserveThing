// using System;
// using System.Collections.Generic;
// using System.Linq;

// namespace ObserveThing
// {
//     public interface IOperation
//     {
//         IObservable source { get; }
//         object value { get; }
//     }

//     public interface IOperation<T> : IOperation
//     {
//         new IObservable<T> source { get; }
//         new T value { get; }

//         IObservable IOperation.source => source;
//         object IOperation.value => value;
//     }

//     public class OperationPool
//     {
//         private class Operation<T> : IOperation<T>
//         {
//             public OperationPool sourceOperationPool { get; }
//             public IObservable<T> source { get; set; }
//             public T value { get; set; }

//             public Operation(OperationPool sourceOperationPool)
//             {
//                 this.sourceOperationPool = sourceOperationPool;
//             }
//         }

//         private class Pool<T>
//         {
//             private OperationPool sourceOperationPool;
//             private HashSet<Operation<T>> _allocatedInstances = new HashSet<Operation<T>>();
//             private HashSet<Operation<T>> _unallocatedInstances = new HashSet<Operation<T>>();

//             public Pool(OperationPool sourceOperationPool)
//             {
//                 this.sourceOperationPool = sourceOperationPool;
//             }

//             public Operation<T> Allocate(IObservable<T> source, T value)
//             {
//                 Operation<T> instance;

//                 if (_unallocatedInstances.Count > 0)
//                 {
//                     instance = _unallocatedInstances.First();
//                     _unallocatedInstances.Remove(instance);
//                 }
//                 else
//                 {
//                     instance = new Operation<T>(sourceOperationPool);
//                 }

//                 instance.source = source;
//                 instance.value = value;

//                 _allocatedInstances.Add(instance);

//                 return instance;
//             }

//             public void Deallocate(Operation<T> instance)
//             {
//                 instance.value = default;
//                 _allocatedInstances.Remove(instance);
//                 _unallocatedInstances.Add(instance);
//             }
//         }

//         public Dictionary<Type, object> _pools = new Dictionary<Type, object>();

//         public IOperation<T> Allocate<T>(IObservable<T> source, T value)
//         {
//             if (!_pools.TryGetValue(typeof(T), out var poolObject))
//             {
//                 var newPool = new Pool<T>(this);
//                 _pools.Add(typeof(T), newPool);
//                 poolObject = newPool;
//             }

//             var pool = (Pool<T>)poolObject;
//             return pool.Allocate(source, value);
//         }

//         public void Deallocate<T>(IOperation<T> operation)
//         {
//             if (!(operation is Operation<T> op) || op.sourceOperationPool != this)
//                 throw new Exception("This operation did not come from this pool.");

//             ((Pool<T>)_pools[typeof(T)]).Deallocate(op);
//         }
//     }
// }