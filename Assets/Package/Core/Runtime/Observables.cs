using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public static class Observables
    {
        public static IValueObservable<T> ObservableCast<T>(this IValueObservable source)
            => new ValueOperator<T>(source.context, operand => new CastValueObservable<T>(source, operand));

        public static ICollectionObservable<T> ObservableCast<T>(this ICollectionObservable source)
            => new CollectionOperator<T>(source.context, operand => new CastCollectionObservable<T>(source, operand));

        public static IListObservable<T> ObservableCast<T>(this IListObservable source)
            => new ListOperator<T>(source.context, operand => new CastListObservable<T>(source, operand));

        public static ISetObservable<T> ObservableCast<T>(this ISetObservable source)
            => new SetOperator<T>(source.context, operand => new CastSetObservable<T>(source, operand));

        public static IDictionaryObservable<TKey, TValue> ObservableCast<TKey, TValue>(this IDictionaryObservable source)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new CastDictionaryObservable<TKey, TValue>(source, operand));

        public static IObservable<ValueOp<T>> ObservableOperationStream<T>(this IValueObservable<T> source)
            => new ObservableOperator<ValueOp<T>>(source.context, operand => new ValueOperationStreamObservable<T>(source, operand));

        public static IObservable<CollectionOp<T>> ObservableOperationStream<T>(this ICollectionObservable<T> source)
            => new ObservableOperator<CollectionOp<T>>(source.context, operand => new CollectionOperationStreamObservable<T>(source, operand));

        public static IObservable<ListOp<T>> ObservableOperationStream<T>(this IListObservable<T> source)
            => new ObservableOperator<ListOp<T>>(source.context, operand => new ListOperationStreamObservable<T>(source, operand));

        public static IObservable<DictionaryOp<TKey, TValue>> ObservableOperationStream<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source)
            => new ObservableOperator<DictionaryOp<TKey, TValue>>(source.context, operand => new DictionaryOperationStreamObservable<TKey, TValue>(source, operand));

        public static IObservable<SetOp<T>> ObservableOperationStream<T>(this ISetObservable<T> source)
            => new ObservableOperator<SetOp<T>>(source.context, operand => new SetOperationStreamObservable<T>(source, operand));

        public static IObservable<BatchOp<ValueOp<T>>> ObservableBatch<T>(this IValueObservable<T> source)
            => source.ObservableOperationStream().ObservableBatch();

        public static IObservable<BatchOp<CollectionOp<T>>> ObservableBatch<T>(this ICollectionObservable<T> source)
            => source.ObservableOperationStream().ObservableBatch();

        public static IObservable<BatchOp<ListOp<T>>> ObservableBatch<T>(this IListObservable<T> source)
            => source.ObservableOperationStream().ObservableBatch();

        public static IObservable<BatchOp<DictionaryOp<TKey, TValue>>> ObservableBatch<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source)
            => source.ObservableOperationStream().ObservableBatch();

        public static IObservable<BatchOp<SetOp<T>>> ObservableBatch<T>(this ISetObservable<T> source)
            => source.ObservableOperationStream().ObservableBatch();

        public static IObservable<BatchOp<T>> ObservableBatch<T>(this IObservable<T> source) where T : IOperation
            => new ObservableOperator<BatchOp<T>>(source.context, operand => new BatchObservable<T>(source, operand));

        public static IObservable<T> ObservableCombine<T>(this ISetObservable<IObservable<T>> source, bool disposeOnSourceEmpty = true) where T : IOperation
            => new ObservableOperator<T>(source.context, operand => new CombineObservable<T>(source, operand, disposeOnSourceEmpty));

        public static IObservable<T> ObservableCombine<T>(params IObservable<T>[] observables) where T : IOperation
            => new ObservableSet<IObservable<T>>(observables[0].context, observables).ObservableCombine(disposeOnSourceEmpty: true);

        public static IObservable<T> ObservableCombine<T>(bool disposeOnSourceEmpty, params IObservable<T>[] observables) where T : IOperation
            => new ObservableSet<IObservable<T>>(observables[0].context, observables).ObservableCombine(disposeOnSourceEmpty: disposeOnSourceEmpty);

        public static IValueObservable<T> ObservableThen<T>(this IValueObservable<T> source, IValueObserver<T> thenObserver)
            => new ValueOperator<T>(source.context, operand => new ThenValueObservable<T>(source, thenObserver, operand));

        public static IValueObservable<T> ObservableThen<T>(this IValueObservable<T> source, Action<T> onNext = default, Action onDispose = default, Action<Exception> onError = default)
            => source.ObservableThen(new ValueObserver<T>(onNext, onDispose, onError));

        public static ICollectionObservable<T> ObservableThen<T>(this ICollectionObservable<T> source, ICollectionObserver<T> thenObserver)
            => new CollectionOperator<T>(source.context, operand => new ThenCollectionObservable<T>(source, thenObserver, operand));

        public static ICollectionObservable<T> ObservableThen<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
            => source.ObservableThen(new CollectionObserver<T>((id, x) => onAdd?.Invoke(x), (id, x) => onRemove?.Invoke(x), onDispose, onError));

        public static IListObservable<T> ObservableThen<T>(this IListObservable<T> source, IListObserver<T> thenObserver)
            => new ListOperator<T>(source.context, operand => new ThenListObservable<T>(source, thenObserver, operand));

        public static IListObservable<T> ObservableThen<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
            => source.ObservableThen(new ListObserver<T>((id, index, x) => onAdd?.Invoke(index, x), (id, index, x) => onRemove?.Invoke(index, x), onDispose, onError));

        public static ISetObservable<T> ObservableThen<T>(this ISetObservable<T> source, ISetObserver<T> thenObserver)
            => new SetOperator<T>(source.context, operand => new ThenSetObservable<T>(source, thenObserver, operand));

        public static ISetObservable<T> ObservableThen<T>(this ISetObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
            => source.ObservableThen(new SetObserver<T>((id, x) => onAdd?.Invoke(x), (id, x) => onRemove?.Invoke(x), onDispose, onError));

        public static IDictionaryObservable<TKey, TValue> ObservableThen<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IDictionaryObserver<TKey, TValue> thenObserver)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ThenDictionaryObservable<TKey, TValue>(source, thenObserver, operand));

        public static IDictionaryObservable<TKey, TValue> ObservableThen<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action onDispose = default, Action<Exception> onError = default)
            => source.ObservableThen(new DictionaryObserver<TKey, TValue>((id, x) => onAdd?.Invoke(x), (id, x) => onRemove?.Invoke(x), onDispose, onError));

        public static IValueObservable<(T1, T2)> ObservableCombineValues<T1, T2>(IValueObservable<T1> source1, IValueObservable<T2> source2)
            => new ValueOperator<(T1, T2)>(source1.context, operand => new CombineValueObservable<T1, T2>(source1, source2, operand));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, TResult> select)
            => ObservableCombineValues(source1, source2).ObservableSelect(x => select(x.Item1, x.Item2));

        public static IValueObservable<(T1, T2, T3)> ObservableCombineValues<T1, T2, T3>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3)
            => new ValueOperator<(T1, T2, T3)>(source1.context, operand => new CombineValueObservable<T1, T2, T3>(source1, source2, source3, operand));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, TResult> select)
            => ObservableCombineValues(source1, source2, source3).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3));

        public static IValueObservable<(T1, T2, T3, T4)> ObservableCombineValues<T1, T2, T3, T4>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4)
            => new ValueOperator<(T1, T2, T3, T4)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4, operand));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IValueObservable<(T1, T2, T3, T4, T5)> ObservableCombineValues<T1, T2, T3, T4, T5>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5)
            => new ValueOperator<(T1, T2, T3, T4, T5)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5, operand));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5));

        public static IValueObservable<(T1, T2, T3, T4, T5, T6)> ObservableCombineValues<T1, T2, T3, T4, T5, T6>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6, operand));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6));

        public static IValueObservable<(T1, T2, T3, T4, T5, T6, T7)> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6, T7)>(source1.context, operand => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7, operand));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, IValueObservable<TResult>> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7));

        public static IValueObservable<T> ObservableUnwrap<T>(this IValueObservable<IValueObservable<T>> source)
            => new ValueOperator<T>(source.context, operand => new UnwrapValueObservable<T>(source, operand));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, U> select)
            => new ValueOperator<U>(source.context, operand => new SelectValueOperator<T, U>(source, select, operand));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableUnwrap();

        public static IValueObservable<(T current, T previous)> ObservableWithPrevious<T>(this IValueObservable<T> source)
            => new ValueOperator<(T current, T previous)>(source.context, operand => new WithPreviousObservable<T>(source, operand));

        public static IValueObservable<T> ObservableSkipWhile<T>(this IValueObservable<T> source, Func<bool> skipWhile)
            => new ValueOperator<T>(source.context, operand => new SkipWhileObservable<T>(source, skipWhile, operand));

        public static ICollectionObservable<T> ObservableUnwrap<T>(this ICollectionObservable<IValueObservable<T>> source)
            => new CollectionOperator<T>(source.context, operand => new UnwrapCollectionObservable<T>(source, operand));

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableUnwrap();

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
            => new CollectionOperator<U>(source.context, operand => new SelectCollectionObservable<T, U>(source, select, operand));

        public static ISetObservable<T> ObservableDistinct<T>(this ICollectionObservable<T> source)
            => new SetOperator<T>(source.context, operand => new DistinctObservable<T>(source, operand));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.ObservableWhere(x => new ObservableValue<bool>(source.context, where(x)));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
            => new CollectionOperator<T>(source.context, operand => new WhereObservable<T>(source, where, operand));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ObservableConcat((ICollectionObservable<T>)new ObservableCollection<T>(source1.context, source2));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
            => new CollectionOperator<T>(source1.context, operand => new ConcatObservable<T>(source1, source2, operand));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.ObservableSelectMany(x => (ICollectionObservable<U>)new ObservableCollection<U>(source.context, selectMany(x)));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
            => new CollectionOperator<U>(source.context, operand => new SelectManyObservable<T, U>(source, selectMany, operand));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderBy<T, U>(x => new ObservableValue<U>(source.context, orderBy(x)));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new ListOperator<T>(source.context, operand => new OrderByObservable<T, U>(source, orderBy, false, operand));

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy)
            => source.ObservableOrderByDescending<T, U>(x => new ObservableValue<U>(source.context, orderBy(x)));

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
            => new ListOperator<T>(source.context, operand => new OrderByObservable<T, U>(source, orderBy, true, operand));

        public static IValueObservable<int> ObservableCount<T>(this ICollectionObservable<T> source)
            => new ValueOperator<int>(source.context, operand => new CountObservable<T>(source, operand));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, T contains)
            => source.ObservableContains(new ObservableValue<T>(source.context, contains));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
            => new ValueOperator<bool>(source.context, operand => new ContainsObservable<T>(source, contains, operand));

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, bool> validate)
            => source.ObservableFirstOrDefault(x => new ObservableValue<bool>(source.context, validate(x)));

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate)
            => source.ObservableFirst(validate).ObservableSelect(x => x.found ? x.value : default);

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, bool> validate)
            => source.ObservableFirst(x => new ObservableValue<bool>(source.context, validate(x)));

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate)
            => new ValueOperator<(bool found, T value)>(source.context, operand => new FirstObservable<T>(source, validate, operand));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key)
            => source.ObservableTrack(new ObservableValue<TKey>(source.context, key));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key)
            => new ValueOperator<(bool keyPresent, TValue value)>(source.context, operand => new TrackObservable<TKey, TValue>(source, key, operand));

        public static ICollectionObservable<TValue> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys)
            => keys.ObservableSelect(x => source.ObservableTrack(x)).ObservableWhere(x => x.keyPresent).ObservableSelect(((bool keyPresent, TValue value) x) => x.value);

        public static IListObservable<T> ObservableUnwrap<T>(this IListObservable<IValueObservable<T>> source)
            => new ListOperator<T>(source.context, operand => new UnwrapListObservable<T>(source, operand));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, U> select)
            => new ListOperator<U>(source.context, operand => new SelectListObservable<T, U>(source, select, operand));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.ObservableSelect<T, IValueObservable<U>>(select).ObservableUnwrap();

        public static IValueObservable<(bool found, int index)> ObservableIndexOf<T>(this IListObservable<T> source, T value)
            => source.ObservableIndexOf(new ObservableValue<T>(source.context, value));

        public static IValueObservable<(bool found, int index)> ObservableIndexOf<T>(this IListObservable<T> source, IValueObservable<T> value)
            => new ValueOperator<(bool found, int index)>(source.context, operand => new IndexOfObservable<T>(source, value, operand));

        public static IDictionaryObservable<TKey, TValue> ObservableToDictionary<T, TKey, TValue>(this ICollectionObservable<T> source, Func<T, IValueObservable<TKey>> selectKey, Func<T, IValueObservable<TValue>> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, selectKey, selectValue, operand));

        public static IDictionaryObservable<TKey, TValue> ObservableToDictionary<T, TKey, TValue>(this ICollectionObservable<T> source, Func<T, TKey> selectKey, Func<T, IValueObservable<TValue>> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, x => new ObservableValue<TKey>(source.context, selectKey(x)), selectValue, operand));

        public static IDictionaryObservable<TKey, TValue> ObservableToDictionary<T, TKey, TValue>(this ICollectionObservable<T> source, Func<T, IValueObservable<TKey>> selectKey, Func<T, TValue> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, selectKey, x => new ObservableValue<TValue>(source.context, selectValue(x)), operand));

        public static IDictionaryObservable<TKey, TValue> ObservableToDictionary<T, TKey, TValue>(this ICollectionObservable<T> source, Func<T, TKey> selectKey, Func<T, TValue> selectValue)
            => new DictionaryOperator<TKey, TValue>(source.context, operand => new ToDictionaryObservable<T, TKey, TValue>(source, x => new ObservableValue<TKey>(source.context, selectKey(x)), x => new ObservableValue<TValue>(source.context, selectValue(x)), operand));

        public static IValueObservable<T> AsObservable<T>(this IValueObservable<T> observable)
            => observable;

        public static ICollectionObservable<T> AsObservable<T>(this ICollectionObservable<T> observable)
            => observable;

        public static IListObservable<T> AsObservable<T>(this IListObservable<T> observable)
            => observable;

        public static IDictionaryObservable<TKey, TValue> AsObservable<TKey, TValue>(this IDictionaryObservable<TKey, TValue> observable)
            => observable;

        public static ISetObservable<T> AsObservable<T>(this ISetObservable<T> observable)
            => observable;

        public static T Peek<T>(this IValueObservable<T> source)
        {
            T result = default;
            source.Subscribe(x => result = x).Dispose();
            return result;
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default) where T : IOperation
            => source.Subscribe(new Observer<T>(onNext, onDispose, onError), immediate, priority);

        public static IDisposable Subscribe(this IValueObservable source, Action<object> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new ValueObserver(onNext, onDispose, onError), immediate, priority);

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new ValueObserver<T>(onNext, onDispose, onError), immediate, priority);

        public static IDisposable Subscribe<T>(this IObservable<BatchOp<T>> source, Action<IReadOnlyList<T>> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default) where T : IOperation
            => source.Subscribe(new Observer<BatchOp<T>>(onNext: x => onNext?.Invoke(x.operations), onDispose, onError), immediate, priority);

        public static IDisposable Subscribe(this IDictionaryObservable source, Action<KeyValuePair<object, object>> onAdd = default, Action<KeyValuePair<object, object>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new DictionaryObserver(
                onAdd: onAdd == null ? null : (_, kvp) => onAdd?.Invoke(kvp),
                onRemove: onRemove == null ? null : (_, kvp) => onRemove?.Invoke(kvp),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId(this IDictionaryObservable source, Action<uint, KeyValuePair<object, object>> onAdd = default, Action<uint, KeyValuePair<object, object>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new DictionaryObserver(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd == null ? null : (_, kvp) => onAdd?.Invoke(kvp),
                onRemove: onRemove == null ? null : (_, kvp) => onRemove?.Invoke(kvp),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<uint, KeyValuePair<TKey, TValue>> onAdd = default, Action<uint, KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe(this IListObservable source, Action<int, object> onAdd = default, Action<int, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new ListObserver(
                onAdd: onAdd == null ? null : (_, index, element) => onAdd?.Invoke(index, element),
                onRemove: onRemove == null ? null : (_, index, element) => onRemove?.Invoke(index, element),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId(this IListObservable source, Action<uint, int, object> onAdd = default, Action<uint, int, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new ListObserver(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd == null ? null : (_, index, element) => onAdd?.Invoke(index, element),
                onRemove: onRemove == null ? null : (_, index, element) => onRemove?.Invoke(index, element),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId<T>(this IListObservable<T> source, Action<uint, int, T> onAdd = default, Action<uint, int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe(this ICollectionObservable source, Action<object> onAdd = default, Action<object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new CollectionObserver(
                onAdd: onAdd == null ? null : (_, element) => onAdd?.Invoke(element),
                onRemove: onRemove == null ? null : (_, element) => onRemove?.Invoke(element),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId(this ICollectionObservable source, Action<uint, object> onAdd = default, Action<uint, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new CollectionObserver(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd == null ? null : (_, element) => onAdd?.Invoke(element),
                onRemove: onRemove == null ? null : (_, element) => onRemove?.Invoke(element),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId<T>(this ICollectionObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe(this ISetObservable source, Action<object> onAdd = default, Action<object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new SetObserver(
                onAdd: onAdd == null ? null : (_, element) => onAdd?.Invoke(element),
                onRemove: onRemove == null ? null : (_, element) => onRemove?.Invoke(element),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId(this ISetObservable source, Action<uint, object> onAdd = default, Action<uint, object> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new SetObserver(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable Subscribe<T>(this ISetObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd == null ? null : (_, element) => onAdd?.Invoke(element),
                onRemove: onRemove == null ? null : (_, element) => onRemove?.Invoke(element),
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);

        public static IDisposable SubscribeWithId<T>(this ISetObservable<T> source, Action<uint, T> onAdd = default, Action<uint, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = default, uint? priority = default)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd,
                onRemove: onRemove,
                onError: onError,
                onDispose: onDispose
            ), immediate, priority);
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