using System;
using System.Collections.Generic;
using FofX.Stateful;
using NUnit.Framework;
using ObserveThing.StatefulExtensions;

namespace ObserveThing.Tests
{
    public class DictionaryObservableTests
    {
        [Test]
        public void TestAsObservable()
        {
            int callCount = 0;
            Exception exception = default;
            bool disposed = false;
            var result = new Dictionary<int, ObservablePrimitive<string>>();

            var dict = new ObservableDictionary<int, ObservablePrimitive<string>>();
            dict.Initialize("root", new ObservableNodeContext());

            var observable = dict.AsObservable().Subscribe(
                onAdd: x =>
                {
                    callCount++;
                    result.Add(x.Key, x.Value);
                },
                onRemove: x =>
                {
                    callCount++;
                    result.Remove(x.Key);
                },
                exc => exception = exc,
                () => disposed = true
            );

            Assert.AreEqual(0, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                dict,
                result
            );

            dict.ExecuteAction(
                dict => dict.Add(2).value = "cat"
            );

            Assert.AreEqual(1, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                dict,
                result
            );

            dict.ExecuteAction(
                dict => dict.Add(4).value = "dog"
            );

            Assert.AreEqual(2, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                dict,
                result
            );

            dict.ExecuteAction(
                dict => dict.Remove(4)
            );

            Assert.AreEqual(3, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                dict,
                result
            );

            dict.ExecuteAction(
                dict => dict.Remove(40)
            );

            Assert.AreEqual(3, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                dict,
                result
            );

            dict.ExecuteAction(
                dict => dict.Dispose()
            );

            Assert.AreEqual(3, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(true, disposed);
            CollectionAssert.AreEquivalent(
                dict,
                result
            );
        }
    }
}