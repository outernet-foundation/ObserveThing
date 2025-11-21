using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ObserveThing.Tests
{
    public class ManualListObservable<T> : ICollectionObservable<T>
    {
        private List<T> _mostRecentList = new List<T>();
        private ListEventArgs<T> _args = new ListEventArgs<T>();
        private List<Instance> _instances = new List<Instance>();
        private bool _disposing;

        public void OnAdd(T added)
            => OnInsert(_mostRecentList.Count, added);

        public void OnRemove(T removed)
            => OnRemoveAt(_mostRecentList.IndexOf(removed));

        public void OnRemoveAt(int index)
        {
            T element = _mostRecentList[index];
            _mostRecentList.RemoveAt(index);
            _args.index = index;
            _args.element = element;
            _args.operationType = OpType.Remove;
            foreach (var instance in _instances)
                instance.OnNext(_args);
        }

        public void OnInsert(int index, T inserted)
        {
            _mostRecentList.Insert(index, inserted);
            _args.index = index;
            _args.element = inserted;
            _args.operationType = OpType.Add;
            foreach (var instance in _instances)
                instance.OnNext(_args);
        }

        public void OnError(Exception exception)
        {
            foreach (var instance in _instances)
                instance.OnError(exception);
        }

        public void DisposeAll()
        {
            _disposing = true;

            foreach (var instance in _instances)
                instance.Dispose();

            _instances.Clear();

            _disposing = false;
        }

        public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
        {
            var instance = new Instance(observer, x =>
            {
                if (!_disposing)
                    _instances.Remove(x);
            });

            _instances.Add(instance);

            for (int i = 0; i < _mostRecentList.Count; i++)
            {
                _args.index = i;
                _args.element = _mostRecentList[i];
                _args.operationType = OpType.Add;
                instance.OnNext(_args);
            }

            return instance;
        }

        private class Instance : IDisposable
        {
            private IObserver<ListEventArgs<T>> _observer;
            private Action<Instance> _onDispose;

            public Instance(IObserver<ListEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(ListEventArgs<T> args)
            {
                _observer?.OnNext(args);
            }

            public void OnError(Exception error)
            {
                _observer?.OnError(error);
            }

            public void Dispose()
            {
                if (_observer == null)
                    throw new Exception("ALREADY DISPOSED");

                _observer.OnDispose();
                _observer = null;

                _onDispose(this);
            }
        }
    }

    public class ListObservableTests
    {
        [Test]
        public void TestSelect()
        {
            var collection = new ListObservable<int>();
            var result = new List<string>();
            var stream = collection.SelectDynamic(x => x.ToString()).Subscribe(args =>
            {
                if (args.operationType == OpType.Add)
                {
                    result.Add(args.element);
                }
                else if (args.operationType == OpType.Remove)
                {
                    result.Remove(args.element);
                }
            });

            collection.Add(1);
            collection.Add(1);
            collection.Add(2);
            collection.Add(3);
            collection.Add(4);
            collection.Add(45);

            Assert.AreEqual(
                collection.Select(x => x.ToString()),
                result
            );

            collection.Remove(3);
            collection.Remove(3);
            collection.Add(1);
            collection.Add(45);
            collection.Remove(45);

            Assert.AreEqual(
                collection.Select(x => x.ToString()),
                result
            );

            collection.Clear();

            Assert.AreEqual(
                collection.Select(x => x.ToString()),
                result
            );
        }

        [Test]
        public void TestShallowCopy()
        {
            throw new NotImplementedException();
            var list = new ListObservable<ValueObservable<int>>();
            var result = new List<string>();
            var stream = list.SelectDynamic(x => x.SelectDynamic(x => x.ToString())).Subscribe(args =>
            {
                if (args.operationType == OpType.Add)
                {
                    result.Insert(args.index, args.element);
                }
                else if (args.operationType == OpType.Remove)
                {
                    result.RemoveAt(args.index);
                }
            });

            var element1 = new ValueObservable<int>(2);
            var element2 = new ValueObservable<int>(3);
            var element3 = new ValueObservable<int>(45);
            var element4 = new ValueObservable<int>(11);

            list.Add(element1);
            list.Add(element2);
            list.Add(element3);
            list.Add(element4);

            Assert.AreEqual(
                list.Select(x => x.value.ToString()),
                result
            );

            element1.From(3);
            element1.From(100);
            element1.From(22);
            element1.From(6);

            Assert.AreEqual(
                list.Select(x => x.value.ToString()),
                result
            );

            element2.From(3);
            element3.From(3);
            element4.From(3);

            Assert.AreEqual(
                list.Select(x => x.value.ToString()),
                result
            );

            list.Remove(element2);
            list.Add(element3);

            element2.From(5);
            element3.From(100);
            element3.From(50);

            Assert.AreEqual(
                list.Select(x => x.value.ToString()),
                result
            );
        }
    }
}