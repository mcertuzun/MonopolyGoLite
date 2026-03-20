using System;
using NUnit.Framework;

namespace MonopolyLite.Tests
{
    public class EventBusTests
    {
        struct TestEvent { public int Value; }
        struct OtherEvent { public string Name; }

        IEventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
        }

        [Test] public void Publish_InvokesSubscriber()
        { int received = 0; _bus.Subscribe<TestEvent>(e => received = e.Value); _bus.Publish(new TestEvent { Value = 42 }); Assert.AreEqual(42, received); }

        [Test] public void Publish_InvokesMultipleSubscribers()
        { int count = 0; _bus.Subscribe<TestEvent>(_ => count++); _bus.Subscribe<TestEvent>(_ => count++); _bus.Publish(new TestEvent { Value = 1 }); Assert.AreEqual(2, count); }

        [Test] public void Publish_DoesNotInvokeOtherEventSubscribers()
        { bool otherCalled = false; _bus.Subscribe<OtherEvent>(_ => otherCalled = true); _bus.Publish(new TestEvent { Value = 1 }); Assert.IsFalse(otherCalled); }

        [Test] public void Unsubscribe_RemovesHandler()
        { int count = 0; Action<TestEvent> handler = _ => count++; _bus.Subscribe(handler); _bus.Publish(new TestEvent()); Assert.AreEqual(1, count); _bus.Unsubscribe(handler); _bus.Publish(new TestEvent()); Assert.AreEqual(1, count); }

        [Test] public void Publish_NoSubscribers_DoesNotThrow()
        { Assert.DoesNotThrow(() => _bus.Publish(new TestEvent { Value = 99 })); }

        [Test] public void Clear_RemovesAllSubscribers()
        { int count = 0; _bus.Subscribe<TestEvent>(_ => count++); _bus.Subscribe<OtherEvent>(_ => count++); _bus.Clear(); _bus.Publish(new TestEvent()); _bus.Publish(new OtherEvent()); Assert.AreEqual(0, count); }

        [Test] public void Subscribe_SameHandlerTwice_CalledTwice()
        { int count = 0; Action<TestEvent> handler = _ => count++; _bus.Subscribe(handler); _bus.Subscribe(handler); _bus.Publish(new TestEvent()); Assert.AreEqual(2, count); }

        [Test] public void Publish_PassesCorrectData()
        { string received = null; _bus.Subscribe<OtherEvent>(e => received = e.Name); _bus.Publish(new OtherEvent { Name = "hello" }); Assert.AreEqual("hello", received); }
    }
}
