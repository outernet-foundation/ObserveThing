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

            var rootObservable = new ObservableDictionary<int, ObservablePrimitive<string>>();
            rootObservable.Initialize("root", new ObservableNodeContext());

            var observable = rootObservable.AsObservable().Subscribe(
                args =>
                {
                    callCount++;
                    if (args.operationType == OpType.Add)
                    {
                        result.Add(args.key, args.value);
                    }
                    else if (args.operationType == OpType.Remove)
                    {
                        result.Remove(args.key);
                    }
                },
                exc => exception = exc,
                () => disposed = true
            );

            Assert.AreEqual(0, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                rootObservable,
                result
            );

            rootObservable.ExecuteAction(
                dict => dict.Add(2).value = "cat"
            );

            Assert.AreEqual(1, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                rootObservable,
                result
            );

            rootObservable.ExecuteAction(
                dict => rootObservable.Add(4).value = "dog"
            );

            Assert.AreEqual(2, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                rootObservable,
                result
            );

            rootObservable.ExecuteAction(
                dict => rootObservable.Remove(4)
            );

            Assert.AreEqual(3, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                rootObservable,
                result
            );

            rootObservable.ExecuteAction(
                dict => rootObservable.Remove(40)
            );

            Assert.AreEqual(3, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(false, disposed);
            CollectionAssert.AreEquivalent(
                rootObservable,
                result
            );

            rootObservable.ExecuteAction(
                dict => rootObservable.Dispose()
            );

            Assert.AreEqual(3, callCount);
            Assert.IsNull(exception);
            Assert.AreEqual(true, disposed);
            CollectionAssert.AreEquivalent(
                rootObservable,
                result
            );
        }
    }
}