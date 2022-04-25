using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
    /// <summary>
    /// Represents an operation of a navigator.
    /// A Navigator operation consists of one or multiple requests.
    /// </summary>
    public class NavigationOperation
    {
        public NavigationOperation(string name, long sequenceId)
        {
            Name = name;
            SequenceId = sequenceId;
        }

        /// <summary>
        /// Gets the name of the operation.
        /// e.g. "NavigateBack", "Clear", etc.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the unique identifier of this operation.
        /// This identifier also indicates the sequence (or index) of the operation from the navigator's perspective.
        /// </summary>
        public long SequenceId { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name} SequenceId: {SequenceId}";
        }
    }
}