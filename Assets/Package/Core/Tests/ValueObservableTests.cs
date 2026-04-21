using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ObserveThing.Tests
{
    public class ValueObservableTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.DefaultExceptionHandler = UnityEngine.Debug.LogException;
        }

        [Test]
        public void TestErrorLogging()
        {
            var source = new ValueObservable<bool>();
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

            var toggle = new ValueObservable<bool>();
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
            var source = new ValueObservable<bool>();
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
            var source = new ValueObservable<int>();
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
            var source = new ValueObservable<ValueObservable<int>>(new ValueObservable<int>(10));
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
            source.value = new ValueObservable<int>(-3);

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
        public void TestThen()
        {
            var source = new ValueObservable<int>();

            var thenValue = default(int);
            var thenExc = default(Exception);
            var thenDisposed = default(bool);

            var subValue = default(int);
            var subExc = default(Exception);
            var subDisposed = default(bool);

            var thenObserver = new ValueObserver<int>(
                onNext: x => thenValue = x,
                onError: x => thenExc = x,
                onDispose: () => thenDisposed = true
            );

            var subObserver = new ValueObserver<int>(
                onNext: x => subValue = x,
                onError: x => subExc = x,
                onDispose: () => subDisposed = true
            );

            var observable = source.ObservableThen(thenObserver);
            var stream = observable.Subscribe(subObserver);

            Assert.AreEqual(default(int), thenValue);
            Assert.IsNull(thenExc);
            Assert.IsFalse(thenDisposed);

            Assert.AreEqual(default(int), subValue);
            Assert.IsNull(subExc);
            Assert.IsFalse(subDisposed);

            stream.Dispose();

            Assert.AreEqual(default(int), thenValue);
            Assert.IsNull(thenExc);
            Assert.IsTrue(thenDisposed);

            Assert.AreEqual(default(int), subValue);
            Assert.IsNull(subExc);
            Assert.IsTrue(subDisposed);

            thenDisposed = false;
            subDisposed = false;
            source.value = 3;

            Assert.AreEqual(default(int), thenValue);
            Assert.IsNull(thenExc);
            Assert.IsFalse(thenDisposed);

            Assert.AreEqual(default(int), subValue);
            Assert.IsNull(subExc);
            Assert.IsFalse(subDisposed);

            stream = observable.Subscribe(subObserver);

            Assert.AreEqual(3, thenValue);
            Assert.IsNull(thenExc);
            Assert.IsFalse(thenDisposed);

            Assert.AreEqual(3, subValue);
            Assert.IsNull(subExc);
            Assert.IsFalse(subDisposed);

            source.value = 10;

            Assert.AreEqual(10, thenValue);
            Assert.IsNull(thenExc);
            Assert.IsFalse(thenDisposed);

            Assert.AreEqual(10, subValue);
            Assert.IsNull(subExc);
            Assert.IsFalse(subDisposed);

            stream.Dispose();

            var thenExcToThrow = new Exception("Then Exc To Throw");
            var subExcToThrow = new Exception("Sub Exc To Throw");

            observable = source
                .ObservableThen(
                    onNext: x =>
                    {
                        if (x == 1)
                            throw thenExcToThrow;
                    },
                    onError: exc => thenExc = exc
                );

            stream = observable.Subscribe(
                onNext: x =>
                {
                    if (x == 2)
                        throw subExcToThrow;
                },
                onError: exc => subExc = exc
            );

            Assert.IsNull(thenExc);
            Assert.IsNull(subExc);

            source.value = 1;

            Assert.AreEqual(thenExcToThrow, thenExc);
            Assert.IsNull(subExc);

            thenExc = null;
            subExc = null;

            source.value = 2;

            Assert.IsNull(thenExc);
            Assert.AreEqual(subExcToThrow, subExc);
        }

        [Test]
        public void TestShare()
        {
            var source = new ValueObservable<int>();

            var preShareCallCount = default(int);
            var preShareValue = default(int);

            var stream1CallCount = default(int);
            var stream1Value = default(int);

            var stream2CallCount = default(int);
            var stream2Value = default(int);

            var observable = source
                .ObservableThen(x =>
                {
                    preShareValue = x;
                    preShareCallCount++;
                })
                .ObservableShare();

            IDisposable stream1 = default;
            IDisposable stream2 = default;

            stream1 = observable.Subscribe(
                onNext: x =>
                {
                    stream1Value = x;
                    stream1CallCount++;

                    if (x == 2)
                        stream2.Dispose();
                }
            );

            stream2 = observable.Subscribe(
                onNext: x =>
                {
                    stream2Value = x;
                    stream2CallCount++;
                }
            );

            Assert.AreEqual(1, preShareCallCount);
            Assert.AreEqual(0, preShareValue);

            Assert.AreEqual(1, stream1CallCount);
            Assert.AreEqual(0, stream1Value);

            Assert.AreEqual(1, stream2CallCount);
            Assert.AreEqual(0, stream2Value);

            source.value = 1;

            Assert.AreEqual(2, preShareCallCount);
            Assert.AreEqual(1, preShareValue);

            Assert.AreEqual(2, stream1CallCount);
            Assert.AreEqual(1, stream1Value);

            Assert.AreEqual(2, stream2CallCount);
            Assert.AreEqual(1, stream2Value);

            // Test dispose while notifying
            source.value = 2;

            Assert.AreEqual(3, preShareCallCount);
            Assert.AreEqual(2, preShareValue);

            Assert.AreEqual(3, stream1CallCount);
            Assert.AreEqual(2, stream1Value);

            Assert.AreEqual(2, stream2CallCount);
            Assert.AreEqual(1, stream2Value);


            // Test event with no subscriptions
            stream1.Dispose();

            source.value = 3;

            Assert.AreEqual(3, preShareCallCount);
            Assert.AreEqual(2, preShareValue);

            Assert.AreEqual(3, stream1CallCount);
            Assert.AreEqual(2, stream1Value);

            Assert.AreEqual(2, stream2CallCount);
            Assert.AreEqual(1, stream2Value);

            // Test set value during notifications

            var stream1Values = new List<int>();
            var stream2Values = new List<int>();

            stream1 = observable.Subscribe(onNext: x =>
            {
                stream1Values.Add(x);

                if (x == 4)
                    source.value = 5;
            });

            stream2 = observable.Subscribe(onNext: x =>
            {
                stream2Values.Add(x);
            });

            source.value = 4;

            Assert.That(stream1Values, Is.EqualTo(new int[] { 3, 4, 5 }));
            Assert.That(stream2Values, Is.EqualTo(new int[] { 3, 4, 5 }));
        }

        [Test]
        public void TestImmediateObserverOrder()
        {
            ValueObservable<int> observable = new ValueObservable<int>();
            bool streamCalledFirst = false;
            bool immediateStreamCalledFirst = false;

            var stream = observable.ObservableSelect(x => x * 2).Subscribe(
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

            var immediateStream = observable.ObservableSelect(x => x * 2).Subscribe(
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
            ValueObservable<int> observable = new ValueObservable<int>();

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