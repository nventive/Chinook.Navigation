using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.StackNavigation
{
	/// <summary>
	/// This represents a navigation controller for a single navigation stack.
	/// </summary>
	public interface IStackNavigator
	{
		/// <summary>
		/// Navigates forward to a new page.
		/// </summary>
		/// <remarks>
		/// When calling this method more than once in parallel, only the first invocation executes. Others are discarded.
		/// </remarks>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the parameters navigation operation.</param>
		/// <returns>The ViewModel instance of the active page after the navigation operation.</returns>
		Task<INavigableViewModel> Navigate(CancellationToken ct, StackNavigatorRequest request);

		/// <summary>
		/// Navigates back to the previous page.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <returns>The ViewModel instance of the active page after the back operation.</returns>
		Task<INavigableViewModel> NavigateBack(CancellationToken ct);

		/// <summary>
		/// Clears the navigation stack.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		Task Clear(CancellationToken ct);

		/// <summary>
		/// Removes specific entries from the stack.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="indexes">The indexes at which the entries must be removed.</param>
		Task RemoveEntries(CancellationToken ct, IEnumerable<int> indexes);

		/// <summary>
		/// Represents the current state of this object.
		/// You can observe its changes with the <see cref="StateChanged"/> event.
		/// </summary>
		StackNavigatorState State { get; }

		/// <summary>
		/// Notifies changes related to the <see cref="State"/> property.
		/// </summary>
		event StackNavigatorStateChangedEventHandler StateChanged;
	}	
}
