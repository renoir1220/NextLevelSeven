﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextLevelSeven.Core;
using NextLevelSeven.Routing;

namespace NextLevelSeven.Test.Routing
{
    [TestClass]
    public class ConditionalMethodRouterTests
    {
        [TestMethod]
        public void ConditionalMethodRouter_ReceivesMessages()
        {
            var queried = false;
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalMethodRouter(m => { queried = true; return true; }, m => {});
            Assert.IsFalse(queried, "Test initialized incorrectly.");
            message.RouteTo(router);
            Assert.IsTrue(queried, "Router was not queried.");
        }

        [TestMethod]
        public void ConditionalMethodRouter_PassesMessagesThrough()
        {
            var routed = false;
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalMethodRouter(m => true, m => routed = true);
            Assert.IsFalse(routed, "Test initialized incorrectly.");
            message.RouteTo(router);
            Assert.IsTrue(routed, "Router did not reroute.");
        }

        [TestMethod]
        public void ConditionalMethodRouter_PassesCorrectData()
        {
            IMessage routedData = null;
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalMethodRouter(m => true, m => routedData = m);
            Assert.IsNull(routedData, "Test initialized incorrectly.");
            message.RouteTo(router);
            Assert.AreEqual(message.ToString(), routedData.ToString());
        }

        [TestMethod]
        public void ConditionalMethodRouter_ReturnsSuccessIfNoTarget()
        {
            var message = new Message(ExampleMessages.Standard);
            var listener = new ConditionalMethodRouter(m => true, null);
            Assert.IsTrue(message.RouteTo(listener), "Listener must return True when there is no target router.");
        }
    }
}
