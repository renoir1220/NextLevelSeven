﻿using System.Collections.Generic;
using System.Linq;
using NextLevelSeven.Core;
using NextLevelSeven.Core.Codec;
using NextLevelSeven.Core.Properties;
using NextLevelSeven.Diagnostics;
using NextLevelSeven.Utility;

namespace NextLevelSeven.Building.Elements
{
    /// <summary>
    ///     Represents an HL7 message as discrete parts, which can be quickly modified and exported.
    /// </summary>
    internal sealed class MessageBuilder : BuilderBase, IMessageBuilder
    {
        /// <summary>
        ///     Descendant segments.
        /// </summary>
        private readonly IndexedCache<int, SegmentBuilder> _cache;

        /// <summary>
        ///     Create a message builder with default MSH segment containing only encoding characters.
        /// </summary>
        public MessageBuilder()
        {
            _cache = new IndexedCache<int, SegmentBuilder>(CreateSegmentBuilder);
            ComponentDelimiter = '^';
            EscapeDelimiter = '\\';
            RepetitionDelimiter = '~';
            SubcomponentDelimiter = '&';
            FieldDelimiter = '|';
            Fields(1, "MSH", new string(FieldDelimiter, 1),
                new string(new[] {ComponentDelimiter, RepetitionDelimiter, EscapeDelimiter, SubcomponentDelimiter}));
        }

        /// <summary>
        ///     Create a message builder initialized with the specified message content.
        /// </summary>
        /// <param name="baseMessage">Content to initialize with.</param>
        public MessageBuilder(string baseMessage)
        {
            _cache = new IndexedCache<int, SegmentBuilder>(CreateSegmentBuilder);
            Message(baseMessage);
        }

        /// <summary>
        ///     Create a message builder initialized with the copied content of the specified element.
        /// </summary>
        /// <param name="message">Message or other element to copy content from.</param>
        public MessageBuilder(IElement message) : this(message.Value)
        {
        }

        /// <summary>
        ///     Get a descendant segment builder.
        /// </summary>
        /// <param name="index">Index within the message to get the builder from.</param>
        /// <returns>Segment builder for the specified index.</returns>
        public new ISegmentBuilder this[int index]
        {
            get { return _cache[index]; }
        }

        /// <summary>
        ///     Create a segment builder object.
        /// </summary>
        /// <param name="index">Index to create the object for.</param>
        /// <returns>Segment builder object.</returns>
        private SegmentBuilder CreateSegmentBuilder(int index)
        {
            return new SegmentBuilder(this, index);
        }

        /// <summary>
        ///     Get the number of segments in the message.
        /// </summary>
        public override int ValueCount
        {
            get { return _cache.Max(kv => kv.Key); }
        }

        /// <summary>
        ///     Get or set segment content within this message.
        /// </summary>
        public override IEnumerable<string> Values
        {
            get
            {
                return new WrapperEnumerable<string>(index => _cache[index].Value,
                    (index, data) => Segment(index, data),
                    () => ValueCount,
                    1);
            }
            set { Segments(value.ToArray()); }
        }

        /// <summary>
        ///     Get or set the message string.
        /// </summary>
        public override string Value
        {
            get
            {
                if (_cache.Count == 0)
                {
                    return null;
                }

                var result = string.Join("\xD",
                    _cache.OrderBy(i => i.Key).Select(i => i.Value.Value));
                return result;
            }
            set { Message(value); }
        }

        /// <summary>
        ///     Set a component's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Component(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            string value)
        {
            _cache[segmentIndex].Component(fieldIndex, repetition, componentIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all component values within a field repetition.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Components(int segmentIndex, int fieldIndex, int repetition, params string[] components)
        {
            _cache[segmentIndex].Components(fieldIndex, repetition, components);
            return this;
        }

        /// <summary>
        ///     Set a sequence of components within a field repetition, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="startIndex">Component index to begin replacing at.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Components(int segmentIndex, int fieldIndex, int repetition, int startIndex,
            params string[] components)
        {
            _cache[segmentIndex].Components(fieldIndex, repetition, startIndex, components);
            return this;
        }

        /// <summary>
        ///     Set a field's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Field(int segmentIndex, int fieldIndex, string value)
        {
            _cache[segmentIndex].Field(fieldIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all field values within a segment.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fields">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Fields(int segmentIndex, params string[] fields)
        {
            _cache[segmentIndex].Fields(fields);
            return this;
        }

        /// <summary>
        ///     Set a sequence of fields within a segment, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="startIndex">Field index to begin replacing at.</param>
        /// <param name="fields">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Fields(int segmentIndex, int startIndex, params string[] fields)
        {
            _cache[segmentIndex].Fields(startIndex, fields);
            return this;
        }

        /// <summary>
        ///     Set a field repetition's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder FieldRepetition(int segmentIndex, int fieldIndex, int repetition, string value)
        {
            _cache[segmentIndex].FieldRepetition(fieldIndex, repetition, value);
            return this;
        }

        /// <summary>
        ///     Replace all field repetitions within a field.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetitions">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder FieldRepetitions(int segmentIndex, int fieldIndex, params string[] repetitions)
        {
            _cache[segmentIndex].FieldRepetitions(fieldIndex, repetitions);
            return this;
        }

        /// <summary>
        ///     Set a sequence of field repetitions within a field, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="startIndex">Field repetition index to begin replacing at.</param>
        /// <param name="repetitions">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder FieldRepetitions(int segmentIndex, int fieldIndex, int startIndex,
            params string[] repetitions)
        {
            _cache[segmentIndex].FieldRepetitions(fieldIndex, startIndex, repetitions);
            return this;
        }

        /// <summary>
        ///     Set this message's content.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Message(string value)
        {
            if (value == null)
            {
                throw new BuilderException(ErrorCode.MessageDataMustNotBeNull);
            }
            
            var length = value.Length;
            if (length < 8)
            {
                throw new BuilderException(ErrorCode.MessageDataIsTooShort);
            }

            if (!value.StartsWith("MSH"))
            {
                throw new BuilderException(ErrorCode.MessageDataMustStartWithMsh);
            }

            ComponentDelimiter = (length >= 5) ? value[4] : '^';
            EscapeDelimiter = (length >= 6) ? value[5] : '\\';
            FieldDelimiter = (length >= 3) ? value[3] : '|';
            RepetitionDelimiter = (length >= 7) ? value[6] : '~';
            SubcomponentDelimiter = (length >= 8) ? value[7] : '&';

            _cache.Clear();
            value = value.Replace("\r\n", "\xD");
            var index = 1;

            foreach (var segment in value.Split('\xD'))
            {
                Segment(index++, segment);
            }

            return this;
        }

        /// <summary>
        ///     Set a segment's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Segment(int segmentIndex, string value)
        {
            _cache[segmentIndex].Segment(value);
            return this;
        }

        /// <summary>
        ///     Replace all segments within this message.
        /// </summary>
        /// <param name="segments">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Segments(params string[] segments)
        {
            Message(string.Join("\xD", segments));
            return this;
        }

        /// <summary>
        ///     Set a sequence of segments within this message, beginning at the specified start index.
        /// </summary>
        /// <param name="startIndex">Segment index to begin replacing at.</param>
        /// <param name="segments">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Segments(int startIndex, params string[] segments)
        {
            var index = startIndex;
            foreach (var segment in segments)
            {
                Segment(index++, segment);
            }
            return this;
        }

        /// <summary>
        ///     Set a subcomponent's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponentIndex">Subcomponent index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Subcomponent(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            int subcomponentIndex,
            string value)
        {
            _cache[segmentIndex].Subcomponent(fieldIndex, repetition, componentIndex, subcomponentIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all subcomponents within a component.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponents">Subcomponent index.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Subcomponents(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            params string[] subcomponents)
        {
            _cache[segmentIndex].Subcomponents(fieldIndex, repetition, componentIndex, subcomponents);
            return this;
        }

        /// <summary>
        ///     Set a sequence of subcomponents within a component, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="startIndex">Subcomponent index to begin replacing at.</param>
        /// <param name="subcomponents">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Subcomponents(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            int startIndex, params string[] subcomponents)
        {
            _cache[segmentIndex].Subcomponents(fieldIndex, repetition, componentIndex, startIndex, subcomponents);
            return this;
        }

        /// <summary>
        ///     Get the values at the specific location in the message.
        /// </summary>
        /// <param name="segment">Segment index.</param>
        /// <param name="field">Field index.</param>
        /// <param name="repetition">Repetition index.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>Value at the specified location. Returns null if not found.</returns>
        public IEnumerable<string> GetValues(int segment = -1, int field = -1, int repetition = -1, int component = -1,
            int subcomponent = -1)
        {
            if (segment < 0)
            {
                return Values;
            }
            if (field < 0)
            {
                return _cache[segment].Values;
            }
            if (repetition < 0)
            {
                return _cache[segment][field].Values;
            }
            if (component < 0)
            {
                return _cache[segment][field][repetition].Values;
            }
            return (subcomponent < 0)
                ? _cache[segment][field][repetition][component].Values
                : _cache[segment][field][repetition][component][subcomponent].Value.Yield();
        }

        /// <summary>
        ///     Get the value at the specific location in the message.
        /// </summary>
        /// <param name="segment">Segment index.</param>
        /// <param name="field">Field index.</param>
        /// <param name="repetition">Repetition index.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>Value at the specified location. Returns null if not found.</returns>
        public string GetValue(int segment = -1, int field = -1, int repetition = -1, int component = -1,
            int subcomponent = -1)
        {
            if (segment < 0)
            {
                return Value;
            }
            if (field < 0)
            {
                return _cache[segment].Value;
            }
            if (repetition < 0)
            {
                return _cache[segment][field].Value;
            }
            if (component < 0)
            {
                return _cache[segment][field][repetition].Value;
            }
            return (subcomponent < 0)
                ? _cache[segment][field][repetition][component].Value
                : _cache[segment][field][repetition][component][subcomponent].Value;
        }

        public override IElement Clone()
        {
            return new MessageBuilder(Value);
        }

        IMessage IMessage.Clone()
        {
            return new MessageBuilder(Value);
        }

        public override IEncodedTypeConverter As
        {
            get { return new BuilderCodec(this); }
        }

        public override char Delimiter
        {
            get { return '\xD'; }
        }

        public IMessageDetails Details
        {
            get { return new MessageDetails(this); }
        }

        protected override IElement GetGenericElement(int index)
        {
            return _cache[index];
        }
    }
}