using System;
using System.Collections.Generic;
using System.Text;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This represents a modal navigation stack in the context of a sections navigator.
	/// It's a modal of an <see cref="ISectionsNavigator"/>.
	/// </summary>
	public interface IModalStackNavigator : IStackNavigator, IDisposable
	{
		/// <summary>
		/// Gets the priority of the modal.
		/// The priority defines the precedence when there are multiple modals.
		/// The modal with the highest priority is the one returned by <see cref="SectionsNavigatorState.ActiveModal"/>.
		/// </summary>
		int Priority { get; }

		/// <summary>
		/// Gets the name of this navigator stack.
		/// This is used for mapping purposes.
		/// </summary>
		string Name { get; }
	}
}
