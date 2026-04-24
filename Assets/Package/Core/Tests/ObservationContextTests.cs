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
            ObservableValue<string> stringObservable = new ObservableValue<string>(context);

            List<(int observer, IObservable source, object value)> observerCallOrder = new List<(int observer, IObservable source, object value)>();

            IDisposable stream1 = default;
            IDisposable stream2 = default;
            IDisposable stream3 = default;
            IDisposable stream4 = default;
            IDisposable stream5 = default;

            stream1 = Observables.ObservableCombine(context, intObservable, stringObservable).Subscribe(
                onOperation: ops =>
                {
                    if (ops == null)
                    {
                        observerCallOrder.Add(new(1, null, null));
                        return;
                    }

                    foreach (var op in ops)
                        observerCallOrder.Add(new(1, op.source, op.value));
                }
            );

            stream2 = Observables.ObservableCombine(context, intObservable, stringObservable).Subscribe(
                onOperation: ops =>
                {
                    if (ops == null)
                    {
                        observerCallOrder.Add(new(2, null, null));
                        return;
                    }

                    foreach (var op in ops)
                        observerCallOrder.Add(new(2, op.source, op.value));

                    if (intObservable.value == 1)
                        intObservable.value = 2;
                }
            );

            stream3 = Observables.ObservableCombine(context, intObservable, stringObservable).Subscribe(
                onOperation: ops =>
                {
                    if (ops == null)
                    {
                        observerCallOrder.Add(new(3, null, null));
                        return;
                    }

                    foreach (var op in ops)
                        observerCallOrder.Add(new(3, op.source, op.value));

                    if (stringObservable.value == "cat")
                        intObservable.value = 3;

                    if (intObservable.value == 2)
                    {
                        stringObservable.value = "cat";
                        stream4.Dispose();
                    }
                }
            );

            stream4 = Observables.ObservableCombine(context, intObservable, stringObservable).Subscribe(
                onOperation: ops =>
                {
                    if (ops == null)
                    {
                        observerCallOrder.Add(new(4, null, null));
                        return;
                    }

                    foreach (var op in ops)
                        observerCallOrder.Add(new(4, op.source, op.value));
                }
            );

            stream5 = Observables.ObservableCombine(context, intObservable, stringObservable).Subscribe(
                onOperation: ops =>
                {
                    if (ops == null)
                    {
                        observerCallOrder.Add(new(5, null, null));
                        return;
                    }

                    foreach (var op in ops)
                        observerCallOrder.Add(new(5, op.source, op.value));
                }
            );

            intObservable.value = 1;

            Assert.AreEqual(3, intObservable.value);
            Assert.AreEqual("cat", stringObservable.value);

            Assert.AreEqual(
                new List<(int observer, IObservable source, object value)>()
                {
                    new (1, intObservable, 0), //init observer1
                    new (1, stringObservable, null), //init observer1
                    new (2, intObservable, 0), //init observer2
                    new (2, stringObservable, null), //init observer2
                    new (3, intObservable, 0), //init observer3
                    new (3, stringObservable, null), //init observer3
                    new (4, intObservable, 0), //init observer4
                    new (4, stringObservable, null), //init observer4
                    new (5, intObservable, 0), //init observer5
                    new (5, stringObservable, null), //init observer5
                    new (1, intObservable, 1), //observer1 observes external setting intObservable to 1
                    new (2, intObservable, 1), //observer2 observes external setting intObservable to 1
                    new (1, intObservable, 2), //observer1 observes observer2 setting intObservable to 2
                    new (2, intObservable, 2), //observer2 observes observer2 setting intObservable to 2
                    new (3, intObservable, 1), //observer3 observes external setting intObservable to 1
                    new (3, intObservable, 2), //observer3 observes observer2 setting intObservable to 2
                    new (1, stringObservable, "cat"), //observer1 observes observer3 setting stringObservable to "cat"
                    new (2, stringObservable, "cat"), //observer2 observes observer3 setting stringObservable to "cat"
                    new (3, stringObservable, "cat"), //observer3 observes observer3 setting stringObservable to "cat"
                    //4, //observer4 is never called past the init because it gets unsubscribed by observer3
                    new (1, intObservable, 3), //observer1 observes observer3 setting intObservable to 3
                    new (2, intObservable, 3), //observer2 observes observer3 setting intObservable to 3
                    new (3, intObservable, 3), //observer3 observes observer3 setting intObservable to 3
                    new (5, intObservable, 1), //observer5 observes external setting intObservable to 1
                    new (5, intObservable, 2), //observer5 observes observer2 setting intObservable to 2
                    new (5, stringObservable, "cat"), //observer5 observers observer2 settings stringObservable to "cat"
                    new (5, intObservable, 3) //observer5 observes observer3 setting intObservable to 3
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

            var stream = Observables.ObservableCombine(context, observable1, observable2).Subscribe(
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
