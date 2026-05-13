using System;
using Core.Collections;
using NUnit.Framework;

namespace Tests.Editor.Core
{
    public class TestHeapItem : IHeapItem<TestHeapItem>
    {
        public int Priority { get; set; }
        public int HeapIndex { get; set; }

        public TestHeapItem(int priority)
        {
            Priority = priority;
        }

        public int CompareTo(TestHeapItem other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    public class HeapTests
    {
        [Test]
        public void NewHeap_HasZeroCount()
        {
            var heap = new Heap<TestHeapItem>(10);
            Assert.AreEqual(0, heap.Count);
        }

        [Test]
        public void AddOneItem_CountIsOne_RemoveFirstReturnsIt()
        {
            var heap = new Heap<TestHeapItem>(10);
            var item = new TestHeapItem(5);
            heap.Add(item);
            Assert.AreEqual(1, heap.Count);
            Assert.AreSame(item, heap.RemoveFirst());
            Assert.AreEqual(0, heap.Count);
        }

        [Test]
        public void RemoveFirst_ReturnsHighestPriorityItem()
        {
            var heap = new Heap<TestHeapItem>(10);
            heap.Add(new TestHeapItem(10));
            heap.Add(new TestHeapItem(5));
            heap.Add(new TestHeapItem(1));
            Assert.AreEqual(1, heap.RemoveFirst().Priority);
        }

        [Test]
        public void RemoveFirst_MaintainsMaxHeapProperty()
        {
            var heap = new Heap<TestHeapItem>(10);
            heap.Add(new TestHeapItem(3));
            heap.Add(new TestHeapItem(10));
            heap.Add(new TestHeapItem(1));
            heap.Add(new TestHeapItem(7));
            heap.Add(new TestHeapItem(5));

            int lastPriority = int.MinValue;
            while (heap.Count > 0)
            {
                int current = heap.RemoveFirst().Priority;
                Assert.GreaterOrEqual(current, lastPriority);
                lastPriority = current;
            }
        }

        [Test]
        public void Contains_ReturnsTrue_ForAddedItem()
        {
            var heap = new Heap<TestHeapItem>(10);
            var item = new TestHeapItem(42);
            heap.Add(item);
            Assert.IsTrue(heap.Contains(item));
        }

        [Test]
        public void Contains_ReturnsFalse_ForNonAddedItem()
        {
            var heap = new Heap<TestHeapItem>(10);
            var added = new TestHeapItem(1);
            var notAdded = new TestHeapItem(2);
            heap.Add(added);
            Assert.IsFalse(heap.Contains(notAdded));
        }

        [Test]
        public void Contains_UsesHeapIndex()
        {
            var heap = new Heap<TestHeapItem>(10);
            var item = new TestHeapItem(5);
            heap.Add(item);
            item.HeapIndex = 99;
            Assert.IsFalse(heap.Contains(item));
        }

        [Test]
        public void UpdateItem_IncreasesPriority_CorrectsPosition()
        {
            var heap = new Heap<TestHeapItem>(10);
            var low = new TestHeapItem(1);
            var mid = new TestHeapItem(50);
            var high = new TestHeapItem(100);
            heap.Add(low);
            heap.Add(mid);
            heap.Add(high);

            low.Priority = 200;
            heap.UpdateItem(low);

            Assert.AreSame(mid, heap.RemoveFirst());
        }

        [Test]
        public void UpdateItem_DecreasesPriority_CorrectsPosition()
        {
            var heap = new Heap<TestHeapItem>(10);
            var low = new TestHeapItem(1);
            var mid = new TestHeapItem(50);
            var high = new TestHeapItem(100);
            heap.Add(low);
            heap.Add(mid);
            heap.Add(high);

            high.Priority = 0;
            heap.UpdateItem(high);

            Assert.AreSame(high, heap.RemoveFirst());
        }

        [Test]
        public void RemoveFirst_Throws_WhenEmpty()
        {
            var heap = new Heap<TestHeapItem>(10);
            Assert.Throws<IndexOutOfRangeException>(() => heap.RemoveFirst());
        }

        [Test]
        public void Add_RespectsMaxSize()
        {
            var heap = new Heap<TestHeapItem>(2);
            heap.Add(new TestHeapItem(1));
            heap.Add(new TestHeapItem(2));
            Assert.Throws<IndexOutOfRangeException>(() => heap.Add(new TestHeapItem(3)));
        }

        [Test]
        public void ItemsWithEqualPriority_DoNotBreakHeap()
        {
            var heap = new Heap<TestHeapItem>(10);
            heap.Add(new TestHeapItem(5));
            heap.Add(new TestHeapItem(5));
            heap.Add(new TestHeapItem(5));
            heap.Add(new TestHeapItem(5));

            int count = 0;
            int lastPriority = int.MinValue;
            while (heap.Count > 0)
            {
                int current = heap.RemoveFirst().Priority;
                Assert.GreaterOrEqual(current, lastPriority);
                lastPriority = current;
                count++;
            }
            Assert.AreEqual(4, count);
        }

        [Test]
        public void LargeNumberOfItems_MaintainsHeapProperty()
        {
            var heap = new Heap<TestHeapItem>(1000);
            for (int i = 0; i < 1000; i++)
                heap.Add(new TestHeapItem(i));

            int lastPriority = int.MinValue;
            for (int i = 0; i < 1000; i++)
            {
                int current = heap.RemoveFirst().Priority;
                Assert.GreaterOrEqual(current, lastPriority);
                lastPriority = current;
            }
            Assert.AreEqual(0, heap.Count);
        }
    }
}
