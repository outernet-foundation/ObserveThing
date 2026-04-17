using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObservable { }

    public interface IValueOperator<out T>
    {
        IDisposable Subscribe(IValueObserver<T> observer);
    }

    public interface ICollectionOperator<out T>
    {
        IDisposable Subscribe(ICollectionObserver<T> observer);
    }

    public interface ISetOperator<out T> : ICollectionOperator<T>
    {
        IDisposable Subscribe(ISetObserver<T> observer);
    }

    public interface IDictionaryOperator<TKey, TValue> : ICollectionOperator<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer);
    }

    public interface IListOperator<out T> : ICollectionOperator<T>
    {
        IDisposable Subscribe(IListObserver<T> observer);
    }

    public static class Observables
    {
        public static IValueOperator<TResult> Combine<T1, T2, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, Func<T1, T2, IValueOperator<TResult>> select)
            => Combine(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueOperator<TResult> Combine<T1, T2, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, Func<T1, T2, TResult> select)
            => Combine(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueOperator<(T1, T2)> Combine<T1, T2>(IValueOperator<T1> source1, IValueOperator<T2> source2)
            => new ValueOperatorFactory<(T1, T2)>(receiver => new CombineValueObservable<T1, T2>(source1, source2, receiver));

        public static IValueOperator<TResult> Combine<T1, T2, T3, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, Func<T1, T2, T3, IValueOperator<TResult>> select)
            => Combine(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueOperator<TResult> Combine<T1, T2, T3, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, Func<T1, T2, T3, TResult> select)
            => Combine(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueOperator<(T1, T2, T3)> Combine<T1, T2, T3>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3)
            => new ValueOperatorFactory<(T1, T2, T3)>(receiver => new CombineValueObservable<T1, T2, T3>(source1, source2, source3, receiver));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, Func<T1, T2, T3, T4, IValueOperator<TResult>> select)
            => Combine(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, Func<T1, T2, T3, T4, TResult> select)
            => Combine(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueOperator<(T1, T2, T3, T4)> Combine<T1, T2, T3, T4>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4)
            => new ValueOperatorFactory<(T1, T2, T3, T4)>(receiver => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4, receiver));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, T5, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, Func<T1, T2, T3, T4, T5, IValueOperator<TResult>> select)
            => Combine(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, T5, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, Func<T1, T2, T3, T4, T5, TResult> select)
            => Combine(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueOperator<(T1, T2, T3, T4, T5)> Combine<T1, T2, T3, T4, T5>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5)
            => new ValueOperatorFactory<(T1, T2, T3, T4, T5)>(receiver => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5, receiver));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, T5, T6, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, IValueOperator<T6> source6, Func<T1, T2, T3, T4, T5, T6, IValueOperator<TResult>> select)
            => Combine(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, T5, T6, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, IValueOperator<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select)
            => Combine(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueOperator<(T1, T2, T3, T4, T5, T6)> Combine<T1, T2, T3, T4, T5, T6>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, IValueOperator<T6> source6)
            => new ValueOperatorFactory<(T1, T2, T3, T4, T5, T6)>(receiver => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6, receiver));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, IValueOperator<T6> source6, IValueOperator<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, IValueOperator<TResult>> select)
            => Combine(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueOperator<TResult> Combine<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, IValueOperator<T6> source6, IValueOperator<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select)
            => Combine(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueOperator<(T1, T2, T3, T4, T5, T6, T7)> Combine<T1, T2, T3, T4, T5, T6, T7>(IValueOperator<T1> source1, IValueOperator<T2> source2, IValueOperator<T3> source3, IValueOperator<T4> source4, IValueOperator<T5> source5, IValueOperator<T6> source6, IValueOperator<T7> source7)
            => new ValueOperatorFactory<(T1, T2, T3, T4, T5, T6, T7)>(receiver => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7, receiver));
    }

    public static class ObservableExtensions
    {
        public static IValueOperator<T> ObservableShallowCopy<T>(this IValueOperator<IValueOperator<T>> source)
            => new ValueOperatorFactory<T>(receiver => new ShallowCopyValueObservable<T>(source, receiver));

        public static IValueOperator<T> ObservableThen<T>(this IValueOperator<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
            => new ValueOperatorFactory<T>(receiver => new ThenObservable<T>(source, new ValueObserver<T>(onNext, onError, onDispose), receiver));

        public static IValueOperator<T> ObservableThen<T>(this IValueOperator<T> source, IValueObserver<T> thenObserver)
            => new ValueOperatorFactory<T>(receiver => new ThenObservable<T>(source, thenObserver, receiver));

        public static IValueOperator<T> ObservableShare<T>(this IValueOperator<T> source, ObservationContext context = default)
            => new ShareValueObservable<T>(source, context);

        public static IValueOperator<U> ObservableSelect<T, U>(this IValueOperator<T> source, Func<T, U> select)
            => new ValueOperatorFactory<U>(receiver => new SelectValueObservable<T, U>(source, select, receiver));

        public static IValueOperator<U> ObservableSelect<T, U>(this IValueOperator<T> source, Func<T, IValueOperator<U>> select)
            => source.ObservableSelect<T, IValueOperator<U>>(select).ObservableShallowCopy();

        public static IValueOperator<(T current, T previous)> ObservableWithPrevious<T>(this IValueOperator<T> source)
            => new ValueOperatorFactory<(T current, T previous)>(receiver => new WithPreviousObservable<T>(source, receiver));

        public static IValueOperator<T> ObservableSkipWhile<T>(this IValueOperator<T> source, Func<bool> skipWhile)
            => new ValueOperatorFactory<T>(receiver => new SkipWhileObservable<T>(source, skipWhile, receiver));

        public static ICollectionOperator<T> ObservableShallowCopy<T>(this ICollectionOperator<IValueOperator<T>> source)
            => new CollectionOperatorFactory<T>(receiver => new ShallowCopyCollectionObservable<T>(source, receiver));

        public static ICollectionOperator<T> ObservableForEach<T>(this ICollectionOperator<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new CollectionObserver<T>(
                onAdd == null ? null : (_, value) => onAdd(value),
                onRemove == null ? null : (_, value) => onRemove(value),
                onError,
                onDispose
            ));

        public static ICollectionOperator<T> ObservableForEachWithIds<T>(this ICollectionOperator<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new CollectionObserver<T>(onAdd, onRemove, onError, onDispose));

        public static ICollectionOperator<T> ObservableForEach<T>(this ICollectionOperator<T> source, ICollectionObserver<T> forEachObserver)
            => new CollectionOperatorFactory<T>(receiver => new ForEachCollectionObservable<T>(source, forEachObserver, receiver));

        // public static ICollectionOperator<T> ObservableShare<T>(this ICollectionOperator<T> source, ObservationContext context = default)
        //     => new ShareCollectionObservable<T>(source, context);

        public static ICollectionOperator<U> ObservableSelect<T, U>(this ICollectionOperator<T> source, Func<T, IValueOperator<U>> select)
            => source.ObservableSelect<T, IValueOperator<U>>(select).ObservableShallowCopy();

        public static ICollectionOperator<U> ObservableSelect<T, U>(this ICollectionOperator<T> source, Func<T, U> select)
            => new CollectionOperatorFactory<U>(receiver => new SelectCollectionObservable<T, U>(source, select, receiver));

        public static ICollectionOperator<T> ObservableDistinct<T>(this ICollectionOperator<T> source)
            => new CollectionOperatorFactory<T>(receiver => new DistinctObservable<T>(source, receiver));

        public static ICollectionOperator<T> ObservableWhere<T>(this ICollectionOperator<T> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ValueObservable<bool>(where(x)));

        public static ICollectionOperator<T> ObservableWhere<T>(this ICollectionOperator<T> source, Func<T, IValueOperator<bool>> where)
            => new CollectionOperatorFactory<T>(receiver => new WhereObservable<T>(source, where, receiver));

        public static ICollectionOperator<T> ObservableConcat<T>(this ICollectionOperator<T> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((ICollectionOperator<T>)new ReadonlyCollectionObservable<T>(source2));

        public static ICollectionOperator<T> ObservableConcat<T>(this ICollectionOperator<T> source1, ICollectionOperator<T> source2)
            => new CollectionOperatorFactory<T>(receiver => new ConcatObservable<T>(source1, source2, receiver));

        public static ICollectionOperator<U> ObservableSelectMany<T, U>(this ICollectionOperator<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (ICollectionOperator<U>)new ReadonlyCollectionObservable<U>(selectMany(x)));

        public static ICollectionOperator<U> ObservableSelectMany<T, U>(this ICollectionOperator<T> source, Func<T, ICollectionOperator<U>> selectMany)
            => new CollectionOperatorFactory<U>(receiver => new SelectManyObservable<T, U>(source, selectMany, receiver));

        public static IListOperator<T> ObservableOrderBy<T, U>(this ICollectionOperator<T> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ValueObservable<U>(orderBy(x)));

        public static IListOperator<T> ObservableOrderBy<T, U>(this ICollectionOperator<T> source, Func<T, IValueOperator<U>> orderBy)
            => new ListOperatorFactory<T>(receiver => new OrderByObservable<T, U>(source, orderBy, false, receiver));

        public static IListOperator<T> ObservableOrderByDescending<T, U>(this ICollectionOperator<T> source, Func<T, U> orderBy)
            => source.ObservableOrderByDescending<T, U>(x => new ValueObservable<U>(orderBy(x)));

        public static IListOperator<T> ObservableOrderByDescending<T, U>(this ICollectionOperator<T> source, Func<T, IValueOperator<U>> orderBy)
            => new ListOperatorFactory<T>(receiver => new OrderByObservable<T, U>(source, orderBy, true, receiver));

        public static IValueOperator<int> ObservableCount<T>(this ICollectionOperator<T> source)
            => new ValueOperatorFactory<int>(receiver => new CountObserverable<T>(source, receiver));

        public static IValueOperator<bool> ObservableContains<T>(this ICollectionOperator<T> source, T contains)
            => source.ObservableContains(new ValueObservable<T>(contains));

        public static IValueOperator<bool> ObservableContains<T>(this ICollectionOperator<T> source, IValueOperator<T> contains)
            => new ValueOperatorFactory<bool>(receiver => new ContainsObservable<T>(source, contains, receiver));

        public static IValueOperator<T> ObservableFirstOrDefault<T>(this ICollectionOperator<T> source, Func<T, bool> validate)
            => source.ObservableFirstOrDefault(x => new ValueObservable<bool>(validate(x)));

        public static IValueOperator<T> ObservableFirstOrDefault<T>(this ICollectionOperator<T> source, Func<T, IValueOperator<bool>> validate)
            => source.ObservableFirst(validate).ObservableSelect(x => x.found ? x.value : default);

        public static IValueOperator<(bool found, T value)> ObservableFirst<T>(this ICollectionOperator<T> source, Func<T, bool> validate)
            => source.ObservableFirst(x => new ValueObservable<bool>(validate(x)));

        public static IValueOperator<(bool found, T value)> ObservableFirst<T>(this ICollectionOperator<T> source, Func<T, IValueOperator<bool>> validate)
            => new ValueOperatorFactory<(bool found, T value)>(receiver => new FirstObservable<T>(source, validate, receiver));

        public static IDictionaryOperator<TKey, TValue> ObservableForEach<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new DictionaryObserver<TKey, TValue>(
                onAdd == null ? null : (_, value) => onAdd(value),
                onRemove == null ? null : (_, value) => onRemove(value),
                onError,
                onDispose
            ));

        public static IDictionaryOperator<TKey, TValue> ObservableForEachWithIds<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new DictionaryObserver<TKey, TValue>(onAdd, onRemove, onError, onDispose));

        public static IDictionaryOperator<TKey, TValue> ObservableForEach<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, IDictionaryObserver<TKey, TValue> forEachObserver)
            => new DictionaryOperatorFactory<TKey, TValue>(receiver => new ForEachDictionaryObservable<TKey, TValue>(source, forEachObserver, receiver));

        public static IDictionaryOperator<TKey, TValue> ObservableShare<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, ObservationContext context = default)
            => new ShareDictionaryObservable<TKey, TValue>(source, context);

        public static IValueOperator<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, TKey key)
            => source.ObservableTrack(new ValueObservable<TKey>(key));

        public static IValueOperator<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, IValueOperator<TKey> key)
            => new ValueOperatorFactory<(bool keyPresent, TValue value)>(receiver => new TrackObservable<TKey, TValue>(source, key, receiver));

        public static ICollectionOperator<TValue> ObservableTrack<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, ICollectionOperator<TKey> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(x => x.value);

        public static IListOperator<T> ObservableShallowCopy<T>(this IListOperator<IValueOperator<T>> source)
            => new ListOperatorFactory<T>(receiver => new ShallowCopyListObservable<T>(source, receiver));

        public static IListOperator<T> ObservableForEach<T>(this IListOperator<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new ListObserver<T>(
                onAdd == null ? null : (_, index, value) => onAdd(index, value),
                onRemove == null ? null : (_, index, value) => onRemove(index, value),
                onError,
                onDispose
            ));

        public static IListOperator<T> ObservableForEachWithIds<T>(this IListOperator<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableForEach(new ListObserver<T>(onAdd, onRemove, onError, onDispose));

        public static IListOperator<T> ObservableForEach<T>(this IListOperator<T> source, IListObserver<T> forEachObserver)
            => new ListOperatorFactory<T>(receiver => new ForEachListObservable<T>(source, forEachObserver, receiver));

        public static IListOperator<T> ObservableShare<T>(this IListOperator<T> source, ObservationContext context = default)
            => new ShareListObservable<T>(source, context);

        public static IListOperator<U> ObservableSelect<T, U>(this IListOperator<T> source, Func<T, U> select)
            => new ListOperatorFactory<U>(receiver => new SelectListObservable<T, U>(source, select, receiver));

        public static IListOperator<U> ObservableSelect<T, U>(this IListOperator<T> source, Func<T, IValueOperator<U>> select)
            => source.ObservableSelect<T, IValueOperator<U>>(select).ObservableShallowCopy();

        public static IValueOperator<int> ObservableIndexOf<T>(this IListOperator<T> source, T value)
            => source.ObservableIndexOf(new ValueObservable<T>(value));

        public static IValueOperator<int> ObservableIndexOf<T>(this IListOperator<T> source, IValueOperator<T> value)
            => new ValueOperatorFactory<int>(receiver => new IndexOfObservable<T>(source, value, receiver));

        public static ISetOperator<T> ObservableShare<T>(this ISetOperator<T> source, ObservationContext context = default)
            => new ShareSetObservable<T>(source, context);

        public static IValueOperator<T> AsObservable<T>(this IValueOperator<T> observable)
            => observable;

        public static ICollectionOperator<T> AsObservable<T>(this ICollectionOperator<T> observable)
            => observable;

        public static IListOperator<T> AsObservable<T>(this IListOperator<T> observable)
            => observable;

        public static IDictionaryOperator<TKey, TValue> AsObservable<TKey, TValue>(this IDictionaryOperator<TKey, TValue> observable)
            => observable;

        // public static T Peek<T>(this IValueObservable<T> source)
        // {
        //     T result = default;
        //     source.Subscribe(x => result = x).Dispose();
        //     return result;
        // }

        public static IDisposable Subscribe<T>(this IValueOperator<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<T>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd == null ? null : (_, x) => onAdd(x),
                onRemove: onRemove == null ? null : (_, x) => onRemove(x),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<TKey, TValue>(this IDictionaryOperator<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IListOperator<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd == null ? null : (_, index, x) => onAdd(index, x),
                onRemove: onRemove == null ? null : (_, index, x) => onRemove(index, x),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IListOperator<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ISetOperator<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd == null ? null : (_, x) => onAdd(x),
                onRemove: onRemove == null ? null : (_, x) => onRemove(x),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this ISetOperator<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ICollectionOperator<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd == null ? null : (_, x) => onAdd(x),
                onRemove: onRemove == null ? null : (_, x) => onRemove(x),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this ICollectionOperator<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T1, T2>(this IValueOperator<(T1, T2)> source, Action<T1, T2> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3>(this IValueOperator<(T1, T2, T3)> source, Action<T1, T2, T3> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this IValueOperator<(T1, T2, T3, T4)> source, Action<T1, T2, T3, T4> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5>(this IValueOperator<(T1, T2, T3, T4, T5)> source, Action<T1, T2, T3, T4, T5> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4, T5)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6>(this IValueOperator<(T1, T2, T3, T4, T5, T6)> source, Action<T1, T2, T3, T4, T5, T6> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<(T1, T2, T3, T4, T5, T6)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6, T7>(this IValueOperator<(T1, T2, T3, T4, T5, T6, T7)> source, Action<T1, T2, T3, T4, T5, T6, T7> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
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