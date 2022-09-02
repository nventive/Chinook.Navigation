using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chinook.StackNavigation;

namespace Chinook.SectionsNavigation
{
	/// <summary>
	/// This objects represents the state of a <see cref="ISectionsNavigator"/>.
	/// This object is immutable.
	/// </summary>
	public class SectionsNavigatorState
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SectionsNavigatorState"/> class.
		/// </summary>
		/// <param name="sections">The list of sections.</param>
		/// <param name="activeSection">The active section.</param>
		/// <param name="modals">The list of modals.</param>
		/// <param name="lastRequestState">The state of the last request.</param>
		/// <param name="lastRequest">The last request.</param>
		public SectionsNavigatorState(
			IReadOnlyDictionary<string, ISectionStackNavigator> sections,
			ISectionStackNavigator activeSection,
			IReadOnlyList<IModalStackNavigator> modals,
			NavigatorRequestState lastRequestState,
			SectionsNavigatorRequest lastRequest)
		{
			Sections = sections;
			ActiveSection = activeSection;

			Modals = modals;
			ActiveModal = modals?.LastOrDefault();

			LastRequestState = lastRequestState;
			LastRequest = lastRequest;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SectionsNavigatorState"/> class.
		/// </summary>
		/// <param name="state">Another <see cref="SectionsNavigatorState"/> instance from which to copy the sections and modals.</param>
		/// <param name="lastRequestState">The state of the last request.</param>
		/// <param name="lastRequest">The last request.</param>
		public SectionsNavigatorState(
			SectionsNavigatorState state,
			NavigatorRequestState lastRequestState,
			SectionsNavigatorRequest lastRequest)
			: this(state.Sections, state.ActiveSection, state.Modals, lastRequestState, lastRequest)
		{
		}

		/// <summary>
		/// Gets a dictionary of <see cref="ISectionStackNavigator"/> instances, mapped by their <see cref="ISectionStackNavigator.Name"/> property.
		/// </summary>
		public IReadOnlyDictionary<string, ISectionStackNavigator> Sections { get; }

		/// <summary>
		/// Gets the <see cref="ISectionStackNavigator"/> instance that is currently active.
		/// </summary>
		public ISectionStackNavigator ActiveSection { get; }

		/// <summary>
		/// Gets an ordered list of <see cref="IModalStackNavigator"/> representing the modal stack. The last item is the active modal.
		/// </summary>
		public IReadOnlyList<IModalStackNavigator> Modals { get; }

		/// <summary>
		/// Gets the <see cref="ISectionStackNavigator"/> instance that is currently active.
		/// The active modal takes precedence over the <see cref="ActiveSection"/>.
		/// </summary>
		public IModalStackNavigator ActiveModal { get; }

		/// <summary>
		/// Gets the state of the most recent <see cref="SectionsNavigatorRequest"/> of the <see cref="ISectionsNavigator"/>.
		/// </summary>
		public NavigatorRequestState LastRequestState { get; }

		/// <summary>
		/// Gets the most recent <see cref="SectionsNavigatorRequest"/> of the <see cref="ISectionsNavigator"/>.
		/// </summary>
		public SectionsNavigatorRequest LastRequest { get; }
	}
}
