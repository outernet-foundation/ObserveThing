using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ObserveThing.Tests
{
    public class ObservationContextTests
    {
        public struct TestValueOperation
        {
            public IObservable source;
            public object value;
        }

        public struct TestValueSubscriptionOperation
        {
            public int source;
            public object value;
        }

        public struct TestCollectionOperation
        {
            public IObservable source;
            public int index;
            public object value;
            public bool isRemove;
        }

        [Test]
        public void TestOperationOrder()
        {
            var context = new ObservationContext();

            ObservableValue<int> intObservable = new ObservableValue<int>(context, 100);
            ObservableValue<string> stringObservable = new ObservableValue<string>(context, "200");
            List<TestValueOperation> operations = new List<TestValueOperation>();

            int callCount = 0;

            var stream = Observables.ObservableCombine(
                intObservable.ObservableEventStream(x => new TestValueOperation() { source = intObservable, value = x }),
                stringObservable.ObservableEventStream(x => new TestValueOperation() { source = stringObservable, value = x })
            ).Subscribe(
                onNext: op =>
                {
                    callCount++;
                    operations.Add(op);
                }
            );

            Assert.AreEqual(2, callCount);
            Assert.AreEqual(
                new List<TestValueOperation>
                {
                    new() { source = intObservable, value = 100 },
                    new() { source = stringObservable, value = "200" }
                },
                operations
            );

            callCount = 0;
            operations.Clear();

            context.ExecuteBatchOperation(() =>
            {
                intObservable.value = 1;
                stringObservable.value = "2";
                intObservable.value = 3;
                intObservable.value = 4;
                stringObservable.value = "5";
            });

            Assert.AreEqual(5, callCount);
            Assert.AreEqual(
                new List<TestValueOperation>
                {
                    new() { source = intObservable, value = 1 },
                    new() { source = stringObservable, value = "2" },
                    new() { source = intObservable, value = 3 },
                    new() { source = intObservable, value = 4 },
                    new() { source = stringObservable, value = "5" },
                },
                operations
            );
        }

        [Test]
        public void TestRollingObserverOrder()
        {
            var context = new ObservationContext();

            ObservableValue<int> intObservable = new ObservableValue<int>(context);

            List<TestValueSubscriptionOperation> observerCallOrder = new List<TestValueSubscriptionOperation>();

            IDisposable subscription1 = default;
            IDisposable subscription2 = default;
            IDisposable subscription3 = default;
            IDisposable subscription4 = default;
            IDisposable subscription5 = default;

            subscription1 = intObservable.Subscribe(
                onNext: value => observerCallOrder.Add(new() { source = 1, value = value })
            );

            subscription2 = intObservable.Subscribe(
                onNext: value =>
                {
                    observerCallOrder.Add(new() { source = 2, value = value });

                    if (value == 1)
                        intObservable.value = 2;
                }
            );

            subscription3 = intObservable.Subscribe(
                onNext: value =>
                {
                    observerCallOrder.Add(new() { source = 3, value = value });

                    if (value == 2)
                    {
                        intObservable.value = 3;
                        subscription4.Dispose();
                    }
                }
            );

            subscription4 = intObservable.Subscribe(
                onNext: value => observerCallOrder.Add(new() { source = 4, value = value })
            );

            subscription5 = intObservable.Subscribe(
                onNext: value => observerCallOrder.Add(new() { source = 5, value = value })
            );

            intObservable.value = 1;

            Assert.AreEqual(3, intObservable.value);

            Assert.AreEqual(
                new List<TestValueSubscriptionOperation>()
                {
                    new () { source = 1, value = 0}, //init observer1
                    new () { source = 2, value = 0}, //init observer2
                    new () { source = 3, value = 0}, //init observer3
                    new () { source = 4, value = 0}, //init observer4
                    new () { source = 5, value = 0}, //init observer5
                    new () { source = 1, value = 1}, //observer1 observes external setting intObservable to 1
                    new () { source = 2, value = 1}, //observer2 observes external setting intObservable to 1
                    new () { source = 1, value = 2}, //observer1 observes observer2 setting intObservable to 2
                    new () { source = 2, value = 2}, //observer2 observes observer2 setting intObservable to 2
                    new () { source = 3, value = 1}, //observer3 observes external setting intObservable to 1
                    new () { source = 3, value = 2}, //observer3 observes observer2 setting intObservable to 2
                    //4, //observer4 is never called past the init because it gets unsubscribed by observer3
                    new () { source = 1, value = 3}, //observer1 observes observer3 setting intObservable to 3
                    new () { source = 2, value = 3}, //observer2 observes observer3 setting intObservable to 3
                    new () { source = 3, value = 3}, //observer3 observes observer3 setting intObservable to 3
                    new () { source = 5, value = 1}, //observer5 observes external setting intObservable to 1
                    new () { source = 5, value = 2}, //observer5 observes observer2 setting intObservable to 2
                    new () { source = 5, value = 3} //observer5 observes observer3 setting intObservable to 3
                },
                observerCallOrder
            );
        }

        [Test]
        public void TestCombineObservable()
        {
            var context = new ObservationContext();

            ObservableValue<int> intObservable = new ObservableValue<int>(context);
            ObservableValue<string> stringObservable = new ObservableValue<string>(context);

            var query = Observables.ObservableCombine(
                intObservable.ObservableEventStream(x => new TestValueOperation() { source = intObservable, value = x }),
                stringObservable.ObservableEventStream(x => new TestValueOperation() { source = stringObservable, value = x })
            );

            List<TestValueOperation> observerCallOrder = new List<TestValueOperation>();

            IDisposable subscription = query.Subscribe(
                onNext: op =>
                {
                    observerCallOrder.Add(op);

                    if (intObservable.value == 2)
                    {
                        intObservable.value = 6;
                        intObservable.value = 7;
                        intObservable.value = 8;
                        stringObservable.value = "mouse";
                    }
                }
            );

            intObservable.value = 1;
            stringObservable.value = "cat";
            stringObservable.value = "dog";
            intObservable.value = 2;
            stringObservable.value = "frog";
            intObservable.value = 3;
            intObservable.value = 4;

            Assert.AreEqual(
                new List<TestValueOperation>()
                {
                    new() { source = intObservable, value = 0},
                    new() { source = stringObservable, value = null},
                    new() { source = intObservable, value = 1},
                    new() { source = stringObservable, value = "cat"},
                    new() { source = stringObservable, value = "dog"},
                    new() { source = intObservable, value = 2},
                    new() { source = intObservable, value = 6},
                    new() { source = intObservable, value = 7},
                    new() { source = intObservable, value = 8},
                    new() { source = stringObservable, value = "mouse"},
                    new() { source = stringObservable, value = "frog"},
                    new() { source = intObservable, value = 3},
                    new() { source = intObservable, value = 4},
                },
                observerCallOrder
            );
        }

        [Test]
        public void TestAllObservablesDispose()
        {
            var context = new ObservationContext();
            var observable1 = new ObservableValue<int>(context);
            var observable2 = new ObservableValue<int>(context);
            var disposeCallCount = 0;
            var disposed = false;

            var query = Observables.ObservableCombine(
                observable1.ObservableCast<object>().ObservableEventStream(),
                observable2.ObservableCast<object>().ObservableEventStream()
            );

            var stream = query.Subscribe(
                onDispose: () =>
                {
                    disposeCallCount++;
                    disposed = true;
                }
            );

            observable1.Dispose();

            Assert.IsFalse(disposed);
            Assert.AreEqual(0, disposeCallCount);

            observable2.Dispose();

            Assert.IsTrue(disposed);
            Assert.AreEqual(1, disposeCallCount);

            disposeCallCount = 0;
            disposed = false;

            Assert.Throws(
                typeof(ObjectDisposedException),
                () => query.Subscribe(
                    onDispose: () =>
                    {
                        disposeCallCount++;
                        disposed = true;
                    }
                )
            );
        }

        [Test]
        public void TestBatchObservable()
        {
            var context = new ObservationContext();
            var value = new ObservableValue<int>(context, 2);

            var init = false;
            var lastValue = 0;
            var callCount = 0;

            value.ObservableBatch().Subscribe(
                onNext: ops =>
                {
                    callCount++;
                    lastValue = ops.Last().value;
                }
            );

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(value.value, lastValue);

            init = false;
            value.value = 4;

            Assert.AreEqual(2, callCount);
            Assert.IsFalse(init);
            Assert.AreEqual(value.value, lastValue);

            context.ExecuteBatchOperation(() =>
            {
                value.value = 5;
                value.value = 6;
                value.value = 7;
                value.value = 8;
            });

            Assert.AreEqual(3, callCount);
            Assert.IsFalse(init);
            Assert.AreEqual(value.value, lastValue);
        }

        [Test]
        public void TestCollectionInitialization()
        {
            ObservationContext context = new ObservationContext();
            var dictionary = new ObservableDictionary<string, int>(context);

            dictionary.Add("cat", 1);
            dictionary.Add("dog", 2);
            dictionary.Add("frog", 3);

            var initOps = new List<TestCollectionOperation>();

            Observables.ObservableCombine(dictionary.ObservableEventStream()).Subscribe(onNext: op =>
            {
                initOps.Add(new() { source = op.source, value = new KeyValuePair<string, int>(op.key, op.value), isRemove = op.isRemove });
            });

            Assert.That(
                initOps,
                Is.EqualTo(
                    new List<TestCollectionOperation>()
                    {
                        new TestCollectionOperation() { source = dictionary, value=  KeyValuePair.Create("cat", 1), isRemove = false},
                        new TestCollectionOperation() { source = dictionary, value = KeyValuePair.Create("dog", 2), isRemove = false },
                        new TestCollectionOperation() { source = dictionary, value = KeyValuePair.Create("frog", 3), isRemove = false }
                    }
                )
            );

            initOps.Clear();

            var list = new ObservableList<float>(context);

            list.Add(0.22f);
            list.Add(0.11f);
            list.Add(-1000f);
            list.Insert(1, 50f);

            var subscription = Observables.ObservableCombine(
                false,
                dictionary.ObservableEventStream(
                    (id, kvp) => new TestCollectionOperation() { source = dictionary, value = kvp, isRemove = false },
                    (id, kvp) => new TestCollectionOperation() { source = dictionary, value = kvp, isRemove = true }
                ),
                list.ObservableEventStream(
                    (id, index, element) => new TestCollectionOperation() { source = list, index = index, value = element, isRemove = false },
                    (id, index, element) => new TestCollectionOperation() { source = list, index = index, value = element, isRemove = true }
                )
            ).Subscribe(onNext: op => initOps.Add(op));

            Assert.That(
                initOps,
                Is.EqualTo(
                    new List<TestCollectionOperation>()
                    {
                        new () { source = dictionary, value = KeyValuePair.Create("cat", 1), isRemove = false },
                        new () { source = dictionary, value = KeyValuePair.Create("dog", 2), isRemove = false },
                        new () { source = dictionary, value = KeyValuePair.Create("frog", 3), isRemove = false },
                        new () { source = list, index = 0, value = 0.22f, isRemove = false },
                        new () { source = list, index = 1, value = 50f, isRemove = false },
                        new () { source = list, index = 2, value = 0.11f, isRemove = false },
                        new () { source = list, index = 3, value = -1000f, isRemove = false }
                    }
                )
            );
        }
    }
}
