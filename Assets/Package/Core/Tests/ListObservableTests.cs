using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ObserveThing.Tests
{
    public class ListObservableTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.DefaultExceptionHandler = UnityEngine.Debug.LogException;
        }

        [Test]
        public void TestSelect()
        {
            var result = new List<string>();
            var list = new ObservableList<int>();
            bool disposed = false;
            bool receivedCall = false;
            var select = list.ObservableSelect(x => x.ToString()).Subscribe(
                onAdd: (index, value) =>
                {
                    result.Insert(index, value);
                    receivedCall = true;
                },
                onRemove: (index, value) =>
                {
                    result.RemoveAt(index);
                    receivedCall = true;
                },
                onDispose: () => disposed = true
            );

            list.Add(1);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);
            list.Add(45);

            Assert.AreEqual(
                Enumerable.Select(list, x => x.ToString()),
                result
            );

            list.Remove(3);
            list.Remove(3);
            list.Add(1);
            list.Add(45);
            list.Remove(45);

            Assert.AreEqual(
                Enumerable.Select(list, x => x.ToString()),
                result
            );

            list.Clear();

            Assert.AreEqual(
                Enumerable.Select(list, x => x.ToString()),
                result
            );

            receivedCall = false;
            select.Dispose();
            Assert.IsTrue(disposed);
            list.Add(100);
            Assert.IsFalse(receivedCall);
        }

        [Test]
        public void TestShallowCopy()
        {
            var list = new ObservableList<ObservableValue<int>>();
            var result = new List<string>();
            bool disposed = false;
            bool receivedCall = false;
            var stream = list.ObservableSelect(x => x.ObservableSelect(x => x.ToString())).Subscribe(
                onAdd: (index, x) =>
                {
                    result.Insert(index, x);
                    receivedCall = true;
                },
                onRemove: (index, x) =>
                {
                    result.RemoveAt(index);
                    receivedCall = true;
                },
                onDispose: () => disposed = true
            );

            var element1 = new ObservableValue<int>(2);
            var element2 = new ObservableValue<int>(3);
            var element3 = new ObservableValue<int>(45);
            var element4 = new ObservableValue<int>(11);

            list.Add(element1);
            list.Add(element2);
            list.Add(element3);
            list.Add(element4);

            Assert.AreEqual(
                Enumerable.Select(list, x => x.value.ToString()),
                result
            );

            element1.value = 3;
            element1.value = 100;
            element1.value = 22;
            element1.value = 6;

            Assert.AreEqual(
                Enumerable.Select(list, x => x.value.ToString()),
                result
            );

            element2.value = 3;
            element3.value = 3;
            element4.value = 3;

            Assert.AreEqual(
                Enumerable.Select(list, x => x.value.ToString()),
                result
            );

            list.Remove(element2);
            list.Add(element3);

            element2.value = 5;
            element3.value = 100;
            element3.value = 50;

            Assert.AreEqual(
                Enumerable.Select(list, x => x.value.ToString()),
                result
            );

            receivedCall = false;
            stream.Dispose();
            Assert.IsTrue(disposed);
            list.Add(new ObservableValue<int>(150));
            Assert.IsFalse(receivedCall);
        }
    }
}