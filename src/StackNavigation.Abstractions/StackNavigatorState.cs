using System;
using System.Collections.Generic;
using System.Text;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// Represents the state of a <see cref="IStackNavigator"/>.
	/// This object is immutable.
	/// </summary>
	public class StackNavigatorState
	{
		/// <summary>
		/// Initializes a new instance of <see cref="StackNavigatorState"/>.
		/// </summary>
		/// <param name="stack">The navigation stack.</param>
		/// <param name="lastRequestState">The state of the last request.</param>
		/// <param name="lastRequest">The last request.</param>
		public StackNavigatorState(IReadOnlyList<NavigationStackEntry> stack, NavigatorRequestState lastRequestState, StackNavigatorRequest lastRequest)
		{
			Stack = stack;
			LastRequestState = lastRequestState;
			LastRequest = lastRequest;
		}

		/// <summary>
		/// Gets the list of navigation entries.
		/// This represents the navigation stack.
		/// The first item is the oldest entry and the last item is the active entry.
		/// </summary>
		public IReadOnlyList<NavigationStackEntry> Stack { get; }

		/// <summary>
		/// Gets the state of the most recent <see cref="StackNavigatorRequest"/> of the <see cref="IStackNavigator"/>.
		/// </summary>
		public NavigatorRequestState LastRequestState { get; }

		/// <summary>
		/// Gets the most recent <see cref="StackNavigatorRequest"/> of the <see cref="IStackNavigator"/>.
		/// </summary>
		public StackNavigatorRequest LastRequest { get; }
	}
}
