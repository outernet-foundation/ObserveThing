using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace ObserveThing.Tests
{
    // public class ManualCollectionObservable<T> : ICollectionObservable<T>, IEnumerable<T>
    // {
    //     private List<T> _mostRecentCollection = new List<T>();
    //     private CollectionEventArgs<T> _args = new CollectionEventArgs<T>();
    //     private List<Instance> _instances = new List<Instance>();
    //     private bool _disposing;

    //     public ManualCollectionObservable() { }
    //     public ManualCollectionObservable(IEnumerable<T> values)
    //     {
    //         _mostRecentCollection.AddRange(values);
    //     }

    //     public void OnAdd(T added)
    //     {
    //         _mostRecentCollection.Add(added);
    //         _args.element = added;
    //         _args.operationType = OpType.Add;
    //         foreach (var instance in _instances)
    //             instance.OnNext(_args);
    //     }

    //     public void OnRemove(T removed)
    //     {
    //         _mostRecentCollection.Remove(removed);
    //         _args.element = removed;
    //         _args.operationType = OpType.Remove;
    //         foreach (var instance in _instances)
    //             instance.OnNext(_args);
    //     }

    //     public void OnError(Exception exception)
    //     {
    //         foreach (var instance in _instances)
    //             instance.OnError(exception);
    //     }

    //     public void DisposeAll()
    //     {
    //         _disposing = true;

    //         foreach (var instance in _instances)
    //             instance.Dispose();

    //         _instances.Clear();

    //         _disposing = false;
    //     }

    //     public IDisposable Subscribe(IObserver<ICollectionEventArgs<T>> observer)
    //     {
    //         var instance = new Instance(observer, x =>
    //         {
    //             if (!_disposing)
    //                 _instances.Remove(x);
    //         });

    //         _instances.Add(instance);

    //         foreach (var kvp in _mostRecentCollection)
    //         {
    //             _args.element = kvp;
    //             _args.operationType = OpType.Add;
    //             instance.OnNext(_args);
    //         }

    //         return instance;
    //     }

    //     public IEnumerator<T> GetEnumerator()
    //         => _mostRecentCollection.GetEnumerator();

    //     IEnumerator IEnumerable.GetEnumerator()
    //         => _mostRecentCollection.GetEnumerator();

    //     private class Instance : IDisposable
    //     {
    //         private IObserver<ICollectionEventArgs<T>> _observer;
    //         private Action<Instance> _onDispose;

    //         public Instance(IObserver<ICollectionEventArgs<T>> observer, Action<Instance> onDispose)
    //         {
    //             _observer = observer;
    //             _onDispose = onDispose;
    //         }

    //         public void OnNext(ICollectionEventArgs<T> args)
    //         {
    //             _observer?.OnNext(args);
    //         }

    //         public void OnError(Exception error)
    //         {
    //             _observer?.OnError(error);
    //         }

    //         public void Dispose()
    //         {
    //             if (_observer == null)
    //                 throw new Exception("ALREADY DISPOSED");

    //             _observer.OnDispose();
    //             _observer = null;

    //             _onDispose(this);
    //         }
    //     }
    // }

    public class CollectionObservableTests
    {
        private T Peek<T>(IValueObservable<T> observable)
        {
            T result = default;
            var observer = observable.Subscribe(x => result = x);
            observer.Dispose();
            return result;
        }

        private List<T> Peek<T>(IListObservable<T> observable)
        {
            List<T> result = new List<T>();
            var observer = observable.Subscribe(x => result.Add(x));
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
            List<ValueObservable<int>> results = new List<ValueObservable<int>>();
            Exception exception = default;
            bool disposed = false;

            var list = new ListObservable<ValueObservable<int>>();
            var orderBy = list.ObservableOrderBy(x => x.AsObservable()).Subscribe(
                onAdd: (index, value) =>
                {
                    callCount++;
                    results.Insert(index, value);
                },
                onRemove: (index, value) =>
                {
                    callCount++;
                    results.RemoveAt(index);
                },
                onError: exc => exception = exc,
                onDispose: () => disposed = true
            );

            list.Add(new ValueObservable<int>(1));
            list.Add(new ValueObservable<int>(2));
            list.Add(new ValueObservable<int>(13));
            list.Add(new ValueObservable<int>(2));
            list.Add(new ValueObservable<int>(4));

            Assert.AreEqual(5, callCount);

            AreEqual(new int[] { 1, 2, 2, 4, 13 }, results);

            list.Remove(results[1]);

            Assert.AreEqual(6, callCount);
            AreEqual(new int[] { 1, 2, 4, 13 }, results);

            results[1].value = 22;

            Assert.AreEqual(8, callCount);
            AreEqual(new int[] { 1, 4, 13, 22 }, results);

            results[2].value = -33;

            Assert.AreEqual(10, callCount);
            AreEqual(new int[] { -33, 1, 4, 22 }, results);

            var multiAdd = new ValueObservable<int>(3);

            list.Add(multiAdd);
            list.Add(multiAdd);
            list.Add(multiAdd);
            list.Add(multiAdd);
            list.Add(multiAdd);

            Assert.AreEqual(15, callCount);
            AreEqual(new int[] { -33, 1, 3, 3, 3, 3, 3, 4, 22 }, results);

            multiAdd.value = 10;

            Assert.AreEqual(25, callCount);
            AreEqual(new int[] { -33, 1, 4, 10, 10, 10, 10, 10, 22 }, results);

            list.Remove(multiAdd);
            list.Remove(multiAdd);

            Assert.AreEqual(27, callCount);
            AreEqual(new int[] { -33, 1, 4, 10, 10, 10, 22 }, results);

            multiAdd.value = 1;

            Assert.AreEqual(33, callCount);
            AreEqual(new int[] { -33, 1, 1, 1, 1, 4, 22 }, results);

            multiAdd.value = 5;

            Assert.AreEqual(39, callCount);
            AreEqual(new int[] { -33, 1, 4, 5, 5, 5, 22 }, results);

            // var exc = new Exception();
            // list.OnError(exc);
            // Assert.AreEqual(exc, exception);

            orderBy.Dispose();
            Assert.IsTrue(disposed);

            list.Remove(results[1]);

            AreEqual(new int[] { -33, 1, 4, 5, 5, 5, 22 }, results);
        }

        [Test]
        public void TestOrderByExternalCollection()
        {
            ListObservable<string> source = new ListObservable<string>();
            List<string> destination = new List<string>();

            source
                .ObservableSelect(x => x)
                .ObservableOrderBy(x => source.ObservableIndexOf(x))
                .Subscribe(
                    onAdd: (index, value) => destination.Insert(index, value),
                    onRemove: (index, value) => destination.RemoveAt(index)
                );

            source.Add("cat");
            source.Add("dog");
            source.Add("frog");

            Assert.That(destination, Is.EqualTo(source));

            source.Remove("dog");
            source.Add("me");
            source.Insert(0, "meee");

            Assert.That(destination, Is.EqualTo(source));
        }

        [Test]
        public void TestWhere()
        {
            int callCount = 0;
            List<ValueObservable<int>> results = new List<ValueObservable<int>>();
            Exception exception = default;
            bool disposed = false;

            var list = new ListObservable<ValueObservable<int>>();
            var where = list.ObservableWhere(x => x.ObservableSelect(x => x % 2 == 0)).Subscribe(
                onAdd: x =>
                {
                    callCount++;
                    results.Add(x);
                },
                onRemove: x =>
                {
                    callCount++;
                    results.Remove(x);
                },
                exc => exception = exc,
                () => disposed = true
            );

            var v1 = new ValueObservable<int>(1);
            var v2 = new ValueObservable<int>(6);
            var v3 = new ValueObservable<int>(13);
            var v4 = new ValueObservable<int>(2);
            var v5 = new ValueObservable<int>(4);

            list.Add(v1);
            list.Add(v2);
            list.Add(v3);
            list.Add(v4);
            list.Add(v5);

            Assert.AreEqual(3, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v4, v5 }));

            list.Remove(v4);

            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            // list.Remove(new ValueObservable<int>(22));

            // Assert.IsNotNull(exception);
            // Assert.AreEqual(4, callCount);
            // Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            // exception = null;
            // list.Remove(new ValueObservable<int>(6));

            // Assert.IsNotNull(exception);
            // Assert.AreEqual(4, callCount);
            // Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            list.Remove(v3);

            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            list.Add(v3);

            Assert.AreEqual(4, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            var v6 = new ValueObservable<int>(8);

            list.Add(v6);

            Assert.AreEqual(5, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5, v6 }));

            v6.value = 7;

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            v2.value = 8;

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new[] { v2, v5 }));

            // TODO : Implement?
            // var exc = new Exception();
            // rootObservable.OnError(exc);
            // Assert.AreEqual(exc, exception);

            where.Dispose();
            Assert.IsTrue(disposed);

            list.Remove(v2);

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

            var list = new ListObservable<int>();
            var distinct = list.ObservableDistinct().Subscribe(
                onAdd: x =>
                {
                    callCount++;
                    results.Add(x);
                },
                onRemove: x =>
                {
                    callCount++;
                    results.Remove(x);
                },
                exc => exception = exc,
                () => disposed = true
            );

            list.Add(1);
            list.Add(6);
            list.Add(6);
            list.Add(6);
            list.Add(13);
            list.Add(2);
            list.Add(4);
            list.Add(4);

            Assert.AreEqual(5, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 6, 2, 4, 13 }));

            list.Remove(4);

            Assert.AreEqual(5, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 6, 2, 4, 13 }));

            list.Remove(1);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 6, 2, 4, 13 }));

            list.Add(13);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 6, 2, 4, 13 }));

            // TODO : Implement?
            // var exc = new Exception();
            // rootObservable.OnError(exc);
            // Assert.AreEqual(exc, exception);

            distinct.Dispose();
            Assert.IsTrue(disposed);

            list.Add(100);

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

            var list1 = new ListObservable<int>();
            var list2 = new ListObservable<int>();
            var concat = list1.ObservableConcat(list2.AsObservable()).Subscribe(
                onAdd: x =>
                {
                    callCount++;
                    results.Add(x);
                },
                onRemove: x =>
                {
                    callCount++;
                    results.Remove(x);
                },
                exc => exception = exc,
                () => disposed = true
            );

            list1.Add(1);
            list1.Add(2);
            list1.Add(3);

            list2.Add(4);
            list2.Add(5);
            list2.Add(6);

            Assert.AreEqual(6, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6 }));

            list2.Add(7);

            Assert.AreEqual(7, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 7 }));

            list2.Remove(4);

            Assert.AreEqual(8, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 5, 6, 7 }));

            list1.Add(8);

            Assert.AreEqual(9, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 5, 6, 7, 8 }));

            list1.Remove(3);

            Assert.AreEqual(10, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8 }));

            list2.Add(8);

            Assert.AreEqual(11, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8, 8 }));

            list1.Remove(8);

            Assert.AreEqual(12, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 5, 6, 7, 8 }));

            // TODO : Implement?
            // var exc1 = new Exception();
            // observable1.OnError(exc1);
            // Assert.AreEqual(exc1, exception);

            list1.Dispose();
            Assert.IsTrue(disposed);

            list2.Add(100);

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

            var list = new ListObservable<ListObservable<int>>();
            var selectMany = list.ObservableSelectMany(x => (ICollectionObservable<int>)x).Subscribe(
                x =>
                {
                    callCount++;
                    results.Add(x);
                },
                x =>
                {
                    callCount++;
                    results.Remove(x);
                },
                exc => exception = exc,
                () => disposed = true
            );

            var arr1 = new ListObservable<int>(new[] { 1, 2, 3 });
            var arr2 = new ListObservable<int>(new[] { 4, 5, 6 });
            var arr3 = new ListObservable<int>(new[] { 7, 8, 9 });

            list.Add(arr1);
            list.Add(arr2);
            list.Add(arr3);

            Assert.AreEqual(9, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));

            list.Remove(arr2);

            Assert.AreEqual(12, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 7, 8, 9 }));

            list.Add(arr2);
            list.Add(arr2);

            Assert.AreEqual(18, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 9 }));

            arr1.Add(100);

            Assert.AreEqual(19, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 9, 100 }));

            arr3.Remove(9);

            Assert.AreEqual(20, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 5, 6, 4, 5, 6, 7, 8, 100 }));

            arr2.Remove(5);

            Assert.AreEqual(22, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 6, 4, 6, 7, 8, 100 }));

            arr2.Add(44);

            Assert.AreEqual(24, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 6, 4, 6, 7, 8, 100, 44, 44 }));

            // TODO : Implement?
            // var exc = new Exception();
            // observableRoot.OnError(exc);
            // Assert.AreEqual(exc, exception);

            list.Dispose();
            Assert.IsTrue(disposed);

            list.Add(arr1);

            Assert.AreEqual(24, callCount);
            Assert.That(results, Is.EquivalentTo(new int[] { 1, 2, 3, 4, 6, 4, 6, 7, 8, 100, 44, 44 }));
        }

        [Test]
        public void TestSelect()
        {
            var result = new List<string>();
            var list = new ListObservable<int>();
            var select = ((ICollectionObservable<int>)list).ObservableSelect(x => x.ToString()).Subscribe(
                onAdd: x => result.Add(x),
                onRemove: x => result.Remove(x)
            );

            list.Add(1);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(45);

            CollectionAssert.AreEquivalent(
                Enumerable.Select(list, x => x.ToString()),
                result
            );

            list.Remove(3);
            list.Remove(3);
            list.Add(1);
            list.Add(45);
            list.Remove(45);

            CollectionAssert.AreEquivalent(
                Enumerable.Select(list, x => x.ToString()),
                result
            );

            list.Clear();

            CollectionAssert.AreEquivalent(
                Enumerable.Select(list, x => x.ToString()),
                result
            );
        }

        // TODO: Implement
        // [Test]
        // public void TestShallowCopy()
        // {
        //     throw new NotImplementedException();
        // }

        public class TestElement
        {
            public ValueObservable<int> intValue = new ValueObservable<int>();
            public ValueObservable<string> stringValue = new ValueObservable<string>();
        }

        [Test]
        public void TestErrorLog()
        {
            var dict = new DictionaryObservable<int, string>();
            var stream = dict.ObservableSelect(x =>
            {
                if (x.Key != 0)
                    throw new Exception("Oh no!");

                return x.Key;

            }).ObservableSelect(x =>
            {
                if (x != 0)
                    throw new Exception("Oh no!");

                return x;

            }).Subscribe(onAdd: null, onRemove: null, onError: null, onDispose: null);

            dict.Add(10, "cat");
            dict.Add(11, "dog");
        }

        // [Test]
        // public void TestToDictionary()
        // {
        //     int callCount = 0;
        //     Exception exception = default;
        //     bool disposed = false;
        //     var result = new Dictionary<int, string>();

        //     var list = new ListObservable<TestElement>();
        //     var toDict = list.ToDictionaryDynamic(x => x.intValue.AsObservable(), x => x.stringValue.AsObservable());
        //     var observable = toDict.Subscribe(
        //         onAdd: x =>
        //         {
        //             UnityEngine.Debug.Log("EP: Got add");
        //             callCount++;
        //             result.Add(x.Key, x.Value);
        //         },
        //         onRemove: x =>
        //         {
        //             UnityEngine.Debug.Log("EP: Got remove");
        //             callCount++;
        //             result.Remove(x.Key);
        //         },
        //         onError: exc => exception = exc,
        //         onDispose: () => disposed = true
        //     );

        //     list.Add(new TestElement());

        //     Assert.AreEqual(3, callCount);
        //     Assert.IsNull(exception);
        //     CollectionAssert.AreEquivalent(
        //         list.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
        //         result
        //     );

        //     var element = new TestElement();
        //     element.intValue.value = 3;
        //     element.stringValue.value = "cat";

        //     //Test change key/value before adding
        //     list.Add(element);

        //     Assert.AreEqual(6, callCount);
        //     Assert.IsNull(exception);
        //     CollectionAssert.AreEquivalent(
        //         list.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
        //         result
        //     );

        //     //Test change key/value after adding
        //     element.intValue.value = 100;
        //     element.stringValue.value = "dog";
        //     Assert.AreEqual(10, callCount);
        //     Assert.IsNull(exception);
        //     CollectionAssert.AreEquivalent(
        //         list.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
        //         result
        //     );

        //     //Test remove element
        //     list.Remove(element);
        //     Assert.AreEqual(11, callCount);
        //     Assert.IsNull(exception);
        //     CollectionAssert.AreEquivalent(
        //         list.ToDictionary(x => x.intValue.value, x => x.stringValue.value),
        //         result
        //     );

        //     // Reset observable and source collection
        //     observable.Dispose(); // clear subscription to test errors
        //     list.Clear();
        //     observable = toDict.Subscribe(); // need a subscription for operations to function

        //     //Test double add element
        //     list.Add(element);
        //     list.Add(element);
        //     Assert.IsNotNull(exception);
        //     exception = null;

        //     // Reset observable and source collection
        //     observable.Dispose();
        //     list.Clear();
        //     observable = toDict.Subscribe(); // need a subscription for operations to function

        //     //Test double add key
        //     list.Add(element);
        //     var conflictingElement = new TestElement();
        //     conflictingElement.intValue.value = 100;
        //     conflictingElement.stringValue.value = "cat";
        //     Assert.Throws<Exception>(() => list.Add(conflictingElement));

        //     // Reset observable and source collection
        //     observable.Dispose();
        //     list.Clear();
        //     observable = toDict.Subscribe(); // need a subscription for operations to function

        //     //Test converge keys
        //     conflictingElement.intValue.value = 30;
        //     list.Add(element);
        //     list.Add(conflictingElement);
        //     Assert.Throws<Exception>(() => conflictingElement.intValue.value = 100);
        // }
    }
}