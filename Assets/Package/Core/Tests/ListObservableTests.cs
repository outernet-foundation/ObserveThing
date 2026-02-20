using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ObserveThing.Tests
{
    public class ListObservableTests
    {
        [Test]
        public void TestSelect()
        {
            var result = new List<string>();
            var list = new ListObservable<int>();
            var select = list.ObservableSelect(x => x.ToString()).Subscribe(
                onAdd: (index, value) => result.Insert(index, value),
                onRemove: (index, value) => result.RemoveAt(index)
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
        }

        [Test]
        public void TestShallowCopy()
        {
            var list = new ListObservable<ValueObservable<int>>();
            var result = new List<string>();
            var stream = list.ObservableSelect(x => x.ObservableSelect(x => x.ToString())).Subscribe(
                onAdd: (index, x) => result.Insert(index, x),
                onRemove: (index, x) => result.RemoveAt(index)
            );

            var element1 = new ValueObservable<int>(2);
            var element2 = new ValueObservable<int>(3);
            var element3 = new ValueObservable<int>(45);
            var element4 = new ValueObservable<int>(11);

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
        }
    }
}