using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

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

    public interface ISetObservable<out T> : ICollectionObservable<T>
    {
        IDisposable Subscribe(ISetObserver<T> observer);
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
        public static IValueObservable<TResult> Combine<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, IValueObservable<TResult>> select)
            => Combine(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueObservable<TResult> Combine<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, TResult> select)
            => Combine(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueObservable<(T1, T2)> Combine<T1, T2>(IValueObservable<T1> source1, IValueObservable<T2> source2)
            => new FactoryValueObservable<(T1, T2)>(receiver => new CombineValueObservable<T1, T2>(source1, source2, receiver));

        public static IValueObservable<TResult> Combine<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, IValueObservable<TResult>> select)
            => Combine(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueObservable<TResult> Combine<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, TResult> select)
            => Combine(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueObservable<(T1, T2, T3)> Combine<T1, T2, T3>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3)
            => new FactoryValueObservable<(T1, T2, T3)>(receiver => new CombineValueObservable<T1, T2, T3>(source1, source2, source3, receiver));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, IValueObservable<TResult>> select)
            => Combine(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, TResult> select)
            => Combine(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueObservable<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4)
            => new FactoryValueObservable<(T1, T2, T3, T4)>(receiver => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4, receiver));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, IValueObservable<TResult>> select)
            => Combine(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> select)
            => Combine(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueObservable<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5)
            => new FactoryValueObservable<(T1, T2, T3, T4, T5)>(receiver => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5, receiver));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, IValueObservable<TResult>> select)
            => Combine(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select)
            => Combine(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueObservable<(T1, T2, T3, T4, T5, T6)> Combine<T1, T2, T3, T4, T5, T6>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6)
            => new FactoryValueObservable<(T1, T2, T3, T4, T5, T6)>(receiver => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6, receiver));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, IValueObservable<TResult>> select)
            => Combine(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueObservable<TResult> Combine<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select)
            => Combine(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueObservable<(T1, T2, T3, T4, T5, T6, T7)> Combine<T1, T2, T3, T4, T5, T6, T7>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7)
            => new FactoryValueObservable<(T1, T2, T3, T4, T5, T6, T7)>(receiver => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7, receiver));

        public static IObservable Any(params IObservable[] observables)
            => new FactoryObservable(receiver => new AnyObservable(observables, receiver));
    }

    public static class ObservableExtensions
    {
        public static IValueObservable<T> ObservableShallowCopy<T>(this IValueObservable<IValueObservable<T>> source)
            => new FactoryValueObservable<T>(receiver => new ShallowCopyValueObservable<T>(source, receiver));

        public static IValueObservable<T> ObservableThen<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
            => new FactoryValueObservable<T>(receiver => new ThenObservable<T>(source, new ValueObserver<T>(onNext, onError, onDispose), receiver));

        public static IValueObservable<T> ObservableThen<T>(this IValueObservable<T> source, IValueObserver<T> thenObserver)
            => new FactoryValueObservable<T>(receiver => new ThenObservable<T>(source, thenObserver, receiver));

        public static IValueObservable<T> ObservableShare<T>(this IValueObservable<T> source, SynchronizationContext context = default)
            => new ShareValueObservable<T>(source, context);

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, U> select)
            => new FactoryValueObservable<U>(receiver => new SelectValueObservable<T, U>(source, select, receiver));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static IValueObservable<(T current, T previous)> ObservableWithPrevious<T>(this IValueObservable<T> source)
            => new FactoryValueObservable<(T current, T previous)>(receiver => new WithPreviousObservable<T>(source, receiver));

        public static IValueObservable<T> ObservableSkipWhile<T>(this IValueObservable<T> source, Func<bool> skipWhile)
            => new FactoryValueObservable<T>(receiver => new SkipWhileObservable<T>(source, skipWhile, receiver));

        public static ICollectionObservable<T> ObservableShallowCopy<T>(this ICollectionObservable<IValueObservable<T>> source)
            => new FactoryCollectionObservable<T>(receiver => new ShallowCopyCollectionObservable<T>(source, receiver));

        public static ICollectionObservable<T> ObservableForEach<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new CollectionObserver<T>(
                onAdd == null ? null : (_, value) => onAdd(value),
                onRemove == null ? null : (_, value) => onRemove(value),
                onError,
                onDispose
            ));

        public static ICollectionObservable<T> ObservableForEachWithIds<T>(this ICollectionObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new CollectionObserver<T>(onAdd, onRemove, onError, onDispose));

        public static ICollectionObservable<T> ObservableForEach<T>(this ICollectionObservable<T> source, ICollectionObserver<T> forEachObserver)
            => new FactoryCollectionObservable<T>(receiver => new ForEachCollectionObservable<T>(source, forEachObserver, receiver));

        public static ICollectionObservable<T> ObservableShare<T>(this ICollectionObservable<T> source, SynchronizationContext context = default)
            => new ShareCollectionObservable<T>(source, context);

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new FactoryCollectionObservable<U>(receiver => new SelectCollectionObservable<T, U>(source, select, receiver));

        public static ICollectionObservable<T> ObservableDistinct<T>(this ICollectionObservable<T> source)
            => new FactoryCollectionObservable<T>(receiver => new DistinctObservable<T>(source, receiver));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ValueObservable<bool>(where(x)));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new FactoryCollectionObservable<T>(receiver => new WhereObservable<T>(source, where, receiver));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((ICollectionObservable<T>)new ReadonlyCollectionObservable<T>(source2));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new FactoryCollectionObservable<T>(receiver => new ConcatObservable<T>(source1, source2, receiver));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (ICollectionObservable<U>)new ReadonlyCollectionObservable<U>(selectMany(x)));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new FactoryCollectionObservable<U>(receiver => new SelectManyObservable<T, U>(source, selectMany, receiver));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ValueObservable<U>(orderBy(x)));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new FactoryListObservable<T>(receiver => new OrderByObservable<T, U>(source, orderBy, false, receiver));

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderByDescending<T, U>(x => new ValueObservable<U>(orderBy(x)));

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new FactoryListObservable<T>(receiver => new OrderByObservable<T, U>(source, orderBy, true, receiver));

        public static IValueObservable<int> ObservableCount<T>(this ICollectionObservable<T> source)
            => new FactoryValueObservable<int>(receiver => new CountObserverable<T>(source, receiver));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, T contains)
            => source.ObservableContains(new ValueObservable<T>(contains));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new FactoryValueObservable<bool>(receiver => new ContainsObservable<T>(source, contains, receiver));

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, bool> validate)
            => source.ObservableFirstOrDefault(x => new ValueObservable<bool>(validate(x)));

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate)
            => source.ObservableFirst(validate).ObservableSelect(x => x.found ? x.value : default);

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, bool> validate)
            => source.ObservableFirst(x => new ValueObservable<bool>(validate(x)));

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate)
            => new FactoryValueObservable<(bool found, T value)>(receiver => new FirstObservable<T>(source, validate, receiver));

        public static IDictionaryObservable<TKey, TValue> ObservableForEach<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new DictionaryObserver<TKey, TValue>(
                onAdd == null ? null : (_, value) => onAdd(value),
                onRemove == null ? null : (_, value) => onRemove(value),
                onError,
                onDispose
            ));

        public static IDictionaryObservable<TKey, TValue> ObservableForEachWithIds<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new DictionaryObserver<TKey, TValue>(onAdd, onRemove, onError, onDispose));

        public static IDictionaryObservable<TKey, TValue> ObservableForEach<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IDictionaryObserver<TKey, TValue> forEachObserver)
            => new FactoryDictionaryObservable<TKey, TValue>(receiver => new ForEachDictionaryObservable<TKey, TValue>(source, forEachObserver, receiver));

        public static IDictionaryObservable<TKey, TValue> ObservableShare<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, SynchronizationContext context = default)
            => new ShareDictionaryObservable<TKey, TValue>(source, context);

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => source.ObservableTrack(new ValueObservable<TKey>(key));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
            => new FactoryValueObservable<(bool keyPresent, TValue value)>(receiver => new TrackObservable<TKey, TValue>(source, key, receiver));

        public static ICollectionObservable<TValue> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(x => x.value);

        public static IListObservable<T> ObservableShallowCopy<T>(this IListObservable<IValueObservable<T>> source)
            => new FactoryListObservable<T>(receiver => new ShallowCopyListObservable<T>(source, receiver));

        public static IListObservable<T> ObservableForEach<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new ListObserver<T>(
                onAdd == null ? null : (_, index, value) => onAdd(index, value),
                onRemove == null ? null : (_, index, value) => onRemove(index, value),
                onError,
                onDispose
            ));

        public static IListObservable<T> ObservableForEachWithIds<T>(this IListObservable<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new ListObserver<T>(onAdd, onRemove, onError, onDispose));

        public static IListObservable<T> ObservableForEach<T>(this IListObservable<T> source, IListObserver<T> forEachObserver)
            => new FactoryListObservable<T>(receiver => new ForEachListObservable<T>(source, forEachObserver, receiver));

        public static IListObservable<T> ObservableShare<T>(this IListObservable<T> source, SynchronizationContext context = default)
            => new ShareListObservable<T>(source, context);

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, U> select)
            => new FactoryListObservable<U>(receiver => new SelectListObservable<T, U>(source, select, receiver));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, T value)
            => source.ObservableIndexOf(new ValueObservable<T>(value));

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, IValueObservable<T> value)
            => new FactoryValueObservable<int>(receiver => new IndexOfObservable<T>(source, value, receiver));

        public static ISetObservable<T> ObservableShare<T>(this ISetObservable<T> source, SynchronizationContext context = default)
            => new ShareSetObservable<T>(source, context);

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

        public static IDisposable Subscribe(this IObservable source, Action onChange = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer(onChange: onChange, onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<T>(onNext: onNext, onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd == null ? null : (_, value) => onAdd.Invoke(value),
                onRemove: onRemove == null ? null : (_, value) => onRemove.Invoke(value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd == null ? null : (_, index, value) => onAdd.Invoke(index, value),
                onRemove: onRemove == null ? null : (_, index, value) => onRemove.Invoke(index, value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ISetObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd == null ? null : (_, value) => onAdd.Invoke(value),
                onRemove: onRemove == null ? null : (_, value) => onRemove.Invoke(value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd == null ? null : (_, value) => onAdd.Invoke(value),
                onRemove: onRemove == null ? null : (_, value) => onRemove.Invoke(value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IListObservable<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this ISetObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this ICollectionObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T1, T2>(this IValueObservable<(T1, T2)> source, Action<T1, T2> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3>(this IValueObservable<(T1, T2, T3)> source, Action<T1, T2, T3> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this IValueObservable<(T1, T2, T3, T4)> source, Action<T1, T2, T3, T4> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5>(this IValueObservable<(T1, T2, T3, T4, T5)> source, Action<T1, T2, T3, T4, T5> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4, T5)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6>(this IValueObservable<(T1, T2, T3, T4, T5, T6)> source, Action<T1, T2, T3, T4, T5, T6> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4, T5, T6)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6, T7>(this IValueObservable<(T1, T2, T3, T4, T5, T6, T7)> source, Action<T1, T2, T3, T4, T5, T6, T7> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4, T5, T6, T7)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7), onError: onError, onDispose: onDispose, immediate: immediate));
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