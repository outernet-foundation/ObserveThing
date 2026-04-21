using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ObserveThing.Tests
{
    public class DictionaryObservableTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.DefaultExceptionHandler = UnityEngine.Debug.LogException;
        }

        [Test]
        public void TestTrack()
        {
            int callCount = 0;
            (bool keyPresent, string value) value = default;
            Exception exception = default;
            bool disposed = false;

            DictionaryObservable<int, string> dict = new DictionaryObservable<int, string>();
            ValueObservable<int> key = new ValueObservable<int>();

            dict.ObservableTrack(key).Subscribe(
                x =>
                {
                    callCount++;
                    value = x;
                },
                exc => exception = exc,
                () => disposed = true
            );

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(false, value.keyPresent);
            Assert.AreEqual(default, value.value);

            dict.Add(2, "cat");

            Assert.AreEqual(1, callCount);
            Assert.AreEqual(false, value.keyPresent);
            Assert.AreEqual(default, value.value);

            key.value = 2;

            Assert.AreEqual(2, callCount);
            Assert.AreEqual(true, value.keyPresent);
            Assert.AreEqual("cat", value.value);

            dict.Remove(2);

            Assert.AreEqual(3, callCount);
            Assert.AreEqual(false, value.keyPresent);
            Assert.AreEqual(default, value.value);

            dict.Remove(40);

            Assert.AreEqual(3, callCount);
            Assert.AreEqual(false, value.keyPresent);
            Assert.AreEqual(default, value.value);

            dict.Add(2, "dog");

            Assert.AreEqual(4, callCount);
            Assert.AreEqual(true, value.keyPresent);
            Assert.AreEqual("dog", value.value);

            // var exc = new Exception();
            // dict.OnError(exc);
            // Assert.AreEqual(exc, exception);

            // var keyProviderExc = new Exception();
            // dict.OnError(keyProviderExc);
            // Assert.AreEqual(keyProviderExc, exception);

            dict.Dispose();
            Assert.IsTrue(disposed);
            Assert.Throws(typeof(ObjectDisposedException), () => dict.Add(100, "me"));
            Assert.AreEqual(4, callCount);
        }
    }
}