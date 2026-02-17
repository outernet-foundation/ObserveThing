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

    public class Observer : IObserver
    {
        public Action onChange { get; set; }
        public Action<Exception> onError { get; set; }
        public Action onDispose { get; set; }
    }

    public interface IObserver
    {
        Action onChange { get; }
        Action<Exception> onError { get; }
        Action onDispose { get; }
    }

    public class ValueObserver<T> : IValueObserver<T>
    {
        public Action<T> onNext { get; set; }
        public Action<Exception> onError { get; set; }
        public Action onDispose { get; set; }
    }

    public interface IValueObserver<in T>
    {
        Action<T> onNext { get; }
        Action<Exception> onError { get; }
        Action onDispose { get; }
    }

    public class CollectionObserver<T> : ICollectionObserver<T>
    {
        public Action<T> onAdd { get; set; }
        public Action<T> onRemove { get; set; }
        public Action<Exception> onError { get; set; }
        public Action onDispose { get; set; }
    }

    public interface ICollectionObserver<in T>
    {
        Action<T> onAdd { get; }
        Action<T> onRemove { get; }
        Action<Exception> onError { get; }
        Action onDispose { get; }
    }

    public class ListObserver<T> : IListObserver<T>
    {
        public Action<int, T> onAdd { get; set; }
        public Action<int, T> onRemove { get; set; }
        public Action<Exception> onError { get; set; }
        public Action onDispose { get; set; }
    }

    public interface IListObserver<in T>
    {
        Action<int, T> onAdd { get; }
        Action<int, T> onRemove { get; }
        Action<Exception> onError { get; }
        Action onDispose { get; }
    }

    public class DictionaryObserver<TKey, TValue> : IDictionaryObserver<TKey, TValue>
    {
        public Action<KeyValuePair<TKey, TValue>> onAdd { get; set; }
        public Action<KeyValuePair<TKey, TValue>> onRemove { get; set; }
        public Action<Exception> onError { get; set; }
        public Action onDispose { get; set; }
    }

    public interface IDictionaryObserver<TKey, TValue>
    {
        Action<KeyValuePair<TKey, TValue>> onAdd { get; }
        Action<KeyValuePair<TKey, TValue>> onRemove { get; }
        Action<Exception> onError { get; }
        Action onDispose { get; }
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
            => Subscribe(new ValueObserver<T>()
            {
                onNext = _ => observer.onChange?.Invoke(),
                onError = observer.onError,
                onDispose = observer.onDispose
            });
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
            => Subscribe(new CollectionObserver<T>()
            {
                onAdd = _ => observer.onChange?.Invoke(),
                onRemove = _ => observer.onChange?.Invoke(),
                onError = observer.onError,
                onDispose = observer.onDispose
            });
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
            => Subscribe(new ListObserver<T>()
            {
                onAdd = (_, _) => observer.onChange?.Invoke(),
                onRemove = (_, _) => observer.onChange?.Invoke(),
                onError = observer.onError,
                onDispose = observer.onDispose
            });

        public IDisposable Subscribe(ICollectionObserver<T> observer)
            => Subscribe(new ListObserver<T>()
            {
                onAdd = (_, x) => observer.onAdd?.Invoke(x),
                onRemove = (_, x) => observer.onRemove?.Invoke(x),
                onError = observer.onError,
                onDispose = observer.onDispose
            });
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
            => _subscribe(new DictionaryObserver<TKey, TValue>()
            {
                onAdd = observer.onAdd,
                onRemove = observer.onRemove,
                onError = observer.onError,
                onDispose = observer.onDispose
            });

        public IDisposable Subscribe(IObserver observer)
            => _subscribe(new DictionaryObserver<TKey, TValue>()
            {
                onAdd = _ => observer.onChange?.Invoke(),
                onRemove = _ => observer.onChange?.Invoke(),
                onError = observer.onError,
                onDispose = observer.onDispose
            });
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
        {
            return new FactoryValueObservable<T>(receiver =>
            {
                IDisposable nestedSubscription = default;
                bool changingNestedSource = false;

                var nestedObserver = new ValueObserver<T>()
                {
                    onNext = receiver.onNext,
                    onError = receiver.onError,
                    onDispose = () =>
                    {
                        if (!changingNestedSource)
                            receiver.onNext(default);
                    }
                };

                return source.Subscribe(new ValueObserver<IValueObservable<T>>()
                {
                    onNext = nestedObservable =>
                    {
                        changingNestedSource = true;
                        nestedSubscription?.Dispose();
                        changingNestedSource = false;

                        if (nestedObserver == null)
                        {
                            nestedSubscription = null;
                            receiver.onNext(default);
                            return;
                        }

                        nestedSubscription = nestedObservable?.Subscribe(nestedObserver);
                    },
                    onError = receiver.onError,
                    onDispose = () =>
                    {
                        receiver.onDispose();
                        nestedSubscription?.Dispose();
                    }
                });
            });
        }

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, U> select)
        {
            return new FactoryValueObservable<U>(receiver => source.Subscribe(new ValueObserver<T>()
            {
                onNext = receiver.onNext == null ? null : x => receiver.onNext(select(x)),
                onError = receiver.onError,
                onDispose = receiver.onDispose
            }));
        }

        public static IValueObservable<U> SelectDynamic<T, U>(this IValueObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public class ShallowCopyData<T>
        {
            public IDisposable subscription;
            public int count;
            public T latest;
        }

        public static ICollectionObservable<T> ShallowCopyDynamic<T>(this ICollectionObservable<IValueObservable<T>> source)
        {
            return new FactoryCollectionObservable<T>(receiver => source.ElementwiseSubscribe(
                onFirstAdd: element =>
                {
                    var data = new ShallowCopyData<T>();
                    data.subscription = element.Subscribe(x =>
                    {
                        for (int i = 0; i < data.count; i++)
                            receiver.onRemove?.Invoke(data.latest);

                        data.latest = x;

                        for (int i = 0; i < data.count; i++)
                            receiver.onAdd?.Invoke(data.latest);
                    });

                    return data;
                },
                onLastRemove: (element, data) => data.subscription.Dispose(),
                onIncrement: (element, data, count) =>
                {
                    data.count = count;
                    receiver.onAdd?.Invoke(data.latest);
                    return data;
                },
                onDecrement: (element, data, count) =>
                {
                    data.count = count;
                    receiver.onRemove?.Invoke(data.latest);
                    return data;
                },
                onError: receiver.onError,
                onDispose: x =>
                {
                    foreach (var element in x)
                        element.Value.state.subscription.Dispose();

                    receiver.onDispose?.Invoke();
                }
            ));
        }

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static ICollectionObservable<U> SelectDynamic<T, U>(this ICollectionObservable<T> source, Func<T, U> select)
        {
            return new FactoryCollectionObservable<U>(receiver => source.ElementwiseSubscribe(
                onFirstAdd: select,
                onIncrement: (_, selected, _) =>
                {
                    receiver.onAdd?.Invoke(selected);
                    return selected;
                },
                onDecrement: (_, selected, _) =>
                {
                    receiver.onRemove?.Invoke(selected);
                    return selected;
                },
                onError: receiver.onError,
                onDispose: receiver.onDispose == null ? null : _ => receiver.onDispose.Invoke()
            ));
        }

        public static ICollectionObservable<T> DistinctDynamic<T>(this ICollectionObservable<T> source)
        {
            return new FactoryCollectionObservable<T>(receiver =>
            {
                return source.ElementwiseSubscribe(
                    onFirstAdd: x => receiver.onAdd?.Invoke(x),
                    onLastRemove: x => receiver.onRemove?.Invoke(x),
                    onError: receiver.onError,
                    onDispose: receiver.onDispose
                );
            });
        }

        public class WhereDynamicData<T>
        {
            public IDisposable subscription;
            public bool included;
            public int count;
        }

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, bool> where)
            => source.WhereDynamic(x => new ValueObservable<bool>(where(x)));

        public static ICollectionObservable<T> WhereDynamic<T>(this ICollectionObservable<T> source, Func<T, IValueObservable<bool>> where)
        {
            return new FactoryCollectionObservable<T>(receiver => source.ElementwiseSubscribe(
                onFirstAdd: x =>
                {
                    var data = new WhereDynamicData<T>();
                    data.subscription = where(x).Subscribe(included =>
                    {
                        if (included == data.included)
                            return;

                        data.included = included;

                        if (included)
                        {
                            for (int i = 0; i < data.count; i++)
                                receiver.onAdd?.Invoke(x);
                        }
                        else
                        {
                            for (int i = 0; i < data.count; i++)
                                receiver.onRemove?.Invoke(x);
                        }
                    });

                    return data;
                },
                onLastRemove: (x, data) => data.subscription.Dispose(),
                onIncrement: (x, data, count) =>
                {
                    data.count = count;

                    if (data.included)
                        receiver.onAdd?.Invoke(x);

                    return data;
                },
                onDecrement: (x, data, count) =>
                {
                    data.count = count;

                    if (data.included)
                        receiver.onRemove?.Invoke(x);

                    return data;
                },
                onError: receiver.onError,
                onDispose: x =>
                {
                    foreach (var element in x)
                        element.Value.state.subscription.Dispose();

                    receiver?.onDispose();
                }
            ));
        }

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, IEnumerable<T> source2)
            => source1.ConcatDynamic((ICollectionObservable<T>)new CollectionObservable<T>(source2));

        public static ICollectionObservable<T> ConcatDynamic<T>(this ICollectionObservable<T> source1, ICollectionObservable<T> source2)
        {
            return new FactoryCollectionObservable<T>(receiver => new ComposedDisposable(
                source1.Subscribe(
                    onAdd: receiver.onAdd,
                    onRemove: receiver.onRemove,
                    onError: receiver.onError,
                    onDispose: receiver.onDispose
                ),
                source2.Subscribe(
                    onAdd: receiver.onAdd,
                    onRemove: receiver.onRemove,
                    onError: receiver.onError,
                    onDispose: receiver.onDispose
                )
            ));
        }

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IEnumerable<U>> selectMany)
            => source.SelectManyDynamic(x => (ICollectionObservable<U>)new CollectionObservable<U>(selectMany(x)));

        public class SelectManyDynamicData<T>
        {
            public List<T> selected = new List<T>();
            public IDisposable subscription;
            public int count;
        }

        public static ICollectionObservable<U> SelectManyDynamic<T, U>(this ICollectionObservable<T> source, Func<T, ICollectionObservable<U>> selectMany)
        {
            return new FactoryCollectionObservable<U>(receiver => source.ElementwiseSubscribe(
                onFirstAdd: x =>
                {
                    var data = new SelectManyDynamicData<U>();
                    data.subscription = selectMany(x).Subscribe(
                        onAdd: x =>
                        {
                            data.selected.Add(x);
                            for (int i = 0; i < data.count; i++)
                                receiver.onAdd?.Invoke(x);
                        },
                        onRemove: x =>
                        {
                            data.selected.Remove(x);
                            for (int i = 0; i < data.count; i++)
                                receiver.onRemove?.Invoke(x);
                        }
                    );

                    return data;
                },
                onLastRemove: (x, data) => data.subscription.Dispose(),
                onIncrement: (x, data, count) =>
                {
                    data.count = count;
                    foreach (var element in data.selected)
                        receiver.onAdd?.Invoke(element);

                    return data;
                },
                onDecrement: (x, data, count) =>
                {
                    data.count = count;
                    foreach (var element in data.selected)
                        receiver.onRemove?.Invoke(element);

                    return data;
                },
                onError: receiver.onError,
                onDispose: x =>
                {
                    foreach (var element in x)
                        element.Value.state.subscription.Dispose();

                    receiver?.onDispose();
                }
            ));
        }

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
            => source.OrderByDynamic(x => new ValueObservable<U>(orderBy(x)));

        public class OrderByDynamicData<T>
        {
            public int count;
            public IDisposable subscription;
        }

        public static IListObservable<T> OrderByDynamic<T, U>(this ICollectionObservable<T> source, Func<T, IValueObservable<U>> orderBy)
        {
            return new FactoryListObservable<T>(receiver =>
            {
                Comparer<U> comparer = Comparer<U>.Default;
                Dictionary<T, U> orderByElements = new Dictionary<T, U>();
                List<T> elementsInOrder = new List<T>();
                Comparison<T> comparison = (x, y) => comparer.Compare(orderByElements[x], orderByElements[y]);

                source.ElementwiseSubscribe(
                    onFirstAdd: x =>
                    {
                        var data = new OrderByDynamicData<U>();
                        data.subscription = orderBy(x).Subscribe(x =>
                        {
                            elementsInOrder.Sort(comparison);
                            for (int i = 0; i < data.count; i++)
                            {

                            }
                        });
                    },
                    onLastRemove: x =>
                    {

                    }
                );
            });
        }

        public static IValueObservable<int> CountDynamic<T>(this ICollectionObservable<T> source)
        {
            return new FactoryValueObservable<int>(receiver =>
            {
                int count = 0;
                return source.Subscribe(
                    onAdd: _ =>
                    {
                        count++;
                        receiver.onNext(count);
                    },
                    onRemove: _ =>
                    {
                        count--;
                        receiver.onNext(count);
                    },
                    receiver.onError,
                    receiver.onDispose
                );
            });
        }

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, T contains)
            => source.ContainsDynamic(new ValueObservable<T>(contains));

        public static IValueObservable<bool> ContainsDynamic<T>(this ICollectionObservable<T> source, IValueObservable<T> contains)
        {
            return new FactoryValueObservable<bool>(receiver =>
            {
                var latest = default(T);
                var present = false;
                var collection = new List<T>();

                return new ComposedDisposable(
                    contains.Subscribe(
                        onNext: x =>
                        {
                            bool wasPresent = present;
                            latest = x;
                            present = collection.Contains(x);

                            if (wasPresent == present)
                                return;

                            receiver.onNext(present);
                        },
                        onError: receiver.onError,
                        onDispose: receiver.onDispose
                    ),
                    source.Subscribe(
                        onAdd: x =>
                        {
                            collection.Add(x);

                            if (present)
                                return;

                            if (Equals(latest, x))
                            {
                                present = true;
                                receiver.onNext(true);
                            }

                        },
                        onRemove: x =>
                        {
                            collection.Remove(x);

                            if (!present)
                                return;

                            if (!collection.Contains(x))
                            {
                                present = false;
                                receiver.onNext(false);
                            }
                        },
                        onError: receiver.onError,
                        onDispose: receiver.onDispose
                    )
                );
            });
        }

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
                    receiver.onNext(new(x, previous));
                    previous = x;
                },
                onError: receiver.onError,
                onDispose: receiver.onDispose
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
                            receiver.onRemove?.Invoke(index, element.latest);

                            element = new(element.subscription, x);
                            data.Insert(index, element);

                            receiver.onAdd?.Invoke(index, x);
                        },
                        onError: receiver.onError,
                        onDispose: receiver.onDispose
                    );
                },
                onRemove: (index, x) =>
                {
                    var element = data[index];
                    data.RemoveAt(index);
                    receiver.onRemove?.Invoke(index, data[index].latest);
                },
                onError: receiver.onError,
                onDispose: receiver.onDispose
            ));
        }

        public static IListObservable<U> SelectDynamic<T, U>(this IListObservable<T> source, Func<T, U> select)
        {
            return new FactoryListObservable<U>(receiver =>
            {
                var selectedElements = new List<U>();
                return source.Subscribe(new ListObserver<T>()
                {
                    onAdd = (index, value) =>
                    {
                        var selected = select(value);
                        selectedElements.Insert(index, selected);
                        receiver.onAdd?.Invoke(index, selected);
                    },
                    onRemove = (index, _) =>
                    {
                        var selected = selectedElements[index];
                        selectedElements.RemoveAt(index);
                        receiver.onRemove?.Invoke(index, selected);
                    },
                    onError = receiver.onError,
                    onDispose = receiver.onDispose
                });
            });
        }

        public static IListObservable<U> SelectDynamic<T, U>(this IListObservable<T> source, Func<T, IValueObservable<U>> select)
            => source.SelectDynamic<T, IValueObservable<U>>(select).ShallowCopyDynamic();

        public static IValueObservable<int> IndexOfDynamic<T>(this IListObservable<T> source, T value)
            => source.IndexOfDynamic(new ValueObservable<T>(value));

        public static IValueObservable<int> IndexOfDynamic<T>(this IListObservable<T> source, IValueObservable<T> value)
        {
            return new FactoryValueObservable<int>(receiver =>
            {
                var latest = default(T);
                var list = new List<T>();
                var index = -1;

                receiver.onNext?.Invoke(-1);

                return new ComposedDisposable(
                    value.Subscribe(
                        onNext: x =>
                        {
                            latest = x;
                            var newIndex = list.IndexOf(latest);

                            if (index == newIndex)
                                return;

                            receiver.onNext?.Invoke(index);
                        },
                        onError: receiver.onError,
                        onDispose: receiver.onDispose
                    ),
                    source.Subscribe(
                        onAdd: x =>
                        {
                            list.Add(x);
                            var newIndex = list.IndexOf(latest);

                            if (index == newIndex)
                                return;

                            receiver.onNext?.Invoke(index);
                        },
                        onRemove: x =>
                        {
                            list.Remove(x);
                            var newIndex = list.IndexOf(latest);

                            if (index == newIndex)
                                return;

                            receiver.onNext?.Invoke(index);
                        },
                        onError: receiver.onError,
                        onDispose: receiver.onDispose
                    )
                );
            });
        }

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
            => source.Subscribe(new Observer() { onChange = onChange, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<T>(this IValueObservable<T> source, Action<T> onNext = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ValueObserver<T>() { onNext = onNext, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<TKey, TValue>(this IDictionaryObservable<TKey, TValue> source, Action<KeyValuePair<TKey, TValue>> onAdd = default, Action<KeyValuePair<TKey, TValue>> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new DictionaryObserver<TKey, TValue>() { onAdd = onAdd, onRemove = onRemove, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<T>(this IListObservable<T> source, Action<int, T> onAdd = default, Action<int, T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new ListObserver<T>() { onAdd = onAdd, onRemove = onRemove, onError = onError, onDispose = onDispose });

        public static IDisposable Subscribe<T>(this ICollectionObservable<T> source, Action<T> onAdd = default, Action<T> onRemove = default, Action<Exception> onError = default, Action onDispose = default)
            => source.Subscribe(new CollectionObserver<T>() { onAdd = onAdd, onRemove = onRemove, onError = onError, onDispose = onDispose });

        public static IDisposable ElementwiseSubscribe<T>(this ICollectionObservable<T> source,
            Action<T> onFirstAdd = default,
            Action<T> onLastRemove = default,
            Action<T, int> onIncrement = default,
            Action<T, int> onDecrement = default,
            Action<Exception> onError = default,
            Action onDispose = default
        )
        {
            return source.ElementwiseSubscribe(
                onFirstAdd: x =>
                {
                    onFirstAdd?.Invoke(x);
                    return false;
                },
                onLastRemove: (x, _) => onLastRemove?.Invoke(x),
                onIncrement: (x, _, count) =>
                {
                    onIncrement?.Invoke(x, count);
                    return false;
                },
                onDecrement: (x, _, count) =>
                {
                    onDecrement?.Invoke(x, count);
                    return false;
                },
                onError: onError,
                onDispose: onDispose == null ? null : _ => onDispose?.Invoke()
            );
        }

        public static IDisposable ElementwiseSubscribe<T, U>(this ICollectionObservable<T> source,
            Func<T, U> onFirstAdd = default,
            Action<T, U> onLastRemove = default,
            Func<T, U, int, U> onIncrement = default,
            Func<T, U, int, U> onDecrement = default,
            Action<Exception> onError = default,
            Action<Dictionary<T, (U state, int count)>> onDispose = default
        )
        {
            Dictionary<T, (U state, int count)> elements = new Dictionary<T, (U state, int count)>();

            return source.Subscribe(
                onAdd: x =>
                {
                    if (!elements.TryGetValue(x, out var data))
                    {
                        U state = default;
                        elements.Add(x, (state, 1));

                        if (onFirstAdd != null)
                        {
                            state = onFirstAdd(x);
                            elements[x] = (state, 1);
                        }

                        if (onIncrement != null)
                        {
                            state = onIncrement(x, state, 1);
                            elements[x] = (state, 1);
                        }

                        return;
                    }

                    elements[x] = (data.state, data.count + 1);

                    if (onIncrement != null)
                    {
                        var state = onIncrement(x, data.state, data.count + 1);
                        elements[x] = (state, data.count + 1);
                    }
                },
                onRemove: x =>
                {
                    var data = elements[x];

                    if (data.count == 1)
                    {
                        elements.Remove(x);
                        onDecrement?.Invoke(x, data.state, 0);
                        onLastRemove?.Invoke(x, data.state);
                        return;
                    }

                    if (onDecrement != null)
                    {
                        var state = onDecrement(x, data.state, data.count - 1);
                        elements[x] = (state, data.count - 1);
                    }
                },
                onError: onError,
                onDispose: onDispose == null ? null : () => onDispose?.Invoke(elements)
            );
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