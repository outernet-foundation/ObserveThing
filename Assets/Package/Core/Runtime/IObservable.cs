using System;
using System.Collections.Generic;

namespace ObserveThing
{
    public interface IObservable
    {
        ObservationContext context { get; }
        IDisposable Subscribe(IObserver observer, bool immediate = default, uint? priority = default);
    }

    public interface IValueObservable<out T> : IObservable
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

    public interface IBatchObservable : IObservable
    {
        public IDisposable Subscribe(IBatchObserver observer, bool immediate = false, uint? priority = default);
    }
}
