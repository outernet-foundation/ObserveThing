using System;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ObserveThing.Tests
{
    public class ValueObservableTests
    {
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
            Assert.IsTrue(disposed); //should not produce an OnDisposed call

            toggle.value = true;
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
            int currentValue = 0;
            int previousValue = 0;
            var source = new ValueObservable<int>();
            var stream = source.ObservableWithPrevious().Subscribe(x =>
            {
                currentValue = x.current;
                previousValue = x.previous;
            });

            Assert.AreEqual(currentValue, 0);
            Assert.AreEqual(previousValue, 0);

            source.value = 1;

            Assert.AreEqual(currentValue, 1);
            Assert.AreEqual(previousValue, 0);

            source.value = 2;

            Assert.AreEqual(currentValue, 2);
            Assert.AreEqual(previousValue, 1);
        }
    }
}