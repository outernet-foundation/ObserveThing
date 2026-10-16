using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObservable
    {
        ObservationContext context { get; }
    }

    public interface IEventStreamObservable : IObservable
    {
        IDisposable Subscribe(IEventStreamObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface IEventStreamObservable<out T> : IEventStreamObservable
    {
        IDisposable Subscribe(IEventStreamObserver<T> observer, bool immediate = default, uint? priority = default);
    }

    public interface IValueObservable : IObservable
    {
        IDisposable Subscribe(IValueObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface IValueObservable<out T> : IValueObservable
    {
        IDisposable Subscribe(IValueObserver<T> observer, bool immediate = default, uint? priority = default);
    }

    public interface ICollectionObservable : IObservable
    {
        IDisposable Subscribe(ICollectionObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface ICollectionObservable<out T> : ICollectionObservable
    {
        IDisposable Subscribe(ICollectionObserver<T> observer, bool immediate = default, uint? priority = default);
    }

    public interface IListObservable : ICollectionObservable
    {
        IDisposable Subscribe(IListObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface IListObservable<out T> : IListObservable, ICollectionObservable<T>
    {
        IDisposable Subscribe(IListObserver<T> observer, bool immediate = default, uint? priority = default);
    }

    public interface ISetObservable : ICollectionObservable
    {
        IDisposable Subscribe(ISetObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface ISetObservable<out T> : ISetObservable, ICollectionObservable<T>
    {
        IDisposable Subscribe(ISetObserver<T> observer, bool immediate = default, uint? priority = default);
    }

    public interface IDictionaryObservable : ICollectionObservable
    {
        IDisposable Subscribe(IDictionaryObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface IDictionaryObservable<TKey, TValue> : IDictionaryObservable, ICollectionObservable<KeyValuePair<TKey, TValue>>
    {
        IDisposable Subscribe(IDictionaryObserver<TKey, TValue> observer, bool immediate = default, uint? priority = default);
    }
}
