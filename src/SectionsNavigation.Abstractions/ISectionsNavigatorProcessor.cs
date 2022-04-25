using Chinook.StackNavigation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chinook.SectionsNavigation
{
	public interface ISectionsNavigatorProcessor : INavigationProcessor
	{
		/// <summary>
		/// Creates and opens a new modal.
		/// Calling this adds an item in <see cref="SectionsNavigatorState.Modals"/>.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the parameters for the new modal.</param>
		/// <returns>The newly created secton navigator.</returns>
		Task<IModalStackNavigator> OpenModal(CancellationToken ct, SectionsNavigatorRequest request, NavigationOperation operation);

		/// <summary>
		/// Sets the active section.
		/// The active section is the one a user currently interacts with.
		/// Modals are displayed on top of sections.
		/// The inactive sections can still be interacted with programmatically.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the section name to set as active.</param>
		/// <returns>The active section after the set operation.</returns>
		Task<ISectionStackNavigator> SetActiveSection(CancellationToken ct, SectionsNavigatorRequest request, NavigationOperation operation);

		/// <summary>
		/// Closes a modal.
		/// </summary>
		/// <param name="ct">The cancellation token; note that cancellation is checked only at the very start of the execution.</param>
		/// <param name="request">The request containing the modal priority.</param>
		Task CloseModal(CancellationToken ct, SectionsNavigatorRequest request, NavigationOperation operation);
	}
}
