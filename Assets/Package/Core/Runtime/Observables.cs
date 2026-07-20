using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public static class Observables
    {
        public static IObservable<IBatchOperation<T>> ObservableBatch<T>(this IObservable<T> source) where T : IOperation
            => new ObservableOperator<IBatchOperation<T>>(source.context, operand => new BatchObservable<T>(source, operand));

        public static IObservable<T> ObservableCombine<T>(this IObservable<ISetOperation<IObservable<T>>> source, bool disposeOnSourceEmpty = true) where T : IOperation
            => new ObservableOperator<T>(source.context, operand => new CombineObservable<T>(source, operand, disposeOnSourceEmpty));

        public static IObservable<T> ObservableCombine<T>(params IObservable<T>[] observables) where T : IOperation
            => new ObservableSet<IObservable<T>>(observables).ObservableCombine(disposeOnSourceEmpty: true);

        public static IObservable<T> ObservableCombine<T>(bool disposeOnSourceEmpty, params IObservable<T>[] observables) where T : IOperation
            => new ObservableSet<IObservable<T>>(observables).ObservableCombine(disposeOnSourceEmpty: disposeOnSourceEmpty);

        public static IObservable<T> ObservableCombine<T>(IEnumerable<IObservable<T>> observables, bool disposeOnSourceEmpty = true) where T : IOperation
            => new ObservableSet<IObservable<T>>(observables).ObservableCombine(disposeOnSourceEmpty);

        public static IObservable<T> ObservableOnEach<T>(this IObservable<T> source, IObserver<T> thenObserver) where T : IOperation
            => new ObservableOperator<T>(source.context, operand => new OnEachObservable<T>(source, thenObserver, operand));

        public static IObservable<T> ObservableOnEach<T>(this IObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default) where T : IOperation
            => source.ObservableOnEach(new Observer<T>(onNext, onError, onDispose));

        public static IObservable<IOperation<(T1, T2)>> ObservableCombineValues<T1, T2>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2)
            => new ValueOperator<(T1, T2)>(source1.context, operand => new CombineValueObservable<T1, T2>(source1, source2, operand));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, Func<T1, T2, IObservable<IOperation<TResult>>> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, Func<T1, T2, TResult> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IObservable<IOperation<(T1, T2, T3)>> ObservableCombineValues<T1, T2, T3>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3)
            => new ValueOperator<(T1, T2, T3)>(source1.context, operand => new CombineValueObservable<T1, T2, T3>(source1, source2, source3, operand));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, Func<T1, T2, T3, IObservable<IOperation<TResult>>> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, Func<T1, T2, T3, TResult> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IObservable<IOperation<(T1, T2, T3, T4)>> ObservableCombineValues<T1, T2, T3, T4>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4)
            => new ValueOperator<(T1, T2, T3, T4)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4, operand));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, Func<T1, T2, T3, T4, IObservable<IOperation<TResult>>> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, Func<T1, T2, T3, T4, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IObservable<IOperation<(T1, T2, T3, T4, T5)>> ObservableCombineValues<T1, T2, T3, T4, T5>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5)
            => new ValueOperator<(T1, T2, T3, T4, T5)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5, operand));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, Func<T1, T2, T3, T4, T5, IObservable<IOperation<TResult>>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, Func<T1, T2, T3, T4, T5, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IObservable<IOperation<(T1, T2, T3, T4, T5, T6)>> ObservableCombineValues<T1, T2, T3, T4, T5, T6>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, IObservable<IOperation<T6>> source6)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6, operand));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, IObservable<IOperation<T6>> source6, Func<T1, T2, T3, T4, T5, T6, IObservable<IOperation<TResult>>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, IObservable<IOperation<T6>> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IObservable<IOperation<(T1, T2, T3, T4, T5, T6, T7)>> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, IObservable<IOperation<T6>> source6, IObservable<IOperation<T7>> source7)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6, T7)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7, operand));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, IObservable<IOperation<T6>> source6, IObservable<IOperation<T7>> source7, Func<T1, T2, T3, T4, T5, T6, T7, IObservable<IOperation<TResult>>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IObservable<IOperation<TResult>> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IObservable<IOperation<T1>> source1, IObservable<IOperation<T2>> source2, IObservable<IOperation<T3>> source3, IObservable<IOperation<T4>> source4, IObservable<IOperation<T5>> source5, IObservable<IOperation<T6>> source6, IObservable<IOperation<T7>> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IObservable<IOperation<T>> ObservableShallowCopy<T>(this IObservable<IOperation<IObservable<IOperation<T>>>> source)
            => new ValueOperator<T>(source.context, operand => new ShallowCopyValueObservable<T>(source, operand));

        public static IObservable<IOperation<U>> ObservableSelect<T, U>(this IObservable<IOperation<T>> source, Func<T, U> select)
            => new ValueOperator<U>(source.context, operand => new SelectValueOperator<T, U>(source, select, operand));

        public static IObservable<IOperation<U>> ObservableSelect<T, U>(this IObservable<IOperation<T>> source, Func<T, IObservable<IOperation<U>>> select)
            => source.ObservableSelect<T, IObservable<IOperation<U>>>(select).ObservableShallowCopy();

        public static IObservable<IOperation<(T current, T previous)>> ObservableWithPrevious<T>(this IObservable<IOperation<T>> source)
            => new ValueOperator<(T current, T previous)>(source.context, operand => new WithPreviousObservable<T>(source, operand));

        public static IObservable<IOperation<T>> ObservableSkipWhile<T>(this IObservable<IOperation<T>> source, Func<bool> skipWhile)
            => new ValueOperator<T>(source.context, operand => new SkipWhileObservable<T>(source, skipWhile, operand));

        public static IObservable<ICollectionOperation<T>> ObservableShallowCopy<T>(this IObservable<ICollectionOperation<IObservable<IOperation<T>>>> source)
            => new CollectionOperator<T>(source.context, operand => new ShallowCopyCollectionObservable<T>(source, operand));

        public static IObservable<ICollectionOperation<U>> ObservableSelect<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<U>>> select)
            => source.ObservableSelect<T, IObservable<IOperation<U>>>(select).ObservableShallowCopy();

        public static IObservable<ICollectionOperation<U>> ObservableSelect<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, U> select)
            => new CollectionOperator<U>(source.context, operand => new SelectCollectionObservable<T, U>(source, select, operand));

        public static IObservable<ISetOperation<T>> ObservableDistinct<T>(this IObservable<ICollectionOperation<T>> source)
            => new SetOperator<T>(source.context, operand => new DistinctObservable<T>(source, operand));

        public static IObservable<ICollectionOperation<T>> ObservableWhere<T>(this IObservable<ICollectionOperation<T>> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ObservableValue<bool>(where(x)));

        public static IObservable<ICollectionOperation<T>> ObservableWhere<T>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<bool>>> where)
            => new CollectionOperator<T>(source.context, operand => new WhereObservable<T>(source, where, operand));

        public static IObservable<ICollectionOperation<T>> ObservableConcat<T>(this IObservable<ICollectionOperation<T>> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((IObservable<ICollectionOperation<T>>)new ObservableReadOnlyCollection<T>(source2));

        public static IObservable<ICollectionOperation<T>> ObservableConcat<T>(this IObservable<ICollectionOperation<T>> source1, IObservable<ICollectionOperation<T>> source2)
            => new CollectionOperator<T>(source1.context, operand => new ConcatObservable<T>(source1, source2, operand));

        public static IObservable<ICollectionOperation<U>> ObservableSelectMany<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (IObservable<ICollectionOperation<U>>)new ObservableReadOnlyCollection<U>(selectMany(x)));

        public static IObservable<ICollectionOperation<U>> ObservableSelectMany<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<ICollectionOperation<U>>> selectMany)
            => new CollectionOperator<U>(source.context, operand => new SelectManyObservable<T, U>(source, selectMany, operand));

        public static IObservable<IListOperation<T>> ObservableOrderBy<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ObservableValue<U>(orderBy(x)));

        public static IObservable<IListOperation<T>> ObservableOrderBy<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<U>>> orderBy)
            => new ListOperator<T>(source.context, operand => new OrderByObservable<T, U>(source, orderBy, false, operand));

        public static IObservable<IListOperation<T>> ObservableOrderByDescending<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, U> orderBy)
            => source.ObservableOrderByDescending<T, U>(x => new ObservableValue<U>(orderBy(x)));

        public static IObservable<IListOperation<T>> ObservableOrderByDescending<T, U>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<U>>> orderBy)
            => new ListOperator<T>(source.context, operand => new OrderByObservable<T, U>(source, orderBy, true, operand));

        public static IObservable<IOperation<int>> ObservableCount<T>(this IObservable<ICollectionOperation<T>> source)
            => new ValueOperator<int>(source.context, operand => new CountObservable<T>(source, operand));

        public static IObservable<IOperation<bool>> ObservableContains<T>(this IObservable<ICollectionOperation<T>> source, T contains)
            => source.ObservableContains(new ObservableValue<T>(contains));

        public static IObservable<IOperation<bool>> ObservableContains<T>(this IObservable<ICollectionOperation<T>> source, IObservable<IOperation<T>> contains)
            => new ValueOperator<bool>(source.context, operand => new ContainsObservable<T>(source, contains, operand));

        public static IObservable<IOperation<T>> ObservableFirstOrDefault<T>(this IObservable<ICollectionOperation<T>> source, Func<T, bool> validate)
            => source.ObservableFirstOrDefault(x => new ObservableValue<bool>(validate(x)));

        public static IObservable<IOperation<T>> ObservableFirstOrDefault<T>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<bool>>> validate)
            => source.ObservableFirst(validate).ObservableSelect(x => x.found ? x.value : default);

        public static IObservable<IOperation<(bool found, T value)>> ObservableFirst<T>(this IObservable<ICollectionOperation<T>> source, Func<T, bool> validate)
            => source.ObservableFirst(x => new ObservableValue<bool>(validate(x)));

        public static IObservable<IOperation<(bool found, T value)>> ObservableFirst<T>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<bool>>> validate)
            => new ValueOperator<(bool found, T value)>(source.context, operand => new FirstObservable<T>(source, validate, operand));

        public static IObservable<IOperation<(bool keyPresent, TValue value)>> ObservableTrack<TKey, TValue>(this IObservable<IDictionaryOperation<TKey, TValue>> source, TKey key)
            => source.ObservableTrack(new ObservableValue<TKey>(key));

        public static IObservable<IOperation<(bool keyPresent, TValue value)>> ObservableTrack<TKey, TValue>(this IObservable<IDictionaryOperation<TKey, TValue>> source, IObservable<IOperation<TKey>> key)
            => new ValueOperator<(bool keyPresent, TValue value)>(source.context, operand => new TrackObservable<TKey, TValue>(source, key, operand));

        public static IObservable<ICollectionOperation<TValue>> ObservableTrack<TKey, TValue>(this IObservable<IDictionaryOperation<TKey, TValue>> source, IObservable<ICollectionOperation<TKey>> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(((bool keyPresent, TValue value) x) => x.value);

        public static IObservable<IListOperation<T>> ObservableShallowCopy<T>(this IObservable<IListOperation<IObservable<IOperation<T>>>> source)
            => new ListOperator<T>(source.context, operand => new ShallowCopyListObservable<T>(source, operand));

        public static IObservable<IListOperation<U>> ObservableSelect<T, U>(this IObservable<IListOperation<T>> source, Func<T, U> select)
            => new ListOperator<U>(source.context, operand => new SelectListObservable<T, U>(source, select, operand));

        public static IObservable<IListOperation<U>> ObservableSelect<T, U>(this IObservable<IListOperation<T>> source, Func<T, IObservable<IOperation<U>>> select)
            => source.ObservableSelect<T, IObservable<IOperation<U>>>(select).ObservableShallowCopy();

        public static IObservable<IOperation<(bool found, int index)>> ObservableIndexOf<T>(this IObservable<IListOperation<T>> source, T value)
            => source.ObservableIndexOf(new ObservableValue<T>(value));

        public static IObservable<IOperation<(bool found, int index)>> ObservableIndexOf<T>(this IObservable<IListOperation<T>> source, IObservable<IOperation<T>> value)
            => new ValueOperator<(bool found, int index)>(source.context, operand => new IndexOfObservable<T>(source, value, operand));

        public static IObservable<IDictionaryOperation<TKey, TValue>> ObservableToDictionary<T, TKey, TValue>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<TKey>>> selectKey, Func<T, IObservable<IOperation<TValue>>> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, selectKey, selectValue, operand));

        public static IObservable<IDictionaryOperation<TKey, TValue>> ObservableToDictionary<T, TKey, TValue>(this IObservable<ICollectionOperation<T>> source, Func<T, TKey> selectKey, Func<T, IObservable<IOperation<TValue>>> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, x => new ObservableValue<TKey>(source.context, selectKey(x)), selectValue, operand));

        public static IObservable<IDictionaryOperation<TKey, TValue>> ObservableToDictionary<T, TKey, TValue>(this IObservable<ICollectionOperation<T>> source, Func<T, IObservable<IOperation<TKey>>> selectKey, Func<T, TValue> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, selectKey, x => new ObservableValue<TValue>(source.context, selectValue(x)), operand));

        public static IObservable<IDictionaryOperation<TKey, TValue>> ObservableToDictionary<T, TKey, TValue>(this IObservable<ICollectionOperation<T>> source, Func<T, TKey> selectKey, Func<T, TValue> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, x => new ObservableValue<TKey>(source.context, selectKey(x)), x => new ObservableValue<TValue>(source.context, selectValue(x)), operand));

        public static IObservable<IOperation<T>> AsObservable<T>(this IObservable<IOperation<T>> observable)
            => observable;

        public static IObservable<ICollectionOperation<T>> AsObservable<T>(this IObservable<ICollectionOperation<T>> observable)
            => observable;

        public static IObservable<IListOperation<T>> AsObservable<T>(this IObservable<IListOperation<T>> observable)
            => observable;

        public static IObservable<IDictionaryOperation<TKey, TValue>> AsObservable<TKey, TValue>(this IObservable<IDictionaryOperation<TKey, TValue>> observable)
            => observable;

        public static IObservable<ISetOperation<T>> AsObservable<T>(this IObservable<ISetOperation<T>> observable)
            => observable;

        public static T Peek<T>(this IObservable<IOperation<T>> source)
        {
            T result = default;
            source.Subscribe(x => result = x).Dispose();
            return result;
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onOperation = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false) where T : IOperation
            => source.Subscribe(new Observer<T>(
                onNext: onOperation,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<IOperation<T>> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<T>>(
                onNext: onNext == null ? null : op => onNext(op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IObservable<IDictionaryOperation<TKey, TValue>> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<TKey, TValue>(this IObservable<IDictionaryOperation<TKey, TValue>> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IDictionaryOperation<TKey, TValue>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<IListOperation<T>> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.index, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IObservable<IListOperation<T>> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IListOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.index, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<ICollectionOperation<T>> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IObservable<ICollectionOperation<T>> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ICollectionOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<ISetOperation<T>> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable SubscribeWithId<T>(this IObservable<ISetOperation<T>> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<ISetOperation<T>>(
                onNext: op => (op.opType == OpType.Add ? onAdd : onRemove)?.Invoke(op.elementId, op.value),
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T1, T2>(this IObservable<IOperation<(T1, T2)>> source, Action<T1, T2> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<(T1, T2)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3>(this IObservable<IOperation<(T1, T2, T3)>> source, Action<T1, T2, T3> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<(T1, T2, T3)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4>(this IObservable<IOperation<(T1, T2, T3, T4)>> source, Action<T1, T2, T3, T4> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<(T1, T2, T3, T4)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5>(this IObservable<IOperation<(T1, T2, T3, T4, T5)>> source, Action<T1, T2, T3, T4, T5> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<(T1, T2, T3, T4, T5)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4, x.value.Item5), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6>(this IObservable<IOperation<(T1, T2, T3, T4, T5, T6)>> source, Action<T1, T2, T3, T4, T5, T6> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<(T1, T2, T3, T4, T5, T6)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4, x.value.Item5, x.value.Item6), onError: onError, onDispose: onDispose, immediate: immediate));

        public static IDisposable Subscribe<T1, T2, T3, T4, T5, T6, T7>(this IObservable<IOperation<(T1, T2, T3, T4, T5, T6, T7)>> source, Action<T1, T2, T3, T4, T5, T6, T7> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<IOperation<(T1, T2, T3, T4, T5, T6, T7)>>(onNext: x => onNext?.Invoke(x.value.Item1, x.value.Item2, x.value.Item3, x.value.Item4, x.value.Item5, x.value.Item6, x.value.Item7), onError: onError, onDispose: onDispose, immediate: immediate));
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