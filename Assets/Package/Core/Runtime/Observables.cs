using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObservable
    {
        IDisposable Subscribe(IObserver observer);
    }

    public interface IValueObservable<out T> : IObservable
    {
        IDisposable Subscribe(IValueObserver<T> observer);
    }

    public interface ICollectionObservable<out T> : IObservable
    {
        IDisposable Subscribe(ICollectionObserver<T> observer);
    }

    public interface IDictionaryObservable<TKey, TValue> : ICollectionObservable<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer);
    }

    public interface IListObservable<out T> : ICollectionObservable<T>
    {
        IDisposable Subscribe(IListObserver<T> observer);
    }

    public interface IObserver
    {
        void OnChange();
        void OnError(Exception error);
        void OnDispose();
    }

    public class Observer : IObserver
    {
        private Action _onChange;
        private Action<Exception> _onError;
        private Action _onDispose;

        public Observer(Action onChange = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onChange = onChange;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnChange() => _onChange?.Invoke();
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IValueObserver<in T>
    {
        void OnNext(T value);
        void OnError(Exception error);
        void OnDispose();
    }

    public class ValueObserver<T> : IValueObserver<T>
    {
        private Action<T> _onNext;
        private Action<Exception> _onError;
        private Action _onDispose;

        public ValueObserver(Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onNext = onNext;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnNext(T value) => _onNext?.Invoke(value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface ICollectionObserver<in T>
    {
        public void OnAdd(T value);
        public void OnRemove(T value);
        public void OnError(Exception error);
        public void OnDispose();
    }

    public class CollectionObserver<T> : ICollectionObserver<T>
    {
        private Action<T> _onAdd;
        private Action<T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public CollectionObserver(Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnAdd(T value) => _onAdd?.Invoke(value);
        public void OnRemove(T value) => _onRemove?.Invoke(value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IListObserver<in T>
    {
        public void OnAdd(int index, T value);
        public void OnRemove(int index, T value);
        public void OnError(Exception error);
        public void OnDispose();
    }

    public class ListObserver<T> : IListObserver<T>
    {
        private Action<int, T> _onAdd;
        private Action<int, T> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public ListObserver(Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnAdd(int index, T value) => _onAdd?.Invoke(index, value);
        public void OnRemove(int index, T value) => _onRemove?.Invoke(index, value);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public interface IDictionaryObserver<TKey, TValue>
    {
        public void OnAdd(KeyValuePair<TKey, TValue> keyValuePair);
        public void OnRemove(KeyValuePair<TKey, TValue> keyValuePair);
        public void OnError(Exception error);
        public void OnDispose();
    }

    public class DictionaryObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
    {
        private Action<KeyValuePair<TKey, TValue>> _onAdd;
        private Action<KeyValuePair<TKey, TValue>> _onRemove;
        private Action<Exception> _onError;
        private Action _onDispose;

        public DictionaryObserver(Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
        {
            _onAdd = onAdd;
            _onRemove = onRemove;
            _onError = onError;
            _onDispose = onDispose;
        }

        public void OnAdd(KeyValuePair<TKey, TValue> keyValuePair) => _onAdd?.Invoke(keyValuePair);
        public void OnRemove(KeyValuePair<TKey, TValue> keyValuePair) => _onRemove?.Invoke(keyValuePair);
        public void OnError(Exception error) => _onError?.Invoke(error);
        public void OnDispose() => _onDispose?.Invoke();
    }

    public class FactoryValueObservable<T> : IValueObservable<T>
    {
        private Func<IValueObserver<T>, IDisposable> _subscribe;

        public FactoryValueObservable(Func<IValueObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IValueObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ValueObserver<T>(
                onNext: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }

    public class FactoryCollectionObservable<T> : ICollectionObservable<T>
    {
        private Func<ICollectionObserver<T>, IDisposable> _subscribe;

        public FactoryCollectionObservable(Func<ICollectionObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new CollectionObserver<T>(
                onAdd: _ => observer.OnChange(),
                onRemove: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }

    public class FactoryListObservable<T> : IListObservable<T>
    {
        private Func<IListObserver<T>, IDisposable> _subscribe;

        public FactoryListObservable(Func<IListObserver<T>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IListObserver<T> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(IObserver observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, _) => observer.OnChange(),
                onRemove: (_, _) => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>(
                onAdd: (_, x) => observer.OnAdd(x),
                onRemove: (_, x) => observer.OnRemove(x),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }

    public class FactoryDictionaryObservable<TKey, TValue> : IDictionaryObservable<TKey, TValue>
    {
        private Func<IDictionaryObserver<TKey, TValue>, IDisposable> _subscribe;

        public FactoryDictionaryObservable(Func<IDictionaryObserver<TKey, TValue>, IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer)
            => _subscribe(observer);

        public IDisposable Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer)
            => _subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: observer.OnAdd,
                onRemove: observer.OnRemove,
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));

        public IDisposable Subscribe(IObserver observer)
            => _subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: _ => observer.OnChange(),
                onRemove: _ => observer.OnChange(),
                onError: observer.OnError,
                onDispose: observer.OnDispose
            ));
    }

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
    }

    public static class ObservableExtensions
    {
        public static IValueObservable<T> ShallowCopyDynamic<T>(this IValueObservable<IValueObservable<T>> source)
            => new FactoryValueObservable<T>(receiver => new ValueShallowCopyDynamic<T>(source, receiver));

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, U> select)
            => new FactoryValueObservable<U>(receiver => new ValueSelectDynamic<T, U>(source, select, receiver));

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static ICollectionObservable<T> ShallowCopyDynamic<T>(this ICollectionObservable<IValueObservable<T>> source)
            => new FactoryCollectionObservable<T>(receiver => new CollectionShallowCopyDynamic<T>(source, receiver));

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new FactoryCollectionObservable<U>(receiver => new CollectionSelectDynamic<T, U>(source, select, receiver));

        public static ICollectionObservable<T> DistinctDynamic<T>(this ICollectionObservable<T> source)
            => new FactoryCollectionObservable<T>(receiver => new DistinctDynamic<T>(source, receiver));

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.WhereDynamic(x => new ValueObservable<bool>(where(x)));

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new FactoryCollectionObservable<T>(receiver => new WhereDynamic<T>(source, where, receiver));

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ConcatDynamic((ICollectionObservable<T>)new CollectionObservable<T>(source2));

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new FactoryCollectionObservable<T>(receiver => new ConcatDynamic<T>(source1, source1, receiver));

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.SelectManyDynamic(x => (ICollectionObservable<U>)new CollectionObservable<U>(selectMany(x)));

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new FactoryCollectionObservable<U>(receiver => new SelectManyDynamic<T, U>(source, selectMany, receiver));

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, TKey> selectKey, Func<TSource, TValue> selectValue)
            => source.ToDictionaryDynamic<TSource, TKey, TValue>(x => new ValueObservable<TKey>(selectKey(x)), x => new ValueObservable<TValue>(selectValue(x)));

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, TKey> selectKey, Func<TSource, IValueObservable<TValue>> selectValue)
            => source.ToDictionaryDynamic<TSource, TKey, TValue>(x => new ValueObservable<TKey>(selectKey(x)), selectValue);

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, IValueObservable<TKey>> selectKey, Func<TSource, TValue> selectValue)
            => source.ToDictionaryDynamic<TSource, TKey, TValue>(selectKey, x => new ValueObservable<TValue>(selectValue(x)));

        public static IDictionaryObservable<TKey, TValue> ToDictionaryDynamic<TSource, TKey, TValue>(this ICollectionObservable<TSource> source, Func<TSource, IValueObservable<TKey>> selectKey, Func<TSource, IValueObservable<TValue>> selectValue)
        {

        }

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.OrderByDynamic<T, U>(x => new ValueObservable<U>(orderBy(x)));

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new FactoryListObservable<T>(receiver => new OrderByDynamic<T, U>(source, orderBy, receiver));

        public static IValueObservable<int> CountDynamic<T>(this ICollectionObservable<T> source)
            => new FactoryValueObservable<int>(receiver => new CountDynamic<T>(source, receiver));

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, T contains)
            => source.ContainsDynamic(new ValueObservable<T>(contains));

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new FactoryValueObservable<bool>(receiver => new ContainsDynamic<T>(source, contains, receiver));

        public static IValueObservable<(T1 value1, T2 value2)> WithDynamic<T1, T2>(this IValueObservable<T1> source1, IValueObservable<T2> source2)
            => new WithValueObservable<T1, T2>(source1, source2);

        public static IValueObservable<(bool keyPresent, TValue value)> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => source.TrackDynamic(new ValueObservable<TKey>(key));

        public static IValueObservable<(bool keyPresent, TValue value)> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
        {

        }

        public static ICollectionObservable<TValue> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.SelectDynamic(x => source.TrackDynamic(x)).WhereDynamic(x => x.keyPresent).SelectDynamic(x => x.value);

        public static IValueObservable<(T current, T previous)> WithPrevious<T>(this IValueObservable<T> source)
        {
            T previous = default;

            return new FactoryValueObservable<(T current, T previous)>(receiver => source.Subscribe(
                onNext: x =>
                {
                    receiver.OnNext(new(x, previous));
                    previous = x;
                },
                onError: receiver.OnError,
                onDispose: receiver.OnDispose
            ));
        }

        public static IListObservable<T> ShallowCopyDynamic<T>(this IListObservable<IValueObservable<T>> source)
        {
            var data = new List<(IDisposable subscription, T latest)>();

            return new FactoryListObservable<T>(receiver => source.Subscribe(
                onAdd: (index, x) =>
                {
                    var subscription = default(IDisposable);
                    subscription = x.Subscribe(
                        onNext: x =>
                        {
                            var index = data.FindIndex(x => x.subscription == subscription);
                            var element = data[index];

                            data.RemoveAt(index);
                            receiver.OnRemove(index, element.latest);

                            element = new(element.subscription, x);
                            data.Insert(index, element);

                            receiver.OnAdd(index, x);
                        },
                        onError: receiver.OnError,
                        onDispose: receiver.OnDispose
                    );
                },
                onRemove: (index, x) =>
                {
                    var element = data[index];
                    data.RemoveAt(index);
                    receiver.OnRemove(index, data[index].latest);
                },
                onError: receiver.OnError,
                onDispose: receiver.OnDispose
            ));
        }

        public static IListObservable<U> SelectDynamic<T, U>(this IListObservable<T> source, Func<T, U> select)
            => new FactoryListObservable<U>(receiver => new ListSelectDynamic<T, U>(source, select, receiver));

        public static IListObservable<U> SelectDynamic<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static IValueObservable<int> IndexOfDynamic<T>(this IListObservable<T> source, T value)
            => source.IndexOfDynamic(new ValueObservable<T>(value));

        public static IValueObservable<int> IndexOfDynamic<T>(this IListObservable<T> source, IValueObservable<T> value)
            => new FactoryValueObservable<int>(receiver => new IndexOfDynamic<T>(source, value, receiver));

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
            source.Subscribe(x => result = x).Dispose();
            return result;
        }

        public static IDisposable Subscribe<T>(this IObservable source, Action onChange = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer(onChange: onChange, onError: onError, onDispose: onDispose));

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ValueObserver<T>(onNext: onNext, onError: onError, onDispose: onDispose));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(onAdd: onAdd, onRemove: onRemove, onError: onError, onDispose: onDispose));

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ListObserver<T>(onAdd: onAdd, onRemove: onRemove, onError: onError, onDispose: onDispose));

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new CollectionObserver<T>(onAdd: onAdd, onRemove: onRemove, onError: onError, onDispose: onDispose));
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