using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public static class Observables
    {
        public static IObservable<IReadOnlyList<T>> ObservableBatch<T>(this IObservable<T> source)
            => new ObservableOperator<IReadOnlyList<T>>(source.context, operand => new BatchObservable<T>(source, operand));

        public static IObservable<T> ObservableCombine<T>(this IObservable<CollectionOp<IObservable<T>>> source, bool disposeOnSourceEmpty = false)
            => new ObservableOperator<T>(source.context, operand => new CombineObservable<T>(source, operand, disposeOnSourceEmpty));

        public static IObservable<T> ObservableCombine<T>(params IObservable<T>[] observables)
            => new ObservableSet<IObservable<T>>(observables).ObservableCombine(disposeOnSourceEmpty: true);

        public static IObservable<T> ObservableCombine<T>(bool disposeOnSourceEmpty, params IObservable<T>[] observables)
            => new ObservableSet<IObservable<T>>(observables).ObservableCombine(disposeOnSourceEmpty: disposeOnSourceEmpty);

        public static IObservable<T> ObservableCombine<T>(IEnumerable<IObservable<T>> observables, bool disposeOnSourceEmpty = true)
            => new ObservableSet<IObservable<T>>(observables).ObservableCombine(disposeOnSourceEmpty);

        public static IObservable<T> ObservableOnEach<T>(this IObservable<T> source, IObserver<T> thenObserver)
            => new ObservableOperator<T>(source.context, operand => new OnEachObservable<T>(source, thenObserver, operand));

        public static IObservable<T> ObservableOnEach<T>(this IObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
            => source.ObservableOnEach(new Observer<T>(onNext, onError, onDispose));

        public static IObservable<(T1, T2)> ObservableCombineValues<T1, T2>(IObservable<T1> source1, IObservable<T2> source2)
            => new ValueOperator<(T1, T2)>(source1.context, operand => new CombineValueObservable<T1, T2>(source1, source2, operand));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IObservable<T1> source1, IObservable<T2> source2, Func<T1, T2, IObservable<TResult>> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IObservable<T1> source1, IObservable<T2> source2, Func<T1, T2, TResult> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IObservable<(T1, T2, T3)> ObservableCombineValues<T1, T2, T3>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3)
            => new ValueOperator<(T1, T2, T3)>(source1.context, operand => new CombineValueObservable<T1, T2, T3>(source1, source2, source3, operand));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, Func<T1, T2, T3, IObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, Func<T1, T2, T3, TResult> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IObservable<(T1, T2, T3, T4)> ObservableCombineValues<T1, T2, T3, T4>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4)
            => new ValueOperator<(T1, T2, T3, T4)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4, operand));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, Func<T1, T2, T3, T4, IObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, Func<T1, T2, T3, T4, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IObservable<(T1, T2, T3, T4, T5)> ObservableCombineValues<T1, T2, T3, T4, T5>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5)
            => new ValueOperator<(T1, T2, T3, T4, T5)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5, operand));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, Func<T1, T2, T3, T4, T5, IObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IObservable<(T1, T2, T3, T4, T5, T6)> ObservableCombineValues<T1, T2, T3, T4, T5, T6>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6, operand));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, IObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IObservable<(T1, T2, T3, T4, T5, T6, T7)> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6, T7)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7, operand));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, IObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IObservable<T1> source1, IObservable<T2> source2, IObservable<T3> source3, IObservable<T4> source4, IObservable<T5> source5, IObservable<T6> source6, IObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IObservable<T> ObservableShallowCopy<T>(this IObservable<IObservable<T>> source)
            => new ValueOperator<T>(source.context, operand => new ShallowCopyValueObservable<T>(source, operand));

        public static IObservable<U> ObservableSelect<T, U>(this IObservable<T> source, Func<T, U> select)
            => new ValueOperator<U>(source.context, operand => new SelectValueOperator<T, U>(source, select, operand));

        public static IObservable<U> ObservableSelect<T, U>(this IObservable<T> source, Func<T, IObservable<U>> select)
            => source.ObservableSelect<T, IObservable<U>>(select).ObservableShallowCopy();

        public static IObservable<(T current, T previous)> ObservableWithPrevious<T>(this IObservable<T> source)
            => new ValueOperator<(T current, T previous)>(source.context, operand => new WithPreviousObservable<T>(source, operand));

        public static IObservable<T> ObservableSkipWhile<T>(this IObservable<T> source, Func<bool> skipWhile)
            => new ValueOperator<T>(source.context, operand => new SkipWhileObservable<T>(source, skipWhile, operand));

        public static IObservable<CollectionOp<T>> ObservableShallowCopy<T>(this IObservable<CollectionOp<IObservable<T>>> source)
            => new CollectionOperator<T>(source.context, operand => new ShallowCopyCollectionObservable<T>(source, operand));

        public static IObservable<CollectionOp<U>> ObservableSelect<T, U>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<U>> select)
            => source.ObservableSelect<T, IObservable<U>>(select).ObservableShallowCopy();

        public static IObservable<CollectionOp<U>> ObservableSelect<T, U>(this IObservable<CollectionOp<T>> source, Func<T, U> select)
            => new CollectionOperator<U>(source.context, operand => new SelectCollectionObservable<T, U>(source, select, operand));

        public static IObservable<CollectionOp<T>> ObservableDistinct<T>(this IObservable<CollectionOp<T>> source)
            => new SetOperator<T>(source.context, operand => new DistinctObservable<T>(source, operand));

        public static IObservable<CollectionOp<T>> ObservableWhere<T>(this IObservable<CollectionOp<T>> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ObservableValue<bool>(where(x)));

        public static IObservable<CollectionOp<T>> ObservableWhere<T>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<bool>> where)
            => new CollectionOperator<T>(source.context, operand => new WhereObservable<T>(source, where, operand));

        public static IObservable<CollectionOp<T>> ObservableConcat<T>(this IObservable<CollectionOp<T>> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((IObservable<CollectionOp<T>>)new ObservableReadOnlyCollection<T>(source2));

        public static IObservable<CollectionOp<T>> ObservableConcat<T>(this IObservable<CollectionOp<T>> source1, IObservable<CollectionOp<T>> source2)
            => new CollectionOperator<T>(source1.context, operand => new ConcatObservable<T>(source1, source2, operand));

        public static IObservable<CollectionOp<U>> ObservableSelectMany<T, U>(this IObservable<CollectionOp<T>> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (IObservable<CollectionOp<U>>)new ObservableReadOnlyCollection<U>(selectMany(x)));

        public static IObservable<CollectionOp<U>> ObservableSelectMany<T, U>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<CollectionOp<U>>> selectMany)
            => new CollectionOperator<U>(source.context, operand => new SelectManyObservable<T, U>(source, selectMany, operand));

        public static IObservable<CollectionOp<ListData<T>>> ObservableOrderBy<T, U>(this IObservable<CollectionOp<T>> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ObservableValue<U>(orderBy(x)));

        public static IObservable<CollectionOp<ListData<T>>> ObservableOrderBy<T, U>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<U>> orderBy)
            => new ListOperator<T>(source.context, operand => new OrderByObservable<T, U>(source, orderBy, false, operand));

        public static IObservable<CollectionOp<ListData<T>>> ObservableOrderByDescending<T, U>(this IObservable<CollectionOp<T>> source, Func<T, U> orderBy)
            => source.ObservableOrderByDescending<T, U>(x => new ObservableValue<U>(orderBy(x)));

        public static IObservable<CollectionOp<ListData<T>>> ObservableOrderByDescending<T, U>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<U>> orderBy)
            => new ListOperator<T>(source.context, operand => new OrderByObservable<T, U>(source, orderBy, true, operand));

        public static IObservable<int> ObservableCount<T>(this IObservable<CollectionOp<T>> source)
            => new ValueOperator<int>(source.context, operand => new CountObservable<T>(source, operand));

        public static IObservable<bool> ObservableContains<T>(this IObservable<CollectionOp<T>> source, T contains)
            => source.ObservableContains(new ObservableValue<T>(contains));

        public static IObservable<bool> ObservableContains<T>(this IObservable<CollectionOp<T>> source, IObservable<T> contains)
            => new ValueOperator<bool>(source.context, operand => new ContainsObservable<T>(source, contains, operand));

        public static IObservable<T> ObservableFirstOrDefault<T>(this IObservable<CollectionOp<T>> source, Func<T, bool> validate)
            => source.ObservableFirstOrDefault(x => new ObservableValue<bool>(validate(x)));

        public static IObservable<T> ObservableFirstOrDefault<T>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<bool>> validate)
            => source.ObservableFirst(validate).ObservableSelect(x => x.found ? x.value : default);

        public static IObservable<(bool found, T value)> ObservableFirst<T>(this IObservable<CollectionOp<T>> source, Func<T, bool> validate)
            => source.ObservableFirst(x => new ObservableValue<bool>(validate(x)));

        public static IObservable<(bool found, T value)> ObservableFirst<T>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<bool>> validate)
            => new ValueOperator<(bool found, T value)>(source.context, operand => new FirstObservable<T>(source, validate, operand));

        public static IObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> source, TKey key)
            => source.ObservableTrack(new ObservableValue<TKey>(key));

        public static IObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> source, IObservable<TKey> key)
            => new ValueOperator<(bool keyPresent, TValue value)>(source.context, operand => new TrackObservable<TKey, TValue>(source, key, operand));

        public static IObservable<CollectionOp<TValue>> ObservableTrack<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> source, IObservable<CollectionOp<TKey>> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(((bool keyPresent, TValue value) x) => x.value);

        public static IObservable<CollectionOp<ListData<T>>> ObservableShallowCopy<T>(this IObservable<CollectionOp<ListData<IObservable<T>>>> source)
            => new ListOperator<T>(source.context, operand => new ShallowCopyListObservable<T>(source, operand));

        public static IObservable<CollectionOp<ListData<U>>> ObservableSelect<T, U>(this IObservable<CollectionOp<ListData<T>>> source, Func<T, U> select)
            => new ListOperator<U>(source.context, operand => new SelectListObservable<T, U>(source, select, operand));

        public static IObservable<CollectionOp<ListData<U>>> ObservableSelect<T, U>(this IObservable<CollectionOp<ListData<T>>> source, Func<T, IObservable<U>> select)
            => source.ObservableSelect<T, IObservable<U>>(select).ObservableShallowCopy();

        public static IObservable<(bool found, int index)> ObservableIndexOf<T>(this IObservable<CollectionOp<ListData<T>>> source, T value)
            => source.ObservableIndexOf(new ObservableValue<T>(value));

        public static IObservable<(bool found, int index)> ObservableIndexOf<T>(this IObservable<CollectionOp<ListData<T>>> source, IObservable<T> value)
            => new ValueOperator<(bool found, int index)>(source.context, operand => new IndexOfObservable<T>(source, value, operand));

        public static IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> ObservableToDictionary<T, TKey, TValue>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<TKey>> selectKey, Func<T, IObservable<TValue>> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, selectKey, selectValue, operand));

        public static IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> ObservableToDictionary<T, TKey, TValue>(this IObservable<CollectionOp<T>> source, Func<T, TKey> selectKey, Func<T, IObservable<TValue>> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, x => new ObservableValue<TKey>(source.context, selectKey(x)), selectValue, operand));

        public static IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> ObservableToDictionary<T, TKey, TValue>(this IObservable<CollectionOp<T>> source, Func<T, IObservable<TKey>> selectKey, Func<T, TValue> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, selectKey, x => new ObservableValue<TValue>(source.context, selectValue(x)), operand));

        public static IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> ObservableToDictionary<T, TKey, TValue>(this IObservable<CollectionOp<T>> source, Func<T, TKey> selectKey, Func<T, TValue> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, x => new ObservableValue<TKey>(source.context, selectKey(x)), x => new ObservableValue<TValue>(source.context, selectValue(x)), operand));

        public static IObservable<T> AsObservable<T>(this IObservable<T> observable)
            => observable;

        public static IObservable<CollectionOp<T>> AsObservable<T>(this IObservable<CollectionOp<T>> observable)
            => observable;

        public static IObservable<CollectionOp<ListData<T>>> AsObservable<T>(this IObservable<CollectionOp<ListData<T>>> observable)
            => observable;

        public static IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> AsObservable<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> observable)
            => observable;

        public static T Peek<T>(this IObservable<T> source)
        {
            T result = default;
            source.Subscribe(x => result = x).Dispose();
            return result;
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<T>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> source, Action<CollectionOp<KeyValuePair<TKey, TValue>>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<KeyValuePair<TKey, TValue>>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<KeyValuePair<TKey, TValue>>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<TKey, TValue>(this IObservable<CollectionOp<KeyValuePair<TKey, TValue>>> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<KeyValuePair<TKey, TValue>>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<CollectionOp<ListData<T>>> source, Action<CollectionOp<ListData<T>>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<ListData<T>>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<CollectionOp<ListData<T>>> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<ListData<T>>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.value.index, op.value.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IObservable<CollectionOp<ListData<T>>> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<ListData<T>>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.value.index, op.value.element),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<CollectionOp<T>> source, Action<CollectionOp<T>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<T>>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));


        public static IDisposable Subscribe<T>(this IObservable<CollectionOp<T>> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IObservable<CollectionOp<T>> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<CollectionOp<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T1, T2>(this IObservable<(T1, T2)> source, Action<T1, T2> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<(T1, T2)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3>(this IObservable<(T1, T2, T3)> source, Action<T1, T2, T3> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<(T1, T2, T3)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this IObservable<(T1, T2, T3, T4)> source, Action<T1, T2, T3, T4> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<(T1, T2, T3, T4)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5>(this IObservable<(T1, T2, T3, T4, T5)> source, Action<T1, T2, T3, T4, T5> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<(T1, T2, T3, T4, T5)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6>(this IObservable<(T1, T2, T3, T4, T5, T6)> source, Action<T1, T2, T3, T4, T5, T6> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<(T1, T2, T3, T4, T5, T6)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6, T7>(this IObservable<(T1, T2, T3, T4, T5, T6, T7)> source, Action<T1, T2, T3, T4, T5, T6, T7> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<(T1, T2, T3, T4, T5, T6, T7)>(onNext: x => onNext?.Invoke(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7), onError: onError, onDispose: onDispose, immediate: immediate));
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