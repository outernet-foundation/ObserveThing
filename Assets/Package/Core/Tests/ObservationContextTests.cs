using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ObserveThing.Tests
{
    public class ObservationContextTests
    {
        [Test]
        public void TestOperationOrder()
        {
            var context = new ObservationContext();

            ObservableValue<int> intObservable = new ObservableValue<int>(context, 100);
            ObservableValue<string> stringObservable = new ObservableValue<string>(context, "200");
            List<(IObservable source, object value)> operations = new List<(IObservable source, object value)>();

            int callCount = 0;

            var stream = Observables.ObservableCombine(context, intObservable, stringObservable).Subscribe(
                onOperation: ops =>
                {
                    callCount++;
                    operations.AddRange(ops.Select<IOperation, (IObservable source, object value)>(x => new(x.source, x.value)));
                }
            );

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(
                new List<(IObservable, object)>
                {
                    new(intObservable, 100),
                    new(stringObservable, "200")
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

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(
                new List<(IObservable, object)>
                {
                    new(intObservable, 1),
                    new(stringObservable, "2"),
                    new(intObservable, 3),
                    new(intObservable, 4),
                    new(stringObservable, "5"),
                },
                operations
            );
        }

        [Test]
        public void TestRollingObserverOrder()
        {
            var context = new ObservationContext();

            ObservableValue<int> intObservable = new ObservableValue<int>(context);

            List<(int observer, object value)> observerCallOrder = new List<(int observer, object value)>();

            IDisposable subscription1 = default;
            IDisposable subscription2 = default;
            IDisposable subscription3 = default;
            IDisposable subscription4 = default;
            IDisposable subscription5 = default;

            subscription1 = intObservable.Subscribe(
                onOperation: ops =>
                {
                    foreach (var op in ops)
                        observerCallOrder.Add(new(1, op.value));
                }
            );

            subscription2 = intObservable.Subscribe(
                onOperation: ops =>
                {
                    foreach (var op in ops)
                        observerCallOrder.Add(new(2, op.value));

                    if (intObservable.value == 1)
                        intObservable.value = 2;
                }
            );

            subscription3 = intObservable.Subscribe(
                onOperation: ops =>
                {
                    foreach (var op in ops)
                        observerCallOrder.Add(new(3, op.value));

                    if (intObservable.value == 2)
                    {
                        intObservable.value = 3;
                        subscription4.Dispose();
                    }
                }
            );

            subscription4 = intObservable.Subscribe(
                onOperation: ops =>
                {
                    foreach (var op in ops)
                        observerCallOrder.Add(new(4, op.value));
                }
            );

            subscription5 = intObservable.Subscribe(
                onOperation: ops =>
                {
                    foreach (var op in ops)
                        observerCallOrder.Add(new(5, op.value));
                }
            );

            intObservable.value = 1;

            Assert.AreEqual(3, intObservable.value);

            Assert.AreEqual(
                new List<(int observer, object value)>()
                {
                    new (1, 0), //init observer1
                    new (2, 0), //init observer2
                    new (3, 0), //init observer3
                    new (4, 0), //init observer4
                    new (5, 0), //init observer5
                    new (1, 1), //observer1 observes external setting intObservable to 1
                    new (2, 1), //observer2 observes external setting intObservable to 1
                    new (1, 2), //observer1 observes observer2 setting intObservable to 2
                    new (2, 2), //observer2 observes observer2 setting intObservable to 2
                    new (3, 1), //observer3 observes external setting intObservable to 1
                    new (3, 2), //observer3 observes observer2 setting intObservable to 2
                    //4, //observer4 is never called past the init because it gets unsubscribed by observer3
                    new (1, 3), //observer1 observes observer3 setting intObservable to 3
                    new (2, 3), //observer2 observes observer3 setting intObservable to 3
                    new (3, 3), //observer3 observes observer3 setting intObservable to 3
                    new (5, 1), //observer5 observes external setting intObservable to 1
                    new (5, 2), //observer5 observes observer2 setting intObservable to 2
                    new (5, 3) //observer5 observes observer3 setting intObservable to 3
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

            var query = Observables.ObservableCombine(context, intObservable, stringObservable);

            List<(IObservable source, object value)> observerCallOrder = new List<(IObservable source, object value)>();

            IDisposable subscription = query.Subscribe(
                onOperation: ops =>
                {
                    UnityEngine.Debug.Log("EP: Batch is\n" + string.Join("\n", ops.Select(x => $"{x.source} {x.value}")));

                    foreach (var op in ops)
                        observerCallOrder.Add(new(op.source, op.value));

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

            UnityEngine.Debug.Log("EP:\n" + string.Join("\n", observerCallOrder.Select(x => $"{x.source} {x.value}")));

            Assert.AreEqual(
                new List<(IObservable source, object value)>()
                {
                    new(intObservable, 0),
                    new(stringObservable, null),
                    new(intObservable, 1),
                    new(stringObservable, "cat"),
                    new(stringObservable, "dog"),
                    new(intObservable, 2),
                    new(stringObservable, "frog"),
                    new(intObservable, 3),
                    new(intObservable, 4),
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

            var query = Observables.ObservableCombine(context, observable1, observable2);

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

            query.Subscribe(
                onDispose: () =>
                {
                    disposeCallCount++;
                    disposed = true;
                }
            );

            Assert.IsTrue(disposed);
            Assert.AreEqual(1, disposeCallCount);
        }

        [Test]
        public void TestObservable()
        {
            var context = new ObservationContext();
            var value = new ObservableValue<int>(context, 2);

            var init = false;
            var lastValue = 0;
            var callCount = 0;

            value.Subscribe(new Observer<int>(
                onOperation: ops =>
                {
                    callCount++;
                    lastValue = ops.Last();
                }
            ));

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

            List<(IObservable source, object value)> initOps = new List<(IObservable source, object value)>();

            dictionary.Subscribe((IReadOnlyList<IOperation> x) => initOps.AddRange(x.Select<IOperation, (IObservable source, object value)>(x => new(x.source, x.value))));

            Assert.That(
                initOps,
                Is.EquivalentTo(
                    new List<(IObservable source, object value)>()
                    {
                        new(dictionary, new DictionaryOpArgs<string, int>(0, KeyValuePair.Create("cat", 1), false)),
                        new(dictionary, new DictionaryOpArgs<string, int>(1, KeyValuePair.Create("dog", 2), false)),
                        new(dictionary, new DictionaryOpArgs<string, int>(2, KeyValuePair.Create("frog", 3), false))
                    }
                )
            );

            initOps.Clear();

            var list = new ObservableList<float>(context);

            list.Add(0.22f);
            list.Add(0.11f);
            list.Add(-1000);
            list.Insert(1, 50);

            var subscription = Observables.ObservableCombine(dictionary, list).Subscribe(x => initOps.AddRange(x.Select<IOperation, (IObservable source, object value)>(x => new(x.source, x.value))));

            Assert.That(
                initOps,
                Is.EquivalentTo(
                    new List<(IObservable source, object value)>()
                    {
                        new(dictionary, new DictionaryOpArgs<string, int>(0, KeyValuePair.Create("cat", 1), false)),
                        new(dictionary, new DictionaryOpArgs<string, int>(1, KeyValuePair.Create("dog", 2), false)),
                        new(dictionary, new DictionaryOpArgs<string, int>(2, KeyValuePair.Create("frog", 3), false)),
                        new(list, new ListOpArgs<float>(0, 0, 0.22f, false)),
                        new(list, new ListOpArgs<float>(1, 2, 0.11f, false)),
                        new(list, new ListOpArgs<float>(2, 3, -1000, false)),
                        new(list, new ListOpArgs<float>(3, 1, 50, false))
                    }
                )
            );
        }
    }
}
