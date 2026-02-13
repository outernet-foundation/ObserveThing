using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public enum OpType
    {
        Set,
        Add,
        Remove,
        Dispose,
        Error
    }

    public interface IObservableEventArgs
    {
        IObservable source { get; }
        OpType operationType { get; }
        Exception error { get; }
    }

    public interface IValueEventArgs<out T> : IObservableEventArgs
    {
        new IValueObservable<T> source { get; }
        T value { get; }

        IObservable IObservableEventArgs.source => source;
    }

    public interface ICollectionEventArgs<out T> : IObservableEventArgs
    {
        new ICollectionObservable<T> source { get; }
        T element { get; }

        IObservable IObservableEventArgs.source => source;
    }

    public interface IDictionaryEventArgs<TKey, TValue> : ICollectionEventArgs<KeyValuePair<TKey, TValue>>
    {
        new IDictionaryObservable<TKey, TValue> source { get; }
        ICollectionObservable<KeyValuePair<TKey, TValue>> ICollectionEventArgs<KeyValuePair<TKey, TValue>>.source => source;
    }

    public interface IListEventArgs<out T> : ICollectionEventArgs<T>
    {
        new IListObservable<T> source { get; }
        int index { get; }

        ICollectionObservable<T> ICollectionEventArgs<T>.source => source;
    }

    public struct ValueEventArgs<T> : IValueEventArgs<T>
    {
        public IValueObservable<T> source { get; set; }
        public OpType operationType { get; set; }
        public Exception error { get; set; }
        public T value { get; set; }
    }

    public struct CollectionEventArgs<T> : ICollectionEventArgs<T>
    {
        public ICollectionObservable<T> source { get; set; }
        public OpType operationType { get; set; }
        public Exception error { get; set; }
        public T element { get; set; }
    }

    public struct DictionaryEventArgs<TKey, TValue> : IDictionaryEventArgs<TKey, TValue>
    {
        public IDictionaryObservable<TKey, TValue> source { get; set; }
        public OpType operationType { get; set; }
        public Exception error { get; set; }
        public KeyValuePair<TKey, TValue> element { get; set; }
    }

    public struct ListEventArgs<T> : IListEventArgs<T>
    {
        public IListObservable<T> source { get; set; }
        public OpType operationType { get; set; }
        public Exception error { get; set; }
        public T element { get; set; }
        public int index { get; set; }
    }

    public interface IObservable
    {
        IDisposable Subscribe(ObserverDelegate<IObservableEventArgs> observer);
    }

    public interface IValueObservable<out T> : IObservable
    {
        IDisposable Subscribe(ObserverDelegate<IValueEventArgs<T>> observer);

        IDisposable IObservable.Subscribe(ObserverDelegate<IObservableEventArgs> observer)
            => Subscribe(observer);
    }

    public interface ICollectionObservable<out T> : IObservable
    {
        IDisposable Subscribe(ObserverDelegate<ICollectionEventArgs<T>> observer);

        IDisposable IObservable.Subscribe(ObserverDelegate<IObservableEventArgs> observer)
            => Subscribe(observer);
    }

    public interface IDictionaryObservable<TKey, TValue> : ICollectionObservable<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(ObserverDelegate<IDictionaryEventArgs<TKey, TValue>> observer);

        IDisposable ICollectionObservable<KeyValuePair<TKey, TValue>>.Subscribe(ObserverDelegate<ICollectionEventArgs<KeyValuePair<TKey, TValue>>> observer)
            => Subscribe(observer);
    }

    public interface IListObservable<out T> : ICollectionObservable<T>
    {
        IDisposable Subscribe(ObserverDelegate<IListEventArgs<T>> observer);

        IDisposable ICollectionObservable<T>.Subscribe(ObserverDelegate<ICollectionEventArgs<T>> observer)
            => Subscribe(observer);
    }

    public delegate void ObserverDelegate<in T>(T args);

    public static class Observables
    {
        public static IValueObservable<TResult> Combine<T1, T2, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, Func<T1, T2, TResult> combine)
            => v1.SelectDynamic(x1 => v2.SelectDynamic(x2 => combine(x1, x2)));

        public static IValueObservable<TResult> Combine<T1, T2, T3, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, IValueObservable<T3> v3, Func<T1, T2, T3, TResult> combine)
            => v1.SelectDynamic(x1 => v2.SelectDynamic(x2 => v3.SelectDynamic(x3 => combine(x1, x2, x3))));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, IValueObservable<T3> v3, IValueObservable<T4> v4, Func<T1, T2, T3, T4, TResult> combine)
            => v1.SelectDynamic(x1 => v2.SelectDynamic(x2 => v3.SelectDynamic(x3 => v4.SelectDynamic(x4 => combine(x1, x2, x3, x4)))));

        public static IObservable Any(params IObservable[] observables)
            => new AnyObservable(observables);

        public static IValueObservable<T> AsValueObservable<T>(T value)
            => new ValueObservable<T>(value);

        public static ICollectionObservable<T> AsCollectionObservable<T>(params T[] values)
            => new CollectionObservable<T>(values);

        public static ICollectionObservable<T> AsCollectionObservable<T>(IEnumerable<T> values)
            => new CollectionObservable<T>(values);

        public static IListObservable<T> AsListObservable<T>(params T[] values)
            => new ListObservable<T>(values);

        public static IListObservable<T> AsListObservable<T>(IEnumerable<T> values)
            => new ListObservable<T>(values);
    }

    public class ValueOperator<T> : IValueObservable<T>
    {
        public event Action onFirstObserverSubscribed;
        public event Action onLastObserverDisposed;

        private T _mostRecentValue = default;
        private bool _notifyingObservers = false;
        private List<ObserverData> _observers = new List<ObserverData>();
        private List<ObserverData> _disposedObservers = new List<ObserverData>();

        private class ObserverData : IDisposable
        {
            public bool disposed;
            public ObserverDelegate<IValueEventArgs<T>> method;
            public event Action<ObserverData> onDisposed;

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                onDisposed?.Invoke(this);
            }
        }

        public IDisposable Subscribe(ObserverDelegate<IValueEventArgs<T>> observer)
        {
            var observerData = new ObserverData() { method = observer };
            observerData.onDisposed += HandleObserverDisposed;
            _observers.Add(observerData);

            if (_observers.Count == 1)
                onFirstObserverSubscribed?.Invoke();

            observerData.method.Invoke(new ValueEventArgs<T>()
            {
                source = this,
                operationType = OpType.Set,
                value = _mostRecentValue
            });

            return observerData;
        }

        private void HandleObserverDisposed(ObserverData observer)
        {
            bool observerRemoved = false;

            if (_notifyingObservers)
            {
                _disposedObservers.Add(observer);
            }
            else
            {
                observerRemoved = _observers.Remove(observer);
            }

            observer.method(new ValueEventArgs<T>()
            {
                source = this,
                operationType = OpType.Dispose
            });

            if (observerRemoved && _observers.Count == 0)
                onLastObserverDisposed?.Invoke();
        }

        public void Emit(IValueEventArgs<T> args)
        {
            if (_observers.Count == 0 || args.operationType == OpType.Set && Equals(args.value, _mostRecentValue))
                return;

            bool wasNotifyingObservers = _notifyingObservers;
            _notifyingObservers = true;

            int observerCount = _observers.Count;

            for (int i = 0; i < observerCount; i++)
            {
                var observer = _observers[i];

                if (observer.disposed)
                    continue;

                try
                {
                    observer.method(args);
                }
                catch (Exception exc)
                {
                    observer.method(new ValueEventArgs<T>()
                    {
                        source = this,
                        operationType = OpType.Error,
                        error = exc
                    });
                }
            }

            foreach (var disposed in _disposedObservers)
                _observers.Remove(disposed);

            _disposedObservers.Clear();

            _notifyingObservers = wasNotifyingObservers;

            if (_observers.Count == 0)
                onLastObserverDisposed?.Invoke();
        }
    }

    public static class ObservableExtensions
    {
        // public static IValueObservable<T> ShallowCopyDynamic<T>(this IValueObservable<IValueObservable<T>> source)
        //     => new ShallowCopyValueObservable<T>(source);

        // public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, U> select)
        //     => new SelectValueObservable<T, U>(source, select);

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, U> select)
        {
            var opt = new ValueOperator<U>();
            IDisposable subscription = default;

            ObserverDelegate<IValueEventArgs<T>> observer = args =>
            {
                opt.Emit(new ValueEventArgs<U>()
                {
                    source = opt,
                    operationType = args.operationType,
                    error = args.error,
                    value = args.operationType == OpType.Set ? select(args.value) : default,
                });

                if (args.operationType == OpType.Dispose)
                    subscription.Dispose();
            };

            opt.onFirstObserverSubscribed += () => subscription = source.Subscribe(observer);
            opt.onLastObserverDisposed += () => subscription?.Dispose();

            return opt;
        }

        public static IValueObservable<T> ShallowCopyDynamic<T>(this IValueObservable<IValueObservable<T>> source)
        {
            var opt = new ValueOperator<T>();

            IDisposable nestedSubscription = default;
            IDisposable subscription = default;

            ObserverDelegate<IValueEventArgs<T>> nestedObserver = args => opt.Emit(new ValueEventArgs<T>()
            {
                source = opt,
                operationType = args.operationType,
                error = args.error,
                value = args.value
            });

            ObserverDelegate<IValueEventArgs<IValueObservable<T>>> observer = args =>
            {
                if (args.operationType == OpType.Set)
                {
                    nestedSubscription?.Dispose();
                    nestedSubscription = args.value?.Subscribe(nestedObserver);
                }
                else if (args.operationType == OpType.Dispose)
                {
                    nestedSubscription?.Dispose();
                    subscription?.Dispose();

                    opt.Emit(new ValueEventArgs<T>()
                    {
                        source = opt,
                        operationType = OpType.Dispose
                    });
                }
                else if (args.operationType == OpType.Error)
                {
                    opt.Emit(new ValueEventArgs<T>()
                    {
                        source = opt,
                        operationType = OpType.Error,
                        error = args.error
                    });
                }
            };

            opt.onFirstObserverSubscribed += () => subscription = source.Subscribe(observer);
            opt.onLastObserverDisposed += () => subscription?.Dispose();

            return opt;
        }

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static ICollectionObservable<T> ShallowCopyDynamic<T>(this ICollectionObservable<IValueObservable<T>> source)
            => new ShallowCopyCollectionObservable<T>(source);

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new SelectCollectionObservable<T, U>(source, select);

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.WhereDynamic(x => Observables.AsValueObservable(where(x)));

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new WhereCollectionObservable<T>(source, where);

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ConcatDynamic(Observables.AsCollectionObservable(source2));

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new ConcatCollectionObservable<T>(source1, source2);

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.SelectManyDynamic(x => Observables.AsCollectionObservable(selectMany(x)));

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new SelectManyCollectionObservable<T, U>(source, selectMany);

        public static ICollectionObservable<T> DistinctDynamic<T>(this ICollectionObservable<T> source)
            => new DistinctCollectionObservable<T>(source);

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, TKey> selectKey, Func<TSource, TValue> selectValue)
            => source.ToDictionaryDynamic(x => Observables.AsValueObservable(selectKey(x)), x => Observables.AsValueObservable(selectValue(x)));

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, TKey> selectKey, Func<TSource, IValueObservable<TValue>> selectValue)
            => source.ToDictionaryDynamic(x => Observables.AsValueObservable(selectKey(x)), selectValue);

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, IValueObservable<TKey>> selectKey, Func<TSource, TValue> selectValue)
            => source.ToDictionaryDynamic(selectKey, x => Observables.AsValueObservable(selectValue(x)));

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, IValueObservable<TKey>> selectKey, Func<TSource, IValueObservable<TValue>> selectValue)
            => new ToDictionaryObservable<TSource, TKey, TValue>(source, selectKey, selectValue);

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.OrderByDynamic(x => Observables.AsValueObservable(orderBy(x)));

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new OrderByCollectionObservable<T, U>(source, orderBy);

        public static IValueObservable<int> CountDynamic<T>(this ICollectionObservable<T> source)
            => new CountCollectionObservable<T>(source);

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, T contains)
            => source.ContainsDynamic(Observables.AsValueObservable(contains));

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new ContainsCollectionObservable<T>(source, contains);

        public static IValueObservable<(T1 value1, T2 value2)> WithDynamic<T1, T2>(this IValueObservable<T1> source1, IValueObservable<T2> source2)
            => new WithValueObservable<T1, T2>(source1, source2);

        public static IValueObservable<(bool keyPresent, TValue value)> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => source.TrackDynamic(Observables.AsValueObservable(key));

        public static IValueObservable<(bool keyPresent, TValue value)> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
            => new TrackValueObservable<TKey, TValue>(source, key);

        public static ICollectionObservable<TValue> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.SelectDynamic(x => source.TrackDynamic(x)).WhereDynamic(x => x.keyPresent).SelectDynamic(x => x.value);

        public static IListObservable<T> ShallowCopyDynamic<T>(this IListObservable<IValueObservable<T>> source)
            => new ShallowCopyListObservable<T>(source);

        public static IListObservable<U> SelectDynamic<T, U>(this IListObservable<T> source, Func<T, U> select)
            => new SelectListObservable<T, U>(source, select);

        public static IListObservable<U> SelectDynamic<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static IValueObservable<int> IndexOfDynamic<T>(this IListObservable<T> source, T value)
            => new IndexOfObservable<T>(source, value);

        public static IDisposable Subscribe(this IObservable source, Action<IObservableEventArgs> observer = default, Action<Exception> onError = default, Action onDispose = default, string name = default)
            => source.Subscribe(new Observer<IObservableEventArgs>(observer, onError, onDispose, name));

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<IValueEventArgs<T>> observer = default, Action<Exception> onError = default, Action onDispose = default, string name = default)
            => source.Subscribe(new Observer<IValueEventArgs<T>>(observer, onError, onDispose, name));

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<ICollectionEventArgs<T>> observer = default, Action<Exception> onError = default, Action onDispose = default, string name = default)
            => source.Subscribe(new Observer<ICollectionEventArgs<T>>(observer, onError, onDispose, name));

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<IListEventArgs<T>> observer = default, Action<Exception> onError = default, Action onDispose = default, string name = default)
            => source.Subscribe(new Observer<IListEventArgs<T>>(observer, onError, onDispose, name));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<IDictionaryEventArgs<TKey, TValue>> observer = default, Action<Exception> onError = default, Action onDispose = default, string name = default)
            => source.Subscribe(new Observer<IDictionaryEventArgs<TKey, TValue>>(observer, onError, onDispose, name));

        public static IValueObservable<T> AsObservable<T>(this IValueObservable<T> observable)
            => observable;

        public static ICollectionObservable<T> AsObservable<T>(this ICollectionObservable<T> observable)
            => observable;

        public static IListObservable<T> AsObservable<T>(this IListObservable<T> observable)
            => observable;

        public static IDictionaryObservable<TKey, TValue> AsObservable<TKey, TValue>(this IDictionaryObservable<TKey, TValue> observable)
            => observable;

        public static T Peek<T>(this IValueObservable<T> source)
        {
            T result = default;
            source.Subscribe(x => result = x.currentValue).Dispose();
            return result;
        }
    }

    public class Disposable : IDisposable
    {
        private bool _disposed;
        private Action _onDispose;

        public Disposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _onDispose?.Invoke();
        }
    }

    public class ComposedDisposable : IDisposable
    {
        private bool _disposed;
        private IDisposable[] _disposables;

        public ComposedDisposable(params IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            foreach (var disposable in _disposables)
                disposable?.Dispose();
        }
    }
}