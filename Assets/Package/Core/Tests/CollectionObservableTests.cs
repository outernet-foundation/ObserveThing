using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace ObserveThing.Tests
{
    public class ManualCollectionObservable<T> : ICollectionObservable<T>, IEnumerable<T>
    {
        private List<T> _mostRecentCollection = new List<T>();
        private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
        private List<Instance> _instances = new List<Instance>();
        private bool _disposing;

        public ManualCollectionObservable() { }
        public ManualCollectionObservable(IEnumerable<T> values)
        {
            _mostRecentCollection.AddRange(values);
        }

        public void OnAdd(T added)
        {
            _mostRecentCollection.Add(added);
            _args.element = added;
            _args.operationType = OpType.Add;
            foreach (var instance in _instances)
                instance.OnNext(_args);
        }

        public void OnRemove(T removed)
        {
            _mostRecentCollection.Remove(removed);
            _args.element = removed;
            _args.operationType = OpType.Remove;
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

            foreach (var kvp in _mostRecentCollection)
            {
                _args.element = kvp;
                _args.operationType = OpType.Add;
                instance.OnNext(_args);
            }

            return instance;
        }

        public IEnumerator<T> GetEnumerator()
            => _mostRecentCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _mostRecentCollection.GetEnumerator();

        private class Instance : IDisposable
        {
            private IObserver<ICollectionEventArgs<T>> _observer;
            private Action<Instance> _onDispose;

            public Instance(IObserver<ICollectionEventArgs<T>> observer, Action<Instance> onDispose)
            {
                _observer = observer;
                _onDispose = onDispose;
            }

            public void OnNext(ICollectionEventArgs<T> args)
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

    public class CollectionObservableTests
    {
        private T Peek<T>(IValueObservable<T> observable)
        {
            T result = default;
            var observer = observable.Subscribe(x => result = x.currentValue);
            observer.Dispose();
            return result;
        }

        private List<T> Peek<T>(IListObservable<T> observable)
        {
            List<T> result = new List<T>();
            var observer = observable.Subscribe(x => result.Add(x.element));
            observer.Dispose();
            return result;
        }

        private void AreEqual<T>(T expected, IValueObservable<T> observable)
            => Assert.AreEqual(expected, Peek(observable));

        private void AreEqual<T>(IEnumerable<T> expected, IEnumerable<IValueObservable<T>> actual)
            => Assert.AreEqual(expected, actual.Select(x => Peek(x)));

        private void AreEqual<T>(IEnumerable<T> expected, IListObservable<T> observable)
            => Assert.AreEqual(expected, observable);

        [Test]
        public void TestOrderBy()
        {
            int callCount = 0;
            List<ManualValueObservable<int>> results = new List<ManualValueObservable<int>>();
            Exception exception = default;
            bool disposed = false;

            var rootObservable = new ManualCollectionObservable<ManualValueObservable<int>>();
            var orderByObservable = rootObservable.OrderByDynamic(x => x.AsObservable()).Subscribe(
                x =>
                {
                    callCount++;
                    if (x.operationType == OpType.Add)
                    {
                        results.Insert(x.index, x.element);
                    }
                    else
                    {
                        results.RemoveAt(x.index);
                    }
                },
                exc => exception = exc,
                () => disposed = true
            );

            rootObservable.OnAdd(new ManualValueObservable<int>(1));
            rootObservable.OnAdd(new ManualValueObservable<int>(2));
            rootObservable.OnAdd(new ManualValueObservable<int>(13));
            rootObservable.OnAdd(new ManualValueObservable<int>(2));
            rootObservable.OnAdd(new ManualValueObservable<int>(4));

            Assert.AreEqual(5, callCount);
            AreEqual(new int[] { 1, 2, 2, 4, 13 }, results);

            rootObservable.OnRemove(results[1]);

            Assert.AreEqual(6, callCount);
            AreEqual(new int[] { 1, 2, 4, 13 }, results);

            results[1].OnNext(22);

            Assert.AreEqual(8, callCount);
            AreEqual(new int[] { 1, 4, 13, 22 }, results);

            results[2].OnNext(-33);

            Assert.AreEqual(10, callCount);
            AreEqual(new int[] { -33, 1, 4, 22 }, results);

            var multiAdd = new ManualValueObservable<int>(3);

            rootObservable.OnAdd(multiAdd);
            rootObservable.OnAdd(multiAdd);
            rootObservable.OnAdd(multiAdd);
            rootObservable.OnAdd(multiAdd);
            rootObservable.OnAdd(multiAdd);

            Assert.AreEqual(15, callCount);
            AreEqual(new int[] { -33, 1, 3, 3, 3, 3, 3, 4, 22 }, results);

            multiAdd.OnNext(10);

            Assert.AreEqual(25, callCount);
            AreEqual(new int[] { -33, 1, 4, 10, 10, 10, 10, 10, 22 }, results);

            rootObservable.OnRemove(multiAdd);
            rootObservable.OnRemove(multiAdd);

            Assert.AreEqual(27, callCount);
            AreEqual(new int[] { -33, 1, 4, 10, 10, 10, 22 }, results);

            multiAdd.OnNext(1);

            Assert.AreEqual(33, callCount);
            AreEqual(new int[] { -33, 1, 1, 1, 1, 4, 22 }, results);

            multiAdd.OnNext(5);

            Assert.AreEqual(39, callCount);
            AreEqual(new int[] { -33, 1, 4, 5, 5, 5, 22 }, results);

            var exc = new Exception();
            rootObservable.OnError(exc);
            Assert.AreEqual(exc, exception);

            orderByObservable.Dispose();
            Assert.IsTrue(disposed);

            rootObservable.OnRemove(results[1]);

            AreEqual(new int[] { -33, 1, 4, 5, 5, 5, 22 }, results);
        }

        [Test]
        public void TestOrderByExternalCollection()
        {
            ListObservable<string> source = new ListObservable<string>();
            List<string> destination = new List<string>();

            source
                .SelectDynamic(x => x)
                .OrderByDynamic(x => source.IndexOfDynamic(x))
                .Subscribe(x =>
                {
                    if (x.operationType == OpType.Add)
                    {
                        destination.Insert(x.index, x.element);
                    }
                    else if (x.operationType == OpType.Remove)
                    {
                        destination.RemoveAt(x.index);
                    }
                });

            source.Add("cat");
            source.Add("dog");
            source.Add("frog");

            Assert.AreEqual(source, destination);

            source.Remove("dog");
            source.Add("me");
            source.Insert(0, "meee");

            Assert.AreEqual(source, destination);
        }

        [Test]
        public void TestWhere()
        {
            int callCount = 0;
            List<ManualValueObservable<int>> results = new List<ManualValueObservable<int>>();
            Exception exception = default;
            bool disposed = false;

            var rootObservable = new ManualCollectionObservable<ManualValueObservable<int>>();
            var whereObservable = rootObservable.WhereDynamic(x => x.SelectDynamic(x => x % 2 == 0)).Subscribe(
                x =>
                {
                    callCount++;
                    if (x.operationType == OpType.Add)
                    {
                        results.Add(x.element);
                    }
                    else
                    {
                        results.Remove(x.element);
                    }
                },
                exc => exception = exc,
                () => disposed = true
            );

            var v1 = new ManualValueObservable<int>(1);
            var v2 = new ManualValueObservable<int>(6);
            var v3 = new ManualValueObservable<int>(13);
            var v4 = new ManualValueObservable<int>(2);
            var v5 = new ManualValueObservable<int>(4);

            rootObservable.OnAdd(v1);
            rootObservable.OnAdd(v2);
            rootObservable.OnAdd(v3);
            rootObservable.OnAdd(v4);
            rootObservable.OnAdd(v5);

            Assert.AreEqual(3, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v4, v5 }));

            rootObservable.OnRemove(v4);

            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            rootObservable.OnRemove(new ManualValueObservable<int>(22));

            Assert.IsNotNull(exception);
            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            exception = null;
            rootObservable.OnRemove(new ManualValueObservable<int>(6));

            Assert.IsNotNull(exception);
            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            rootObservable.OnRemove(v3);

            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            rootObservable.OnAdd(v3);

            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            var v6 = new ManualValueObservable<int>(8);

            rootObservable.OnAdd(v6);

            Assert.AreEqual(5, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5, v6 }));

            v6.OnNext(7);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            v2.OnNext(8);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            var exc = new Exception();
            rootObservable.OnError(exc);
            Assert.AreEqual(exc, exception);

            whereObservable.Dispose();
            Assert.IsTrue(disposed);

            rootObservable.OnRemove(v2);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));
        }

        [Test]
        public void TestDistinct()
        {
            int callCount = 0;
            List<int> results = new List<int>();
            Exception exception = default;
            bool disposed = false;

            var rootObservable = new ManualCollectionObservable<int>();
            var distinctObservable = rootObservable.DistinctDynamic().Subscribe(
                x =>
                {
                    callCount++;
                    if (x.operationType == OpType.Add)
                    {
                        results.Add(x.element);
                    }
                    else
                    {
                        results.Remove(x.element);
                    }
                },
                exc => exception = exc,
                () => disposed = true
            );

            rootObservable.OnAdd(1);
            rootObservable.OnAdd(6);
            rootObservable.OnAdd(6);
            rootObservable.OnAdd(6);
            rootObservable.OnAdd(13);
            rootObservable.OnAdd(2);
            rootObservable.OnAdd(4);
            rootObservable.OnAdd(4);

            Assert.AreEqual(5, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 6, 2, 4, 13 }));

            rootObservable.OnRemove(4);

            Assert.AreEqual(5, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 6, 2, 4, 13 }));

            rootObservable.OnRemove(1);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 6, 2, 4, 13 }));

            rootObservable.OnAdd(13);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 6, 2, 4, 13 }));

            var exc = new Exception();
            rootObservable.OnError(exc);
            Assert.AreEqual(exc, exception);

            distinctObservable.Dispose();
            Assert.IsTrue(disposed);

            rootObservable.OnAdd(100);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 6, 2, 4, 13 }));
        }

        [Test]
        public void TestConcat()
        {
            int callCount = 0;
            List<int> results = new List<int>();
            Exception exception = default;
            bool disposed = false;

            var observable1 = new ManualCollectionObservable<int>();
            var observable2 = new ManualCollectionObservable<int>();
            var concatObservable = observable1.ConcatDynamic((ICollectionObservable<int>)observable2).Subscribe(
                x =>
                {
                    callCount++;
                    if (x.operationType == OpType.Add)
                    {
                        results.Add(x.element);
                    }
                    else
                    {
                        results.Remove(x.element);
                    }
                },
                exc => exception = exc,
                () => disposed = true
            );

            observable1.OnAdd(1);
            observable1.OnAdd(2);
            observable1.OnAdd(3);

            observable2.OnAdd(4);
            observable2.OnAdd(5);
            observable2.OnAdd(6);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6 }));

            observable2.OnAdd(7);

            Assert.AreEqual(7, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 7 }));

            observable2.OnRemove(4);

            Assert.AreEqual(8, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 5, 6, 7 }));

            observable1.OnAdd(8);

            Assert.AreEqual(9, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 5, 6, 7, 8 }));

            observable1.OnRemove(3);

            Assert.AreEqual(10, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8 }));

            observable2.OnAdd(8);

            Assert.AreEqual(11, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8, 8 }));

            observable1.OnRemove(8);

            Assert.AreEqual(12, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8 }));

            var exc1 = new Exception();
            observable1.OnError(exc1);
            Assert.AreEqual(exc1, exception);

            observable1.DisposeAll();
            Assert.IsTrue(disposed);

            observable2.OnAdd(100);

            Assert.AreEqual(12, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8 }));
        }

        [Test]
        public void TestSelectMany()
        {
            int callCount = 0;
            List<int> results = new List<int>();
            Exception exception = default;
            bool disposed = false;

            var observableRoot = new ManualCollectionObservable<ManualCollectionObservable<int>>();
            var concatObservable = observableRoot.SelectManyDynamic(x => (ICollectionObservable<int>)x).Subscribe(
                x =>
                {
                    callCount++;
                    if (x.operationType == OpType.Add)
                    {
                        results.Add(x.element);
                    }
                    else
                    {
                        results.Remove(x.element);
                    }
                },
                exc => exception = exc,
                () => disposed = true
            );

            var arr1 = new ManualCollectionObservable<int>(new[] { 1, 2, 3 });
            var arr2 = new ManualCollectionObservable<int>(new[] { 4, 5, 6 });
            var arr3 = new ManualCollectionObservable<int>(new[] { 7, 8, 9 });

            observableRoot.OnAdd(arr1);
            observableRoot.OnAdd(arr2);
            observableRoot.OnAdd(arr3);

            Assert.AreEqual(9, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));

            observableRoot.OnRemove(arr2);

            Assert.AreEqual(12, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 7, 8, 9 }));

            observableRoot.OnAdd(arr2);
            observableRoot.OnAdd(arr2);

            Assert.AreEqual(18, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 9 }));

            arr1.OnAdd(100);

            Assert.AreEqual(19, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 9, 100 }));

            arr3.OnRemove(9);

            Assert.AreEqual(20, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 100 }));

            arr2.OnRemove(5);

            Assert.AreEqual(22, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 6, 4, 6, 7, 8, 100 }));

            arr2.OnAdd(44);

            Assert.AreEqual(24, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 6, 4, 6, 7, 8, 100, 44, 44 }));

            var exc = new Exception();
            observableRoot.OnError(exc);
            Assert.AreEqual(exc, exception);

            observableRoot.DisposeAll();
            Assert.IsTrue(disposed);

            observableRoot.OnAdd(arr1);

            Assert.AreEqual(24, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 6, 4, 6, 7, 8, 100, 44, 44 }));
        }

        [Test]
        public void TestSelect()
        {
            var collection = new CollectionObservable<int>();
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

            CollectionAssert.AreEquivalent(
                collection.Select(x => x.ToString()),
                result
            );

            collection.Remove(3);
            collection.Remove(3);
            collection.Add(1);
            collection.Add(45);
            collection.Remove(45);

            CollectionAssert.AreEquivalent(
                collection.Select(x => x.ToString()),
                result
            );

            collection.Clear();

            CollectionAssert.AreEquivalent(
                collection.Select(x => x.ToString()),
                result
            );
        }

        [Test]
        public void TestShallowCopy()
        {
            throw new NotImplementedException();
        }

        public class TestElement
        {
            public ValueObservable<int> intValue = new ValueObservable<int>();
            public ValueObservable<string> stringValue = new ValueObservable<string>();
        }

        [Test]
        public void TestErrorLog()
        {
            var dict = new DictionaryObservable<int, string>();
            var stream = dict.SelectDynamic(x =>
            {
                if (x.Key != 0)
                    throw new Exception("Oh no!");

                return x.Key;

            }).SelectDynamic(x =>
            {
                if (x != 0)
                    throw new Exception("Oh no!");

                return x;

            }).Subscribe(args => { }, null, () => { });
            dict.Add(10, "cat");
            dict.Add(11, "dog");
        }

        [Test]
        public void TestToDictionary()
        {
            int callCount = 0;
            Exception exception = default;
            bool disposed = false;
            var result = new Dictionary<int, string>();

            var collection = new CollectionObservable<TestElement>();
            var toDict = collection.ToDictionaryDynamic(x => x.intValue.AsObservable(), x => x.stringValue.AsObservable());
            var observable = toDict.Subscribe(
                args =>
                {
                    callCount++;
                    if (args.operationType == OpType.Add)
                    {
                        result.Add(args.key, args.value);
                    }
                    else if (args.operationType == OpType.Remove)
                    {
                        result.Remove(args.key);
                    }
                },
                onDispose: () => disposed = true,
                name: "final observer"
            );

            collection.Add(new TestElement());

            Assert.AreEqual(1, callCount);
            Assert.IsNull(exception);
            CollectionAssert.AreEquivalent(
                collection.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
                result
            );

            var element = new TestElement();
            element.intValue.value = 3;
            element.stringValue.value = "cat";

            //Test change key/value before adding
            collection.Add(element);

            Assert.AreEqual(2, callCount);
            Assert.IsNull(exception);
            CollectionAssert.AreEquivalent(
                collection.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
                result
            );

            //Test change key/value after adding
            element.intValue.value = 100;
            element.stringValue.value = "dog";
            Assert.AreEqual(6, callCount);
            Assert.IsNull(exception);
            CollectionAssert.AreEquivalent(
                collection.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
                result
            );

            //Test remove element
            collection.Remove(element);
            Assert.AreEqual(7, callCount);
            Assert.IsNull(exception);
            CollectionAssert.AreEquivalent(
                collection.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
                result
            );

            // Reset observable and source collection
            observable.Dispose(); // clear subscription to test errors
            collection.Clear();
            observable = toDict.Subscribe(); // need a subscription for operations to function

            //Test double add element
            collection.Add(element);
            Assert.Throws<InternalObserverException>(() => collection.Add(element));

            // Reset observable and source collection
            observable.Dispose();
            collection.Clear();
            observable = toDict.Subscribe(); // need a subscription for operations to function

            //Test double add key
            collection.Add(element);
            var conflictingElement = new TestElement();
            conflictingElement.intValue.value = 100;
            conflictingElement.stringValue.value = "cat";
            Assert.Throws<InternalObserverException>(() => collection.Add(conflictingElement));

            // Reset observable and source collection
            observable.Dispose();
            collection.Clear();
            observable = toDict.Subscribe(); // need a subscription for operations to function

            //Test converge keys
            conflictingElement.intValue.value = 30;
            collection.Add(element);
            collection.Add(conflictingElement);
            Assert.Throws<InternalObserverException>(() => conflictingElement.intValue.value = 100);
        }
    }
}