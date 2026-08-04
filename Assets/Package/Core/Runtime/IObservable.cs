using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IOperationObservable
    {
        ObservationContext context { get; }
        IDisposable Subscribe(IOperationObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface IValueObservable<T> : IOperationObservable
    {
        IDisposable Subscribe(IValueObserver<T> observer, bool immediate = default, uint? priority = default);
    }

    public interface ICollectionObservable : IOperationObservable
    {
        IDisposable Subscribe(ICollectionObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface ICollectionObservable<T> : ICollectionObservable
    {
        IDisposable Subscribe(ICollectionObserver<T> observer, bool immediate = default, uint? priority = default);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new CollectionObserver(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }

    public interface IListObservable : ICollectionObservable
    {
        IDisposable Subscribe(IListObserver observer, bool immediate = default, uint? priority = default);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new ListObserver(
                onAdd: (id, index, element) => observer.OnAdd(id, element),
                onRemove: (id, index, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }

    public interface IListObservable<T> : IListObservable, ICollectionObservable<T>
    {
        IDisposable Subscribe(IListObserver<T> observer, bool immediate = default, uint? priority = default);

        IDisposable IListObservable.Subscribe(IListObserver observer, bool immediate, uint? priority)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, index, element) => observer.OnAdd(id, index, element),
                onRemove: (id, index, element) => observer.OnRemove(id, index, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer, bool immediate, uint? priority)
            => Subscribe(new ListObserver<T>(
                onAdd: (id, index, element) => observer.OnAdd(id, element),
                onRemove: (id, index, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }

    public interface ISetObservable : ICollectionObservable
    {
        IDisposable Subscribe(ISetObserver observer, bool immediate = default, uint? priority = default);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new SetObserver(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }

    public interface ISetObservable<T> : ISetObservable, ICollectionObservable<T>
    {
        IDisposable Subscribe(ISetObserver<T> observer, bool immediate = default, uint? priority = default);

        IDisposable ISetObservable.Subscribe(ISetObserver observer, bool immediate, uint? priority)
            => Subscribe(new SetObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<T>.Subscribe(ICollectionObserver<T> observer, bool immediate, uint? priority)
            => Subscribe(new SetObserver<T>(
                onAdd: (id, element) => observer.OnAdd(id, element),
                onRemove: (id, element) => observer.OnRemove(id, element),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }

    public interface IDictionaryObservable : ICollectionObservable
    {
        IDisposable Subscribe(IDictionaryObserver observer, bool immediate = default, uint? priority = default);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }

    public interface IDictionaryObservable<TKey, TValue> : IDictionaryObservable, ICollectionObservable<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer, bool immediate = default, uint? priority = default);

        IDisposable IDictionaryObservable.Subscribe(IDictionaryObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, new KeyValuePair<object, object>(kvp.Key, kvp.Value)),
                onRemove: (id, kvp) => observer.OnRemove(id, new KeyValuePair<object, object>(kvp.Key, kvp.Value)),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable<KeyValuePair<TKey, TValue>>.Subscribe(ICollectionObserver<KeyValuePair<TKey, TValue>> observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver<TKey, TValue>(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);

        IDisposable ICollectionObservable.Subscribe(ICollectionObserver observer, bool immediate, uint? priority)
            => Subscribe(new DictionaryObserver(
                onAdd: (id, kvp) => observer.OnAdd(id, kvp),
                onRemove: (id, kvp) => observer.OnRemove(id, kvp),
                onDispose: observer.OnDispose,
                onError: observer.OnError
            ), immediate, priority);
    }
}
