using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This represents a navigator that has multiple navigation stacks.
	/// </summary>
	public interface ISectionsNavigator
	{
		/// <summary>
		/// Creates and opens a new modal.
		/// Calling this adds an item in <see cref="SectionsNavigatorState.Modals"/>.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the parameters for the new modal.</param>
		/// <returns>The newly created secton navigator.</returns>
		Task<IModalStackNavigator> OpenModal(CancellationToken ct, SectionsNavigatorRequest request);

		/// <summary>
		/// Sets the active section.
		/// The active section is the one a user currently interacts with.
		/// Modals are displayed on top of sections.
		/// The inactive sections can still be interacted with programmatically.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the section name to set as active.</param>
		/// <returns>The active controller after the set operation.</returns>
		Task<ISectionStackNavigator> SetActiveSection(CancellationToken ct, SectionsNavigatorRequest request);

		/// <summary>
		/// Closes a modal.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the modal priority.</param>
		Task CloseModal(CancellationToken ct, SectionsNavigatorRequest request);

		/// <summary>
		/// Gets the state of this object.
		/// Changes to this property can be observed using <see cref="StateChanged"/>.
		/// </summary>
		SectionsNavigatorState State { get; }

		/// <summary>
		/// Notifies changes related to the <see cref="State"/> property.
		/// </summary>
		event SectionsNavigatorStateChangedEventHandler StateChanged;
	}
}
