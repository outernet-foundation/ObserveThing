using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ObserveThing.Tests
{
    public class ObservableValueTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.DefaultExceptionHandler = UnityEngine.Debug.LogException;
        }

        [Test]
        public void TestErrorLogging()
        {
            var source = new ObservableValue<bool>();
            var errorSelect = source
                .ObservableSelect(x => x)
                .ObservableSelect(x => x)
                .ObservableSelect(x => x)
                .ObservableSelect(x =>
                {
                    if (x)
                        throw new Exception("This is an exception");

                    return x;
                });

            var errorObservable = errorSelect.Subscribe();

            LogAssert.Expect(UnityEngine.LogType.Exception, "Exception: This is an exception");
            source.value = true;

            errorObservable.Dispose();
            source.value = false;

            errorObservable = errorSelect.Subscribe(
                onError: exc => UnityEngine.Debug.Log("Got exception")
            );

            LogAssert.Expect(UnityEngine.LogType.Log, "Got exception");
            source.value = true;
        }

        [Test]
        public void TestSelect()
        {
            int callCount = 0;
            int result = 0;

            Exception exception = null;
            bool disposed = false;

            var toggle = new ObservableValue<bool>();
            var selectObservable = toggle
                .ObservableSelect(x => x ? 0 : 1)
                .Subscribe(
                    x =>
                    {
                        callCount++;
                        result = x;
                    },
                    exc => exception = exc,
                    () => disposed = true
                );

            // init call
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(1, result);

            toggle.value = true;
            Assert.AreEqual(2, callCount);
            Assert.AreEqual(0, result);

            toggle.value = false;
            Assert.AreEqual(3, callCount);
            Assert.AreEqual(1, result);

            toggle.value = false;
            Assert.AreEqual(3, callCount);
            Assert.AreEqual(1, result);

            toggle.Dispose();
            Assert.IsTrue(disposed);

            Assert.Throws(typeof(ObjectDisposedException), () => toggle.value = true);
            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void TestSelectRaisesException()
        {
            Exception exception = null;
            var source = new ObservableValue<bool>();
            var selectChain = source.ObservableSelect(x => x).ObservableSelect(x => x);

            var stream = selectChain.Subscribe(
                onNext: x =>
                {
                    if (x)
                        throw new Exception("This is an exception");
                },
                onError: exc => exception = exc
            );

            source.value = true;
            Assert.IsNotNull(exception);

            exception = null;

            source.value = false;
            Assert.IsNull(exception);

            stream.Dispose();

            source.value = true;
            Assert.IsNull(exception);

            source.value = false;
            Assert.IsNull(exception);

            stream = selectChain.Subscribe(
                onNext: x =>
                {
                    if (x)
                        throw new Exception("This is an exception");
                }
            );

            LogAssert.Expect(UnityEngine.LogType.Exception, "Exception: This is an exception");
            source.value = true;
        }

        [Test]
        public void TestWithPrevious()
        {
            bool disposed = false;
            bool receivedCall = false;
            int currentValue = 0;
            int previousValue = 0;
            var source = new ObservableValue<int>();
            var stream = source.ObservableWithPrevious().Subscribe(
                onNext: x =>
                {
                    currentValue = x.current;
                    previousValue = x.previous;
                },
                onDispose: () => disposed = true
            );

            Assert.AreEqual(currentValue, 0);
            Assert.AreEqual(previousValue, 0);

            source.value = 1;

            Assert.AreEqual(currentValue, 1);
            Assert.AreEqual(previousValue, 0);

            source.value = 2;

            Assert.AreEqual(currentValue, 2);
            Assert.AreEqual(previousValue, 1);

            receivedCall = false;
            stream.Dispose();
            Assert.IsTrue(disposed);
            source.value = 100;
            Assert.IsFalse(receivedCall);
        }

        [Test]
        public void TestShallowCopy()
        {
            var result = 0;
            var source = new ObservableValue<ObservableValue<int>>(new ObservableValue<int>(10));
            bool disposed = false;
            bool receivedCall = false;
            var subscription = source.ObservableShallowCopy().Subscribe(
                onNext: x =>
                {
                    result = x;
                    receivedCall = true;
                },
                onDispose: () => disposed = true
            );

            Assert.AreEqual(10, result);

            source.value.value = 100;

            Assert.AreEqual(100, result);

            var prevValue = source.value;
            source.value = new ObservableValue<int>(-3);

            Assert.AreEqual(-3, result);

            prevValue.value = 2;

            Assert.AreEqual(-3, result);

            source.value = null;

            Assert.AreEqual(0, result);

            source.value = prevValue;

            Assert.AreEqual(2, result);

            receivedCall = false;
            subscription.Dispose();
            Assert.IsTrue(disposed);
            source.value.value = 100;
            Assert.IsFalse(receivedCall);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void TestImmediateObserverOrder()
        {
            ObservableValue<int> observable = new ObservableValue<int>();
            bool streamCalledFirst = false;
            bool immediateStreamCalledFirst = false;
            var chainedObservable = observable.ObservableSelect(x => x * 2);

            var stream = chainedObservable.Subscribe(
                onNext: x =>
                {
                    if (!immediateStreamCalledFirst)
                        streamCalledFirst = true;
                },
                onDispose: () =>
                {
                    if (!immediateStreamCalledFirst)
                        streamCalledFirst = true;
                }
            );

            var immediateStream = chainedObservable.Subscribe(
                immediate: true,
                onNext: x =>
                {
                    if (!streamCalledFirst)
                        immediateStreamCalledFirst = true;
                },
                onDispose: () =>
                {
                    if (!streamCalledFirst)
                        immediateStreamCalledFirst = true;
                }
            );

            streamCalledFirst = false;
            immediateStreamCalledFirst = false;

            observable.value++;

            Assert.IsTrue(immediateStreamCalledFirst);
            Assert.IsFalse(streamCalledFirst);

            streamCalledFirst = false;
            immediateStreamCalledFirst = false;

            observable.Dispose();

            Assert.IsTrue(immediateStreamCalledFirst);
            Assert.IsFalse(streamCalledFirst);
        }

        [Test]
        public void TestImmediateSubscription()
        {
            ObservableValue<int> observable = new ObservableValue<int>();

            bool standardFired = false;
            bool immediateFired = false;

            bool standardFiredFirst = false;
            bool immediateFiredFirst = false;

            var standardStream = observable.Subscribe(
                onNext: x =>
                {
                    standardFired = true;

                    if (!immediateFiredFirst)
                        standardFiredFirst = true;
                }
            );

            var immediateStream = observable.Subscribe(
                immediate: true,
                onNext: x =>
                {
                    immediateFired = true;

                    if (!standardFiredFirst)
                        immediateFiredFirst = true;
                }
            );

            standardFired = false;
            immediateFired = false;

            standardFiredFirst = false;
            immediateFiredFirst = false;

            observable.value = 1;

            Assert.IsTrue(standardFired);
            Assert.IsTrue(immediateFired);
            Assert.IsTrue(immediateFiredFirst);
            Assert.IsFalse(standardFiredFirst);
        }
    }
}