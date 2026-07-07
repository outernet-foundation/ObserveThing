using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObservable
    {
        ObservationContext context { get; }
        IDisposable Subscribe(IObserver observer);
    }

    public interface IObservable<out T> : IObservable where T : IOperation
    {
        IDisposable Subscribe(IObserver<T> observer);
    }

    public interface IValueObservable : IObservable
    {
        IDisposable Subscribe(IObserver<IValueOperation> observer);
    }

    public interface IValueObservable<out T> : IValueObservable
    {
        IDisposable Subscribe(IObserver<IValueOperation<T>> observer);
    }

    public interface ICollectionObservable : IObservable
    {
        IDisposable Subscribe(IObserver<ICollectionOperation> observer);
    }

    public interface ICollectionObservable<out T> : ICollectionObservable
    {
        IDisposable Subscribe(IObserver<ICollectionOperation<T>> observer);
    }

    public interface ISetObservable : ICollectionObservable
    {
        IDisposable Subscribe(IObserver<ISetOperation> observer);
    }

    public interface ISetObservable<out T> : ICollectionObservable<T>
    {
        IDisposable Subscribe(IObserver<ISetOperation<T>> observer);
    }

    public interface IDictionaryObservable : ICollectionObservable
    {
        IDisposable Subscribe(IObserver<IDictionaryOperation> observer);
    }

    public interface IDictionaryObservable<TKey, TValue> : ICollectionObservable<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(IObserver<IDictionaryOperation<TKey, TValue>> observer);
    }

    public interface IListObservable : ICollectionObservable
    {
        IDisposable Subscribe(IObserver<IListOperation> observer);
    }

    public interface IListObservable<out T> : ICollectionObservable<T>, IListObservable
    {
        IDisposable Subscribe(IObserver<IListOperation<T>> observer);
    }

    public static class Observables
    {
        public static IObservable<IBatchOperation<T>> ObservableBatch<T>(this IObservable<T> source) where T : IOperation
            => new BatchObservable<T>(source);

        public static IValueObservable<T> ObservableCast<T>(this IValueObservable source)
            => new CastValueObservable<T>(source);

        public static ICollectionObservable<T> ObservableCast<T>(this ICollectionObservable source)
            => new CastCollectionObservable<T>(source);

        public static IListObservable<T> ObservableCast<T>(this IListObservable source)
            => new CastListObservable<T>(source);

        public static IObservable ObservableCombine(this ISetObservable<IObservable> source, bool disposeOnSourceEmpty = false)
            => new CombineObservable(source, disposeOnSourceEmpty);

        public static IObservable ObservableCombine(params IObservable[] observables)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty: true);

        public static IObservable ObservableCombine(bool disposeOnSourceEmpty, params IObservable[] observables)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty: disposeOnSourceEmpty);

        public static IObservable ObservableCombine(IEnumerable<IObservable> observables, bool disposeOnSourceEmpty = true)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty);

        public static IObservable<T> ObservableOnEach<T>(this IObservable<T> source, IObserver<T> thenObserver) where T : IOperation
            => new OnEachObservable<T>(source, thenObserver);

        public static IObservable<T> ObservableOnEach<T>(this IObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default) where T : IOperation
            => source.ObservableOnEach(new Observer<T>(onNext, onError, onDispose));

        public static IValueObservable<(T1, T2)> ObservableCombineValues<T1, T2>(IValueObservable<T1> source1, IValueObservable<T2> source2)
            => new CombineValueObservable<T1, T2>(source1, source2);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, TResult> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueObservable<(T1, T2, T3)> ObservableCombineValues<T1, T2, T3>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3)
            => new CombineValueObservable<T1, T2, T3>(source1, source2, source3);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, TResult> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueObservable<(T1, T2, T3, T4)> ObservableCombineValues<T1, T2, T3, T4>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4)
            => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueObservable<(T1, T2, T3, T4, T5)> ObservableCombineValues<T1, T2, T3, T4, T5>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5)
            => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueObservable<(T1, T2, T3, T4, T5, T6)> ObservableCombineValues<T1, T2, T3, T4, T5, T6>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6)
            => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueObservable<(T1, T2, T3, T4, T5, T6, T7)> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7)
            => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueObservable<T> ObservableShallowCopy<T>(this IValueObservable<IValueObservable<T>> source)
            => new ShallowCopyValueObservable<T>(source);

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, U> select)
            => new SelectValueObservable<T, U>(source, select);

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static IValueObservable<(T current, T previous)> ObservableWithPrevious<T>(this IValueObservable<T> source)
            => new WithPreviousObservable<T>(source);

        public static IValueObservable<T> ObservableSkipWhile<T>(this IValueObservable<T> source, Func<bool> skipWhile)
            => new SkipWhileObservable<T>(source, skipWhile);

        public static ICollectionObservable<T> ObservableShallowCopy<T>(this ICollectionObservable<IValueObservable<T>> source)
            => new ShallowCopyCollectionObservable<T>(source);

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new SelectCollectionObservable<T, U>(source, select);

        public static ISetObservable<T> ObservableDistinct<T>(this ICollectionObservable<T> source)
            => new DistinctObservable<T>(source);

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ObservableValue<bool>(where(x)));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new WhereObservable<T>(source, where);

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((ICollectionObservable<T>)new ObservableReadOnlyCollection<T>(source2));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new ConcatObservable<T>(source1, source2);

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (ICollectionObservable<U>)new ObservableReadOnlyCollection<U>(selectMany(x)));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new SelectManyObservable<T, U>(source, selectMany);

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ObservableValue<U>(orderBy(x)));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new OrderByObservable<T, U>(source, orderBy, false);

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderByDescending<T, U>(x => new ObservableValue<U>(orderBy(x)));

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new OrderByObservable<T, U>(source, orderBy, true);

        public static IValueObservable<int> ObservableCount<T>(this ICollectionObservable<T> source)
            => new CountObservable<T>(source);

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, T contains)
            => source.ObservableContains(new ObservableValue<T>(contains));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new ContainsObservable<T>(source, contains);

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, bool> validate)
            => source.ObservableFirstOrDefault(x => new ObservableValue<bool>(validate(x)));

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate)
            => source.ObservableFirst(validate).ObservableSelect(x => x.found ? x.value : default);

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, bool> validate)
            => source.ObservableFirst(x => new ObservableValue<bool>(validate(x)));

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate)
            => new FirstObservable<T>(source, validate);

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => source.ObservableTrack(new ObservableValue<TKey>(key));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
            => new TrackObservable<TKey, TValue>(source, key);

        public static ICollectionObservable<TValue> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(x => x.value);

        public static IListObservable<T> ObservableShallowCopy<T>(this IListObservable<IValueObservable<T>> source)
            => new ShallowCopyListObservable<T>(source);

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, U> select)
            => new SelectListObservable<T, U>(source, select);

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableShallowCopy();

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, T value)
            => source.ObservableIndexOf(new ObservableValue<T>(value));

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, IValueObservable<T> value)
            => new IndexOfObservable<T>(source, value);

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

        public static IDisposable Subscribe(this IObservable source, Action<IOperation> onOperation = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer(
                onNext: onOperation,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onOperation = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false) where T : IOperation
            => source.Subscribe(new Observer<T>(
                onNext: onOperation,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<T>>(
                onNext: onNext == null ? null : x => onNext.Invoke(x.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this IValueObservable source, Action<object> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation>(
                onNext: onNext == null ? null : x => onNext.Invoke(x.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<IDictionaryOperation<TKey, TValue>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this IDictionaryObservable source, Action<IDictionaryOperation> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this IDictionaryObservable source, Action<KeyValuePair<object, object>> onAdd = default, Action<KeyValuePair<object, object>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(KeyValuePair.Create(op.key, op.value)),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId(this IDictionaryObservable source, Action<uint, KeyValuePair<object, object>> onAdd = default, Action<uint, KeyValuePair<object, object>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, KeyValuePair.Create(op.key, op.value)),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<IListOperation<T>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation<T>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.index, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IListObservable<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.index, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this IListObservable source, Action<IListOperation> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this IListObservable source, Action<int, object> onAdd = default, Action<int, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.index, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId(this IListObservable source, Action<uint, int, object> onAdd = default, Action<uint, int, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.index, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ISetObservable<T> source, Action<ISetOperation<T>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation<T>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));


        public static IDisposable Subscribe<T>(this ISetObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this ISetObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this ISetObservable source, Action<ISetOperation> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this ISetObservable source, Action<object> onAdd = default, Action<object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId(this ISetObservable source, Action<uint, object> onAdd = default, Action<uint, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<ICollectionOperation<T>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation<T>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this ICollectionObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this ICollectionObservable source, Action<ICollectionOperation> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe(this ICollectionObservable source, Action<object> onAdd = default, Action<object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId(this ICollectionObservable source, Action<uint, object> onAdd = default, Action<uint, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T1, T2>(this IValueObservable<(T1, T2)> source, Action<T1, T2> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<(T1, T2)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3>(this IValueObservable<(T1, T2, T3)> source, Action<T1, T2, T3> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<(T1, T2, T3)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this IValueObservable<(T1, T2, T3, T4)> source, Action<T1, T2, T3, T4> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<(T1, T2, T3, T4)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5>(this IValueObservable<(T1, T2, T3, T4, T5)> source, Action<T1, T2, T3, T4, T5> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<(T1, T2, T3, T4, T5)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4, x.value.Item5), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6>(this IValueObservable<(T1, T2, T3, T4, T5, T6)> source, Action<T1, T2, T3, T4, T5, T6> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<(T1, T2, T3, T4, T5, T6)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4, x.value.Item5, x.value.Item6), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6, T7>(this IValueObservable<(T1, T2, T3, T4, T5, T6, T7)> source, Action<T1, T2, T3, T4, T5, T6, T7> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IValueOperation<(T1, T2, T3, T4, T5, T6, T7)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4, x.value.Item5, x.value.Item6, x.value.Item7), onError: onError, onDispose: onDispose, immediate: immediate));
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