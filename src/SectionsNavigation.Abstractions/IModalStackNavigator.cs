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
		/// </summary>
		/// <remarks>
		/// The priority defines the precedence when there are multiple modals.
		/// The modal with the highest priority is the one returned by <see cref="SectionsNavigatorState.ActiveModal"/>.
		/// </remarks>
		int Priority { get; }

		/// <summary>
		/// Gets the name of this navigator stack.
		/// </summary>
		/// <remarks>
		/// This is used for mapping purposes.
		/// </remarks>
		string Name { get; }

		/// <summary>
		/// Gets the default transition info for the future close modal operation.
		/// </summary>
		SectionsTransitionInfo CloseModalTransitionInfo { get; }
	}
}
