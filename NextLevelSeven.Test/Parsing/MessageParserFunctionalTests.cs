﻿using System;
using System.Diagnostics;
using System.Linq;
using NextLevelSeven.Core;
using NextLevelSeven.Parsing;
using NextLevelSeven.Test.Testing;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace NextLevelSeven.Test.Parsing
{
    [TestFixture]
    public class MessageParserFunctionalTests : ParsingTestFixture
    {
        [Test]
        public void Message_CanProcessMessageWithShortEncoding()
        {
            var field31 = MockFactory.String();
            var field32 = MockFactory.String();
            var field41 = MockFactory.String();
            var field42 = MockFactory.String();
            var message = Message.Parse(string.Format("MSH|^|{0}^{1}|{2}^{3}", field31, field32, field41, field42));
            Assert.AreEqual(field31, message[1][3][1][1].Value);
            Assert.AreEqual(field32, message[1][3][1][2].Value);
            Assert.AreEqual(field41, message[1][4][1][1].Value);
            Assert.AreEqual(field42, message[1][4][1][2].Value);
        }

        [Test]
        public void Message_IsEquivalentWhenReferencesMatch()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var generic = (object)message;
            Assert.IsTrue(message.Equals(generic));
        }

        [Test]
        public void Message_IsNotEquivalentWhenNull()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.IsFalse(message.Equals(null));
        }

        [Test]
        public void Message_ToStringGetsValue()
        {
            var content = MockFactory.Message();
            var message = Message.Parse(content);
            Assert.AreEqual(content, message.ToString());
        }

        [Test]
        public void Message_ToStringGetsEmptyWhenFieldIsNull()
        {
            var content = MockFactory.Message();
            var field = Message.Parse(content)[1][3];
            field.Value = null;
            Assert.AreEqual(string.Empty, field.ToString());
        }

        [Test]
        public void Message_Validates()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            Assert.IsTrue(message.Validate());
        }

        [Test]
        public void Message_DeletesOutOfRangeIndex()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            message.Delete(2);
        }

        [Test]
        public void Message_InsertsElement()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            var segment = Message.Parse(MockFactory.Message())[2];
            message.Insert(2, segment);
            Assert.AreEqual(message[2].Value, segment.Value);
        }

        [Test]
        public void Message_InsertsString()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            var segment = MockFactory.Segment();
            message.Insert(2, segment);        
            Assert.AreEqual(message[2].Value, segment);
        }

        [Test]
        public void Message_ThrowsOnInsertingNegativeIndex()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            AssertAction.Throws<ElementException>(() => message.Insert(-2, MockFactory.String()));
        }

        [Test]
        public void Message_ThrowsOnDeletingNegativeIndex()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            AssertAction.Throws<ElementException>(() => message.Delete(-2));
        }

        [Test]
        public void Message_DeletesZeroLengthItem()
        {
            var message = Message.Parse(ExampleMessages.Minimum + "\r\r");
            message.Delete(2);
        }

        [Test]
        public void Message_ConvertsToBuilder()
        {
            var builder = Message.Parse(ExampleMessages.Standard);
            var beforeMessageString = builder.Value;
            var message = builder.ToBuilder();
            Assert.AreEqual(beforeMessageString, message.Value, "Conversion from parser to builder failed.");
        }

        [Test]
        public void Message_ConvertsFromBuilder()
        {
            var message = Message.Build(ExampleMessages.Standard);
            var beforeBuilderString = message.Value;
            var afterBuilder = Message.Parse(message);
            Assert.AreEqual(beforeBuilderString, afterBuilder.Value, "Conversion from builder to parser failed.");
        }

        [Test]
        public void Message_ThrowsWithIncorrectFirstSegment()
        {
            AssertAction.Throws<ElementException>(() => Message.Parse(MockFactory.String()));
        }

        [Test]
        public void Message_CanGetValues()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            Assert.AreEqual(ExampleMessages.Minimum, message.Values.First());
        }

        [Test]
        public void Message_CanSetValues()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            message.Values = new[] {ExampleMessages.Minimum, ExampleMessages.Minimum, ExampleMessages.Minimum};
            Assert.AreEqual(3, message.ValueCount);
        }

        [Test]
        public void Message_CanIndexPastEnd()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            Assert.IsNull(message[5].Value);
        }

        [Test]
        public void Message_CanMoveSegments()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var newMessage = message.Clone();
            newMessage[2].Move(3);
            Assert.AreEqual(message[2].Value, newMessage[3].Value);
        }

        [Test]
        public void Message_Throws_WhenIndexedBelowOne()
        {
            var element = Message.Parse(ExampleMessages.Standard);
            string value = null;
            AssertAction.Throws<ParserException>(() => { value = element[0].Value; });
            Assert.IsNull(value);
        }

        [Test]
        public void Message_CanNotSetMsh1()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            AssertAction.Throws<ElementException>(() => message[1][1].Value = ":");
        }

        [Test]
        public void Message_CanNotSetMsh2()
        {
            var message = Message.Parse(ExampleMessages.Minimum);
            AssertAction.Throws<ElementException>(() => message[1][2].Value = "!@#$");
        }

        [Test]
        public void Message_ConvertsMshCorrectly()
        {
            var message = Message.Parse(ExampleMessages.MshOnly);
            Assert.AreEqual(ExampleMessages.MshOnly, message.Value, "MSH conversion back to string did not match.");
        }

        [Test]
        public void Message_ReturnsBasicMessage()
        {
            var message = Message.Parse();
            Assert.AreEqual(1, message.ValueCount, @"Default message should not contain multiple segments.");
            Assert.AreEqual("MSH", message[1].Type, @"Default message should create an MSH segment.");
            Assert.AreEqual(@"^~\&", message[1][2].Value,
                @"Default message should use standard HL7 encoding characters.");
            Assert.AreEqual("|", message[1][1].Value,
                @"Default message should use standard HL7 field delimiter character.");
        }

        [Test]
        public void Message_ThrowsOnNullData()
        {
            AssertAction.Throws<ElementException>(() => Message.Parse((string) null));
        }

        [Test]
        public void Message_ThrowsOnEmptyData()
        {
            AssertAction.Throws<ElementException>(() => Message.Parse(string.Empty));
        }

        [Test]
        public void Message_ThrowsOnShortData()
        {
            AssertAction.Throws<ElementException>(() => Message.Parse("MSH|123"));
        }

        [Test]
        public void Message_CanRetrieveMessageTypeAndTriggerEvent()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.AreEqual("ADT", message.Details.Type, "Message type is incorrect.");
            Assert.AreEqual("A17", message.Details.TriggerEvent, "Message trigger event is incorrect.");
        }

        [Test]
        public void Message_CanParseMessageDate()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.IsTrue(message.Details.Time.HasValue, "Parsed message date is incorrect.");
            Assert.AreEqual(new DateTime(2013, 05, 28), message.Details.Time.Value.Date);
        }

        [Test]
        public void Message_CanParseMessageDateTime()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.IsTrue(message.Details.Time.HasValue, "Parsed message date/time is incorrect.");
            Assert.AreEqual(new DateTime(2013, 05, 28, 07, 38, 29), message.Details.Time.Value.DateTime);
        }

        [Test]
        public void Message_CanRetrieveMessageVersion()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.AreEqual("2.3", message.Details.Version, "Message version is incorrect.");
        }

        [Test]
        public void Message_CanRetrievePatientId()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var pid = message.Segments.OfType("PID").First();
            Assert.AreEqual("Colon", pid[5][1][1].Value, "Patient name is incorrect.");
        }

        [Test]
        public void Message_CanRetrieveMultipleSegments()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.AreEqual(3, message.Segments.OfType("OBX").Count(), "Incorrect number of segments were found.");
        }

        [Test]
        public void Message_CanRetrieveRepetitions()
        {
            var message = Message.Parse(ExampleMessages.RepeatingName);
            var pid = message.Segments.OfType("PID").First();
            Assert.AreEqual("Lincoln^Abe", pid[5][1].Value, "Retrieving first repetition returned incorrect data.");
            Assert.AreEqual("Bro", pid[5][2].Value, "Retrieving second repetition returned incorrect data.");
            Assert.AreEqual("Dude", pid[5][3].Value, "Retrieving third repetition returned incorrect data.");
        }

        [Test]
        public void Message_RetrievalMethodsAreIdentical()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            Assert.AreEqual(message.GetValue(1), message[1].Value,
                "Retrieval methods differ at the segment level.");
            Assert.AreEqual(message.GetValue(1, 3), message[1][3].Value,
                "Retrieval methods differ at the field level.");
            Assert.AreEqual(message.GetValue(1, 3, 1), message[1][3][1].Value,
                "Retrieval methods differ at the repetition level.");
            Assert.AreEqual(message.GetValue(1, 3, 1, 1), message[1][3][1][1].Value,
                "Retrieval methods differ at the component level.");
            Assert.AreEqual(message.GetValue(1, 3, 1, 1, 1), message[1][3][1][1][1].Value,
                "Retrieval methods differ at the component level.");
        }

        [Test]
        public void Message_MultiRetrievalMethodsAreIdentical()
        {
            var message = Message.Parse(ExampleMessages.Variety);
            AssertArray.AreEqual(message.GetValues(1).ToArray(), message[1].Values.ToArray());
            AssertArray.AreEqual(message.GetValues(1, 3).ToArray(), message[1][3].Values.ToArray());
            AssertArray.AreEqual(message.GetValues(1, 3, 1).ToArray(), message[1][3][1].Values.ToArray());
            AssertArray.AreEqual(message.GetValues(1, 3, 1, 1).ToArray(), message[1][3][1][1].Values.ToArray());
            AssertArray.AreEqual(message.GetValues(1, 3, 1, 1, 1).ToArray(), message[1][3][1][1][1].Values.ToArray());
        }

        [Test]
        public void Message_HasUniqueDescendantKeys()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var keys = message.Segments.Select(s => s.Key).ToList();
            var distinctKeys = keys.Distinct();

            foreach (var key in keys)
            {
                Debug.WriteLine(key);
            }
            Assert.AreEqual(distinctKeys.Count(), message.Segments.Count(), "Segments don't have entirely unique keys.");
        }

        [Test]
        public void Message_CanBeCloned()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var clone = message.Clone();
            Assert.AreNotSame(message, clone, "Cloned message is the same referenced object.");
            Assert.AreEqual(message.Value, clone.Value, "Cloned message has different contents.");
        }

        [Test]
        public void Message_WithOnlyOneSegment_WillClaimToHaveSignificantDescendants()
        {
            var message = Message.Parse();
            Assert.IsTrue(message.HasSignificantDescendants(),
                @"Message should claim to have significant descendants if any segments do.");
        }

        [Test]
        public void Message_UsesReasonableMemory_WhenParsingLargeMessages()
        {
            var before = GC.GetTotalMemory(true);
            var message = Message.Parse();
            message[1000000][1000000].Value = MockFactory.String();
            var messageString = message.Value;
            var usage = GC.GetTotalMemory(false) - before;
            var overhead = usage - (messageString.Length << 1);
            var usePerCharacter = overhead/(messageString.Length << 1);
            Assert.IsTrue(usePerCharacter < 20);
        }

        [Test]
        public void Message_CanMapSegments()
        {
            var id = MockFactory.String();
            IMessage tree = Message.Parse(string.Format("MSH|^~\\&|{0}", id));
            Assert.AreEqual(string.Format("MSH|^~\\&|{0}", id), tree.GetValue(1));
        }

        [Test]
        public void Message_CanMapFields()
        {
            var id = MockFactory.String();
            IMessage tree = Message.Parse(string.Format("MSH|^~\\&|{0}", id));
            Assert.AreEqual(id, tree.GetValue(1, 3));
        }

        [Test]
        public void Message_CanMapRepetitions()
        {
            var id1 = MockFactory.String();
            var id2 = MockFactory.String();
            IMessage tree = Message.Parse(string.Format("MSH|^~\\&|{0}~{1}", id1, id2));
            Assert.AreEqual(id1, tree.GetValue(1, 3, 1));
            Assert.AreEqual(id2, tree.GetValue(1, 3, 2));
        }

        [Test]
        public void Message_CanMapComponents()
        {
            var id1 = MockFactory.String();
            var id2 = MockFactory.String();
            IMessage tree = Message.Parse(string.Format("MSH|^~\\&|{0}^{1}", id1, id2));
            Assert.AreEqual(id1, tree.GetValue(1, 3, 1, 1));
            Assert.AreEqual(id2, tree.GetValue(1, 3, 1, 2));
        }

        [Test]
        public void Message_CanMapSubcomponents()
        {
            var id1 = MockFactory.String();
            var id2 = MockFactory.String();
            IMessage tree = Message.Parse(string.Format("MSH|^~\\&|{0}&{1}", id1, id2));
            Assert.AreEqual(id1, tree.GetValue(1, 3, 1, 1, 1));
            Assert.AreEqual(id2, tree.GetValue(1, 3, 1, 1, 2));
        }

        [Test]
        public void Message_CanAddDescendantsAtEnd()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var nextIndex = message.NextIndex;
            var count = message.ValueCount;
            var id = MockFactory.String();
            message[nextIndex].Value = id;
            Assert.AreEqual(count + 1, message.ValueCount,
                @"Number of elements after appending at the end of a message is incorrect.");
        }

        [Test]
        public void Message_CanGetSegmentsByIndexer()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var segment = message[1];
            Assert.AreEqual(@"MSH|^~\&|SENDER|DEV|RECEIVER|SYSTEM|20130528073829||ADT^A17|14150278|P|2.3|",
                segment.Value);
        }

        [Test]
        public void Message_CanDeleteSegment()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var segment1 = message[1].Value;
            var segment3 = message[3].Value;
            var segment4 = message[4].Value;
            ElementExtensions.Delete(message, 2);
            Assert.AreEqual(segment1, message[1].Value, @"Expected message[1] to remain the same after delete.");
            Assert.AreEqual(segment3, message[2].Value, @"Expected message[3] to become message[2].");
            Assert.AreEqual(segment4, message[3].Value, @"Expected message[4] to become message[3].");
        }

        [Test]
        public void Message_ValuesReturnsProperlySplitData()
        {
            var message = Message.Parse(ExampleMessages.Standard);
            var segmentStrings = message.Value.Split('\xD');
            var segments = message.Values.ToList();

            Assert.AreEqual(segmentStrings.Length, segments.Count,
                @"Splitting main value and calling Values returns different element counts.");

            for (var i = 0; i < segments.Count; i++)
            {
                Assert.AreEqual(segments[i], segmentStrings[i], @"Values are not equal.");
            }
        }
    }
}