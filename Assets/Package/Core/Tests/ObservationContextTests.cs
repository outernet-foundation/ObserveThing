using System;
using System.Collections.Generic;
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

            ValueObservable<int> intObservable = new ValueObservable<int>(context);
            ValueObservable<string> stringObservable = new ValueObservable<string>(context);
            List<(IOperationObservable source, object value)> operations = new List<(IOperationObservable source, object value)>();

            int callCount = 0;
            bool init = false;

            context.RegisterObserver(
                new OperationObserver(
                    onNext: ops =>
                    {
                        callCount++;

                        if (ops == null)
                        {
                            init = true;
                            return;
                        }

                        operations.AddRange(ops.Select<IOperation, (IOperationObservable source, object value)>(x => new(x.source, x.value)));
                    }
                ),
                intObservable,
                stringObservable
            );

            Assert.AreEqual(1, callCount);
            Assert.IsTrue(init);

            callCount = 0;
            init = false;

            context.ExecuteBatchOperation(() =>
            {
                intObservable.value = 1;
                stringObservable.value = "2";
                intObservable.value = 3;
                intObservable.value = 4;
                stringObservable.value = "5";
            });

            Assert.AreEqual(1, callCount);
            Assert.IsFalse(init);

            AssertValueOp(operations[0], intObservable, 1);
            AssertValueOp(operations[1], stringObservable, "2");
            AssertValueOp(operations[2], intObservable, 3);
            AssertValueOp(operations[3], intObservable, 4);
            AssertValueOp(operations[4], stringObservable, "5");
        }

        private void AssertValueOp<T>((IOperationObservable source, object value) op, IOperationObservable source, T value)
        {
            Assert.AreEqual(source, op.source);
            Assert.AreEqual(value, op.value);
        }

        [Test]
        public void TestRollingObserverOrder()
        {
            var context = new ObservationContext();

            ValueObservable<int> intObservable = new ValueObservable<int>(context);
            ValueObservable<string> stringObservable = new ValueObservable<string>(context);

            List<(int observer, IOperationObservable source, object value)> observerCallOrder = new List<(int observer, IOperationObservable source, object value)>();

            var observer1 = new OperationObserver(
                onNext: ops =>
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

            var observer2 = new OperationObserver(
                onNext: ops =>
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

            var observer4 = new OperationObserver(
                onNext: ops =>
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

            var observer3 = new OperationObserver(
                onNext: ops =>
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
                        context.DeregisterObserver(observer4);
                    }
                }
            );

            var observer5 = new OperationObserver(
                onNext: ops =>
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

            context.RegisterObserver(observer1, intObservable, stringObservable);
            context.RegisterObserver(observer2, intObservable, stringObservable);
            context.RegisterObserver(observer3, intObservable, stringObservable);
            context.RegisterObserver(observer4, intObservable, stringObservable);
            context.RegisterObserver(observer5, intObservable, stringObservable);

            intObservable.value = 1;

            Assert.AreEqual(3, intObservable.value);
            Assert.AreEqual("cat", stringObservable.value);

            Assert.AreEqual(
                new List<(int observer, IOperationObservable source, object value)>()
                {
                    new (1, null, null), //init observer1
                    new (2, null, null), //init observer2
                    new (3, null, null), //init observer3
                    new (4, null, null), //init observer4
                    new (5, null, null), //init observer5
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
            var observable1 = new ValueObservable<int>(context);
            var observable2 = new ValueObservable<int>(context);
            var disposeCallCount = 0;
            var disposed = false;
            var observer = new OperationObserver(onDispose: () =>
            {
                disposeCallCount++;
                disposed = true;
            });

            context.RegisterObserver(observer, observable1, observable2);

            observable1.Dispose();

            Assert.IsFalse(disposed);
            Assert.AreEqual(0, disposeCallCount);

            observable2.Dispose();

            Assert.IsTrue(disposed);
            Assert.AreEqual(1, disposeCallCount);
        }
    }
}
