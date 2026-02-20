using System;
using System.Linq;
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

    public static class Observables
    {
        public static IValueObservable<TResult> Combine<T1, T2, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, Func<T1, T2, TResult> combine)
            => v1.ObservableSelect(x1 => v2.ObservableSelect(x2 => combine(x1, x2)));

        public static IValueObservable<TResult> Combine<T1, T2, T3, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, IValueObservable<T3> v3, Func<T1, T2, T3, TResult> combine)
            => v1.ObservableSelect(x1 => v2.ObservableSelect(x2 => v3.ObservableSelect(x3 => combine(x1, x2, x3))));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, TResult>(IValueObservable<T1> v1, IValueObservable<T2> v2, IValueObservable<T3> v3, IValueObservable<T4> v4, Func<T1, T2, T3, T4, TResult> combine)
            => v1.ObservableSelect(x1 => v2.ObservableSelect(x2 => v3.ObservableSelect(x3 => v4.ObservableSelect(x4 => combine(x1, x2, x3, x4)))));

        public static IObservable Any(params IObservable[] observables)
            => new FactoryObservable(receiver => new AnyDynamic(observables, receiver));
    }

    public static class ObservableExtensions
    {
        public static IValueObservable<T> ObservableShallowCopy<T>(this IValueObservable<IValueObservable<T>> source)
            => new FactoryValueObservable<T>(receiver => new ValueShallowCopyDynamic<T>(source, receiver));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, U> select)
            => new FactoryValueObservable<U>(receiver => new ValueSelectDynamic<T, U>(source, select, receiver));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static IValueObservable<(T current, T previous)> ObservableWithPrevious<T>(this IValueObservable<T> source)
            => new FactoryValueObservable<(T current, T previous)>(receiver => new WithPreviousDynamic<T>(source, receiver));

        public static ICollectionObservable<T> ObservableShallowCopy<T>(this ICollectionObservable<IValueObservable<T>> source)
            => new FactoryCollectionObservable<T>(receiver => new CollectionShallowCopyDynamic<T>(source, receiver));

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new FactoryCollectionObservable<U>(receiver => new CollectionSelectDynamic<T, U>(source, select, receiver));

        public static ICollectionObservable<T> ObservableDistinct<T>(this ICollectionObservable<T> source)
            => new FactoryCollectionObservable<T>(receiver => new DistinctDynamic<T>(source, receiver));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ValueObservable<bool>(where(x)));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new FactoryCollectionObservable<T>(receiver => new WhereDynamic<T>(source, where, receiver));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((ICollectionObservable<T>)new ReadonlyCollectionObservable<T>(source2));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new FactoryCollectionObservable<T>(receiver => new ConcatDynamic<T>(source1, source2, receiver));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (ICollectionObservable<U>)new ReadonlyCollectionObservable<U>(selectMany(x)));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new FactoryCollectionObservable<U>(receiver => new SelectManyDynamic<T, U>(source, selectMany, receiver));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ValueObservable<U>(orderBy(x)));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new FactoryListObservable<T>(receiver => new OrderByDynamic<T, U>(source, orderBy, receiver));

        public static IValueObservable<int> ObservableCount<T>(this ICollectionObservable<T> source)
            => new FactoryValueObservable<int>(receiver => new CountDynamic<T>(source, receiver));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, T contains)
            => source.ObservableContains(new ValueObservable<T>(contains));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new FactoryValueObservable<bool>(receiver => new ContainsDynamic<T>(source, contains, receiver));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => source.ObservableTrack(new ValueObservable<TKey>(key));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
            => new FactoryValueObservable<(bool keyPresent, TValue value)>(receiver => new TrackDynamic<TKey, TValue>(source, key, receiver));

        public static ICollectionObservable<TValue> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(x => x.value);

        public static IListObservable<T> ObservableShallowCopy<T>(this IListObservable<IValueObservable<T>> source)
            => new FactoryListObservable<T>(receiver => new ListShallowCopyDynamic<T>(source, receiver));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, U> select)
            => new FactoryListObservable<U>(receiver => new ListSelectDynamic<T, U>(source, select, receiver));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, T value)
            => source.ObservableIndexOf(new ValueObservable<T>(value));

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, IValueObservable<T> value)
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

        public static IDisposable Subscribe(this IObservable source, Action onChange = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new Observer(onChange: onChange, onError: onError, onDispose: onDispose));

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ValueObserver<T>(onNext: onNext, onError: onError, onDispose: onDispose));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd == null ? null : (_, value) => onAdd?.Invoke(value),
                onRemove: onRemove == null ? null : (_, value) => onRemove?.Invoke(value),
                onError: onError,
                onDispose: onDispose
            ));

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd == null ? null : (_, index, value) => onAdd?.Invoke(index, value),
                onRemove: onRemove == null ? null : (_, index, value) => onRemove?.Invoke(index, value),
                onError: onError,
                onDispose: onDispose
            ));

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd == null ? null : (_, value) => onAdd?.Invoke(value),
                onRemove: onRemove == null ? null : (_, value) => onRemove?.Invoke(value),
                onError: onError,
                onDispose: onDispose
            ));

        public static IDisposable SubscribeWithId<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ));

        public static IDisposable SubscribeWithId<T>(this IListObservable<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ));

        public static IDisposable SubscribeWithId<T>(this ICollectionObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ));
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