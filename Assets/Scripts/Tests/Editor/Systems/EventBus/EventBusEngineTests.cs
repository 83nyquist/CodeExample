using System;
using NUnit.Framework;
using Systems.EventBus.Components;

namespace Tests.Editor.Systems.EventBus
{
    public class IntEvent
    {
        public int Value { get; set; }
    }

    public class EventBusEngineTests
    {
        private EventBusEngine _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new EventBusEngine();
        }

        [Test]
        public void SubscribeThenPublish_ListenerReceivesEvent()
        {
            string received = null;
            _eventBus.Subscribe<string>(msg => received = msg);
            _eventBus.Publish("hello");
            Assert.AreEqual("hello", received);
        }

        [Test]
        public void MultipleSubscribers_AllReceiveEvent()
        {
            int callCount = 0;
            _eventBus.Subscribe<string>(_ => callCount++);
            _eventBus.Subscribe<string>(_ => callCount++);
            _eventBus.Subscribe<string>(_ => callCount++);
            _eventBus.Publish("test");
            Assert.AreEqual(3, callCount);
        }

        [Test]
        public void Subscribers_HaveIndependentState()
        {
            string received1 = null;
            string received2 = null;
            _eventBus.Subscribe<string>(v => received1 = v + "_first");
            _eventBus.Subscribe<string>(v => received2 = v + "_second");
            _eventBus.Publish("test");
            Assert.AreEqual("test_first", received1);
            Assert.AreEqual("test_second", received2);
        }

        [Test]
        public void UnsubscribeThenPublish_ListenerNotCalled()
        {
            int callCount = 0;
            Action<string> listener = _ => callCount++;
            _eventBus.Subscribe(listener);
            _eventBus.Unsubscribe(listener);
            _eventBus.Publish("hello");
            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void PublishWithNoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _eventBus.Publish("hello"));
        }

        [Test]
        public void UnsubscribeNonExistentListener_DoesNotThrow()
        {
            Action<string> listener = _ => { };
            Assert.DoesNotThrow(() => _eventBus.Unsubscribe(listener));
        }

        [Test]
        public void SubscribeSameListenerTwice_OnlyCalledOnce()
        {
            int callCount = 0;
            Action<string> listener = _ => callCount++;
            _eventBus.Subscribe(listener);
            _eventBus.Subscribe(listener);
            _eventBus.Publish("test");
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void DifferentEventTypes_AreIndependent()
        {
            string stringEvent = null;
            IntEvent intEvent = null;
            _eventBus.Subscribe<string>(s => stringEvent = s);
            _eventBus.Subscribe<IntEvent>(i => intEvent = i);
            _eventBus.Publish("hello");
            Assert.AreEqual("hello", stringEvent);
            Assert.IsNull(intEvent);
        }

        [Test]
        public void GetSubscriberCount_ReturnsCorrectCount()
        {
            Assert.AreEqual(0, _eventBus.GetSubscriberCount<string>());
            _eventBus.Subscribe<string>(_ => { });
            Assert.AreEqual(1, _eventBus.GetSubscriberCount<string>());
            _eventBus.Subscribe<string>(_ => { });
            Assert.AreEqual(2, _eventBus.GetSubscriberCount<string>());
            _eventBus.Subscribe<IntEvent>(_ => { });
            Assert.AreEqual(2, _eventBus.GetSubscriberCount<string>());
            Assert.AreEqual(1, _eventBus.GetSubscriberCount<IntEvent>());
        }

        [Test]
        public void NonGenericSubscribeAndUnsubscribe_WorkCorrectly()
        {
            int callCount = 0;
            Action<string> listener = _ => callCount++;
            _eventBus.Subscribe(typeof(string), listener);
            _eventBus.Publish("test");
            Assert.AreEqual(1, callCount);
            _eventBus.Unsubscribe(typeof(string), listener);
            _eventBus.Publish("test2");
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void PublishWhileListenerUnsubscribesItself_IsSafe()
        {
            int secondCallCount = 0;
            Action<string> selfRemover = null;
            selfRemover = msg =>
            {
                _eventBus.Unsubscribe(selfRemover);
            };
            _eventBus.Subscribe(selfRemover);
            _eventBus.Subscribe<string>(_ => secondCallCount++);
            _eventBus.Publish("test");
            Assert.AreEqual(1, secondCallCount);
        }

        [Test]
        public void UnsubscribeLastSubscriber_RemovesTypeEntry()
        {
            Action<string> listener = _ => { };
            _eventBus.Subscribe(listener);
            Assert.AreEqual(1, _eventBus.GetSubscriberCount<string>());
            _eventBus.Unsubscribe(listener);
            Assert.AreEqual(0, _eventBus.GetSubscriberCount<string>());
        }

        [Test]
        public void PublishGenericOverload_CallsCorrectListener()
        {
            IntEvent intEvent = null;
            string stringValue = null;
            _eventBus.Subscribe<IntEvent>(v => intEvent = v);
            _eventBus.Subscribe<string>(s => stringValue = s);
            IntEvent published = new IntEvent { Value = 42 };
            _eventBus.Publish(published);
            Assert.AreSame(published, intEvent);
            Assert.IsNull(stringValue);
        }
    }
}
