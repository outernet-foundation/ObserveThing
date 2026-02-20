using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObserveThing.Tests
{
    public class ValueObservableTests
    {
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

            // TODO: Implement?
            // Exception excRoot = new Exception();
            // toggle.OnError(excRoot);
            // Assert.AreEqual(excRoot, exception);

            toggle.Dispose();
            Assert.IsTrue(disposed); //should not produce an OnDisposed call

            toggle.value = true;
            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void TestSelectRaisesException()
        {
            Exception exception = null;
            var toggle = new ValueObservable<bool>();
            var stream = toggle.ObservableSelect(x => x).ObservableSelect(x => x).Subscribe(
                onNext: x =>
                {
                    if (x)
                        throw new Exception("THIS IS AN EXCEPTION");
                },
                onError: exc => exception = exc
            );

            toggle.value = true;
            Assert.IsNotNull(exception);

            exception = null;

            toggle.value = false;
            Assert.IsNull(exception);

            stream.Dispose();

            toggle.value = true;
            Assert.IsNull(exception);

            toggle.value = false;
            Assert.IsNull(exception);

            // TODO: Decide
            // At one point the intent was that, when an observable generates an error that the
            // observer doesn't catch, the error should throw like normal. I'm not sure if we can
            // actually achieve that goal or if that's the correct intent to have.
            // var streamWithHandler = toggle.SelectDynamic(x => x).SelectDynamic(x => x).Subscribe(
            //     x =>
            //     {
            //         if (x)
            //             throw new Exception("THIS IS AN EXCEPTION");
            //     },
            //     exc => exception = exc
            // );

            // Assert.DoesNotThrow(() => toggle.value = true);
            // Assert.IsNotNull(exception);
        }
    }
}