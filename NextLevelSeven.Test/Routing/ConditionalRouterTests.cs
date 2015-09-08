﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextLevelSeven.Core;
using NextLevelSeven.Routing;

namespace NextLevelSeven.Test.Routing
{
    [TestClass]
    public class ConditionalRouterTests
    {
        [TestMethod]
        public void ConditionalRouter_ReceivesMessages()
        {
            var queried = false;
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalRouter(m => { queried = true; return true; }, new NullRouter(true));
            Assert.IsFalse(queried, "Test initialized incorrectly.");
            message.RouteTo(router);
            Assert.IsTrue(queried, "Router was not queried.");
        }

        [TestMethod]
        public void ConditionalRouter_PassesMessagesThrough()
        {
            var subRouter = new NullRouter(true);
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalRouter(m => true, subRouter);
            message.RouteTo(router);
            Assert.IsTrue(subRouter.Checked, "Router did not reroute.");
        }

        [TestMethod]
        public void ConditionalRouter_PassesCorrectData()
        {
            var subRouter = new NullRouter(true);
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalRouter(m => true, subRouter);
            message.RouteTo(router);
            Assert.AreEqual(message.ToString(), subRouter.LastMessage.ToString(), "Message mismatch.");
        }

        [TestMethod]
        public void ConditionalRouter_ReturnsSuccessIfNoTarget()
        {
            var message = new Message(ExampleMessages.Standard);
            var router = new ConditionalRouter(m => true);
            Assert.IsTrue(message.RouteTo(router), "Router must return True when there is no target router.");
        }

    }
}
