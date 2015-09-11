﻿using NextLevelSeven.Core;

namespace NextLevelSeven.Building
{
    /// <summary>
    ///     Interface for a field repetition element builder.
    /// </summary>
    public interface IRepetitionBuilder : IBuilder, IRepetition
    {
        /// <summary>
        ///     Get a descendant component builder.
        /// </summary>
        /// <param name="index">Index within the field repetition to get the builder from.</param>
        /// <returns>Component builder for the specified index.</returns>
        new IComponentBuilder this[int index] { get; }

        /// <summary>
        ///     Set a component's content.
        /// </summary>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder Component(int componentIndex, string value);

        /// <summary>
        ///     Replace all component values within this field repetition.
        /// </summary>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder Components(params string[] components);

        /// <summary>
        ///     Set a sequence of components within this field repetition, beginning at the specified start index.
        /// </summary>
        /// <param name="startIndex">Component index to begin replacing at.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder Components(int startIndex, params string[] components);

        /// <summary>
        ///     Set a field repetition's value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder FieldRepetition(string value);

        /// <summary>
        ///     Set a subcomponent's content.
        /// </summary>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponentIndex">Subcomponent index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder Subcomponent(int componentIndex, int subcomponentIndex, string value);

        /// <summary>
        ///     Replace all subcomponents within a component.
        /// </summary>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponents">Subcomponent index.</param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder Subcomponents(int componentIndex, params string[] subcomponents);

        /// <summary>
        ///     Set a sequence of subcomponents within a component, beginning at the specified start index.
        /// </summary>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="startIndex">Subcomponent index to begin replacing at.</param>
        /// <param name="subcomponents">Values to replace with.</param>
        /// <returns>This IRepetitionBuilder, for chaining purposes.</returns>
        IRepetitionBuilder Subcomponents(int componentIndex, int startIndex, params string[] subcomponents);
    }
}