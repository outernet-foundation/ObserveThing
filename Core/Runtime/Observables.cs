using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public class ObservableEventArgs
    {
        public IObservable source { get; set; }
    }

    public interface IObservable
    {
        IDisposable Subscribe(IObserver<ObservableEventArgs> observer);
    }

    public interface IValueObservable<T> : IObservable
    {
        IDisposable Subscribe(IObserver<ValueEventArgs<T>> observer);
        IDisposable IObservable.Subscribe(IObserver<ObservableEventArgs> observer)
            => Subscribe(observer);
    }

    public class ValueEventArgs<T> : ObservableEventArgs
    {
        public T currentValue { get; set; }
        public T previousValue { get; set; }
    }

    public enum OpType
    {
        Add,
        Remove
    }

    public interface ICollectionObservable<T> : IObservable
    {
        IDisposable Subscribe(IObserver<CollectionEventArgs<T>> observer);
        IDisposable IObservable.Subscribe(IObserver<ObservableEventArgs> observer)
            => Subscribe(observer);
    }

    public class CollectionEventArgs<T> : ObservableEventArgs
    {
        public T element { get; set; }
        public OpType operationType { get; set; }
    }

    public interface IDictionaryObservable<TKey, TValue> : ICollectionObservable<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(IObserver<DictionaryEventArgs<TKey, TValue>> observer);
        IDisposable ICollectionObservable<KeyValuePair<TKey, TValue>>.Subscribe(IObserver<CollectionEventArgs<KeyValuePair<TKey, TValue>>> observer)
            => Subscribe(observer);
    }

    public class DictionaryEventArgs<TKey, TValue> : CollectionEventArgs<KeyValuePair<TKey, TValue>>
    {
        public TKey key => element.Key;
        public TValue value => element.Value;
    }

    public interface IListObservable<T> : ICollectionObservable<T>
    {
        IDisposable Subscribe(IObserver<ListEventArgs<T>> observer);
        IDisposable ICollectionObservable<T>.Subscribe(IObserver<CollectionEventArgs<T>> observer)
            => Subscribe(observer);
    }

    public class ListEventArgs<T> : CollectionEventArgs<T>
    {
        public int index { get; set; }
    }

    public static class Observables
    {
        public static IValueObservable<TResult> Combine<T1, T2, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, Func<T1, T2, TResult> combine)
            => v1.SelectDynamic(x1 => v2.SelectDynamic(x2 => combine(x1, x2)));

        public static IValueObservable<TResult> Combine<T1, T2, T3, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, IValueObservable<T3> v3, Func<T1, T2, T3, TResult> combine)
            => v1.SelectDynamic(x1 => v2.SelectDynamic(x2 => v3.SelectDynamic(x3 => combine(x1, x2, x3))));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, IValueObservable<T3> v3, IValueObservable<T4> v4, Func<T1, T2, T3, T4, TResult> combine)
            => v1.SelectDynamic(x1 => v2.SelectDynamic(x2 => v3.SelectDynamic(x3 => v4.SelectDynamic(x4 => combine(x1, x2, x3, x4)))));

        public static IObservable All(params IObservable[] observables)
            => new AllObservable(observables);
    }

    public static class ObservableExtensions
    {
        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, U> select)
            => new SelectValueObservable<T, U>(source, select);

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => new SelectValueObservableReactive<T, U>(source, select);

        public static IValueObservable<U> CreateDynamic<T, U>(this IValueObservable<T> source, Func<T, U> select)
            where U : IDisposable => new CreateValueObservable<T, U>(source, select);

        public static IValueObservable<U> CreateDynamic<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            where U : IDisposable => new CreateValueObservableReactive<T, U>(source, select);

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new SelectCollectionObservable<T, U>(source, select);

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => new SelectCollectionObservableReactive<T, U>(source, select);

        public static ICollectionObservable<U> CreateDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            where U : IDisposable => new CreateCollectionObservable<T, U>(source, select);

        public static ICollectionObservable<U> CreateDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            where U : IDisposable => new CreateCollectionObservableReactive<T, U>(source, select);

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => new WhereCollectionObservable<T>(source, where);

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new WhereCollectionObservableReactive<T>(source, where);

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => new ConcatCollectionObservable<T>(source1, source2);

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new ConcatCollectionObservableReactive<T>(source1, source2);

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => new SelectManyCollectionObservable<T, U>(source, selectMany);

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new SelectManyCollectionObservableReactive<T, U>(source, selectMany);

        public static ICollectionObservable<T> DistinctDynamic<T>(this ICollectionObservable<T> source)
            => new DistinctCollectionObservable<T>(source);

        public static IValueObservable<int> CountDynamic<T>(this ICollectionObservable<T> source)
            => new CountCollectionObservable<T>(source);

        public static IValueObservable<(T1 value1, T2 value2)> WithDynamic<T1, T2>(this IValueObservable<T1> source1, IValueObservable<T2> source2)
            => new WithValueObservable<T1, T2>(source1, source2);

        public static IValueObservable<(bool keyPresent, TValue value)> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => new TrackValueObservable<TKey, TValue>(source, key);

        public static IValueObservable<(bool keyPresent, TValue value)> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
            => new TrackValueObservableReactive<TKey, TValue>(source, key);

        public static ICollectionObservable<TValue> TrackDynamic<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.SelectDynamic(x => source.TrackDynamic(x)).WhereDynamic(x => x.keyPresent).SelectDynamic(x => x.value);

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => new OrderByCollectionObservable<T, U>(source, orderBy);

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new OrderByCollectionObservableReactive<T, U>(source, orderBy);

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, T contains)
            => new ContainsCollectionObservable<T>(source, contains);

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new ContainsCollectionObservableReactive<T>(source, contains);

        public static IValueObservable<int> IndexOfDynamic<T>(this IListObservable<T> source, T value)
            => new IndexOfObservable<T>(source, value);

        public static IDisposable Subscribe(this IObservable source, Action<ObservableEventArgs> observer = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer<ObservableEventArgs>() { onNext = observer, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<ValueEventArgs<T>> observer = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer<ValueEventArgs<T>>() { onNext = observer, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<CollectionEventArgs<T>> observer = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer<CollectionEventArgs<T>>() { onNext = observer, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<ListEventArgs<T>> observer = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer<ListEventArgs<T>>() { onNext = observer, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<DictionaryEventArgs<TKey, TValue>> observer = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer<DictionaryEventArgs<TKey, TValue>>() { onNext = observer, onError = onError, onDispose = onDispose });

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