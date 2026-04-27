using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObservable
    {
        IDisposable Subscribe(IObserver observer);
    }

    public interface IObservable<out T> : IObservable
    {
        IDisposable Subscribe(IObserver<T> observer);
    }

    public interface IValueObservable<out T>
    {
        IDisposable Subscribe(IValueObserver<T> observer);
    }

    public interface ICollectionObservable<out T>
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
        public static IObservable ObservableCombine(params IObservable[] observables)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty: true);

        public static IObservable ObservableCombine(ObservationContext context, params IObservable[] observables)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty: true, context);

        public static IObservable ObservableCombine(bool disposeOnSourceEmpty, params IObservable[] observables)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty: disposeOnSourceEmpty);

        public static IObservable ObservableCombine(ObservationContext context, bool disposeOnSourceEmpty, params IObservable[] observables)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty, context);

        public static IObservable ObservableCombine(IEnumerable<IObservable> observables, bool disposeOnSourceEmpty = true, ObservationContext context = default)
            => new ObservableSet<IObservable>(observables).ObservableCombine(disposeOnSourceEmpty, context);

        public static IObservable ObservableCombine(this ISetObservable<IObservable> source, bool disposeOnSourceEmpty = false, ObservationContext context = default)
            => new CombineObservable(context, source, disposeOnSourceEmpty);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, IValueObservable<TResult>> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, context).ObservableSelect(x => select(x.Item1, x.Item2), context);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, Func<T1, T2, TResult> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, context).ObservableSelect(x => select(x.Item1, x.Item2), context);

        public static IValueObservable<(T1, T2)> ObservableCombineValues<T1, T2>(IValueObservable<T1> source1, IValueObservable<T2> source2, ObservationContext context = default)
            => new ValueOperator<(T1, T2)>(context, receiver => new CombineValueObservable<T1, T2>(source1, source2, receiver));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, IValueObservable<TResult>> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3), context);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, Func<T1, T2, T3, TResult> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3), context);

        public static IValueObservable<(T1, T2, T3)> ObservableCombineValues<T1, T2, T3>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, ObservationContext context = default)
            => new ValueOperator<(T1, T2, T3)>(context, receiver => new CombineValueObservable<T1, T2, T3>(source1, source2, source3, receiver));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, IValueObservable<TResult>> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4), context);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, Func<T1, T2, T3, T4, TResult> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4), context);

        public static IValueObservable<(T1, T2, T3, T4)> ObservableCombineValues<T1, T2, T3, T4>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, ObservationContext context = default)
            => new ValueOperator<(T1, T2, T3, T4)>(context, receiver => new CombineValueObservable<T1, T2, T3, T4>(source1, source2, source3, source4, receiver));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, IValueObservable<TResult>> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, source5, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5), context);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, Func<T1, T2, T3, T4, T5, TResult> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, source5, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5), context);

        public static IValueObservable<(T1, T2, T3, T4, T5)> ObservableCombineValues<T1, T2, T3, T4, T5>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, ObservationContext context = default)
            => new ValueOperator<(T1, T2, T3, T4, T5)>(context, receiver => new CombineValueObservable<T1, T2, T3, T4, T5>(source1, source2, source3, source4, source5, receiver));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, IValueObservable<TResult>> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6), context);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, Func<T1, T2, T3, T4, T5, T6, TResult> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6), context);

        public static IValueObservable<(T1, T2, T3, T4, T5, T6)> ObservableCombineValues<T1, T2, T3, T4, T5, T6>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, ObservationContext context = default)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6)>(context, receiver => new CombineValueObservable<T1, T2, T3, T4, T5, T6>(source1, source2, source3, source4, source5, source6, receiver));

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, IValueObservable<TResult>> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7), context);

        public static IValueObservable<TResult> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7, TResult>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, Func<T1, T2, T3, T4, T5, T6, T7, TResult> select, ObservationContext context = default)
            => ObservableCombineValues(source1, source2, source3, source4, source5, source6, source7, context).ObservableSelect(x => select(x.Item1, x.Item2, x.Item3, x.Item4, x.Item5, x.Item6, x.Item7), context);

        public static IValueObservable<(T1, T2, T3, T4, T5, T6, T7)> ObservableCombineValues<T1, T2, T3, T4, T5, T6, T7>(IValueObservable<T1> source1, IValueObservable<T2> source2, IValueObservable<T3> source3, IValueObservable<T4> source4, IValueObservable<T5> source5, IValueObservable<T6> source6, IValueObservable<T7> source7, ObservationContext context = default)
            => new ValueOperator<(T1, T2, T3, T4, T5, T6, T7)>(context, receiver => new CombineValueObservable<T1, T2, T3, T4, T5, T6, T7>(source1, source2, source3, source4, source5, source6, source7, receiver));

        public static IValueObservable<T> ObservableShallowCopy<T>(this IValueObservable<IValueObservable<T>> source, ObservationContext context = default)
            => new ValueOperator<T>(context, receiver => new ShallowCopyValueObservable<T>(source, receiver));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, U> select, ObservationContext context = default)
            => new ValueOperator<U>(context, receiver => new SelectValueObservable<T, U>(source, select, receiver));

        public static IValueObservable<U> ObservableSelect<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select, ObservationContext context = default)
            => source.ObservableSelect<T, IValueObservable<U>>(select, context).ObservableShallowCopy(context);

        public static IValueObservable<(T current, T previous)> ObservableWithPrevious<T>(this IValueObservable<T> source, ObservationContext context = default)
            => new ValueOperator<(T current, T previous)>(context, receiver => new WithPreviousObservable<T>(source, receiver));

        public static IValueObservable<T> ObservableSkipWhile<T>(this IValueObservable<T> source, Func<bool> skipWhile, ObservationContext context = default)
            => new ValueOperator<T>(context, receiver => new SkipWhileObservable<T>(source, skipWhile, receiver));

        public static ICollectionObservable<T> ObservableShallowCopy<T>(this ICollectionObservable<IValueObservable<T>> source, ObservationContext context = default)
            => new CollectionOperator<T>(context, receiver => new ShallowCopyCollectionObservable<T>(source, receiver));

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select, ObservationContext context = default)
            => source.ObservableSelect<T, IValueObservable<U>>(select, context).ObservableShallowCopy(context);

        public static ICollectionObservable<U> ObservableSelect<T, U>(this ICollectionObservable<T> source, Func<T, U> select, ObservationContext context = default)
            => new CollectionOperator<U>(context, receiver => new SelectCollectionObservable<T, U>(source, select, receiver));

        public static ISetObservable<T> ObservableDistinct<T>(this ICollectionObservable<T> source, ObservationContext context = default)
            => new SetOperator<T>(context, receiver => new DistinctObservable<T>(source, receiver));

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, bool> where, ObservationContext context = default)
            => source.ObservableWhere(x => new ObservableValue<bool>(where(x)), context);

        public static ICollectionObservable<T> ObservableWhere<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where, ObservationContext context = default)
            => new CollectionOperator<T>(context, receiver => new WhereObservable<T>(source, where, receiver));

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2, ObservationContext context = default)
            => source1.ObservableConcat((ICollectionObservable<T>)new ObservableReadOnlyCollection<T>(source2), context);

        public static ICollectionObservable<T> ObservableConcat<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2, ObservationContext context = default)
            => new CollectionOperator<T>(context, receiver => new ConcatObservable<T>(source1, source2, receiver));

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany, ObservationContext context = default)
            => source.ObservableSelectMany(x => (ICollectionObservable<U>)new ObservableReadOnlyCollection<U>(selectMany(x)), context);

        public static ICollectionObservable<U> ObservableSelectMany<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany, ObservationContext context = default)
            => new CollectionOperator<U>(context, receiver => new SelectManyObservable<T, U>(source, selectMany, receiver));

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy, ObservationContext context = default)
            => source.ObservableOrderBy<T, U>(x => new ObservableValue<U>(orderBy(x)), context);

        public static IListObservable<T> ObservableOrderBy<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy, ObservationContext context = default)
            => new ListOperator<T>(context, receiver => new OrderByObservable<T, U>(source, orderBy, false, receiver));

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, U> orderBy, ObservationContext context = default)
            => source.ObservableOrderByDescending<T, U>(x => new ObservableValue<U>(orderBy(x)), context);

        public static IListObservable<T> ObservableOrderByDescending<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy, ObservationContext context = default)
            => new ListOperator<T>(context, receiver => new OrderByObservable<T, U>(source, orderBy, true, receiver));

        public static IValueObservable<int> ObservableCount<T>(this ICollectionObservable<T> source, ObservationContext context = default)
            => new ValueOperator<int>(context, receiver => new CountObservable<T>(source, receiver));

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, T contains, ObservationContext context = default)
            => source.ObservableContains(new ObservableValue<T>(contains), context);

        public static IValueObservable<bool> ObservableContains<T>(this ICollectionObservable<T> source, IValueObservable<T> contains, ObservationContext context = default)
            => new ValueOperator<bool>(context, receiver => new ContainsObservable<T>(source, contains, receiver));

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, bool> validate, ObservationContext context = default)
            => source.ObservableFirstOrDefault(x => new ObservableValue<bool>(validate(x)), context);

        public static IValueObservable<T> ObservableFirstOrDefault<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate, ObservationContext context = default)
            => source.ObservableFirst(validate, context).ObservableSelect(x => x.found ? x.value : default, context);

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, bool> validate, ObservationContext context = default)
            => source.ObservableFirst(x => new ObservableValue<bool>(validate(x)), context);

        public static IValueObservable<(bool found, T value)> ObservableFirst<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> validate, ObservationContext context = default)
            => new ValueOperator<(bool found, T value)>(context, receiver => new FirstObservable<T>(source, validate, receiver));

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, TKey key, ObservationContext context = default)
            => source.ObservableTrack(new ObservableValue<TKey>(key), context);

        public static IValueObservable<(bool keyPresent, TValue value)> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, IValueObservable<TKey> key, ObservationContext context = default)
            => new ValueOperator<(bool keyPresent, TValue value)>(context, receiver => new TrackObservable<TKey, TValue>(source, key, receiver));

        public static ICollectionObservable<TValue> ObservableTrack<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, ICollectionObservable<TKey> keys, ObservationContext context = default)
            => keys.ObservableSelect(x => source.ObservableTrack(x), context).ObservableWhere(x => x.keyPresent, context).ObservableSelect(x => x.value, context);

        public static IListObservable<T> ObservableShallowCopy<T>(this IListObservable<IValueObservable<T>> source, ObservationContext context = default)
            => new ListOperator<T>(context, receiver => new ShallowCopyListObservable<T>(source, receiver));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, U> select, ObservationContext context = default)
            => new ListOperator<U>(context, receiver => new SelectListObservable<T, U>(source, select, receiver));

        public static IListObservable<U> ObservableSelect<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select, ObservationContext context = default)
            => source.ObservableSelect<T, IValueObservable<U>>(select, context).ObservableShallowCopy(context);

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, T value, ObservationContext context = default)
            => source.ObservableIndexOf(new ObservableValue<T>(value), context);

        public static IValueObservable<int> ObservableIndexOf<T>(this IListObservable<T> source, IValueObservable<T> value, ObservationContext context = default)
            => new ValueOperator<int>(context, receiver => new IndexOfObservable<T>(source, value, receiver));

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

        public static IDisposable Subscribe(this IObservable source, Action<IReadOnlyList<IOperation>> onOperation = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer(
                onOperation: onOperation,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<IReadOnlyList<T>> onOperation = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new Observer<T>(
                onOperation: onOperation,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ValueObserver<T>(
                onNext: onNext,
                onError: onError,
                onDispose: onDispose,
                immediate: immediate
            ));

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: onAdd == null ? null : (_, x) => onAdd(x),
                onRemove: onRemove == null ? null : (_, x) => onRemove(x),
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

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new ListObserver<T>(
                onAdd: onAdd == null ? null : (_, index, x) => onAdd(index, x),
                onRemove: onRemove == null ? null : (_, index, x) => onRemove(index, x),
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

        public static IDisposable Subscribe<T>(this ISetObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new SetObserver<T>(
                onAdd: onAdd == null ? null : (_, x) => onAdd(x),
                onRemove: onRemove == null ? null : (_, x) => onRemove(x),
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

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default, bool immediate = false)
            => source.Subscribe(new CollectionObserver<T>(
                onAdd: onAdd == null ? null : (_, x) => onAdd(x),
                onRemove: onRemove == null ? null : (_, x) => onRemove(x),
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